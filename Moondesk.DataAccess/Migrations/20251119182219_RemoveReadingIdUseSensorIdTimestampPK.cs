using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Moondesk.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RemoveReadingIdUseSensorIdTimestampPK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_readings",
                table: "readings");

            migrationBuilder.DropIndex(
                name: "ix_readings_sensor_id",
                table: "readings");

            migrationBuilder.DropColumn(
                name: "id",
                table: "readings");

            migrationBuilder.AddPrimaryKey(
                name: "pk_readings",
                table: "readings",
                columns: new[] { "sensor_id", "timestamp" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_readings",
                table: "readings");

            migrationBuilder.AddColumn<long>(
                name: "id",
                table: "readings",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddPrimaryKey(
                name: "pk_readings",
                table: "readings",
                columns: new[] { "id", "timestamp" });

            migrationBuilder.CreateIndex(
                name: "ix_readings_sensor_id",
                table: "readings",
                column: "sensor_id");
        }
    }
}
