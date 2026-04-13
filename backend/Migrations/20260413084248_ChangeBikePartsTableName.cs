using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
	/// <inheritdoc />
	public partial class ChangeBikePartsTableName : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropForeignKey(
				name: "FK_BikePart_Bikes_BikeId",
				table: "BikePart");

			migrationBuilder.DropForeignKey(
				name: "FK_MaintenanceTasks_BikePart_BikePartId",
				table: "MaintenanceTasks");

			migrationBuilder.DropForeignKey(
				name: "FK_ServiceEvents_BikePart_BikePartId",
				table: "ServiceEvents");

			migrationBuilder.DropPrimaryKey(
				name: "PK_BikePart",
				table: "BikePart");

			migrationBuilder.RenameTable(
				name: "BikePart",
				newName: "BikeParts");

			migrationBuilder.RenameIndex(
				name: "IX_BikePart_BikeId",
				table: "BikeParts",
				newName: "IX_BikeParts_BikeId");

			migrationBuilder.AddPrimaryKey(
				name: "PK_BikeParts",
				table: "BikeParts",
				column: "Id");

			migrationBuilder.AddForeignKey(
				name: "FK_BikeParts_Bikes_BikeId",
				table: "BikeParts",
				column: "BikeId",
				principalTable: "Bikes",
				principalColumn: "Id",
				onDelete: ReferentialAction.Cascade);

			migrationBuilder.AddForeignKey(
				name: "FK_MaintenanceTasks_BikeParts_BikePartId",
				table: "MaintenanceTasks",
				column: "BikePartId",
				principalTable: "BikeParts",
				principalColumn: "Id",
				onDelete: ReferentialAction.Cascade);

			migrationBuilder.AddForeignKey(
				name: "FK_ServiceEvents_BikeParts_BikePartId",
				table: "ServiceEvents",
				column: "BikePartId",
				principalTable: "BikeParts",
				principalColumn: "Id",
				onDelete: ReferentialAction.Cascade);
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropForeignKey(
				name: "FK_BikeParts_Bikes_BikeId",
				table: "BikeParts");

			migrationBuilder.DropForeignKey(
				name: "FK_MaintenanceTasks_BikeParts_BikePartId",
				table: "MaintenanceTasks");

			migrationBuilder.DropForeignKey(
				name: "FK_ServiceEvents_BikeParts_BikePartId",
				table: "ServiceEvents");

			migrationBuilder.DropPrimaryKey(
				name: "PK_BikeParts",
				table: "BikeParts");

			migrationBuilder.RenameTable(
				name: "BikeParts",
				newName: "BikePart");

			migrationBuilder.RenameIndex(
				name: "IX_BikeParts_BikeId",
				table: "BikePart",
				newName: "IX_BikePart_BikeId");

			migrationBuilder.AddPrimaryKey(
				name: "PK_BikePart",
				table: "BikePart",
				column: "Id");

			migrationBuilder.AddForeignKey(
				name: "FK_BikePart_Bikes_BikeId",
				table: "BikePart",
				column: "BikeId",
				principalTable: "Bikes",
				principalColumn: "Id",
				onDelete: ReferentialAction.Cascade);

			migrationBuilder.AddForeignKey(
				name: "FK_MaintenanceTasks_BikePart_BikePartId",
				table: "MaintenanceTasks",
				column: "BikePartId",
				principalTable: "BikePart",
				principalColumn: "Id",
				onDelete: ReferentialAction.Cascade);

			migrationBuilder.AddForeignKey(
				name: "FK_ServiceEvents_BikePart_BikePartId",
				table: "ServiceEvents",
				column: "BikePartId",
				principalTable: "BikePart",
				principalColumn: "Id",
				onDelete: ReferentialAction.Cascade);
		}
	}
}
