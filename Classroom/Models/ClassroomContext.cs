using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Classroom.Models;

public partial class ClassroomContext : DbContext
{
    public ClassroomContext()
    {
    }

    public ClassroomContext(DbContextOptions<ClassroomContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Announcement> Announcements { get; set; }

    public virtual DbSet<AnnouncementAttachment> AnnouncementAttachments { get; set; }

    public virtual DbSet<AnnouncementComment> AnnouncementComments { get; set; }

    public virtual DbSet<Assignment> Assignments { get; set; }

    public virtual DbSet<AssignmentAttachment> AssignmentAttachments { get; set; }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<CourseMember> CourseMembers { get; set; }

    public virtual DbSet<Material> Materials { get; set; }

    public virtual DbSet<MaterialAttachment> MaterialAttachments { get; set; }

    public virtual DbSet<Submission> Submissions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=.;Database=Classroom;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Announcement>(entity =>
        {
            entity.HasKey(e => e.AnnouncementId).HasName("PK__Announce__9DE445749C23CC01");

            entity.Property(e => e.AnnouncementGuid).HasDefaultValueSql("(newid())");
            entity.Property(e => e.AuthorAvatar).HasMaxLength(255);
            entity.Property(e => e.AuthorName).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Author).WithMany(p => p.Announcements)
                .HasForeignKey(d => d.AuthorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Announcem__Autho__59063A47");

            entity.HasOne(d => d.Class).WithMany(p => p.Announcements)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Announcem__Class__5812160E");
        });

        modelBuilder.Entity<AnnouncementAttachment>(entity =>
        {
            entity.HasKey(e => e.AttachmentId).HasName("PK__Announce__442C64BE8159921D");

            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Type).HasMaxLength(20);
            entity.Property(e => e.UploadDate).HasColumnType("datetime");
            entity.Property(e => e.Url).HasMaxLength(255);

            entity.HasOne(d => d.Announcement).WithMany(p => p.AnnouncementAttachments)
                .HasForeignKey(d => d.AnnouncementId)
                .HasConstraintName("FK__Announcem__Annou__5BE2A6F2");
        });

        modelBuilder.Entity<AnnouncementComment>(entity =>
        {
            entity.HasKey(e => e.CommentId).HasName("PK__Announce__C3B4DFCAEE06BEC5");

            entity.Property(e => e.AuthorAvatar).HasMaxLength(255);
            entity.Property(e => e.AuthorName).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Announcement).WithMany(p => p.AnnouncementComments)
                .HasForeignKey(d => d.AnnouncementId)
                .HasConstraintName("FK__Announcem__Annou__5EBF139D");

            entity.HasOne(d => d.Author).WithMany(p => p.AnnouncementComments)
                .HasForeignKey(d => d.AuthorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Announcem__Autho__5FB337D6");
        });

        modelBuilder.Entity<Assignment>(entity =>
        {
            entity.HasKey(e => e.AssignmentId).HasName("PK__Assignme__32499E772487C50D");

            entity.Property(e => e.AssignmentGuid).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ClassName).HasMaxLength(100);
            entity.Property(e => e.Color).HasMaxLength(20);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.DueDate).HasMaxLength(50);
            entity.Property(e => e.DueTime).HasMaxLength(20);
            entity.Property(e => e.LateSubmissionPolicy).HasMaxLength(255);
            entity.Property(e => e.Points).HasMaxLength(20);
            entity.Property(e => e.ScheduledFor).HasMaxLength(50);
            entity.Property(e => e.Section).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.Title).HasMaxLength(100);
            entity.Property(e => e.Topic).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Class).WithMany(p => p.Assignments)
                .HasForeignKey(d => d.ClassId)
                .HasConstraintName("FK__Assignmen__Class__4D94879B");

            entity.HasMany(d => d.Users).WithMany(p => p.Assignments)
                .UsingEntity<Dictionary<string, object>>(
                    "AssignmentAssignTo",
                    r => r.HasOne<User>().WithMany()
                        .HasForeignKey("UserId")
                        .HasConstraintName("FK__Assignmen__UserI__5165187F"),
                    l => l.HasOne<Assignment>().WithMany()
                        .HasForeignKey("AssignmentId")
                        .HasConstraintName("FK__Assignmen__Assig__5070F446"),
                    j =>
                    {
                        j.HasKey("AssignmentId", "UserId").HasName("PK__Assignme__E33112B3686D70FC");
                        j.ToTable("AssignmentAssignTo");
                    });
        });

        modelBuilder.Entity<AssignmentAttachment>(entity =>
        {
            entity.HasKey(e => e.AttachmentId).HasName("PK__Assignme__442C64BE8E851826");

            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Thumbnail).HasMaxLength(255);
            entity.Property(e => e.Type).HasMaxLength(20);
            entity.Property(e => e.Url).HasMaxLength(255);

            entity.HasOne(d => d.Assignment).WithMany(p => p.AssignmentAttachments)
                .HasForeignKey(d => d.AssignmentId)
                .HasConstraintName("FK__Assignmen__Assig__5441852A");
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.CourseId).HasName("PK__Courses__C92D71A72F04DAFA");

            entity.Property(e => e.CourseGuid).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CoverImage).HasMaxLength(255);
            entity.Property(e => e.EnrollmentCode).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Room).HasMaxLength(50);
            entity.Property(e => e.Section).HasMaxLength(50);
            entity.Property(e => e.Subject).HasMaxLength(100);
            entity.Property(e => e.TeacherName).HasMaxLength(100);
            entity.Property(e => e.ThemeColor).HasMaxLength(20);
        });

        modelBuilder.Entity<CourseMember>(entity =>
        {
            entity.HasKey(e => new { e.CourseId, e.UserId }).HasName("PK__CourseMe__1855FD6301F70C6B");

            entity.Property(e => e.Role).HasMaxLength(20);

            entity.HasOne(d => d.Course).WithMany(p => p.CourseMembers)
                .HasForeignKey(d => d.CourseId)
                .HasConstraintName("FK__CourseMem__Cours__3E52440B");

            entity.HasOne(d => d.User).WithMany(p => p.CourseMembers)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__CourseMem__UserI__3F466844");
        });

        modelBuilder.Entity<Material>(entity =>
        {
            entity.HasKey(e => e.MaterialId).HasName("PK__Material__C50610F7F56E1318");

            entity.Property(e => e.ClassName).HasMaxLength(100);
            entity.Property(e => e.Color).HasMaxLength(20);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.MaterialGuid).HasDefaultValueSql("(newid())");
            entity.Property(e => e.ScheduledFor).HasMaxLength(50);
            entity.Property(e => e.Section).HasMaxLength(50);
            entity.Property(e => e.Title).HasMaxLength(100);
            entity.Property(e => e.Topic).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Class).WithMany(p => p.Materials)
                .HasForeignKey(d => d.ClassId)
                .HasConstraintName("FK__Materials__Class__4316F928");

            entity.HasMany(d => d.Users).WithMany(p => p.Materials)
                .UsingEntity<Dictionary<string, object>>(
                    "MaterialAssignTo",
                    r => r.HasOne<User>().WithMany()
                        .HasForeignKey("UserId")
                        .HasConstraintName("FK__MaterialA__UserI__46E78A0C"),
                    l => l.HasOne<Material>().WithMany()
                        .HasForeignKey("MaterialId")
                        .HasConstraintName("FK__MaterialA__Mater__45F365D3"),
                    j =>
                    {
                        j.HasKey("MaterialId", "UserId").HasName("PK__Material__147E9C33830DE83A");
                        j.ToTable("MaterialAssignTo");
                    });
        });

        modelBuilder.Entity<MaterialAttachment>(entity =>
        {
            entity.HasKey(e => e.AttachmentId).HasName("PK__Material__442C64BEB040A625");

            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Thumbnail).HasMaxLength(255);
            entity.Property(e => e.Type).HasMaxLength(20);
            entity.Property(e => e.Url).HasMaxLength(255);

            entity.HasOne(d => d.Material).WithMany(p => p.MaterialAttachments)
                .HasForeignKey(d => d.MaterialId)
                .HasConstraintName("FK__MaterialA__Mater__49C3F6B7");
        });

        modelBuilder.Entity<Submission>(entity =>
        {
            entity.HasKey(e => e.SubmissionId).HasName("PK__Submissi__449EE125436F20EF");

            entity.Property(e => e.Grade).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.GradedDate).HasColumnType("datetime");
            entity.Property(e => e.SubmittedAt).HasColumnType("datetime");
            entity.Property(e => e.Content).HasColumnType("nvarchar(max)");

            entity.HasOne(d => d.Assignment).WithMany(p => p.Submissions)
                .HasForeignKey(d => d.AssignmentId)
                .HasConstraintName("FK__Submissio__Assig__628FA481");

            entity.HasOne(d => d.User).WithMany(p => p.Submissions)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Submissio__UserI__6383C8BA");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4C4460BC09");

            entity.HasIndex(e => e.Email, "UQ__Users__A9D10534EB51A6BC").IsUnique();

            entity.Property(e => e.Avatar).HasMaxLength(255);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Password).HasMaxLength(255);
            entity.Property(e => e.Role).HasMaxLength(20);
            entity.Property(e => e.UserGuid).HasDefaultValueSql("(newid())");
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Token).IsRequired();
            entity.Property(e => e.JwtId).IsRequired();
            entity.Property(e => e.AddedDate).IsRequired();
            entity.Property(e => e.ExpiryDate).IsRequired();

            entity.HasOne(d => d.User)
                .WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
