
// DATA FILE: Purpose: Encompasses the data access layer, including repositories for interacting with the database and the Entity Framework DbContext.


using Microsoft.EntityFrameworkCore;
// using InventoryService.Models;
// using Microsoft.EntityFrameworkCore;

public partial class InventoryContext : DbContext
{
    public InventoryContext()
    {
    }

    public InventoryContext(DbContextOptions<InventoryContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Product> Products { get; set; }
    public virtual DbSet<UserInfo> UserInfo { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=vivian;User Id=sa;Password=MyPass@word; Trusted_Connection=True;");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId);

            entity.Property(e => e.Category).HasMaxLength(50);

            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<UserInfo>(entity =>
        {
            entity.HasKey(e => e.UserId);

            entity.Property(e => e.CreatedDate).HasColumnType("datetime");

            entity.Property(e => e.Email).HasMaxLength(50);

            entity.Property(e => e.FirstName).HasMaxLength(50);

            entity.Property(e => e.LastName).HasMaxLength(50);

            entity.Property(e => e.Password).HasMaxLength(50);

            entity.Property(e => e.UserName).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}