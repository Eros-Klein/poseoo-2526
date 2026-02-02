using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SatisfactoryPlanner.Data.Migrations
{
    /// <inheritdoc />
    public partial class initial3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RecipeId",
                table: "Machines",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Machines_RecipeId",
                table: "Machines",
                column: "RecipeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Machines_Recipes_RecipeId",
                table: "Machines",
                column: "RecipeId",
                principalTable: "Recipes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Machines_Recipes_RecipeId",
                table: "Machines");

            migrationBuilder.DropIndex(
                name: "IX_Machines_RecipeId",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "RecipeId",
                table: "Machines");
        }
    }
}
