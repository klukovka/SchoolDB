namespace SchoolDB.Models
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class SchoolContext : DbContext
    {
        public SchoolContext()
            : base("name=SchoolContext")
        {
        }

        public virtual DbSet<Bells> Bells { get; set; }
        public virtual DbSet<Categories> Categories { get; set; }
        public virtual DbSet<Forms> Forms { get; set; }
        public virtual DbSet<Marks> Marks { get; set; }
        public virtual DbSet<Privileges> Privileges { get; set; }
        public virtual DbSet<Students> Students { get; set; }
        public virtual DbSet<Teacher_Subject> Teacher_Subject { get; set; }
        public virtual DbSet<Teachers> Teachers { get; set; }
        public virtual DbSet<Timetable> Timetable { get; set; }
        public virtual DbSet<Types_of_work> Types_of_work { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Bells>()
                .HasMany(e => e.Timetable)
                .WithRequired(e => e.Bells)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Forms>()
                .HasMany(e => e.Students)
                .WithOptional(e => e.Forms)
                .HasForeignKey(e => e.Student_form);

            modelBuilder.Entity<Forms>()
                .HasMany(e => e.Timetable)
                .WithRequired(e => e.Forms)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Students>()
                .Property(e => e.Student_password)
                .IsFixedLength();

            modelBuilder.Entity<Students>()
                .HasMany(e => e.Marks)
                .WithRequired(e => e.Students)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Teacher_Subject>()
                .HasMany(e => e.Marks)
                .WithRequired(e => e.Teacher_Subject)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Teacher_Subject>()
                .HasMany(e => e.Timetable)
                .WithRequired(e => e.Teacher_Subject)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Teachers>()
                .Property(e => e.Teacher_post)
                .IsFixedLength();

            modelBuilder.Entity<Teachers>()
                .Property(e => e.Teacher_password)
                .IsFixedLength();

            modelBuilder.Entity<Teachers>()
                .HasMany(e => e.Forms)
                .WithRequired(e => e.Teachers)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Teachers>()
                .HasMany(e => e.Teacher_Subject)
                .WithRequired(e => e.Teachers)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Types_of_work>()
                .HasMany(e => e.Marks)
                .WithRequired(e => e.Types_of_work)
                .WillCascadeOnDelete(false);
        }
    }
}
