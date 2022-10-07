﻿using DatingApp.Svc.Entities;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.Svc.Data
{
  public class DatingDbContext : DbContext
  {
    public DatingDbContext(DbContextOptions<DatingDbContext> options) : base(options) { }

    public DbSet<AppUser> Users { get; set; }
    public DbSet<UserLike> Likes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      base.OnModelCreating(modelBuilder);

      modelBuilder.Entity<UserLike>().HasKey(k => new { k.SourceUserId, k.LikedUserId });

      modelBuilder.Entity<UserLike>()
        .HasOne(s => s.SourceUser)
        .WithMany(l => l.LikedUsers)
        .HasForeignKey(s => s.SourceUserId)
        .OnDelete(DeleteBehavior.NoAction);

      modelBuilder.Entity<UserLike>()
        .HasOne(s => s.LikedUser)
        .WithMany(l => l.LikedByUsers)
        .HasForeignKey(s => s.LikedUserId)
        .OnDelete(DeleteBehavior.NoAction);
    }
  }
}