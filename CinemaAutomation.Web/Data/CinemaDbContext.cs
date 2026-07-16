// CinemaDbContext.cs
// Purpose: EF Core DbContext that maps entities to database tables

using CinemaAutomation.Web.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CinemaAutomation.Web.Data
{
    public class CinemaDbContext : DbContext // Central EF Core DbContext
    {
        // DbContext options are injected via Dependency Injection
        public CinemaDbContext(DbContextOptions<CinemaDbContext> options)
            : base(options)
        {
        }

        // DbSet properties represent database tables
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Company> Companies => Set<Company>();
        public DbSet<Hall> Halls => Set<Hall>();
        public DbSet<Seat> Seats => Set<Seat>();
        public DbSet<Movie> Movies => Set<Movie>();
        public DbSet<Showtime> Showtimes => Set<Showtime>();
        public DbSet<ShowtimeSeat> ShowtimeSeats => Set<ShowtimeSeat>();
        public DbSet<Booking> Bookings => Set<Booking>();
        public DbSet<BookingSeat> BookingSeats => Set<BookingSeat>();
        public DbSet<Snack> Snacks => Set<Snack>();
        public DbSet<SnackOrder> SnackOrders => Set<SnackOrder>();
        public DbSet<SnackOrderItem> SnackOrderItems => Set<SnackOrderItem>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure RowVersion for optimistic concurrency control
            modelBuilder.Entity<ShowtimeSeat>()
                .Property(x => x.RowVersion)
                .IsRowVersion();

            // Enforce unique seat position within a hall
            modelBuilder.Entity<Seat>()
                .HasIndex(x => new { x.HallId, x.RowNumber, x.SeatNumber })
                .IsUnique();

            // Enforce unique seat assignment per showtime
            modelBuilder.Entity<ShowtimeSeat>()
                .HasIndex(x => new { x.ShowtimeId, x.SeatId })
                .IsUnique();

            // === DATABASE TRIGGER MAPPINGS ===

            // Trigger to decrease snack stock after order item insertion
            modelBuilder.Entity<SnackOrderItem>().ToTable(tb =>
            {
                tb.HasTrigger("trg_DecrementSnackStockOnOrder");
            });

            // Trigger to prevent negative snack stock values
            modelBuilder.Entity<Snack>().ToTable(tb =>
            {
                tb.HasTrigger("trg_PreventNegativeSnackStock");
                tb.HasTrigger("trg_LogSnackUpdate");
            });

            // Trigger: Log user insert operations
            modelBuilder.Entity<User>().ToTable(tb =>
            {
                tb.HasTrigger("trg_LogUserInsert");
            });
        }
    }
}

