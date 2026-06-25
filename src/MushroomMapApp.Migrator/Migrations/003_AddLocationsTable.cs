using FluentMigrator;

namespace MushroomMapApp.Migrator.Migrations;

[Migration(202606151000)]
public class AddLocationsTable : Migration
{
    public override void Up()
    {
        if (!Schema.Table("Locations").Exists())
        {
            Create.Table("Locations")
                .WithColumn("Id").AsInt64().PrimaryKey().Identity()
                .WithColumn("PublicId").AsGuid().WithDefault(SystemMethods.NewGuid).NotNullable()
                .WithColumn("Name").AsString(512).NotNullable()
                .WithColumn("Text").AsString(4096).NotNullable()
                .WithColumn("Coordinates").AsCustom("geometry(Point, 4326)").NotNullable()
                .WithColumn("CreatedById").AsInt64().NotNullable()
                .WithColumn("CreatedAtUtc").AsDateTime2().NotNullable()
                .WithColumn("UpdatedAtUtc").AsDateTime2().Nullable();

            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Create.ForeignKey("FK_Locations_CreatedById")
                .FromTable("Locations").ForeignColumn("CreatedById")
                .ToTable("Users").PrimaryColumn("Id");

            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Create.Index("IX_Locations_PublicId")
                .OnTable("Locations").OnColumn("PublicId").Ascending()
                .WithOptions().Unique();

            IfDatabase("postgresql")
                .Execute.Sql("CREATE INDEX \"IX_Locations_Coordinates\" ON \"Locations\" USING GIST (\"Coordinates\");");
        }

    }

    public override void Down()
    {
        if (Schema.Table("Locations").Exists())
        {
            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Delete.ForeignKey("FK_Locations_CreatedById").OnTable("Locations");

            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Delete.Index("IX_Locations_PublicId").OnTable("Locations");

            IfDatabase("postgresql")
                .Execute.Sql("DROP INDEX IF EXISTS \"IX_Locations_Coordinates\";");

            Delete.Table("Locations");
        }
    }
}
