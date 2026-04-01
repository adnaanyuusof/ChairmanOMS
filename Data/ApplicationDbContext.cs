using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ChairmanOMS.Models;

namespace ChairmanOMS.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<IncomingDocument> IncomingDocuments { get; set; }
        public DbSet<OutgoingDocument> OutgoingDocuments { get; set; }
        public DbSet<WorkflowAction> WorkflowActions { get; set; }
        public DbSet<TaskItem> TaskItems { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<AppointmentLog> AppointmentLogs { get; set; }
        public DbSet<MeetingNote> MeetingNotes { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Force naming to match exactly what's in the DB to avoid Case-Sensitivity issues
            builder.Entity<IncomingDocument>().ToTable("IncomingDocuments");
            builder.Entity<OutgoingDocument>().ToTable("OutgoingDocuments");
            builder.Entity<Appointment>().ToTable("Appointments");
            builder.Entity<TaskItem>().ToTable("TaskItems");

            // Explicit column mapping for OutgoingDocuments (Fixes PostgresException: 42703)
            builder.Entity<OutgoingDocument>().Property(d => d.ConveyerName).HasColumnName("ConveyerName");
            builder.Entity<OutgoingDocument>().Property(d => d.PhoneNumber).HasColumnName("PhoneNumber");
            builder.Entity<OutgoingDocument>().Property(d => d.ReceiverName).HasColumnName("ReceiverName");
            builder.Entity<OutgoingDocument>().Property(d => d.LinkedIncomingDocumentId).HasColumnName("LinkedIncomingDocumentId");
            builder.Entity<OutgoingDocument>().Property(d => d.CreatedAt).HasColumnName("CreatedAt");

            // Explicit column mapping for Appointments
            builder.Entity<Appointment>().Property(a => a.Masuulka).HasColumnName("Masuulka");
            builder.Entity<Appointment>().Property(a => a.VisitorStatus).HasColumnName("VisitorStatus");
            builder.Entity<Appointment>().Property(a => a.CheckInTime).HasColumnName("CheckInTime");
            builder.Entity<Appointment>().Property(a => a.CheckOutTime).HasColumnName("CheckOutTime");
            builder.Entity<Appointment>().Property(a => a.CreatedById).HasColumnName("CreatedById");

            builder.Entity<TaskItem>()
                .HasOne(t => t.CreatedBy)
                .WithMany()
                .HasForeignKey(t => t.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<TaskItem>()
                .HasOne(t => t.AssignedToUser)
                .WithMany()
                .HasForeignKey(t => t.AssignedToUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Appointment>()
                .HasOne(a => a.CreatedBy)
                .WithMany()
                .HasForeignKey(a => a.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<AppointmentLog>()
                .HasOne(l => l.ChangedBy)
                .WithMany()
                .HasForeignKey(l => l.ChangedById)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<MeetingNote>()
                .HasOne(n => n.CreatedBy)
                .WithMany()
                .HasForeignKey(n => n.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
