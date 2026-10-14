using FluentMigrator;

namespace MushroomMapApp.Migrator.Migrations;

[Migration(202610131100)]
public class AddUserSavedLocationsTable : Migration
{
    public override void Up()
    {
        if (!Schema.Table("UserSavedLocations").Exists())
        {
            Create.Table("UserSavedLocations")
                .WithColumn("Id").AsInt64().PrimaryKey().Identity()
                .WithColumn("PublicId").AsGuid().WithDefault(SystemMethods.NewGuid).NotNullable()
                .WithColumn("UserId").AsInt64().NotNullable()
                .WithColumn("LocationName").AsString(255).NotNullable()
                .WithColumn("Coordinates").AsCustom("geometry(Point, 4326)").NotNullable()
                .WithColumn("CreatedAtUtc").AsDateTime2().NotNullable()
                .WithColumn("UpdatedAtUtc").AsDateTime2().Nullable();

            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Create.ForeignKey("FK_UserSavedLocations_UserId")
                .FromTable("UserSavedLocations").ForeignColumn("UserId")
                .ToTable("Users").PrimaryColumn("Id")
                .OnDelete(System.Data.Rule.Cascade);

            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Create.Index("IX_UserSavedLocations_PublicId")
                .OnTable("UserSavedLocations")
                .OnColumn("PublicId").Ascending()
                .WithOptions().Unique();

            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Create.Index("IX_UserSavedLocations_UserId")
                .OnTable("UserSavedLocations")
                .OnColumn("UserId").Ascending();
        }
    }

    public override void Down()
    {
        if (Schema.Table("UserSavedLocations").Exists())
        {
            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Delete.Index("IX_UserSavedLocations_UserId");

            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Delete.Index("IX_UserSavedLocations_PublicId");

            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Delete.ForeignKey("FK_UserSavedLocations_UserId");

            Delete.Table("UserSavedLocations");
        }
    }
}
