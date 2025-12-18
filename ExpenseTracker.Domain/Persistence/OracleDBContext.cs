
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Entities.ExpenseTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Domain.Persistence
{
    public class OracleDbContext : DbContext
    {
        public OracleDbContext(DbContextOptions<OracleDbContext> options) : base(options) { }

        public DbSet<Role> Roles => Set<Role>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Tag> Tags => Set<Tag>();
        public DbSet<Expense> Expenses => Set<Expense>();
        public DbSet<ExpenseTag> ExpenseTags => Set<ExpenseTag>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Role>().ToTable("ROLES");
            modelBuilder.Entity<User>().ToTable("USERS");
            modelBuilder.Entity<Category>().ToTable("CATEGORIES");
            modelBuilder.Entity<Tag>().ToTable("TAGS");

            modelBuilder.Entity<Tag>(b =>
            {
                b.ToTable("TAGS");
                b.HasKey(t => t.Id);
                b.Property(t => t.Id).HasColumnName("ID");
                b.Property(t => t.Label).HasColumnName("LABEL").IsRequired().HasMaxLength(100);
            });

            modelBuilder.Entity<Expense>().ToTable("EXPENSES");

            modelBuilder.Entity<User>(b =>
            {
                b.ToTable("USERS");
                b.HasKey(u => u.Id);
                b.Property(u => u.Id).HasColumnName("ID");
                b.Property(u => u.Name).HasColumnName("NAME").IsRequired().HasMaxLength(200);
                b.Property(u => u.Email).HasColumnName("EMAIL").HasMaxLength(200);
                b.Property(u => u.PasswordHash).HasColumnName("PASSWORD_HASH").HasMaxLength(200);
                b.Property(u => u.RoleId).HasColumnName("ROLE_ID");
                b.Property(u => u.parent_user_id).HasColumnName("PARENT_USER_ID");
            });

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

            modelBuilder.Entity<Role>(b =>
            {
                b.Property(u => u.Id).HasColumnName("ID");
                b.Property(u => u.Name).HasColumnName("NAME");
            });

            // ✅ Correct join table mapping
            modelBuilder.Entity<ExpenseTag>(b =>
            {
                b.ToTable("EXPENSE_TAG");

                // Composite key
                b.HasKey(et => new { et.ExpenseId, et.TagId });

                // Column names
                b.Property(et => et.ExpenseId).HasColumnName("EXPENSE_ID");
                b.Property(et => et.TagId).HasColumnName("TAG_ID");

                // Relationship to Expense (principal: EXPENSES)
                b.HasOne(et => et.Expense)
                 .WithMany(e => e.ExpenseTags)     // if you don't have navigation on Expense, use .WithMany()
                 .HasForeignKey(et => et.ExpenseId)
                 .OnDelete(DeleteBehavior.Cascade);

                // Relationship to Tag (principal: TAGS)
                b.HasOne(et => et.Tag)
                 .WithMany()     // if you don't have navigation on Tag, use .WithMany                 .WithMany(t => t.ExpenseTags)     // if you don't have navigation on Tag, use .WithMany()
                 .HasForeignKey(et => et.TagId)
                 .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
