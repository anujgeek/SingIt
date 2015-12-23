namespace SingItService
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class UserSongModel : DbContext
    {
        public UserSongModel(string cs)
            : base(cs)
        {
            Configuration.ProxyCreationEnabled = false;
        }

        public virtual DbSet<song> songs { get; set; }
        public virtual DbSet<user> users { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<song>()
                .Property(e => e.songfilename)
                .IsUnicode(false);

            modelBuilder.Entity<song>()
                .Property(e => e.username)
                .IsUnicode(false);

            modelBuilder.Entity<song>()
                .Property(e => e.songtitle)
                .IsUnicode(false);

            modelBuilder.Entity<song>()
                .Property(e => e.songgenre)
                .IsUnicode(false);

            modelBuilder.Entity<song>()
                .Property(e => e.ratings)
                .IsUnicode(false);

            modelBuilder.Entity<user>()
                .Property(e => e.username)
                .IsUnicode(false);

            modelBuilder.Entity<user>()
                .Property(e => e.password)
                .IsUnicode(false);

            modelBuilder.Entity<user>()
                .Property(e => e.email)
                .IsUnicode(false);

            modelBuilder.Entity<user>()
                .HasMany(e => e.songs)
                .WithRequired(e => e.user)
                .WillCascadeOnDelete(false);
        }
    }
}
