using System.Data;
using FluentMigrator;

namespace MushroomMapApp.Migrator.Migrations;

[Migration(202609151100)]
public class AddAvatarToUserTable : Migration
{
    public override void Up()
    {
        if (!Schema.Table("Users").Column("AvatarFileResourceId").Exists())
        {
            Alter.Table("Users")
                .AddColumn("AvatarFileResourceId").AsInt64().Nullable();

            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Create.ForeignKey("FK_Users_AvatarFileResourceId")
                .FromTable("Users").ForeignColumn("AvatarFileResourceId")
                .ToTable("FileResources").PrimaryColumn("Id")
                .OnDelete(Rule.SetNull);
        }

        if (!Schema.Table("FileResources").Column("UserId").Exists())
        {
            Alter.Table("FileResources")
                .AddColumn("UserId").AsInt64().Nullable();

            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Create.ForeignKey("FK_FileResources_UserId")
                .FromTable("FileResources").ForeignColumn("UserId")
                .ToTable("Users").PrimaryColumn("Id")
                .OnDelete(Rule.Cascade);
        }
    }

    public override void Down()
    {
        if (Schema.Table("Users").Column("AvatarFileResourceId").Exists())
        {
            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Delete.ForeignKey("FK_Users_AvatarFileResourceId").OnTable("Users");

            Delete.Column("AvatarFileResourceId").FromTable("Users");
        }

        if (Schema.Table("FileResources").Column("UserId").Exists())
        {
            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Delete.ForeignKey("FK_FileResources_UserId").OnTable("FileResources");

            Delete.Column("UserId").FromTable("FileResources");
        }
    }
}
