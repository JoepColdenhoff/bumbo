using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace Bumbo.Migrations
{
    /// <inheritdoc />
    public partial class init_withfixstijn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "afdelingen",
                columns: table => new
                {
                    afdeling_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    naam = table.Column<string>(type: "varchar(45)", unicode: false, maxLength: 45, nullable: false),
                    afdeling_grote_in_meters = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__afdeling__6C5F284B47055CDE", x => x.afdeling_id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "filialen",
                columns: table => new
                {
                    filiaal_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    naam = table.Column<string>(type: "varchar(45)", unicode: false, maxLength: 45, nullable: false),
                    postcode = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: false),
                    huisnummer = table.Column<string>(type: "varchar(45)", unicode: false, maxLength: 45, nullable: false),
                    straatnaam = table.Column<string>(type: "varchar(45)", unicode: false, maxLength: 45, nullable: false),
                    plaats = table.Column<string>(type: "varchar(45)", unicode: false, maxLength: 45, nullable: false),
                    email = table.Column<string>(type: "varchar(45)", unicode: false, maxLength: 45, nullable: false),
                    telefoonnummer = table.Column<string>(type: "varchar(45)", unicode: false, maxLength: 45, nullable: false),
                    locatie = table.Column<Geometry>(type: "geometry", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__filialen__2C34D497B730BE46", x => x.filiaal_id);
                });

            migrationBuilder.CreateTable(
                name: "functie",
                columns: table => new
                {
                    functie_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    naam = table.Column<string>(type: "varchar(45)", unicode: false, maxLength: 45, nullable: false),
                    schaal = table.Column<int>(type: "int", nullable: false),
                    beschrijving = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__functie__C321FA6BA6D21E03", x => x.functie_id);
                });

            migrationBuilder.CreateTable(
                name: "normeringen",
                columns: table => new
                {
                    normering_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    eenheid = table.Column<string>(type: "varchar(45)", unicode: false, maxLength: 45, nullable: false),
                    upload_datum = table.Column<DateTime>(type: "date", nullable: false),
                    duur = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__normerin__A425ADD599BA68A3", x => x.normering_id);
                });

            migrationBuilder.CreateTable(
                name: "activiteiten",
                columns: table => new
                {
                    activiteiten_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    afdeling_id = table.Column<int>(type: "int", nullable: true),
                    naam = table.Column<string>(type: "varchar(45)", unicode: false, maxLength: 45, nullable: false),
                    beschrijving = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__activite__B26CABB6B0B8EE4E", x => x.activiteiten_id);
                    table.ForeignKey(
                        name: "fk_activiteiten_afdelingen1",
                        column: x => x.afdeling_id,
                        principalTable: "afdelingen",
                        principalColumn: "afdeling_id");
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
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
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
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
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
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
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
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
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
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

            migrationBuilder.CreateTable(
                name: "filialen_has_afdelingen",
                columns: table => new
                {
                    afdeling_id = table.Column<int>(type: "int", nullable: false),
                    filiaal_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__filialen__0E9C6502D008BCDF", x => new { x.afdeling_id, x.filiaal_id });
                    table.ForeignKey(
                        name: "fk_afdelingen_has_filialen_afdelingen1",
                        column: x => x.afdeling_id,
                        principalTable: "afdelingen",
                        principalColumn: "afdeling_id");
                    table.ForeignKey(
                        name: "fk_afdelingen_has_filialen_filialen1",
                        column: x => x.filiaal_id,
                        principalTable: "filialen",
                        principalColumn: "filiaal_id");
                });

            migrationBuilder.CreateTable(
                name: "prognose",
                columns: table => new
                {
                    prognose_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    afdeling_id = table.Column<int>(type: "int", nullable: false),
                    filiaal_id = table.Column<int>(type: "int", nullable: false),
                    datum = table.Column<DateTime>(type: "date", nullable: false),
                    potentiele_aantal_bezoekers = table.Column<int>(type: "int", nullable: false),
                    aantal_collies = table.Column<int>(type: "int", nullable: false),
                    uren = table.Column<int>(type: "int", nullable: false),
                    vakantiedag = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Prognose__ID", x => x.prognose_id);
                    table.ForeignKey(
                        name: "FK__Prognose__Afdelingen",
                        column: x => x.afdeling_id,
                        principalTable: "afdelingen",
                        principalColumn: "afdeling_id");
                    table.ForeignKey(
                        name: "FK__Prognose__Filialen",
                        column: x => x.filiaal_id,
                        principalTable: "filialen",
                        principalColumn: "filiaal_id");
                });

            migrationBuilder.CreateTable(
                name: "functie_has_afdelingen",
                columns: table => new
                {
                    afdeling_id = table.Column<int>(type: "int", nullable: false),
                    functie_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__functie___C06D37ED8E8BE39C", x => new { x.afdeling_id, x.functie_id });
                    table.ForeignKey(
                        name: "fk_afdelingen_has_functie_afdelingen1",
                        column: x => x.afdeling_id,
                        principalTable: "afdelingen",
                        principalColumn: "afdeling_id");
                    table.ForeignKey(
                        name: "fk_afdelingen_has_functie_functie1",
                        column: x => x.functie_id,
                        principalTable: "functie",
                        principalColumn: "functie_id");
                });

            migrationBuilder.CreateTable(
                name: "medewerkers",
                columns: table => new
                {
                    medewerker_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    functie_id = table.Column<int>(type: "int", nullable: true),
                    filiaal_id = table.Column<int>(type: "int", nullable: true),
                    voornaam = table.Column<string>(type: "varchar(45)", unicode: false, maxLength: 45, nullable: false),
                    tussenvoegsel = table.Column<string>(type: "varchar(45)", unicode: false, maxLength: 45, nullable: true),
                    achternaam = table.Column<string>(type: "varchar(45)", unicode: false, maxLength: 45, nullable: false),
                    email = table.Column<string>(type: "varchar(45)", unicode: false, maxLength: 45, nullable: false),
                    telefoonnummer = table.Column<string>(type: "varchar(45)", unicode: false, maxLength: 45, nullable: false),
                    geboortedatum = table.Column<DateTime>(type: "date", nullable: false),
                    postcode = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: false),
                    huisnummer = table.Column<int>(type: "int", nullable: false),
                    straatnaam = table.Column<string>(type: "varchar(45)", unicode: false, maxLength: 45, nullable: false),
                    plaats = table.Column<string>(type: "varchar(45)", unicode: false, maxLength: 45, nullable: false),
                    indienst = table.Column<DateTime>(type: "date", nullable: false),
                    verwijdert = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__medewerk__CB866CBE1B297B57", x => x.medewerker_id);
                    table.ForeignKey(
                        name: "fk_medewerker_functie1",
                        column: x => x.functie_id,
                        principalTable: "functie",
                        principalColumn: "functie_id");
                    table.ForeignKey(
                        name: "fk_medewerkers_filialen1",
                        column: x => x.filiaal_id,
                        principalTable: "filialen",
                        principalColumn: "filiaal_id");
                });

            migrationBuilder.CreateTable(
                name: "normeringen_has_activiteiten",
                columns: table => new
                {
                    activiteiten_id = table.Column<int>(type: "int", nullable: false),
                    normering_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__normerin__F82EF16BA1AB84F0", x => new { x.activiteiten_id, x.normering_id });
                    table.ForeignKey(
                        name: "fk_activiteiten_has_normeringen_activiteiten1",
                        column: x => x.activiteiten_id,
                        principalTable: "activiteiten",
                        principalColumn: "activiteiten_id");
                    table.ForeignKey(
                        name: "fk_activiteiten_has_normeringen_normeringen1",
                        column: x => x.normering_id,
                        principalTable: "normeringen",
                        principalColumn: "normering_id");
                });

            migrationBuilder.CreateTable(
                name: "beschikbaarheid",
                columns: table => new
                {
                    beschikbaarheid_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    medewerker_id = table.Column<int>(type: "int", nullable: true),
                    datum = table.Column<DateTime>(type: "date", nullable: false),
                    school_uren = table.Column<int>(type: "int", nullable: true),
                    eind_tijd = table.Column<TimeSpan>(type: "time", nullable: false),
                    start_tijd = table.Column<TimeSpan>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__beschikb__4A080957480D93FA", x => x.beschikbaarheid_id);
                    table.ForeignKey(
                        name: "fk_medewerker_tijdslots_medewerkers1",
                        column: x => x.medewerker_id,
                        principalTable: "medewerkers",
                        principalColumn: "medewerker_id");
                });

            migrationBuilder.CreateTable(
                name: "diensten",
                columns: table => new
                {
                    diensten_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    medewerker_id = table.Column<int>(type: "int", nullable: true),
                    start_tijd = table.Column<TimeSpan>(type: "time", nullable: false),
                    eind_tijd = table.Column<TimeSpan>(type: "time", nullable: false),
                    datum = table.Column<DateTime>(type: "date", nullable: false),
                    beschikbaarheid_id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__diensten__A3C2395FDCA3385F", x => x.diensten_id);
                    table.ForeignKey(
                        name: "FK_diensten_beschikbaarheid",
                        column: x => x.beschikbaarheid_id,
                        principalTable: "beschikbaarheid",
                        principalColumn: "beschikbaarheid_id");
                    table.ForeignKey(
                        name: "fk_diensten_medewerkers1",
                        column: x => x.medewerker_id,
                        principalTable: "medewerkers",
                        principalColumn: "medewerker_id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_activiteiten_afdeling_id",
                table: "activiteiten",
                column: "afdeling_id");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

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
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_beschikbaarheid_medewerker_id",
                table: "beschikbaarheid",
                column: "medewerker_id");

            migrationBuilder.CreateIndex(
                name: "IX_diensten_medewerker_id",
                table: "diensten",
                column: "medewerker_id");

            migrationBuilder.CreateIndex(
                name: "UQ__diensten__4A080956DB00A71A",
                table: "diensten",
                column: "beschikbaarheid_id",
                unique: true,
                filter: "[beschikbaarheid_id] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "postcode_UNIQUE",
                table: "filialen",
                columns: new[] { "postcode", "huisnummer" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "straatnaam_UNIQUE",
                table: "filialen",
                columns: new[] { "postcode", "huisnummer" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_filialen_has_afdelingen_filiaal_id",
                table: "filialen_has_afdelingen",
                column: "filiaal_id");

            migrationBuilder.CreateIndex(
                name: "IX_functie_has_afdelingen_functie_id",
                table: "functie_has_afdelingen",
                column: "functie_id");

            migrationBuilder.CreateIndex(
                name: "IX_medewerkers_email",
                table: "medewerkers",
                column: "email");

            migrationBuilder.CreateIndex(
                name: "IX_medewerkers_filiaal_id",
                table: "medewerkers",
                column: "filiaal_id");

            migrationBuilder.CreateIndex(
                name: "IX_medewerkers_functie_id",
                table: "medewerkers",
                column: "functie_id");

            migrationBuilder.CreateIndex(
                name: "IX_normeringen_has_activiteiten_normering_id",
                table: "normeringen_has_activiteiten",
                column: "normering_id");

            migrationBuilder.CreateIndex(
                name: "IX_Prognose_afdeling_id",
                table: "prognose",
                column: "afdeling_id");

            migrationBuilder.CreateIndex(
                name: "IX_Prognose_filiaal_id",
                table: "prognose",
                column: "filiaal_id");
        }

        /// <inheritdoc />
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
                name: "diensten");

            migrationBuilder.DropTable(
                name: "filialen_has_afdelingen");

            migrationBuilder.DropTable(
                name: "functie_has_afdelingen");

            migrationBuilder.DropTable(
                name: "normeringen_has_activiteiten");

            migrationBuilder.DropTable(
                name: "prognose");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "beschikbaarheid");

            migrationBuilder.DropTable(
                name: "activiteiten");

            migrationBuilder.DropTable(
                name: "normeringen");

            migrationBuilder.DropTable(
                name: "medewerkers");

            migrationBuilder.DropTable(
                name: "afdelingen");

            migrationBuilder.DropTable(
                name: "functie");

            migrationBuilder.DropTable(
                name: "filialen");
        }
    }
}
