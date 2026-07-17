using FluentMigrator;

namespace MushroomMapApp.Migrator.Migrations;

[Migration(202606181000)]
public class AddUserPermissionsInfrastructure : Migration
{
    public override void Up()
    {
        if (!Schema.Table("Permissions").Exists())
        {
            Create.Table("Permissions")
                .WithColumn("Id").AsInt64().PrimaryKey().Identity()
                .WithColumn("PublicId").AsGuid().WithDefault(SystemMethods.NewGuid).NotNullable()
                .WithColumn("Code").AsString(256).NotNullable()
                .WithColumn("Name").AsString(256).NotNullable()
                .WithColumn("IsActive").AsBoolean().NotNullable().WithDefaultValue(true);

            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Create.Index("IX_Permissions_PublicId")
                .OnTable("Permissions").OnColumn("PublicId").Ascending()
                .WithOptions().Unique();
        }

        if (!Schema.Table("RolePermissions").Exists())
        {
            Create.Table("RolePermissions")
                .WithColumn("RoleId").AsInt64().NotNullable()
                .WithColumn("PermissionId").AsInt64().NotNullable();

            Create.PrimaryKey("PK_RolePermissions")
                .OnTable("RolePermissions")
                .Columns("RoleId", "PermissionId");

            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Create.ForeignKey("FK_RolePermissions_RoleId")
                .FromTable("RolePermissions").ForeignColumn("RoleId")
                .ToTable("Roles").PrimaryColumn("Id")
                .OnDelete(System.Data.Rule.Cascade);

            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Create.ForeignKey("FK_RolePermissions_PermissionId")
                .FromTable("RolePermissions").ForeignColumn("PermissionId")
                .ToTable("Permissions").PrimaryColumn("Id")
                .OnDelete(System.Data.Rule.Cascade);
        }

        if (!Schema.Table("UserPermissions").Exists())
        {
            Create.Table("UserPermissions")
                .WithColumn("UserId").AsInt64().NotNullable()
                .WithColumn("PermissionId").AsInt64().NotNullable();

            Create.PrimaryKey("PK_UserPermissions")
                .OnTable("UserPermissions")
                .Columns("UserId", "PermissionId");

            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Create.ForeignKey("FK_UserPermissions_UserId")
                .FromTable("UserPermissions").ForeignColumn("UserId")
                .ToTable("Users").PrimaryColumn("Id")
                .OnDelete(System.Data.Rule.Cascade);

            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Create.ForeignKey("FK_UserPermissions_PermissionId")
                .FromTable("UserPermissions").ForeignColumn("PermissionId")
                .ToTable("Permissions").PrimaryColumn("Id")
                .OnDelete(System.Data.Rule.Cascade);
        }

        if (!Schema.Table("UserRoles").Exists())
        {
            Create.Table("UserRoles")
                .WithColumn("UserId").AsInt64().NotNullable()
                .WithColumn("RoleId").AsInt64().NotNullable();

            Create.PrimaryKey("PK_UserRoles")
                .OnTable("UserRoles")
                .Columns("UserId", "RoleId");

            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Create.ForeignKey("FK_UserRoles_UserId")
                .FromTable("UserRoles").ForeignColumn("UserId")
                .ToTable("Users").PrimaryColumn("Id")
                .OnDelete(System.Data.Rule.Cascade);

            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Create.ForeignKey("FK_UserRoles_RoleId")
                .FromTable("UserRoles").ForeignColumn("RoleId")
                .ToTable("Roles").PrimaryColumn("Id")
                .OnDelete(System.Data.Rule.Cascade);
        }
    }

    public override void Down()
    {
        if (Schema.Table("UserRoles").Exists())
        {
            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Delete.ForeignKey("FK_UserRoles_UserId").OnTable("UserRoles");

            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Delete.ForeignKey("FK_UserRoles_RoleId").OnTable("UserRoles");

            Delete.PrimaryKey("PK_UserRoles").FromTable("UserRoles");
            Delete.Table("UserRoles");
        }

        if (Schema.Table("UserPermissions").Exists())
        {
            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Delete.ForeignKey("FK_UserPermissions_UserId").OnTable("UserPermissions");

            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Delete.ForeignKey("FK_UserPermissions_PermissionId").OnTable("UserPermissions");

            Delete.PrimaryKey("PK_UserPermissions").FromTable("UserPermissions");
            Delete.Table("UserPermissions");
        }

        if (Schema.Table("RolePermissions").Exists())
        {
            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Delete.ForeignKey("FK_RolePermissions_RoleId").OnTable("RolePermissions");

            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Delete.ForeignKey("FK_RolePermissions_PermissionId").OnTable("RolePermissions");

            Delete.PrimaryKey("PK_RolePermissions").FromTable("RolePermissions");
            Delete.Table("RolePermissions");
        }

        if (Schema.Table("Permissions").Exists())
        {
            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Delete.Index("IX_Permissions_PublicId").OnTable("Permissions");

            Delete.Table("Permissions");
        }
    }
}
