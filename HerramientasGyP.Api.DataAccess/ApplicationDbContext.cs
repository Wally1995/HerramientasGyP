using HerramientasGyP.Api.Models;
using HerramientasGyP.Api.Models.Auth;
using HerramientasGyP.Api.Models.Customers;
using HerramientasGyP.Api.Models.Inventories;
using HerramientasGyP.Api.Models.Suppliers;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HerramientasGyP.Api.DataAccess;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {

    }

    public DbSet<Product> Products { get; set; }
    public DbSet<Warehouse> Warehouses { get; set; }
    public DbSet<ProductWarehouse> ProductWarehouses { get; set; }
    public DbSet<Kit> Kits { get; set; }
    public DbSet<KitDetail> KitsDetails { get; set; }

    public DbSet<Customer> Customers { get; set; }
    public DbSet<Sale> Sales { get; set; }
    public DbSet<SaleDetail> SaleDetails { get; set; }

    public DbSet<Purchase> Purchases { get; set; }
    public DbSet<PurchaseDetail> PurchaseDetails { get; set; }
    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<SupplierProduct> SupplierProducts { get; set; }
    
    public DbSet<Branch> Branches { get; set; }
    public DbSet<Person> Persons { get; set; }

    public DbSet<LoginSession> LoginSessions { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(e => e.Quantity).HasDefaultValue(0);
            entity.Property(e => e.Cost).HasDefaultValue(0);
            entity.Property(e => e.Price).HasDefaultValue(0);
            entity.Property(e => e.Deleted).HasDefaultValue(false);
        });
        
        modelBuilder.Entity<Warehouse>(entity =>
        {
            entity.Property(e => e.isDeleted).HasDefaultValue(false);
        });
        
        modelBuilder.Entity<ProductWarehouse>()
            .HasKey(pw => new { pw.ProductId, pw.WarehouseId });
        
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.Property(e => e.HasCredit).HasDefaultValue(false);
            entity.Property(e => e.CreditDays).HasDefaultValue(0);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
        });
        
        modelBuilder.Entity<Sale>(entity =>
        {
            entity.Property(e => e.Status).HasDefaultValue(true);
            entity.Property(e => e.PrintedStatus).HasDefaultValue(false);
        });
        
        modelBuilder.Entity<SupplierProduct>()
            .HasKey(sp => new { sp.SupplierId, sp.ProductId });
        
        modelBuilder.Entity<KitDetail>()
            .HasKey(kd => new { kd.KitId, kd.ProductId });
    }
}