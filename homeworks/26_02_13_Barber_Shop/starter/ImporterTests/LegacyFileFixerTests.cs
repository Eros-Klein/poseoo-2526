using AppServices;
using AppServices.Importer;
using NSubstitute;

namespace ImporterTests;

public class LegacyFileFixerTests
{
    private readonly IFileReader _mockFileReader;
    private readonly LegacyFileFixer _fixer;

    public LegacyFileFixerTests()
    {
        _mockFileReader = Substitute.For<IFileReader>();
        _fixer = new LegacyFileFixer(_mockFileReader);
    }

    [Fact]
    public async Task ImportAsync_ValidXmlWithSingleAppointment_ReturnsSuccessfulImport()
    {
        // Arrange
        var xmlContent = @"<Appointment Id=""101"" Customer=""John Doe"" Date=""2023-10-01"" Start=""09:00"" Duration=""30"" Services=""CUT|SHAVE"" Barber=""Gerrit"" />";

        _mockFileReader.ReadAllTextAsync(Arg.Any<string>()).Returns(xmlContent);

        // Act
        var result = await _fixer.ImportAsync("test.xml");

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Successes);
        Assert.Empty(result.Failures);
        
        var appointment = result.Successes[0];
        Assert.Equal(101, appointment.Id);
        Assert.Equal("John Doe", appointment.CustomerName);
        Assert.Equal(new DateOnly(2023, 10, 1), appointment.Date);
        Assert.Equal(new TimeOnly(9, 0), appointment.StartTime);
        Assert.Equal(TimeSpan.FromMinutes(30), appointment.Duration);
        Assert.Equal("Gerrit", appointment.BarberName);
        Assert.Equal(2, appointment.Services.Count);
    }

    [Fact]
    public async Task ImportAsync_BrokenXmlWithMultipleRoots_FixesAndImports()
    {
        // Arrange - Broken XML with multiple root elements (no wrapping Root)
        var brokenXml = @"<Appointment Id=""201"" Customer=""Alice"" Date=""2024-03-16"" Start=""14:00"" Duration=""60"" Services=""CUT"" Barber=""Todd"" />
<Appointment Id=""202"" Customer=""Bob"" Date=""2024-03-17"" Start=""15:00"" Duration=""30"" Services=""SHAVE"" Barber=""Gerrit"" />";

        _mockFileReader.ReadAllTextAsync(Arg.Any<string>()).Returns(brokenXml);

        // Act
        var result = await _fixer.ImportAsync("test.xml");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Successes.Count);
        Assert.Empty(result.Failures);
        
        Assert.Equal("Alice", result.Successes[0].CustomerName);
        Assert.Equal("Bob", result.Successes[1].CustomerName);
    }

    [Fact]
    public async Task ImportAsync_XmlWithUnescapedAmpersand_FixesAndImports()
    {
        // Arrange - XML with unescaped & character in attribute
        var brokenXml = @"<Appointment Id=""102"" Customer=""Jane Smith & Sons"" Date=""02.10.2023"" Start=""10:00"" Duration=""60"" Services=""COLOR"" Barber=""Todd"" />";

        _mockFileReader.ReadAllTextAsync(Arg.Any<string>()).Returns(brokenXml);

        // Act
        var result = await _fixer.ImportAsync("test.xml");

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Successes);
        Assert.Equal("Jane Smith & Sons", result.Successes[0].CustomerName);
        Assert.Equal(new DateOnly(2023, 10, 2), result.Successes[0].Date);
    }

    [Fact]
    public async Task ImportAsync_DateInGermanFormat_ParsesCorrectly()
    {
        // Arrange - DD.MM.YYYY format
        var xmlContent = @"<Appointment Id=""106"" Customer=""Complex Case"" Date=""05.10.2023"" Start=""14:00"" Duration=""15"" Services=""BEARD|TRIM|SHAVE"" Barber=""Todd"" />";

        _mockFileReader.ReadAllTextAsync(Arg.Any<string>()).Returns(xmlContent);

        // Act
        var result = await _fixer.ImportAsync("test.xml");

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Successes);
        Assert.Equal(new DateOnly(2023, 10, 5), result.Successes[0].Date);
        Assert.Equal("Complex Case", result.Successes[0].CustomerName);
    }

    [Fact]
    public async Task ImportAsync_DateInIsoFormat_ParsesCorrectly()
    {
        // Arrange - YYYY-MM-DD format
        var xmlContent = @"<Appointment Id=""101"" Customer=""John Doe"" Date=""2023-10-01"" Start=""09:00"" Duration=""30"" Services=""CUT|SHAVE"" Barber=""Gerrit"" />";

        _mockFileReader.ReadAllTextAsync(Arg.Any<string>()).Returns(xmlContent);

        // Act
        var result = await _fixer.ImportAsync("test.xml");

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Successes);
        Assert.Equal(new DateOnly(2023, 10, 1), result.Successes[0].Date);
        Assert.Equal("John Doe", result.Successes[0].CustomerName);
    }

    [Fact]
    public async Task ImportAsync_PipeSeparatedServices_ParsesAllServices()
    {
        // Arrange
        var xmlContent = @"<Appointment Id=""106"" Customer=""Complex Case"" Date=""05.10.2023"" Start=""14:00"" Duration=""15"" Services=""BEARD|TRIM|SHAVE"" Barber=""Todd"" />";

        _mockFileReader.ReadAllTextAsync(Arg.Any<string>()).Returns(xmlContent);

        // Act
        var result = await _fixer.ImportAsync("test.xml");

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Successes);
        Assert.Equal(3, result.Successes[0].Services.Count);
    }

    [Fact]
    public async Task ImportAsync_MissingCustomerName_ReturnsFailure()
    {
        // Arrange
        var xmlContent = @"<Appointment Id=""104"" Customer="""" Date=""2023-10-04"" Start=""12:00"" Duration=""30"" Services=""CUT"" Barber=""Todd"" />";

        _mockFileReader.ReadAllTextAsync(Arg.Any<string>()).Returns(xmlContent);

        // Act
        var result = await _fixer.ImportAsync("test.xml");

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Successes);
        Assert.Single(result.Failures);
        Assert.Equal("104", result.Failures[0].RecordId);
        Assert.Equal(ImportError.MissingCompulsoryField, result.Failures[0].Error);
    }

    [Fact]
    public async Task ImportAsync_InvalidDate_ReturnsFailure()
    {
        // Arrange
        var xmlContent = @"<Appointment Id=""105"" Customer=""Invalid Date Guy"" Date=""Not-A-Date"" Start=""13:00"" Duration=""30"" Services=""CUT"" Barber=""Gerrit"" />";

        _mockFileReader.ReadAllTextAsync(Arg.Any<string>()).Returns(xmlContent);

        // Act
        var result = await _fixer.ImportAsync("test.xml");

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Successes);
        Assert.Single(result.Failures);
        Assert.Equal("105", result.Failures[0].RecordId);
        Assert.Equal(ImportError.InvalidDate, result.Failures[0].Error);
    }

    [Fact]
    public async Task ImportAsync_NoServices_ReturnsFailure()
    {
        // Arrange
        var xmlContent = @"<Appointment Id=""103"" Customer=""Bobby Tables"" Date=""2023-10-03"" Start=""11:00"" Duration=""45"" Services="""" Barber=""Gerrit"" />";

        _mockFileReader.ReadAllTextAsync(Arg.Any<string>()).Returns(xmlContent);

        // Act
        var result = await _fixer.ImportAsync("test.xml");

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Successes);
        Assert.Single(result.Failures);
        Assert.Equal("103", result.Failures[0].RecordId);
        Assert.Equal(ImportError.NoServices, result.Failures[0].Error);
    }

    [Fact]
    public async Task ImportAsync_MixedValidAndInvalidRecords_ReturnsPartialSuccess()
    {
        // Arrange - Mix of valid and invalid records from BrokenData.xml
        var xmlContent = @"<Appointment Id=""101"" Customer=""John Doe"" Date=""2023-10-01"" Start=""09:00"" Duration=""30"" Services=""CUT|SHAVE"" Barber=""Gerrit"" />
<Appointment Id=""104"" Customer="""" Date=""2023-10-04"" Start=""12:00"" Duration=""30"" Services=""CUT"" Barber=""Todd"" />
<Appointment Id=""102"" Customer=""Jane Smith & Sons"" Date=""02.10.2023"" Start=""10:00"" Duration=""60"" Services=""COLOR"" Barber=""Todd"" />";

        _mockFileReader.ReadAllTextAsync(Arg.Any<string>()).Returns(xmlContent);

        // Act
        var result = await _fixer.ImportAsync("test.xml");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Successes.Count);
        Assert.Single(result.Failures);
        Assert.Equal("104", result.Failures[0].RecordId);
        Assert.Equal(ImportError.MissingCompulsoryField, result.Failures[0].Error);
    }

    [Fact]
    public async Task ImportAsync_ServiceMapping_MapsToCorrectStyleReferences()
    {
        // Arrange
        var xmlContent = @"<Appointment Id=""101"" Customer=""John Doe"" Date=""2023-10-01"" Start=""09:00"" Duration=""30"" Services=""CUT|SHAVE"" Barber=""Gerrit"" />";

        _mockFileReader.ReadAllTextAsync(Arg.Any<string>()).Returns(xmlContent);

        // Act
        var result = await _fixer.ImportAsync("test.xml");

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Successes);
        
        var services = result.Successes[0].Services;
        Assert.Equal(2, services.Count);
        
        // Verify services use proper StyleReference enum values
        Assert.Contains(services, s => s.StyleReference == StyleReference.Medium); // CUT
        Assert.Contains(services, s => s.StyleReference == StyleReference.CleanShaven); // SHAVE
    }

    [Fact]
    public async Task ImportAsync_OptionalFields_HandlesNullValues()
    {
        // Arrange - BeverageChoice and IsVip are optional fields not in legacy data
        var xmlContent = @"<Appointment Id=""201"" Customer=""Regular Joe"" Date=""2024-01-15"" Start=""09:00"" Duration=""45"" Services=""CUT"" Barber=""Gerrit"" />";

        _mockFileReader.ReadAllTextAsync(Arg.Any<string>()).Returns(xmlContent);

        // Act
        var result = await _fixer.ImportAsync("test.xml");

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Successes);
        
        var appointment = result.Successes[0];
        Assert.Equal("Regular Joe", appointment.CustomerName);
        Assert.Null(appointment.BeverageChoice);
        Assert.False(appointment.IsVip);
    }

    [Fact]
    public async Task ImportAsync_ComplexBrokenXml_FixesAllIssues()
    {
        // Arrange - Combination of issues: multiple roots, unescaped ampersands, mixed date formats
        var brokenXml = @"<Appointment Id=""102"" Customer=""Jane Smith & Sons"" Date=""02.10.2023"" Start=""10:00"" Duration=""60"" Services=""COLOR"" Barber=""Todd"" />
<Appointment Id=""106"" Customer=""Complex Case"" Date=""05.10.2023"" Start=""14:00"" Duration=""15"" Services=""BEARD|TRIM|SHAVE"" Barber=""Todd"" />";

        _mockFileReader.ReadAllTextAsync(Arg.Any<string>()).Returns(brokenXml);

        // Act
        var result = await _fixer.ImportAsync("test.xml");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Successes.Count);
        Assert.Empty(result.Failures);
        
        Assert.Equal("Jane Smith & Sons", result.Successes[0].CustomerName);
        Assert.Equal(new DateOnly(2023, 10, 2), result.Successes[0].Date);
        
        Assert.Equal("Complex Case", result.Successes[1].CustomerName);
        Assert.Equal(new DateOnly(2023, 10, 5), result.Successes[1].Date);
        Assert.Equal(3, result.Successes[1].Services.Count);
    }

    [Fact]
    public async Task ImportAsync_ActualBrokenDataFile_HandlesAllRecords()
    {
        // Arrange - Actual content from BrokenData.xml
        var brokenXml = @"<Appointment Id=""101"" Customer=""John Doe"" Date=""2023-10-01"" Start=""09:00"" Duration=""30"" Services=""CUT|SHAVE"" Barber=""Gerrit"" />
<Appointment Id=""102"" Customer=""Jane Smith & Sons"" Date=""02.10.2023"" Start=""10:00"" Duration=""60"" Services=""COLOR"" Barber=""Todd"" />
<Appointment Id=""103"" Customer=""Bobby Tables"" Date=""2023-10-03"" Start=""11:00"" Duration=""45"" Services="""" Barber=""Gerrit"" />
<Appointment Id=""104"" Customer="""" Date=""2023-10-04"" Start=""12:00"" Duration=""30"" Services=""CUT"" Barber=""Todd"" />
<Appointment Id=""105"" Customer=""Invalid Date Guy"" Date=""Not-A-Date"" Start=""13:00"" Duration=""30"" Services=""CUT"" Barber=""Gerrit"" />
<Appointment Id=""106"" Customer=""Complex Case"" Date=""05.10.2023"" Start=""14:00"" Duration=""15"" Services=""BEARD|TRIM|SHAVE"" Barber=""Todd"" />";

        _mockFileReader.ReadAllTextAsync(Arg.Any<string>()).Returns(brokenXml);

        // Act
        var result = await _fixer.ImportAsync("test.xml");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Successes.Count); // Records 101, 102, 106 should succeed
        Assert.Equal(3, result.Failures.Count);   // Records 103, 104, 105 should fail
        
        // Verify successful imports
        Assert.Contains(result.Successes, a => a.Id == 101 && a.CustomerName == "John Doe");
        Assert.Contains(result.Successes, a => a.Id == 102 && a.CustomerName == "Jane Smith & Sons");
        Assert.Contains(result.Successes, a => a.Id == 106 && a.CustomerName == "Complex Case");
        
        // Verify failures
        Assert.Contains(result.Failures, f => f.RecordId == "103" && f.Error == ImportError.NoServices);
        Assert.Contains(result.Failures, f => f.RecordId == "104" && f.Error == ImportError.MissingCompulsoryField);
        Assert.Contains(result.Failures, f => f.RecordId == "105" && f.Error == ImportError.InvalidDate);
    }
}
