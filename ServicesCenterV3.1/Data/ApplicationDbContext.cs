using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ServicesCenterV3._1.Models;
using System.Reflection.Emit;

namespace ServicesCenterV3._1.Data
{
    public class ApplicationDbContext : IdentityDbContext<User,IdentityRole,string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }

        public DbSet<User> Clients { get; set; }
        public DbSet<TypeTechnic> TypeTechnics { get; set; }
        public DbSet<Technic> Technics { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Invoice> invoices { get; set; }
        public DbSet<Spare> spares { get; set; }
        public DbSet<SpareInvoice> spareInvoices { get; set; }
        public DbSet<Review> reviews { get; set; }
        public DbSet<Supplier> suppliers { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<User>().ToTable("users");
            builder.Entity<IdentityRole>().ToTable("positions");
            builder.Entity<IdentityUserRole<string>>().ToTable("users_position");
            builder.Entity<IdentityUserToken<string>>().ToTable("users_token");
            builder.Entity<IdentityRoleClaim<string>>().ToTable("position_claim");
            builder.Entity<IdentityUserClaim<string>>().ToTable("users_claim");
            builder.Entity<IdentityUserLogin<string>>().ToTable("users_login");


            builder.Entity<IdentityRole>().HasData(
                new IdentityRole
                {
                    Id = "1",
                    Name = "Admin",
                    NormalizedName = "ADMIN"
                },
                new IdentityRole
                {
                    Id = "2",
                    Name = "Manager",
                    NormalizedName = "MANAGER"
                },
                new IdentityRole
                {
                    Id = "3",
                    Name = "Client",
                    NormalizedName = "CLIENT"
                },
                new IdentityRole
                {
                    Id = "4",
                    Name = "Master",
                    NormalizedName = "MASTER"
                }
            );
            builder.Entity<TypeTechnic>().HasData(

                 new TypeTechnic { Type = "Телевізор",
                     Cost = 300 },
                 new TypeTechnic { Type = "Холодильник",
                     Cost = 500 },
                 new TypeTechnic { Type = "Пральна машина",
                     Cost = 400 },
                 new TypeTechnic { Type = "Комп'ютер",
                     Cost = 600 },
                 new TypeTechnic { Type = "Мікрохвильова піч",
                     Cost = 150 },
                 new TypeTechnic { Type = "Ноутбук",
                     Cost = 450
                 },
                 new TypeTechnic { Type = "Телефон",
                     Cost = 290
                 }
            );

            builder.Entity<Technic>().HasData(
                new Technic { NameTechnic = "Samsung QLED", TechnicType = "Телевізор" },
                new Technic { NameTechnic = "LG DoorCooling+", TechnicType = "Холодильник" },
                new Technic { NameTechnic = "Bosch Serie 6", TechnicType = "Пральна машина" },
                new Technic { NameTechnic = "HP Pavilion", TechnicType = "Комп'ютер" },
                new Technic { NameTechnic = "Samsung Smart Oven", TechnicType = "Мікрохвильова піч" },
                new Technic { NameTechnic = "Dell Inspiron", TechnicType = "Ноутбук" },
                new Technic { NameTechnic = "iPhone 13", TechnicType = "Телефон" }
            );
            var hasher = new PasswordHasher<User>();

            var admin = new User
            {
                Id = "11",
                UserName = "admin@example.com",
                NormalizedUserName = "ADMIN@EXAMPLE.COM",
                Email = "admin@example.com",
                NormalizedEmail = "ADMIN@EXAMPLE.COM",
                EmailConfirmed = true,
                PasswordHash = hasher.HashPassword(null, "Test11@"),
                SecurityStamp = Guid.NewGuid().ToString("D")
            };

            var manager = new User
            {
                Id = "12",
                UserName = "manager@example.com",
                NormalizedUserName = "MANAGER@EXAMPLE.COM",
                Email = "manager@example.com",
                NormalizedEmail = "MANAGER@EXAMPLE.COM",
                EmailConfirmed = true,
                PasswordHash = hasher.HashPassword(null, "Test11@"),
                SecurityStamp = Guid.NewGuid().ToString("D")
            };

            var client = new User
            {
                Id = "13",
                UserName = "client@example.com",
                NormalizedUserName = "CLIENT@EXAMPLE.COM",
                Email = "client@example.com",
                NormalizedEmail = "CLIENT@EXAMPLE.COM",
                EmailConfirmed = true,
                PasswordHash = hasher.HashPassword(null, "Test11@"),
                SecurityStamp = Guid.NewGuid().ToString("D")
            };

            var master = new User
            {
                Id = "14",
                UserName = "master@example.com",
                NormalizedUserName = "MASTER@EXAMPLE.COM",
                Email = "master@example.com",
                NormalizedEmail = "MASTER@EXAMPLE.COM",
                EmailConfirmed = true,
                PasswordHash = hasher.HashPassword(null, "Test11@"),
                SecurityStamp = Guid.NewGuid().ToString("D")
            };

            builder.Entity<User>().HasData(admin, manager, client, master);

            // Призначення ролей користувачам
            builder.Entity<IdentityUserRole<string>>().HasData(
                new IdentityUserRole<string> { UserId = "11", RoleId = "1" }, // Admin
                new IdentityUserRole<string> { UserId = "12", RoleId = "2" }, // Manager
                new IdentityUserRole<string> { UserId = "13", RoleId = "3" }, // Client
                new IdentityUserRole<string> { UserId = "14", RoleId = "4" }  // Master
            );

            builder.Entity<Supplier>().HasData(
                new Supplier
                {
                    SupplierId = -1, // Використовуємо від'ємне значення для уникнення колізій
                    SupplierName = "Постачальник 1",
                    SupplierAdress = "Київ, вул. Центральна, 1",
                    Website = "http://supplier1.com",
                    Email = "info@supplier1.com",
                    Telefon = "+380441234567"
                },
                new Supplier
                {
                    SupplierId = -2, // Використовуємо від'ємне значення для уникнення колізій
                    SupplierName = "Постачальник 2",
                    SupplierAdress = "Львів, вул. Головна, 5",
                    Website = "http://supplier2.com",
                    Email = "contact@supplier2.com",
                    Telefon = "+380322345678"
                },
                new Supplier
                {
                    SupplierId = -3, // Використовуємо від'ємне значення для уникнення колізій
                    SupplierName = "Постачальник 3",
                    SupplierAdress = "Одеса, вул. Приморська, 10",
                    Website = "http://supplier3.com",
                    Email = "support@supplier3.com",
                    Telefon = "+380482456789"
                }
            );


        }


    }
}
