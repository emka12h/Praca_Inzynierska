using GlamStudio.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GlamStudio.Data
{
    public class BeautySalonContext : IdentityDbContext<ApplicationUser>
    {
        public BeautySalonContext(DbContextOptions<BeautySalonContext> options) : base(options)
        {
        }

        public DbSet<Service> Services { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<WorkSchedule> WorkSchedules { get; set; }
        public DbSet<EmployeeSpecialization> EmployeeSpecializations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Wyłącza kaskadowe usuwanie dla wizyt
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Employee)
                .WithMany(e => e.EmployeeAppointments)
                .HasForeignKey(a => a.EmployeeID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Client)
                .WithMany(c => c.ClientAppointments)
                .HasForeignKey(a => a.ClientID)
                .OnDelete(DeleteBehavior.Restrict);

            // Wyłącza kaskadowe usuwanie dla wiadomości
            modelBuilder.Entity<Message>()
                .HasOne(m => m.ApplicationUser)
                .WithMany()
                .HasForeignKey(m => m.ApplicationUserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.ReplySentBy)
                .WithMany()
                .HasForeignKey(m => m.ReplySentById)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<WorkSchedule>()
                .HasOne(ws => ws.Employee)
                .WithMany(u => u.WorkSchedules)
                .HasForeignKey(ws => ws.ApplicationUserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Appointment>()
                .HasIndex(a => new { a.EmployeeID, a.AppointmentDate, a.AppointmentTime })
                .IsUnique();
        }
    }
}