using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Hive_IT.Migrations.CustomerData
{
    public partial class HistoryCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ManufacturerName",
                table: "Manufacturers",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Model",
                table: "DeviceModels",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "Histories",
                columns: table => new
                {
                    HistoryID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ActionTaken = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeviceID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FieldValueAfter = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FieldValueBefore = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TimeOfAction = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WorkOrderNumber = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Histories", x => x.HistoryID);
                    table.ForeignKey(
                        name: "FK_Histories_WorkOrders_WorkOrderNumber",
                        column: x => x.WorkOrderNumber,
                        principalTable: "WorkOrders",
                        principalColumn: "WorkOrderNumber",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Histories_WorkOrderNumber",
                table: "Histories",
                column: "WorkOrderNumber");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Histories");

            migrationBuilder.AlterColumn<string>(
                name: "ManufacturerName",
                table: "Manufacturers",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Model",
                table: "DeviceModels",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}
