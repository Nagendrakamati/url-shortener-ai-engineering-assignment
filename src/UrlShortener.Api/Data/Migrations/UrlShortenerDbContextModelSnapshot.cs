using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace UrlShortener.Api.Data.Migrations;

[DbContext(typeof(UrlShortenerDbContext))]
public partial class UrlShortenerDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasAnnotation("ProductVersion", "8.0.0")
            .HasAnnotation("Relational:MaxIdentifierLength", 128);

        SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

        modelBuilder.Entity("UrlShortener.Api.Models.ShortUrl", entity =>
        {
            entity.Property<long>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("bigint");

            entity.Property<long>("ClickCount")
                .ValueGeneratedOnAdd()
                .HasColumnType("bigint")
                .HasDefaultValue(0L);

            entity.Property<DateTime>("CreatedAtUtc")
                .ValueGeneratedOnAdd()
                .HasColumnType("datetime2")
                .HasDefaultValueSql("SYSUTCDATETIME()");

            entity.Property<DateTime?>("LastAccessedAtUtc")
                .HasColumnType("datetime2");

            entity.Property<string>("OriginalUrl")
                .IsRequired()
                .HasMaxLength(2048)
                .HasColumnType("nvarchar(2048)");

            entity.Property<string>("ShortCode")
                .IsRequired()
                .HasMaxLength(10)
                .IsUnicode(false)
                .UseCollation("Latin1_General_100_BIN2")
                .HasColumnType("varchar(10)");

            entity.HasKey("Id");

            entity.HasIndex("ShortCode")
                .IsUnique();

            entity.ToTable("ShortUrls");
        });
    }
}