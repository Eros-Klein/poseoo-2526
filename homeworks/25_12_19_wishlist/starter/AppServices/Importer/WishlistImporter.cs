using System.Collections.ObjectModel;
using System.Data.Common;

namespace AppServices.Importer;

/// <summary>
/// Interface for importing wishlists from JSON files
/// </summary>
public interface IWishlistImporter
{
    /// <summary>
    /// Imports data from JSON files in the specified folder
    /// </summary>
    /// <param name="jsonFolderPath">Path to the folder with JSON files</param>
    /// <param name="isDryRun">If true, rollback transaction after import</param>
    /// <returns>Number of wishlists imported</returns>
    Task<int> ImportFromJsonAsync(string jsonFolderPath, bool isDryRun = false);
}

/// <summary>
/// Implementation for importing wishlists from JSON files
/// </summary>
public class WishlistImporter(IFileReader fileReader, IWishlistJsonParser jsonParser, IWishlistImportDatabaseWriter databaseWriter) : IWishlistImporter
{
    public async Task<int> ImportFromJsonAsync(string jsonFolderPath, bool isDryRun = false)
    {
        var files = fileReader.GetAllJsonFiles(jsonFolderPath);

        var count = 0;
        var unSuccessfulCount = 0;
        List<GiftCategory> categoryCache = [];

        await databaseWriter.BeginTransactionAsync();

        foreach (string filePath in files)
        {
            try
            {
                var fileContent = await fileReader.ReadAllTextAsync(filePath);
                var wishlist = jsonParser.ParseJson(filePath, fileContent);

                if (await databaseWriter.WishlistExistsAsync(wishlist.Wishlist.Name))
                {
                    unSuccessfulCount++;
                    continue;
                }

                Wishlist wishlistEntry = new()
                {
                    Name = wishlist.Wishlist.Name,
                    ParentPin = wishlist.Wishlist.ParentPin,
                    ChildPin = wishlist.Wishlist.ChildPin
                };

                wishlistEntry.Items.AddRange(await Task.WhenAll(wishlist.Items.Select(async i =>
                {
                    var category = categoryCache.Exists(ig => ig.Name.Equals(i.Category)) ? 
                        categoryCache.Find(ig => ig.Name.Equals(i.Category)) 
                            : await databaseWriter.GetOrCreateCategoryAsync(i.Category);

                    if (!categoryCache.Contains(category!))
                    {
                        categoryCache.Add(category!);
                    }
                    
                    return new WishlistItem()
                    {
                        Wishlist = wishlistEntry,
                        Category = category!,
                        ItemName = i.ItemName,
                        Bought = i.Bought
                    };
                })));

                await databaseWriter.WriteWishlistAsync(wishlistEntry);
                count++;
            }
            catch(Exception e)
            {
                await databaseWriter.RollbackTransactionAsync();
                throw e;
            }
        }

        if (isDryRun)
        {
            await databaseWriter.RollbackTransactionAsync();
        }
        else
        {
            await databaseWriter.CommitTransactionAsync();
        }

        Console.WriteLine("Successful counts: " + count);
        Console.WriteLine("Unsuccessful counts: " + unSuccessfulCount);

        return count;
    }
}