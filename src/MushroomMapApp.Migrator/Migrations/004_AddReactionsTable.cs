using FluentMigrator;

namespace MushroomMapApp.Migrator.Migrations;

[Migration(202606161000)]
public class AddReactionsTable : Migration
{
    public override void Up()
    {
        if (!Schema.Table("Reactions").Exists())
        {
            Create.Table("ReactionTypes")
                .WithColumn("Id").AsInt64().Identity().PrimaryKey()
                .WithColumn("PublicId").AsGuid().WithDefault(SystemMethods.NewGuid).NotNullable()
                .WithColumn("Key").AsString(128).NotNullable()
                .WithColumn("Name").AsString(128).NotNullable()
                .WithColumn("Icon").AsString(128).NotNullable()
                .WithColumn("CreatedAtUtc").AsDateTime2().NotNullable();

            Create.Table("Reactions")
                .WithColumn("LocationId").AsInt64().NotNullable()
                .WithColumn("UserId").AsInt64().NotNullable()
                .WithColumn("ReactionTypeId").AsInt64().NotNullable()
                .WithColumn("CreatedAtUtc").AsDateTime2().NotNullable();

            Create.PrimaryKey("PK_Reactions")
                .OnTable("Reactions")
                .Columns("LocationId", "UserId");

            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Create.ForeignKey("FK_Reactions_ReactionTypeId")
                .FromTable("Reactions").ForeignColumn("ReactionTypeId")
                .ToTable("ReactionTypes").PrimaryColumn("Id")
                .OnDelete(System.Data.Rule.None);

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

            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Delete.ForeignKey("FK_Reactions_ReactionTypeId").OnTable("Reactions");

            Delete.PrimaryKey("PK_Reactions").FromTable("Reactions");

            Delete.Table("Reactions");
            Delete.Table("ReactionTypes");
        }
    }
}
