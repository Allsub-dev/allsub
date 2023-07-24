using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AllSub.WebMVC.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public virtual DbSet<UserProperty> UserProperties { get; set; } = default!;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationUser>(b =>
            {
                b.HasMany(e => e.UserProperties).WithOne().HasForeignKey(uc => uc.UserId).IsRequired();
            });

            modelBuilder.Entity<UserProperty>(b =>
            {
                b.HasKey(r => r.Id);
                b.ToTable("AllSubUserProperties");
            });
        }
    }
}