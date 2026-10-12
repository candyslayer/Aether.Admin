using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "sys_tenant",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    status = table.Column<bool>(type: "boolean", nullable: false),
                    expire_time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    permission_version = table.Column<long>(type: "bigint", nullable: false, defaultValue: 1L),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sys_tenant", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sys_audit_log",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    method = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    endpoint = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    parameters = table.Column<string>(type: "jsonb", nullable: false),
                    ip = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    trace_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    result = table.Column<int>(type: "integer", nullable: false),
                    duration_ms = table.Column<long>(type: "bigint", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValue: 1L)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sys_audit_log", x => x.id);
                    table.UniqueConstraint("AK_sys_audit_log_tenant_id_id", x => new { x.tenant_id, x.id });
                    table.ForeignKey(
                        name: "FK_sys_audit_log_sys_tenant_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "sys_tenant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "sys_menu",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    parent_id = table.Column<Guid>(type: "uuid", nullable: true),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    path = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    component = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    permission_code = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    sort = table.Column<int>(type: "integer", nullable: false),
                    visible = table.Column<bool>(type: "boolean", nullable: false),
                    status = table.Column<bool>(type: "boolean", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValue: 1L)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sys_menu", x => x.id);
                    table.UniqueConstraint("AK_sys_menu_tenant_id_id", x => new { x.tenant_id, x.id });
                    table.ForeignKey(
                        name: "FK_sys_menu_sys_menu_tenant_id_parent_id",
                        columns: x => new { x.tenant_id, x.parent_id },
                        principalTable: "sys_menu",
                        principalColumns: new[] { "tenant_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_sys_menu_sys_tenant_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "sys_tenant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "sys_organization",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    status = table.Column<bool>(type: "boolean", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValue: 1L)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sys_organization", x => x.id);
                    table.UniqueConstraint("AK_sys_organization_tenant_id_id", x => new { x.tenant_id, x.id });
                    table.ForeignKey(
                        name: "FK_sys_organization_sys_tenant_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "sys_tenant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "sys_post",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    sort = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<bool>(type: "boolean", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValue: 1L)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sys_post", x => x.id);
                    table.UniqueConstraint("AK_sys_post_tenant_id_id", x => new { x.tenant_id, x.id });
                    table.ForeignKey(
                        name: "FK_sys_post_sys_tenant_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "sys_tenant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "sys_role",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<bool>(type: "boolean", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValue: 1L)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sys_role", x => x.id);
                    table.UniqueConstraint("AK_sys_role_tenant_id_id", x => new { x.tenant_id, x.id });
                    table.ForeignKey(
                        name: "FK_sys_role_sys_tenant_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "sys_tenant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "sys_data_scope",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    scope_type = table.Column<int>(type: "integer", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValue: 1L)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sys_data_scope", x => x.id);
                    table.UniqueConstraint("AK_sys_data_scope_tenant_id_id", x => new { x.tenant_id, x.id });
                    table.ForeignKey(
                        name: "FK_sys_data_scope_sys_role_tenant_id_role_id",
                        columns: x => new { x.tenant_id, x.role_id },
                        principalTable: "sys_role",
                        principalColumns: new[] { "tenant_id", "id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_sys_data_scope_sys_tenant_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "sys_tenant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "sys_role_menus",
                columns: table => new
                {
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    menu_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sys_role_menus", x => new { x.tenant_id, x.role_id, x.menu_id });
                    table.ForeignKey(
                        name: "FK_sys_role_menus_sys_menu_tenant_id_menu_id",
                        columns: x => new { x.tenant_id, x.menu_id },
                        principalTable: "sys_menu",
                        principalColumns: new[] { "tenant_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_sys_role_menus_sys_role_tenant_id_role_id",
                        columns: x => new { x.tenant_id, x.role_id },
                        principalTable: "sys_role",
                        principalColumns: new[] { "tenant_id", "id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sys_dept",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    parent_id = table.Column<Guid>(type: "uuid", nullable: true),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    tree_path = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    leader_id = table.Column<Guid>(type: "uuid", nullable: true),
                    sort = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<bool>(type: "boolean", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValue: 1L)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sys_dept", x => x.id);
                    table.UniqueConstraint("AK_sys_dept_tenant_id_id", x => new { x.tenant_id, x.id });
                    table.UniqueConstraint("AK_sys_dept_tenant_id_organization_id_id", x => new { x.tenant_id, x.organization_id, x.id });
                    table.ForeignKey(
                        name: "FK_sys_dept_sys_dept_tenant_id_organization_id_parent_id",
                        columns: x => new { x.tenant_id, x.organization_id, x.parent_id },
                        principalTable: "sys_dept",
                        principalColumns: new[] { "tenant_id", "organization_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_sys_dept_sys_organization_tenant_id_organization_id",
                        columns: x => new { x.tenant_id, x.organization_id },
                        principalTable: "sys_organization",
                        principalColumns: new[] { "tenant_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_sys_dept_sys_tenant_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "sys_tenant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "sys_role_depts",
                columns: table => new
                {
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    dept_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sys_role_depts", x => new { x.tenant_id, x.role_id, x.dept_id });
                    table.ForeignKey(
                        name: "FK_sys_role_depts_sys_dept_tenant_id_dept_id",
                        columns: x => new { x.tenant_id, x.dept_id },
                        principalTable: "sys_dept",
                        principalColumns: new[] { "tenant_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_sys_role_depts_sys_role_tenant_id_role_id",
                        columns: x => new { x.tenant_id, x.role_id },
                        principalTable: "sys_role",
                        principalColumns: new[] { "tenant_id", "id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sys_user",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    dept_id = table.Column<Guid>(type: "uuid", nullable: false),
                    username = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    nickname = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: true),
                    mobile = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    status = table.Column<bool>(type: "boolean", nullable: false),
                    security_stamp = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    version = table.Column<long>(type: "bigint", nullable: false, defaultValue: 1L)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sys_user", x => x.id);
                    table.UniqueConstraint("AK_sys_user_tenant_id_id", x => new { x.tenant_id, x.id });
                    table.ForeignKey(
                        name: "FK_sys_user_sys_dept_tenant_id_dept_id",
                        columns: x => new { x.tenant_id, x.dept_id },
                        principalTable: "sys_dept",
                        principalColumns: new[] { "tenant_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_sys_user_sys_tenant_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "sys_tenant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "sys_user_depts",
                columns: table => new
                {
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    dept_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sys_user_depts", x => new { x.tenant_id, x.user_id, x.dept_id });
                    table.ForeignKey(
                        name: "FK_sys_user_depts_sys_dept_tenant_id_dept_id",
                        columns: x => new { x.tenant_id, x.dept_id },
                        principalTable: "sys_dept",
                        principalColumns: new[] { "tenant_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_sys_user_depts_sys_user_tenant_id_user_id",
                        columns: x => new { x.tenant_id, x.user_id },
                        principalTable: "sys_user",
                        principalColumns: new[] { "tenant_id", "id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sys_user_posts",
                columns: table => new
                {
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    post_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sys_user_posts", x => new { x.tenant_id, x.user_id, x.post_id });
                    table.ForeignKey(
                        name: "FK_sys_user_posts_sys_post_tenant_id_post_id",
                        columns: x => new { x.tenant_id, x.post_id },
                        principalTable: "sys_post",
                        principalColumns: new[] { "tenant_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_sys_user_posts_sys_user_tenant_id_user_id",
                        columns: x => new { x.tenant_id, x.user_id },
                        principalTable: "sys_user",
                        principalColumns: new[] { "tenant_id", "id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sys_user_roles",
                columns: table => new
                {
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sys_user_roles", x => new { x.tenant_id, x.user_id, x.role_id });
                    table.ForeignKey(
                        name: "FK_sys_user_roles_sys_role_tenant_id_role_id",
                        columns: x => new { x.tenant_id, x.role_id },
                        principalTable: "sys_role",
                        principalColumns: new[] { "tenant_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_sys_user_roles_sys_user_tenant_id_user_id",
                        columns: x => new { x.tenant_id, x.user_id },
                        principalTable: "sys_user",
                        principalColumns: new[] { "tenant_id", "id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_sys_audit_log_tenant_id_created_at",
                table: "sys_audit_log",
                columns: new[] { "tenant_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_sys_data_scope_tenant_id_role_id",
                table: "sys_data_scope",
                columns: new[] { "tenant_id", "role_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sys_dept_tenant_id_leader_id",
                table: "sys_dept",
                columns: new[] { "tenant_id", "leader_id" });

            migrationBuilder.CreateIndex(
                name: "IX_sys_dept_tenant_id_organization_id_parent_id",
                table: "sys_dept",
                columns: new[] { "tenant_id", "organization_id", "parent_id" });

            migrationBuilder.CreateIndex(
                name: "IX_sys_dept_tenant_id_parent_id",
                table: "sys_dept",
                columns: new[] { "tenant_id", "parent_id" });

            migrationBuilder.CreateIndex(
                name: "IX_sys_menu_tenant_id_parent_id",
                table: "sys_menu",
                columns: new[] { "tenant_id", "parent_id" });

            migrationBuilder.CreateIndex(
                name: "IX_sys_organization_tenant_id_code",
                table: "sys_organization",
                columns: new[] { "tenant_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sys_post_tenant_id_code",
                table: "sys_post",
                columns: new[] { "tenant_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sys_role_tenant_id_code",
                table: "sys_role",
                columns: new[] { "tenant_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sys_role_depts_tenant_id_dept_id",
                table: "sys_role_depts",
                columns: new[] { "tenant_id", "dept_id" });

            migrationBuilder.CreateIndex(
                name: "IX_sys_role_menus_tenant_id_menu_id",
                table: "sys_role_menus",
                columns: new[] { "tenant_id", "menu_id" });

            migrationBuilder.CreateIndex(
                name: "IX_sys_tenant_code",
                table: "sys_tenant",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sys_user_tenant_id_dept_id",
                table: "sys_user",
                columns: new[] { "tenant_id", "dept_id" });

            migrationBuilder.CreateIndex(
                name: "IX_sys_user_tenant_id_username",
                table: "sys_user",
                columns: new[] { "tenant_id", "username" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sys_user_depts_tenant_id_dept_id",
                table: "sys_user_depts",
                columns: new[] { "tenant_id", "dept_id" });

            migrationBuilder.CreateIndex(
                name: "IX_sys_user_posts_tenant_id_post_id",
                table: "sys_user_posts",
                columns: new[] { "tenant_id", "post_id" });

            migrationBuilder.CreateIndex(
                name: "IX_sys_user_roles_tenant_id_role_id",
                table: "sys_user_roles",
                columns: new[] { "tenant_id", "role_id" });

            migrationBuilder.AddForeignKey(
                name: "FK_sys_dept_sys_user_tenant_id_leader_id",
                table: "sys_dept",
                columns: new[] { "tenant_id", "leader_id" },
                principalTable: "sys_user",
                principalColumns: new[] { "tenant_id", "id" },
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_sys_dept_sys_tenant_tenant_id",
                table: "sys_dept");

            migrationBuilder.DropForeignKey(
                name: "FK_sys_organization_sys_tenant_tenant_id",
                table: "sys_organization");

            migrationBuilder.DropForeignKey(
                name: "FK_sys_user_sys_tenant_tenant_id",
                table: "sys_user");

            migrationBuilder.DropForeignKey(
                name: "FK_sys_dept_sys_organization_tenant_id_organization_id",
                table: "sys_dept");

            migrationBuilder.DropForeignKey(
                name: "FK_sys_dept_sys_user_tenant_id_leader_id",
                table: "sys_dept");

            migrationBuilder.DropTable(
                name: "sys_audit_log");

            migrationBuilder.DropTable(
                name: "sys_data_scope");

            migrationBuilder.DropTable(
                name: "sys_role_depts");

            migrationBuilder.DropTable(
                name: "sys_role_menus");

            migrationBuilder.DropTable(
                name: "sys_user_depts");

            migrationBuilder.DropTable(
                name: "sys_user_posts");

            migrationBuilder.DropTable(
                name: "sys_user_roles");

            migrationBuilder.DropTable(
                name: "sys_menu");

            migrationBuilder.DropTable(
                name: "sys_post");

            migrationBuilder.DropTable(
                name: "sys_role");

            migrationBuilder.DropTable(
                name: "sys_tenant");

            migrationBuilder.DropTable(
                name: "sys_organization");

            migrationBuilder.DropTable(
                name: "sys_user");

            migrationBuilder.DropTable(
                name: "sys_dept");
        }
    }
}
