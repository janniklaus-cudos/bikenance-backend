using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddedRepeatJourneys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsConnectedToRepeatJourney",
                table: "Journeys",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "RepeatJourneyId",
                table: "Journeys",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RepeatJourneys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BikeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Distance = table.Column<int>(type: "integer", nullable: false),
                    RepeatDays = table.Column<int>(type: "integer", nullable: false),
                    RepeatType = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepeatJourneys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RepeatJourneys_Bikes_BikeId",
                        column: x => x.BikeId,
                        principalTable: "Bikes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Journeys_RepeatJourneyId",
                table: "Journeys",
                column: "RepeatJourneyId");

            migrationBuilder.CreateIndex(
                name: "IX_RepeatJourneys_BikeId",
                table: "RepeatJourneys",
                column: "BikeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Journeys_RepeatJourneys_RepeatJourneyId",
                table: "Journeys",
                column: "RepeatJourneyId",
                principalTable: "RepeatJourneys",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Journeys_RepeatJourneys_RepeatJourneyId",
                table: "Journeys");

            migrationBuilder.DropTable(
                name: "RepeatJourneys");

            migrationBuilder.DropIndex(
                name: "IX_Journeys_RepeatJourneyId",
                table: "Journeys");

            migrationBuilder.DropColumn(
                name: "IsConnectedToRepeatJourney",
                table: "Journeys");

            migrationBuilder.DropColumn(
                name: "RepeatJourneyId",
                table: "Journeys");
        }
    }
}
