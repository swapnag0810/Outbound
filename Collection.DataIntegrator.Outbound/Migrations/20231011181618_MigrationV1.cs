using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Collection.DataIntegrator.Outbound.Migrations
{
    public partial class MigrationV1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EventType",
                columns: table => new
                {
                    EventTypeId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventName = table.Column<string>(nullable: true),
                    IsActive = table.Column<bool>(nullable: false),
                    Handler = table.Column<string>(nullable: true),
                    FileName = table.Column<string>(nullable: true),
                    Type = table.Column<string>(nullable: true),
                    CreatedAt = table.Column<DateTime>(nullable: true),
                    UpdatedAt = table.Column<DateTime>(nullable: true),
                    SFTPOutboundFolderName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventType", x => x.EventTypeId);
                });

            migrationBuilder.CreateTable(
                name: "OutboundDataQueue",
                columns: table => new
                {
                    OutboundDataQueueId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Content = table.Column<string>(nullable: true),
                    Status = table.Column<bool>(nullable: true),
                    CreatedAt = table.Column<DateTime>(nullable: true),
                    UpdatedAt = table.Column<DateTime>(nullable: true),
                    EventTypeId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboundDataQueue", x => x.OutboundDataQueueId);
                });

            migrationBuilder.CreateTable(
                name: "OutboundDataQueueLog",
                columns: table => new
                {
                    OutboundDataQueueLogId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OutboundDataQueueId = table.Column<int>(nullable: false),
                    EventTypeId = table.Column<int>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    Status = table.Column<bool>(nullable: true),
                    CreatedAt = table.Column<DateTime>(nullable: true),
                    UpdatedAt = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboundDataQueueLog", x => x.OutboundDataQueueLogId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventType");

            migrationBuilder.DropTable(
                name: "OutboundDataQueue");

            migrationBuilder.DropTable(
                name: "OutboundDataQueueLog");
        }
    }
}
