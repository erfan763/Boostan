using DominClass.Common.BaseEtity;
using DominClass.Entities.User;
using DominClass.Entities.UserToken;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Inferstructure.Context;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.UseCollation("Persian_100_CI_AI_SC_UTF8");
        var entitiesAssembly = typeof(IEntity).Assembly;
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        //modelBuilder.Entity<UserLogin>().HasKey(ul => ul.UserLoginId);
        modelBuilder.Entity<UserToken>().HasKey(ul => ul.UserTokenId);


        //modelBuilder.Entity<UserLogin>()
        //    .HasOne(ul => ul.User)
        //    .WithMany(u => u.Logins)
        //    .HasForeignKey(ul => ul.UserId)
        //    .OnDelete(DeleteBehavior.NoAction);
        //;
        modelBuilder.Entity<User>()
            .Property(u => u.UserName)
            .HasColumnName("UserName");

        modelBuilder.Entity<UserToken>()
            .HasOne(ut => ut.User)
            .WithMany(u => u.Tokens)
            .HasForeignKey(ut => ut.UserId)
            .OnDelete(DeleteBehavior.NoAction);
        ;
    }

    private void ConfigureEntityDates()
    {
        IEnumerable<ITimeModification> updatedEntities = ChangeTracker.Entries().Where(x =>
                x.Entity is ITimeModification && x.State == EntityState.Modified)
            .Select(x => x.Entity as ITimeModification);

        IEnumerable<ITimeModification> addedEntities = ChangeTracker.Entries().Where(x =>
            x.Entity is ITimeModification && x.State == EntityState.Added).Select(x => x.Entity as ITimeModification);

        foreach (var entity in updatedEntities)
            if (entity != null)
                entity.ModifiedDate = DateTime.Now;

        foreach (var entity in addedEntities)
            if (entity != null)
            {
                entity.CreatedTime = DateTime.Now;
                entity.ModifiedDate = DateTime.Now;
            }
    }

    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var builder = new DbContextOptionsBuilder<AppDbContext>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            builder.UseSqlServer(connectionString);

            return new AppDbContext(builder.Options);
        }
    }
}