﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Kibol_Alert.Models;

namespace Kibol_Alert.Database
{
    public class Kibol_AlertContext : IdentityDbContext<User>
    {
        public Kibol_AlertContext(DbContextOptions<Kibol_AlertContext> options)
            : base(options)
        {
        }

        public DbSet<ClubRelation> ClubRelations { get; set; }
        public DbSet<Club> Clubs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder
                .Entity<User>()
                .HasKey(i => i.Id);

            modelBuilder
                .Entity<User>()
                .HasOne(i => i.Club)
                .WithMany(i => i.Fans)
                .HasForeignKey(i => i.ClubId);

            modelBuilder
                .Entity<Club>()
                .HasKey(i => i.Id);

            modelBuilder
                .Entity<Club>()
                .HasMany(i => i.Fans)
                .WithOne(i => i.Club);

            modelBuilder
                .Entity<ClubRelation>()
                .HasKey(i => new { i.FirstClubId, i.SecondClubId, i.Relation });

            modelBuilder
                .Entity<ClubRelation>()
                .HasOne(i => i.FirstClub)
                .WithMany(i => i.RelationsWith)
                .HasForeignKey(i => i.FirstClubId)
                .OnDelete(DeleteBehavior.Restrict); 

            modelBuilder
                .Entity<ClubRelation>()
                .HasOne(i => i.SecondClub)
                .WithMany(i => i.InRelationsWith)
                .HasForeignKey(i => i.SecondClubId);

        }
    }
}