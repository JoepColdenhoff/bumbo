using Bumbo.Models;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;
using System.Globalization;

#nullable disable

namespace Bumbo.Migrations
{
    /// <inheritdoc />
    public partial class seedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var geometryFactory = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(srid: 0);
            Point location1 = geometryFactory.CreatePoint(new Coordinate(51.685620534878296, 5.2892173087028675));
            Point location2 = geometryFactory.CreatePoint(new Coordinate(51.690263542691056, 5.299453375607226));
            Point location3 = geometryFactory.CreatePoint(new Coordinate(51.69517944456998, 5.268566862615444));
            Point location4 = geometryFactory.CreatePoint(new Coordinate(51.69097219028137, 5.331989663504935));
            Point location5 = geometryFactory.CreatePoint(new Coordinate(51.765956110872196, 5.502054299819827));
            Point location6 = geometryFactory.CreatePoint(new Coordinate(51.77245618501775, 5.538442781490607));
            Point location7 = geometryFactory.CreatePoint(new Coordinate(51.77113591076796, 5.56890586748796));
            Point location8 = geometryFactory.CreatePoint(new Coordinate(51.65822960743, 5.298510608541243));
            Point location9 = geometryFactory.CreatePoint(new Coordinate(51.645150031856794, 5.283914973268574));
            Point location10 = geometryFactory.CreatePoint(new Coordinate(51.72720284995148, 5.306232226266794));

            var filialen = new List<Filialen>
            {
            new Filialen { Naam = "Bumbo Bumbo", Postcode = "5223MC", Huisnummer = "10", Straatnaam = "Hoofdstraat", Plaats = "Bumboland", Email = "info@bumbofiliaal.com", Telefoonnummer = "0733036740", Locatie = location1 },
            new Filialen { Naam = "Bumbo Den Bosch", Postcode = "5224AR", Huisnummer = "54", Straatnaam = "Visstraat", Plaats = "Den Bosch", Email = "info@bumbofiliaal.com", Telefoonnummer = "0736272720", Locatie = location2 },
            new Filialen { Naam = "Bumbo Den Bosch", Postcode = "5215CZ", Huisnummer = "346", Straatnaam = "Helftheuvelpassage", Plaats = "Den Bosch", Email = "info@bumbofiliaal.com", Telefoonnummer = "0733035670", Locatie = location3 },
            new Filialen { Naam = "Bumbo Den Bosch", Postcode = "5246", Huisnummer = "2", Straatnaam = "Molenstraat", Plaats = "Den Bosch", Email = "info@bumbofiliaal.com", Telefoonnummer = "0736901720", Locatie = location4 },
            new Filialen { Naam = "Bumbo Oss", Postcode = "5345MH", Huisnummer = "49", Straatnaam = "Wolfskooi", Plaats = "Oss", Email = "info@bumbofiliaal.com", Telefoonnummer = "0412630741", Locatie = location5 },
            new Filialen { Naam = "Bumbo Oss", Postcode = "5332MH", Huisnummer = "7", Straatnaam = "Vierhoeksingel", Plaats = "Oss", Email = "info@bumbofiliaal.com", Telefoonnummer = "0412724988", Locatie = location6 },
            new Filialen { Naam = "Bumbo Berghem", Postcode = "5341AU", Huisnummer = "2", Straatnaam = "Gielenplein", Plaats = "Berghem", Email = "info@bumbofiliaal.com", Telefoonnummer = "0412745330", Locatie = location7 },
            new Filialen { Naam = "Bumbo Vught", Postcode = "5232HE", Huisnummer = "3", Straatnaam = "Raadhuisstraat", Plaats = "Vught", Email = "info@bumbofiliaal.com", Telefoonnummer = "0736580450", Locatie = location8 },
            new Filialen { Naam = "Bumbo Vught", Postcode = "5261EH", Huisnummer = "109", Straatnaam = "Moleneindplein", Plaats = "Vught", Email = "info@bumbofiliaal.com", Telefoonnummer = "0733034100", Locatie = location9 },
            new Filialen { Naam = "Bumbo Den Bosch", Postcode = "5243MD", Huisnummer = "2", Straatnaam = "Lokerenpassage", Plaats = "Den Bosch", Email = "info@bumbofiliaal.com", Telefoonnummer = "0736450870", Locatie = location10 }
            };

            foreach (var filiaal in filialen)
            {
                migrationBuilder.InsertData(
                    table: "filialen",
                    columns: new[] { "naam", "postcode", "huisnummer", "straatnaam", "plaats", "email", "telefoonnummer", "locatie" },
                    values: new object[] { filiaal.Naam, filiaal.Postcode, filiaal.Huisnummer, filiaal.Straatnaam, filiaal.Plaats, filiaal.Email, filiaal.Telefoonnummer, filiaal.Locatie }
                );
            }


            var functies = new List<Functie>
            {
                new Functie { Naam = "Afdelinghoofd", Schaal = 5, Beschrijving = "Verantwoordelijk voor het beheren van de dagelijkse operaties."},
                new Functie { Naam = "Vakkenvuller", Schaal = 3, Beschrijving = "Verantwoordelijk voor het beheren van de dagelijkse operaties."},
                new Functie { Naam = "Kassamedewerker", Schaal = 3, Beschrijving = "Verantwoordelijk voor het beheren van de dagelijkse operaties."},
                new Functie { Naam = "Slager", Schaal = 3, Beschrijving = "Verantwoordelijk voor het beheren van de dagelijkse operaties."}
            };

            foreach (var functie in functies)
            {
                migrationBuilder.InsertData(
                    table: "functie",
                    columns: new[] { "naam", "schaal", "beschrijving" },
                    values: new object[] { functie.Naam, functie.Schaal, functie.Beschrijving }
                );
            }


            var afdelingen = new List<Afdelingen>
            {
                new Afdelingen { Naam = "Vers", AfdelingGroteInMeters = 80 },
                new Afdelingen { Naam = "Vakkenvuller", AfdelingGroteInMeters = 215 },
                new Afdelingen { Naam = "Kassa", AfdelingGroteInMeters = 40 }
            };

            foreach (var afdeling in afdelingen)
            {
                migrationBuilder.InsertData(
                    table: "afdelingen",
                    columns: new[] { "naam", "afdeling_grote_in_meters" },
                    values: new object[] { afdeling.Naam, afdeling.AfdelingGroteInMeters }
                );
            }

            migrationBuilder.InsertData(
                table: "functie_has_afdelingen",
                columns: new[] { "afdeling_id", "functie_id" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 2, 1 },
                    { 3, 1 },
                    { 2, 2 },
                    { 3, 3 },
                    { 1, 4 },
                });

            migrationBuilder.InsertData(
                table: "filialen_has_afdelingen",
                columns: new[] { "afdeling_id", "filiaal_id" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 2, 1 },
                    { 3, 1 }
                });


            migrationBuilder.InsertData(
                table: "medewerkers",
                columns: new[] { "functie_id", "filiaal_id", "voornaam", "tussenvoegsel", "achternaam", "email", "telefoonnummer", "geboortedatum", "postcode", "huisnummer", "straatnaam", "plaats", "indienst", "verwijdert" },
                values: new object[,]
                {
                    { 1, 1, "Daan", null, "Janssen", "admin@bumbo.nl", "0612345678", "2002-05-10", "1234AB", "2", "Hoofdstraat", "Bumboland", "2022-04-07", 0 }
                });

            migrationBuilder.InsertData(
                table: "activiteiten",
                columns: new[] { "activiteiten_id", "afdeling_id", "naam", "beschrijving" },
                values: new object[,]
                {
                    { 1, 2, "Colli uitladen", ""},
                    { 2, 2, "Vakken vullen", "" },
                    { 3, 3, "Kassa", "" },
                    { 4, 1, "Vers", "" },
                    { 5, 2, "Spiegelen", ""},
                });

            migrationBuilder.InsertData(
                table: "normeringen",
                columns: new[] { "normering_id", "eenheid", "upload_datum", "duur" },
                values: new object[,]
                {
                    { 1, "minuten", DateTime.Now, 5 },
                    { 2, "minuten/coli", DateTime.Now, 30 },
                    { 3, "klanten/uur", DateTime.Now, 30 },
                    { 4, "klanten/uur", DateTime.Now, 100 },
                    { 5, "seconde/meter", DateTime.Now, 30 }
                });

            migrationBuilder.InsertData(
                table: "normeringen_has_activiteiten",
                columns: new[] { "activiteiten_id", "normering_id" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 2, 2 },
                    { 3, 3 },
                    { 4, 4 },
                    { 5, 5 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
