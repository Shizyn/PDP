﻿using Microsoft.EntityFrameworkCore;
using DP.Models;

namespace DP.Database
{
    public class DPContext : DbContext
    {
        public DPContext(DbContextOptions<DPContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<ProfProba> ProfProby { get; set; }
        public DbSet<ExcursionBooking> ExcursionBookings { get; set; }
        public DbSet<AvailableSlot> AvailableSlots { get; set; }
        public DbSet<ExcursionSlot> ExcursionSlots { get; set; }
        public DbSet<ExcursionUploadedFile> ExcursionUploadedFiles { get; set; }
        public DbSet<UploadedFile> UploadedFiles { get; set; }
        public DbSet<Museum> Museums { get; set; }
        public DbSet<Schedule> Schedules { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProfProba>().ToTable("ProfProby");
            modelBuilder.Entity<Event>()
                .HasOne(e => e.ProfProba)
                .WithMany(p => p.Events)
                .HasForeignKey(e => e.ProfProbaId);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.ProfProba)
                .WithMany()
                .HasForeignKey(b => b.ProfProbaId);

            modelBuilder.Entity<UploadedFile>()
                .HasOne(f => f.Booking)
                .WithMany(b => b.Files)
                .HasForeignKey(f => f.BookingId);
            modelBuilder.Entity<ExcursionUploadedFile>()
                 .HasOne(f => f.ExcursionBooking)
                 .WithMany(b => b.Files)
                 .HasForeignKey(f => f.ExcursionBookingId)
                 .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Schedule>()
                .HasOne(s => s.ProfProba)
                .WithMany()
                .HasForeignKey(s => s.ProfProbaId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Schedule>()
                .HasOne(s => s.Excursion)
                .WithMany()
                .HasForeignKey(s => s.ExcursionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
