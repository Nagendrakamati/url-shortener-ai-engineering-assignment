using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UrlShortener.Api.Data.Migrations;

[DbContext(typeof(UrlShortenerDbContext))]
[Migration("20260920000000_InitialCreate")]
public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ShortUrls",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                ShortCode = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false, collation: "Latin1_General_100_BIN2"),
                OriginalUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                ClickCount = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                LastAccessedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ShortUrls", entity => entity.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ShortUrls_ShortCode",
            table: "ShortUrls",
            column: "ShortCode",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "ShortUrls");
    }
}