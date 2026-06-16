using FluentMigrator;

namespace MushroomMapApp.Migrator.Migrations;

[Migration(202606161000)]
public class AddReactionsTable : Migration
{
    public override void Up()
    {
        if (!Schema.Table("Reactions").Exists())
        {
            Create.Table("Reactions")
                .WithColumn("LocationId").AsInt64().NotNullable()
                .WithColumn("UserId").AsInt64().NotNullable()
                .WithColumn("ReactionTypeValue").AsString(64).NotNullable()
                .WithColumn("CreatedAtUtc").AsDateTime2().NotNullable();

            Create.PrimaryKey("PK_Reactions")
                .OnTable("Reactions")
                .Columns("LocationId", "UserId");

            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Create.ForeignKey("FK_Reactions_LocationId")
                .FromTable("Reactions").ForeignColumn("LocationId")
                .ToTable("Locations").PrimaryColumn("Id")
                .OnDelete(System.Data.Rule.Cascade);

            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Create.ForeignKey("FK_Reactions_UserId")
                .FromTable("Reactions").ForeignColumn("UserId")
                .ToTable("Users").PrimaryColumn("Id")
                .OnDelete(System.Data.Rule.Cascade);
        }
    }

    public override void Down()
    {
        if (Schema.Table("Reactions").Exists())
        {
            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Delete.ForeignKey("FK_Reactions_LocationId").OnTable("Reactions");

            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Delete.ForeignKey("FK_Reactions_UserId").OnTable("Reactions");
            Delete.PrimaryKey("PK_Reactions").FromTable("Reactions");

            Delete.Table("Reactions");
        }
    }
}
