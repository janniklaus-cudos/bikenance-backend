using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class changed_maintenance_model : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimeInterval",
                table: "MaintenanceTasks");

            migrationBuilder.AddColumn<int>(
                name: "DaysInterval",
                table: "MaintenanceTasks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DistanceInterval",
                table: "MaintenanceTasks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsDaysIntervalActive",
                table: "MaintenanceTasks",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDistanceIntervalActive",
                table: "MaintenanceTasks",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DaysInterval",
                table: "MaintenanceTasks");

            migrationBuilder.DropColumn(
                name: "DistanceInterval",
                table: "MaintenanceTasks");

            migrationBuilder.DropColumn(
                name: "IsDaysIntervalActive",
                table: "MaintenanceTasks");

            migrationBuilder.DropColumn(
                name: "IsDistanceIntervalActive",
                table: "MaintenanceTasks");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "TimeInterval",
                table: "MaintenanceTasks",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));
        }
    }
}
