using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Bumbo.Models;

public partial class BumboContext : IdentityDbContext
{
    public BumboContext()
    {
    }

    public BumboContext(DbContextOptions<BumboContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Activiteiten> Activiteitens { get; set; }

    public virtual DbSet<Afdelingen> Afdelingens { get; set; }

    public virtual DbSet<Beschikbaarheid> Beschikbaarheids { get; set; }

    public virtual DbSet<Diensten> Dienstens { get; set; }

    public virtual DbSet<Inklokken> Inklokkens { get; set; }

    public virtual DbSet<Filialen> Filialens { get; set; }

    public virtual DbSet<Openingstijden> Openingstijdens { get; set; }

    public virtual DbSet<Functie> Functies { get; set; }

    public virtual DbSet<Medewerker> Medewerkers { get; set; }

    public virtual DbSet<Normeringen> Normeringens { get; set; }

    public virtual DbSet<Prognose> Prognoses { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=localhost;Initial Catalog=Bumbo;Integrated Security=True;Pooling=False;Encrypt=False;Trust Server Certificate=False;", x => x.UseNetTopologySuite());

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Activiteiten>(entity =>
        {
            entity.HasKey(e => e.ActiviteitenId).HasName("PK__activite__B26CABB6B0B8EE4E");

            entity.ToTable("activiteiten");

            entity.HasIndex(e => e.AfdelingId, "IX_activiteiten_afdeling_id");

            entity.Property(e => e.ActiviteitenId).HasColumnName("activiteiten_id");
            entity.Property(e => e.AfdelingId).HasColumnName("afdeling_id");
            entity.Property(e => e.Beschrijving)
                .HasColumnType("text")
                .HasColumnName("beschrijving");
            entity.Property(e => e.Naam)
                .HasMaxLength(45)
                .IsUnicode(false)
                .HasColumnName("naam");

            entity.HasOne(d => d.Afdeling).WithMany(p => p.Activiteitens)
                .HasForeignKey(d => d.AfdelingId)
                .HasConstraintName("fk_activiteiten_afdelingen1");

            entity.HasMany(d => d.Normerings).WithMany(p => p.Activiteitens)
                .UsingEntity<Dictionary<string, object>>(
                    "NormeringenHasActiviteiten",
                    r => r.HasOne<Normeringen>().WithMany()
                        .HasForeignKey("NormeringId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("fk_activiteiten_has_normeringen_normeringen1"),
                    l => l.HasOne<Activiteiten>().WithMany()
                        .HasForeignKey("ActiviteitenId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("fk_activiteiten_has_normeringen_activiteiten1"),
                    j =>
                    {
                        j.HasKey("ActiviteitenId", "NormeringId").HasName("PK__normerin__F82EF16BA1AB84F0");
                        j.ToTable("normeringen_has_activiteiten");
                        j.HasIndex(new[] { "NormeringId" }, "IX_normeringen_has_activiteiten_normering_id");
                        j.IndexerProperty<int>("ActiviteitenId").HasColumnName("activiteiten_id");
                        j.IndexerProperty<int>("NormeringId").HasColumnName("normering_id");
                    });
        });

        modelBuilder.Entity<Afdelingen>(entity =>
        {
            entity.HasKey(e => e.AfdelingId).HasName("PK__afdeling__6C5F284B47055CDE");

            entity.ToTable("afdelingen");

            entity.Property(e => e.AfdelingId).HasColumnName("afdeling_id");
            entity.Property(e => e.AfdelingGroteInMeters).HasColumnName("afdeling_grote_in_meters");
            entity.Property(e => e.Naam)
                .HasMaxLength(45)
                .IsUnicode(false)
                .HasColumnName("naam");

            entity.HasMany(d => d.Filiaals).WithMany(p => p.Afdelings)
                .UsingEntity<Dictionary<string, object>>(
                    "FilialenHasAfdelingen",
                    r => r.HasOne<Filialen>().WithMany()
                        .HasForeignKey("FiliaalId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("fk_afdelingen_has_filialen_filialen1"),
                    l => l.HasOne<Afdelingen>().WithMany()
                        .HasForeignKey("AfdelingId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("fk_afdelingen_has_filialen_afdelingen1"),
                    j =>
                    {
                        j.HasKey("AfdelingId", "FiliaalId").HasName("PK__filialen__0E9C6502D008BCDF");
                        j.ToTable("filialen_has_afdelingen");
                        j.HasIndex(new[] { "FiliaalId" }, "IX_filialen_has_afdelingen_filiaal_id");
                        j.IndexerProperty<int>("AfdelingId").HasColumnName("afdeling_id");
                        j.IndexerProperty<int>("FiliaalId").HasColumnName("filiaal_id");
                    });

            entity.HasMany(d => d.Functies).WithMany(p => p.Afdelings)
                .UsingEntity<Dictionary<string, object>>(
                    "FunctieHasAfdelingen",
                    r => r.HasOne<Functie>().WithMany()
                        .HasForeignKey("FunctieId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("fk_afdelingen_has_functie_functie1"),
                    l => l.HasOne<Afdelingen>().WithMany()
                        .HasForeignKey("AfdelingId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("fk_afdelingen_has_functie_afdelingen1"),
                    j =>
                    {
                        j.HasKey("AfdelingId", "FunctieId").HasName("PK__functie___C06D37ED8E8BE39C");
                        j.ToTable("functie_has_afdelingen");
                        j.HasIndex(new[] { "FunctieId" }, "IX_functie_has_afdelingen_functie_id");
                        j.IndexerProperty<int>("AfdelingId").HasColumnName("afdeling_id");
                        j.IndexerProperty<int>("FunctieId").HasColumnName("functie_id");
                    });
        });



        modelBuilder.Entity<Beschikbaarheid>(entity =>
        {
            entity.HasKey(e => e.BeschikbaarheidId).HasName("PK__beschikb__4A080957480D93FA");

            entity.ToTable("beschikbaarheid");

            entity.HasIndex(e => e.MedewerkerId, "IX_beschikbaarheid_medewerker_id");

            entity.Property(e => e.BeschikbaarheidId).HasColumnName("beschikbaarheid_id");
            entity.Property(e => e.Datum)
                .HasColumnType("date")
                .HasColumnName("datum");
            entity.Property(e => e.EindTijd).HasColumnName("eind_tijd");
            entity.Property(e => e.MedewerkerId).HasColumnName("medewerker_id");
            entity.Property(e => e.SchoolUren).HasColumnName("school_uren");
            entity.Property(e => e.StartTijd).HasColumnName("start_tijd");

            entity.HasOne(d => d.Medewerker).WithMany(p => p.Beschikbaarheids)
                .HasForeignKey(d => d.MedewerkerId)
                .HasConstraintName("fk_medewerker_tijdslots_medewerkers1");
        });

        modelBuilder.Entity<Diensten>(entity =>
        {
            entity.HasKey(e => e.DienstenId).HasName("PK__diensten__A3C2395FDCA3385F");

            entity.ToTable("diensten");

            entity.HasIndex(e => e.MedewerkerId, "IX_diensten_medewerker_id");

            entity.HasIndex(e => e.BeschikbaarheidId, "UQ__diensten__4A080956DB00A71A").IsUnique();

            entity.Property(e => e.DienstenId).HasColumnName("diensten_id");
            entity.Property(e => e.BeschikbaarheidId).HasColumnName("beschikbaarheid_id");
            entity.Property(e => e.Datum)
                .HasColumnType("date")
                .HasColumnName("datum");
            entity.Property(e => e.EindTijd).HasColumnName("eind_tijd");
            entity.Property(e => e.MedewerkerId).HasColumnName("medewerker_id");
            entity.Property(e => e.StartTijd).HasColumnName("start_tijd");

            entity.HasOne(d => d.Beschikbaarheid).WithOne(p => p.Diensten)
                .HasForeignKey<Diensten>(d => d.BeschikbaarheidId)
                .HasConstraintName("FK_diensten_beschikbaarheid");

            entity.HasOne(d => d.Medewerker).WithMany(p => p.Dienstens)
                .HasForeignKey(d => d.MedewerkerId)
                .HasConstraintName("fk_diensten_medewerkers1");

        });

        modelBuilder.Entity<Inklokken>(entity =>
        {
            entity.HasKey(e => e.InklokkenId).HasName("PK_inklokken");

            entity.ToTable("inklokken");

            entity.HasIndex(e => e.DienstenId, "IX_inklokken_diensten_id");

            entity.Property(e => e.InklokkenId).HasColumnName("inklokken_id");
            entity.Property(e => e.DienstenId).HasColumnName("diensten_id");
            entity.Property(e => e.Start).HasColumnName("start");
            entity.Property(e => e.Eind).HasColumnName("eind");
            entity.Property(e => e.Goedkeuring).HasColumnName("goedgekeurd");

            entity.HasOne(d => d.Diensten).WithMany(p => p.Inklokken)
                .HasForeignKey(d => d.DienstenId)
                .HasConstraintName("fk_inklokken_diensten");
        });

        modelBuilder.Entity<Filialen>(entity =>
        {
            entity.HasKey(e => e.FiliaalId).HasName("PK__filialen__2C34D497B730BE46");

            entity.ToTable("filialen");

            entity.HasIndex(e => new { e.Postcode, e.Huisnummer }, "postcode_UNIQUE").IsUnique();

            entity.HasIndex(e => new { e.Postcode, e.Huisnummer }, "straatnaam_UNIQUE").IsUnique();

            entity.Property(e => e.FiliaalId).HasColumnName("filiaal_id");
            entity.Property(e => e.Email)
                .HasMaxLength(45)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.Huisnummer)
                .HasMaxLength(45)
                .IsUnicode(false)
                .HasColumnName("huisnummer");
            entity.Property(e => e.Locatie)
                .HasColumnType("geometry")
                .HasColumnName("locatie");
            entity.Property(e => e.Naam)
                .HasMaxLength(45)
                .IsUnicode(false)
                .HasColumnName("naam");
            entity.Property(e => e.Plaats)
                .HasMaxLength(45)
                .IsUnicode(false)
                .HasColumnName("plaats");
            entity.Property(e => e.Postcode)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("postcode");
            entity.Property(e => e.Straatnaam)
                .HasMaxLength(45)
                .IsUnicode(false)
                .HasColumnName("straatnaam");
            entity.Property(e => e.Telefoonnummer)
                .HasMaxLength(45)
                .IsUnicode(false)
                .HasColumnName("telefoonnummer");

            entity.HasMany(d => d.Openingstijdens).WithMany(p => p.Filialens)
                    .UsingEntity<Dictionary<string, object>>(
                        "FilialenHasOpeningstijden",
                        r => r.HasOne<Openingstijden>().WithMany()
                            .HasForeignKey("OpeningstijdenId")
                            .OnDelete(DeleteBehavior.ClientSetNull)
                            .HasConstraintName("fk_filialen_has_openingstijd_openingstijd1"),
                        l => l.HasOne<Filialen>().WithMany()
                            .HasForeignKey("FiliaalId")
                            .OnDelete(DeleteBehavior.ClientSetNull)
                            .HasConstraintName("fk_filialen_has_openingstijd_filialen1"),
                        j =>
                        {
                            j.HasKey("FiliaalId", "OpeningstijdenId").HasName("PK_openingstijden_1");
                            j.ToTable("filialen_has_openingstijden");
                            j.HasIndex(new[] { "OpeningstijdenId" }, "IX_filialen_has_openingstijden_openingstijden_id");
                            j.IndexerProperty<int>("FiliaalId").HasColumnName("filiaal_id");
                            j.IndexerProperty<int>("OpeningstijdenId").HasColumnName("openingstijden_id");
            });
        });

        modelBuilder.Entity<Openingstijden>(entity =>
        {
            entity.HasKey(e => e.OpeningstijdenId).HasName("PK_openingstijden");

            entity.ToTable("openingstijden");

            entity.Property(e => e.OpeningstijdenId).HasColumnName("openingstijden_id");
            entity.Property(e => e.DagVanWeek).HasColumnName("dag_van_week");
            entity.Property(e => e.OpeningsTijd).HasColumnName("openingstijd");
            entity.Property(e => e.SluitingsTijd).HasColumnName("sluitingstijd");
        });

        modelBuilder.Entity<Functie>(entity =>
        {
            entity.HasKey(e => e.FunctieId).HasName("PK__functie__C321FA6BA6D21E03");

            entity.ToTable("functie");

            entity.Property(e => e.FunctieId).HasColumnName("functie_id");
            entity.Property(e => e.Beschrijving)
                .HasColumnType("text")
                .HasColumnName("beschrijving");
            entity.Property(e => e.Naam)
                .HasMaxLength(45)
                .IsUnicode(false)
                .HasColumnName("naam");
            entity.Property(e => e.Schaal).HasColumnName("schaal");
        });

        modelBuilder.Entity<Medewerker>(entity =>
        {
            entity.HasKey(e => e.MedewerkerId).HasName("PK__medewerk__CB866CBE1B297B57");

            entity.ToTable("medewerkers");

            entity.HasIndex(e => e.Email, "IX_medewerkers_email");

            entity.HasIndex(e => e.FiliaalId, "IX_medewerkers_filiaal_id");

            entity.HasIndex(e => e.FunctieId, "IX_medewerkers_functie_id");

            entity.Property(e => e.MedewerkerId).HasColumnName("medewerker_id");
            entity.Property(e => e.Achternaam)
                .HasMaxLength(45)
                .IsUnicode(false)
                .HasColumnName("achternaam");
            entity.Property(e => e.Email)
                .HasMaxLength(45)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.FiliaalId).HasColumnName("filiaal_id");
            entity.Property(e => e.FunctieId).HasColumnName("functie_id");
            entity.Property(e => e.Geboortedatum)
                .HasColumnType("date")
                .HasColumnName("geboortedatum");
            entity.Property(e => e.Huisnummer).HasColumnName("huisnummer")
                .HasMaxLength(45)
                .IsUnicode(false)
                .HasColumnName("huisnummer");
            entity.Property(e => e.Indienst)
                .HasColumnType("date")
                .HasColumnName("indienst");
            entity.Property(e => e.Plaats)
                .HasMaxLength(45)
                .IsUnicode(false)
                .HasColumnName("plaats");
            entity.Property(e => e.Postcode)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("postcode");
            entity.Property(e => e.Straatnaam)
                .HasMaxLength(45)
                .IsUnicode(false)
                .HasColumnName("straatnaam");
            entity.Property(e => e.Telefoonnummer)
                .HasMaxLength(45)
                .IsUnicode(false)
                .HasColumnName("telefoonnummer");
            entity.Property(e => e.Tussenvoegsel)
                .HasMaxLength(45)
                .IsUnicode(false)
                .HasColumnName("tussenvoegsel");
            entity.Property(e => e.Verwijdert).HasColumnName("verwijdert");
            entity.Property(e => e.Voornaam)
                .HasMaxLength(45)
                .IsUnicode(false)
                .HasColumnName("voornaam");

            entity.HasOne(d => d.Filiaal).WithMany(p => p.Medewerkers)
                .HasForeignKey(d => d.FiliaalId)
                .HasConstraintName("fk_medewerkers_filialen1");

            entity.HasOne(d => d.Functie).WithMany(p => p.Medewerkers)
                .HasForeignKey(d => d.FunctieId)
                .HasConstraintName("fk_medewerker_functie1");
        });

        modelBuilder.Entity<Normeringen>(entity =>
        {
            entity.HasKey(e => e.NormeringId).HasName("PK__normerin__A425ADD599BA68A3");

            entity.ToTable("normeringen");

            entity.Property(e => e.NormeringId).HasColumnName("normering_id");
            entity.Property(e => e.Duur).HasColumnName("duur");
            entity.Property(e => e.Eenheid)
                .HasMaxLength(45)
                .IsUnicode(false)
                .HasColumnName("eenheid");
            entity.Property(e => e.UploadDatum)
                .HasColumnType("date")
                .HasColumnName("upload_datum");
        });

        modelBuilder.Entity<Prognose>(entity =>
        {
            entity.HasKey(e => e.PrognoseId).HasName("PK__Prognose__ID");

            entity.ToTable("prognose");

            entity.HasIndex(e => e.AfdelingId, "IX_Prognose_afdeling_id");

            entity.HasIndex(e => e.FiliaalId, "IX_Prognose_filiaal_id");

            entity.Property(e => e.PrognoseId).HasColumnName("prognose_id");
            entity.Property(e => e.AantalCollies).HasColumnName("aantal_collies");
            entity.Property(e => e.AfdelingId).HasColumnName("afdeling_id");
            entity.Property(e => e.Datum)
                .HasColumnType("date")
                .HasColumnName("datum");
            entity.Property(e => e.FiliaalId).HasColumnName("filiaal_id");
            entity.Property(e => e.PotentieleAantalBezoekers).HasColumnName("potentiele_aantal_bezoekers");
            entity.Property(e => e.Uren).HasColumnName("uren");
            entity.Property(e => e.Vakantiedag).HasColumnName("vakantiedag");

            entity.HasOne(d => d.Afdeling).WithMany(p => p.Prognoses)
                .HasForeignKey(d => d.AfdelingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Prognose__Afdelingen");

            entity.HasOne(d => d.Filiaal).WithMany(p => p.Prognoses)
                .HasForeignKey(d => d.FiliaalId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Prognose__Filialen");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
