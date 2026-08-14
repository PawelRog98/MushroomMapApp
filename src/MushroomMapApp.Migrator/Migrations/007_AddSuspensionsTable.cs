using FluentMigrator;

namespace MushroomMapApp.Migrator.Migrations;

[Migration(202606181100)]
public class AddSuspensionsTable : Migration
{
    public override void Up()
    {
        if (!Schema.Table("Suspensions").Exists())
        {
            Create.Table("Suspensions")
                .WithColumn("Id").AsInt64().Identity().PrimaryKey()
                .WithColumn("PublicId").AsGuid().WithDefault(SystemMethods.NewGuid).NotNullable()
                .WithColumn("StartDate").AsDateTime2().NotNullable()
                .WithColumn("EndDate").AsDateTime2().Nullable()
                .WithColumn("Reason").AsString(1024).NotNullable()
                .WithColumn("Status").AsInt32().NotNullable()
                .WithColumn("UserId").AsInt64().NotNullable()
                .WithColumn("SuspendedById").AsInt64().NotNullable();

            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Create.Index("IX_Suspensions_PublicId")
                .OnTable("Suspensions").OnColumn("PublicId").Ascending()
                .WithOptions().Unique();

            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Create.ForeignKey("FK_Suspensions_UserId")
                .FromTable("Suspensions").ForeignColumn("UserId")
                .ToTable("Users").PrimaryColumn("Id")
                .OnDelete(System.Data.Rule.None);

            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Create.ForeignKey("FK_Suspensions_SuspendedById")
                .FromTable("Suspensions").ForeignColumn("SuspendedById")
                .ToTable("Users").PrimaryColumn("Id")
                .OnDelete(System.Data.Rule.None);
        }
    }

    public override void Down()
    {
        if (Schema.Table("Suspensions").Exists())
        {
            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Delete.ForeignKey("FK_Suspensions_UserId").OnTable("Suspensions");

            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Delete.ForeignKey("FK_Suspensions_SuspendedById").OnTable("Suspensions");

            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Delete.Index("IX_Suspensions_PublicId").OnTable("Suspensions");

            Delete.Table("Suspensions");
        }
    }
}
