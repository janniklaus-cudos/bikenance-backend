using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
	/// <inheritdoc />
	public partial class InitialCreate : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "Bikes",
				columns: table => new
				{
					Id = table.Column<Guid>(type: "uuid", nullable: false),
					Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
					Brand = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
					IconId = table.Column<int>(type: "integer", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Bikes", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "BikePart",
				columns: table => new
				{
					Id = table.Column<Guid>(type: "uuid", nullable: false),
					Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
					Position = table.Column<int>(type: "integer", nullable: false),
					BikeId = table.Column<Guid>(type: "uuid", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_BikePart", x => x.Id);
					table.ForeignKey(
						name: "FK_BikePart_Bikes_BikeId",
						column: x => x.BikeId,
						principalTable: "Bikes",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "Journeys",
				columns: table => new
				{
					Id = table.Column<Guid>(type: "uuid", nullable: false),
					BikeId = table.Column<Guid>(type: "uuid", nullable: false),
					Title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
					Kilometer = table.Column<int>(type: "integer", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Journeys", x => x.Id);
					table.ForeignKey(
						name: "FK_Journeys_Bikes_BikeId",
						column: x => x.BikeId,
						principalTable: "Bikes",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "MaintenanceTasks",
				columns: table => new
				{
					Id = table.Column<Guid>(type: "uuid", nullable: false),
					BikePartId = table.Column<Guid>(type: "uuid", nullable: false),
					Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
					TimeInterval = table.Column<TimeSpan>(type: "interval", nullable: false),
					Cost = table.Column<int>(type: "integer", nullable: false),
					Importance = table.Column<int>(type: "integer", nullable: false),
					IsActive = table.Column<bool>(type: "boolean", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_MaintenanceTasks", x => x.Id);
					table.ForeignKey(
						name: "FK_MaintenanceTasks_BikePart_BikePartId",
						column: x => x.BikePartId,
						principalTable: "BikePart",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "ServiceEvents",
				columns: table => new
				{
					Id = table.Column<Guid>(type: "uuid", nullable: false),
					BikePartId = table.Column<Guid>(type: "uuid", nullable: false),
					Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
					StateAfterService = table.Column<int>(type: "integer", nullable: false),
					Cost = table.Column<int>(type: "integer", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_ServiceEvents", x => x.Id);
					table.ForeignKey(
						name: "FK_ServiceEvents_BikePart_BikePartId",
						column: x => x.BikePartId,
						principalTable: "BikePart",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateIndex(
				name: "IX_BikePart_BikeId",
				table: "BikePart",
				column: "BikeId");

			migrationBuilder.CreateIndex(
				name: "IX_Journeys_BikeId",
				table: "Journeys",
				column: "BikeId");

			migrationBuilder.CreateIndex(
				name: "IX_MaintenanceTasks_BikePartId",
				table: "MaintenanceTasks",
				column: "BikePartId");

			migrationBuilder.CreateIndex(
				name: "IX_ServiceEvents_BikePartId",
				table: "ServiceEvents",
				column: "BikePartId");
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "Journeys");

			migrationBuilder.DropTable(
				name: "MaintenanceTasks");

			migrationBuilder.DropTable(
				name: "ServiceEvents");

			migrationBuilder.DropTable(
				name: "BikePart");

			migrationBuilder.DropTable(
				name: "Bikes");
		}
	}
}
