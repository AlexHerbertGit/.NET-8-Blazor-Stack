using KobraKai.API.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace KobraKai.API.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Meal> Meals => Set<Meal>();
        public DbSet<Order> Orders => Set<Order>();

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Meal>(e =>
            {
                e.Property(m => m.Title).IsRequired();
                e.Property(m => m.DietaryTagsCsv).HasDefaultValue("");
                e.HasOne(m => m.Provider)
                 .WithMany()
                 .HasForeignKey(m => m.ProviderId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            b.Entity<Order>(e =>
            {
                e.HasOne(o => o.Meal)
                 .WithMany()
                 .HasForeignKey(o => o.MealId)
                 .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}