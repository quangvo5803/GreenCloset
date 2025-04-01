using DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ItemImage> ItemImages { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Cart> Carts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //Relition N-N
            modelBuilder
                .Entity<Product>()
                .HasMany(x => x.Categories)
                .WithMany(x => x.Products)
                .UsingEntity(y => y.ToTable("ProductCategory"));

            // Relation 1-1 between  Product and ProductAvatar
            modelBuilder
                .Entity<Product>()
                .HasOne(p => p.ProductAvatar)
                .WithOne()
                .HasForeignKey<Product>("ProductAvatarId")
                .OnDelete(DeleteBehavior.SetNull);

            // Relation 1-N between  Product and ProductAvatar
            modelBuilder
                .Entity<Product>()
                .HasMany(p => p.ProductImages)
                .WithOne(i => i.Product)
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
