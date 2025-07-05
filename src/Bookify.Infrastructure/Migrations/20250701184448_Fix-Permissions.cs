using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bookify.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_permission_role_role_id",
                table: "permission");

            migrationBuilder.DropIndex(
                name: "ix_permission_role_id",
                table: "permission");

            migrationBuilder.DropColumn(
                name: "role_id",
                table: "permission");

            migrationBuilder.CreateIndex(
                name: "ix_role_permissions_permission_id",
                table: "role_permissions",
                column: "permission_id");

            migrationBuilder.AddForeignKey(
                name: "fk_role_permissions_permission_permission_id",
                table: "role_permissions",
                column: "permission_id",
                principalTable: "permission",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_role_permissions_roles_role_id",
                table: "role_permissions",
                column: "role_id",
                principalTable: "roles",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_role_permissions_permission_permission_id",
                table: "role_permissions");

            migrationBuilder.DropForeignKey(
                name: "fk_role_permissions_roles_role_id",
                table: "role_permissions");

            migrationBuilder.DropIndex(
                name: "ix_role_permissions_permission_id",
                table: "role_permissions");

            migrationBuilder.AddColumn<int>(
                name: "role_id",
                table: "permission",
                type: "integer",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "permission",
                keyColumn: "id",
                keyValue: 1,
                column: "role_id",
                value: null);

            migrationBuilder.CreateIndex(
                name: "ix_permission_role_id",
                table: "permission",
                column: "role_id");

            migrationBuilder.AddForeignKey(
                name: "fk_permission_role_role_id",
                table: "permission",
                column: "role_id",
                principalTable: "roles",
                principalColumn: "id");
        }
    }
}
