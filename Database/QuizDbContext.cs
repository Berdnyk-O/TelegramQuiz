using Microsoft.EntityFrameworkCore;
using TelegramQuiz.Database.Entities;

namespace TelegramQuiz.Database
{
    public class QuizDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<QuizTest> Tests { get; set; }

        public QuizDbContext(DbContextOptions<QuizDbContext> options): base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasMany(e => e.QuizTests)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId)
                .IsRequired();
        }
    }
}
