using FluentMigrator;

namespace MushroomMapApp.Migrator.Migrations;

[Migration(202606171000)]
public class AddFileResourcesTable : Migration
{
    public override void Up()
    {
        if (!Schema.Table("FileResources").Exists())
        {
            Create.Table("FileResources")
                .WithColumn("Id").AsInt64().PrimaryKey().Identity()
                .WithColumn("PublicId").AsGuid().WithDefault(SystemMethods.NewGuid).NotNullable()
                .WithColumn("FileName").AsString(1024).NotNullable()
                .WithColumn("Description").AsString(2048).Nullable()
                .WithColumn("ContentType").AsString(256).Nullable()
                .WithColumn("Size").AsInt64().NotNullable()
                .WithColumn("CreatedAtUtc").AsDateTime2().NotNullable()
                .WithColumn("Type").AsString(64).NotNullable()
                .WithColumn("LocationId").AsInt64().Nullable();

            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Create.ForeignKey("FK_FileResources_LocationId")
                .FromTable("FileResources").ForeignColumn("LocationId")
                .ToTable("Locations").PrimaryColumn("Id")
                .OnDelete(System.Data.Rule.Cascade);

            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Create.Index("IX_FileResources_PublicId")
                .OnTable("FileResources").OnColumn("PublicId").Ascending()
                .WithOptions().Unique();
        }
    }

    public override void Down()
    {
        if (Schema.Table("FileResources").Exists())
        {
            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Delete.ForeignKey("FK_FileResources_LocationId").OnTable("FileResources");

            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Delete.Index("IX_FileResources_PublicId").OnTable("FileResources");

            Delete.Table("FileResources");
        }
    }
}
