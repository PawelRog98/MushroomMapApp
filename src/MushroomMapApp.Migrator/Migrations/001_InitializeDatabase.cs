using FluentMigrator;

namespace MushroomMapApp.Migrator.Migrations;

[Migration(202605301100)]
public class InitializeDatabase : Migration
{
    public override void Up()
    {
        IfDatabase("postgresql")
            .Execute.Sql("CREATE EXTENSION IF NOT EXISTS \"pgcrypto\";");
        
        IfDatabase("postgresql")
            .Execute.Sql("CREATE EXTENSION IF NOT EXISTS \"uuid-ossp\";");

        if (!Schema.Table("Roles").Exists())
        {
            Create.Table("Roles")
                .WithColumn("Id").AsInt64().PrimaryKey().Identity()
                .WithColumn("PublicId").AsGuid().WithDefault(SystemMethods.NewGuid).NotNullable()
                .WithColumn("Name").AsString(512).NotNullable();
        }

        if (!Schema.Table("Users").Exists())
        {
            Create.Table("Users")
                .WithColumn("Id").AsInt64().PrimaryKey().Identity()
                .WithColumn("PublicId").AsGuid().WithDefault(SystemMethods.NewGuid).NotNullable()
                .WithColumn("PublicNick").AsString(512).NotNullable()
                .WithColumn("FirstName").AsString(512).NotNullable()
                .WithColumn("LastName").AsString(512).NotNullable()
                .WithColumn("Email").AsString(512).NotNullable()
                .WithColumn("PasswordHash").AsString(4096).NotNullable()
                .WithColumn("DateOfBirth").AsDateTime().NotNullable()
                .WithColumn("IsEmailConfirmed").AsBoolean().NotNullable()
                .WithColumn("AccountInfo").AsString(4096).Nullable()
                .WithColumn("RoleId").AsInt64().NotNullable()
                .WithColumn("CreatedAtUtc").AsDateTime2().NotNullable()
                .WithColumn("ModifiedAtUtc").AsDateTime2().Nullable();

            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Create.ForeignKey("FK_Users_RoleId")
                .FromTable("Users").ForeignColumn("RoleId")
                .ToTable("Roles").PrimaryColumn("Id")
                .OnDelete(System.Data.Rule.None);

            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Create.Index("IX_Users_PublicId")
                .OnTable("Users").OnColumn("PublicId").Ascending()
                .WithOptions().Unique();
        }
    }

    public override void Down()
    {
        if (Schema.Table("Users").Exists())
        {
            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Delete.Index("IX_Users_PublicId").OnTable("Users");

            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Delete.ForeignKey("FK_Users_RoleId").OnTable("Users");
            Delete.Table("Users");
        }

        if (Schema.Table("Roles").Exists())
        {
            Delete.Table("Roles");
        }
    }
}
