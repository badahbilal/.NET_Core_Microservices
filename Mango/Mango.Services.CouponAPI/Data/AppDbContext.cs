using Microsoft.EntityFrameworkCore;

namespace Mango.Services.CouponAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<Models.Coupon> Coupons { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Models.Coupon>().HasData(
                new Models.Coupon
                {
                    CouponId = 1,
                    CouponCode = "10OFF",
                    DiscountAmount = 10.0,
                    MinAmount = 50
                },
                new Models.Coupon
                {
                    CouponId = 2,
                    CouponCode = "20OFF",
                    DiscountAmount = 20.0,
                    MinAmount = 100
                },
                new Models.Coupon
                {
                    CouponId = 3,
                    CouponCode = "30OFF",
                    DiscountAmount = 30.0,
                    MinAmount = 150
                }
            );
        }
    }
}
