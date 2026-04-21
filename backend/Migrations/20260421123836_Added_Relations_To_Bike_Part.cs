using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class Added_Relations_To_Bike_Part : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MaintenanceTasks_BikePartId",
                table: "MaintenanceTasks");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceTasks_BikePartId",
                table: "MaintenanceTasks",
                column: "BikePartId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MaintenanceTasks_BikePartId",
                table: "MaintenanceTasks");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceTasks_BikePartId",
                table: "MaintenanceTasks",
                column: "BikePartId");
        }
    }
}
