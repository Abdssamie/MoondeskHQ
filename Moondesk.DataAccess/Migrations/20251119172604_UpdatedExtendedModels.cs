using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moondesk.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedExtendedModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "organization_id",
                table: "sensors",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "organization_id",
                table: "commands",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "organization_id",
                table: "assets",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "organization_id",
                table: "alerts",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);

            migrationBuilder.CreateIndex(
                name: "ix_sensors_asset_id",
                table: "sensors",
                column: "asset_id");

            migrationBuilder.CreateIndex(
                name: "ix_readings_sensor_id",
                table: "readings",
                column: "sensor_id");

            migrationBuilder.CreateIndex(
                name: "ix_commands_sensor_id",
                table: "commands",
                column: "sensor_id");

            migrationBuilder.CreateIndex(
                name: "ix_alerts_sensor_id",
                table: "alerts",
                column: "sensor_id");

            migrationBuilder.AddForeignKey(
                name: "fk_alerts_sensors_sensor_id",
                table: "alerts",
                column: "sensor_id",
                principalTable: "sensors",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_commands_sensors_sensor_id",
                table: "commands",
                column: "sensor_id",
                principalTable: "sensors",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_readings_sensors_sensor_id",
                table: "readings",
                column: "sensor_id",
                principalTable: "sensors",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_sensors_assets_asset_id",
                table: "sensors",
                column: "asset_id",
                principalTable: "assets",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_alerts_sensors_sensor_id",
                table: "alerts");

            migrationBuilder.DropForeignKey(
                name: "fk_commands_sensors_sensor_id",
                table: "commands");

            migrationBuilder.DropForeignKey(
                name: "fk_readings_sensors_sensor_id",
                table: "readings");

            migrationBuilder.DropForeignKey(
                name: "fk_sensors_assets_asset_id",
                table: "sensors");

            migrationBuilder.DropIndex(
                name: "ix_sensors_asset_id",
                table: "sensors");

            migrationBuilder.DropIndex(
                name: "ix_readings_sensor_id",
                table: "readings");

            migrationBuilder.DropIndex(
                name: "ix_commands_sensor_id",
                table: "commands");

            migrationBuilder.DropIndex(
                name: "ix_alerts_sensor_id",
                table: "alerts");

            migrationBuilder.AlterColumn<string>(
                name: "organization_id",
                table: "sensors",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "organization_id",
                table: "commands",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "organization_id",
                table: "assets",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "organization_id",
                table: "alerts",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
