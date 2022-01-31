using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Conflux.Database.Migrations
{
    public partial class InitialSchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Name = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    UserName = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(maxLength: 256, nullable: true),
                    Email = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(nullable: false),
                    PasswordHash = table.Column<string>(nullable: true),
                    SecurityStamp = table.Column<string>(nullable: true),
                    ConcurrencyStamp = table.Column<string>(nullable: true),
                    PhoneNumber = table.Column<string>(nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(nullable: false),
                    TwoFactorEnabled = table.Column<bool>(nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(nullable: true),
                    LockoutEnabled = table.Column<bool>(nullable: false),
                    AccessFailedCount = table.Column<int>(nullable: false),
                    EntityId = table.Column<long>(nullable: false),
                    Active = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "cfx_config",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    CreatedUserId = table.Column<long>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    ModifiedUserId = table.Column<long>(nullable: false),
                    Key = table.Column<string>(nullable: true),
                    Type = table.Column<string>(nullable: true),
                    ValueDate = table.Column<DateTime>(nullable: false),
                    ValueString = table.Column<string>(nullable: true),
                    ValueDecimal = table.Column<decimal>(nullable: false),
                    ValueInt = table.Column<long>(nullable: false),
                    ValueData = table.Column<byte[]>(nullable: true),
                    UserId = table.Column<long>(nullable: false),
                    SubsystemId = table.Column<long>(nullable: false),
                    Mode = table.Column<string>(maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cfx_config", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "cfx_entities",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    CreatedUserId = table.Column<long>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    ModifiedUserId = table.Column<long>(nullable: false),
                    Class = table.Column<string>(maxLength: 200, nullable: true),
                    Code = table.Column<string>(maxLength: 50, nullable: true),
                    Json = table.Column<string>(type: "json", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cfx_entities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "cfx_entityhistory",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    CreatedUserId = table.Column<long>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    ModifiedUserId = table.Column<long>(nullable: false),
                    RecordType = table.Column<int>(nullable: false),
                    EntityId = table.Column<long>(nullable: false),
                    Class = table.Column<string>(maxLength: 200, nullable: true),
                    Code = table.Column<string>(maxLength: 50, nullable: true),
                    Json = table.Column<string>(type: "json", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cfx_entityhistory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "cfx_links",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FromClass = table.Column<string>(maxLength: 200, nullable: true),
                    FromId = table.Column<long>(nullable: false),
                    FromCode = table.Column<string>(nullable: true),
                    ToClass = table.Column<string>(maxLength: 200, nullable: true),
                    ToId = table.Column<long>(nullable: false),
                    ToCode = table.Column<string>(nullable: true),
                    Relationship = table.Column<int>(nullable: false),
                    LinkType = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cfx_links", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "cfx_log",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Timestamp = table.Column<DateTime>(nullable: false),
                    CreatedUserId = table.Column<long>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    Class = table.Column<string>(maxLength: 200, nullable: true),
                    Level = table.Column<string>(maxLength: 50, nullable: true),
                    Message = table.Column<string>(maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cfx_log", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "cfx_storage",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    LinkClass = table.Column<string>(maxLength: 200, nullable: true),
                    LinkId = table.Column<long>(nullable: false),
                    LinkCode = table.Column<string>(maxLength: 50, nullable: true),
                    StorageCode = table.Column<string>(maxLength: 50, nullable: true),
                    StorageFilename = table.Column<string>(maxLength: 1000, nullable: true),
                    StorageURL = table.Column<string>(maxLength: 1000, nullable: true),
                    Size = table.Column<long>(nullable: false),
                    Data = table.Column<byte[]>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cfx_storage", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "cfx_storagehistory",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    LinkClass = table.Column<string>(maxLength: 200, nullable: true),
                    LinkId = table.Column<long>(nullable: false),
                    LinkCode = table.Column<string>(maxLength: 50, nullable: true),
                    StorageCode = table.Column<string>(maxLength: 50, nullable: true),
                    StorageFilename = table.Column<string>(maxLength: 1000, nullable: true),
                    StorageURL = table.Column<string>(maxLength: 1000, nullable: true),
                    Size = table.Column<long>(nullable: false),
                    Data = table.Column<byte[]>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cfx_storagehistory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "cfx_transact",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Timestamp = table.Column<DateTime>(nullable: false),
                    CreatedUserId = table.Column<long>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    Class = table.Column<string>(maxLength: 200, nullable: true),
                    Version = table.Column<string>(maxLength: 50, nullable: true),
                    Json = table.Column<string>(type: "json", nullable: true),
                    LinkId = table.Column<long>(nullable: false),
                    LinkClass = table.Column<string>(maxLength: 200, nullable: true),
                    Relationship = table.Column<int>(nullable: false),
                    LinkType = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cfx_transact", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RoleId = table.Column<string>(nullable: false),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<string>(nullable: false),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(nullable: false),
                    ProviderKey = table.Column<string>(nullable: false),
                    ProviderDisplayName = table.Column<string>(nullable: true),
                    UserId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    RoleId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    LoginProvider = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cfx_entities_Code",
                table: "cfx_entities",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_cfx_links_FromClass_FromCode",
                table: "cfx_links",
                columns: new[] { "FromClass", "FromCode" });

            migrationBuilder.CreateIndex(
                name: "IX_cfx_links_FromClass_FromId",
                table: "cfx_links",
                columns: new[] { "FromClass", "FromId" });

            migrationBuilder.CreateIndex(
                name: "IX_cfx_links_ToClass_ToCode_Relationship",
                table: "cfx_links",
                columns: new[] { "ToClass", "ToCode", "Relationship" });

            migrationBuilder.CreateIndex(
                name: "IX_cfx_links_ToClass_ToId_Relationship",
                table: "cfx_links",
                columns: new[] { "ToClass", "ToId", "Relationship" });

            migrationBuilder.CreateIndex(
                name: "IX_cfx_log_Timestamp",
                table: "cfx_log",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_cfx_storage_LinkClass_LinkCode",
                table: "cfx_storage",
                columns: new[] { "LinkClass", "LinkCode" });

            migrationBuilder.CreateIndex(
                name: "IX_cfx_storage_LinkClass_LinkId",
                table: "cfx_storage",
                columns: new[] { "LinkClass", "LinkId" });

            migrationBuilder.CreateIndex(
                name: "IX_cfx_transact_Date",
                table: "cfx_transact",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_cfx_transact_Timestamp",
                table: "cfx_transact",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_cfx_transact_LinkClass_Relationship_LinkId",
                table: "cfx_transact",
                columns: new[] { "LinkClass", "Relationship", "LinkId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "cfx_config");

            migrationBuilder.DropTable(
                name: "cfx_entities");

            migrationBuilder.DropTable(
                name: "cfx_entityhistory");

            migrationBuilder.DropTable(
                name: "cfx_links");

            migrationBuilder.DropTable(
                name: "cfx_log");

            migrationBuilder.DropTable(
                name: "cfx_storage");

            migrationBuilder.DropTable(
                name: "cfx_storagehistory");

            migrationBuilder.DropTable(
                name: "cfx_transact");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");
        }
    }
}
