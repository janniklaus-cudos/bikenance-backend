using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddOwnerToBike : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OwnerId",
                table: "Bikes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Bikes_OwnerId",
                table: "Bikes",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bikes_Users_OwnerId",
                table: "Bikes",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bikes_Users_OwnerId",
                table: "Bikes");

            migrationBuilder.DropIndex(
                name: "IX_Bikes_OwnerId",
                table: "Bikes");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Bikes");
        }
    }
}
