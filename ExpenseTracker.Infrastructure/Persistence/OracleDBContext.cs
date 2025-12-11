using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using global::ExpenseTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Infrastructure.Persistence
{

    public class OracleDbContext : DbContext
    {
        public OracleDbContext(DbContextOptions<OracleDbContext> options) : base(options) { }
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Tag> Tags => Set<Tag>();
        public DbSet<Expense> Expenses => Set<Expense>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
          
            modelBuilder.Entity<Role>().ToTable("ROLES");
            modelBuilder.Entity<User>().ToTable("USERS");
            modelBuilder.Entity<Category>().ToTable("CATEGORIES");
            modelBuilder.Entity<Tag>().ToTable("TAGS");

            modelBuilder.Entity<Tag>(b =>
            {
                // If your table is in a specific schema, use:
                // b.ToTable("TAG", schema: "EXPENSE_APP");
                b.ToTable("TAGS");

                b.HasKey(t => t.Id);

                // Column mappings (ensure exact names as in Oracle)
                b.Property(t => t.Id)
                    .HasColumnName("ID");

                b.Property(t => t.Label)
                    .HasColumnName("LABEL")
                    .IsRequired()
                    .HasMaxLength(100);
            });


            modelBuilder.Entity<Expense>().ToTable("EXPENSES");

            // Expense money mapping (owned value object)
            modelBuilder.Entity<Expense>(b =>
            {
                b.OwnsOne(e => e.Money, mb =>
                {
                    mb.Property(m => m.Amount).HasColumnName("AMOUNT").HasPrecision(19, 4);
                });
                b.Property(e => e.Description).HasColumnName("DESCRIPTION").HasMaxLength(500);
                b.Property(e => e.TxnDate).HasColumnName("TXN_DATE");
                b.Property(e => e.CreatedAt).HasColumnName("CREATED_AT");
                b.Property(e => e.UserId).HasColumnName("USER_ID");
                b.Property(e => e.Id).HasColumnName("ID");
            });
        }

    }
}
