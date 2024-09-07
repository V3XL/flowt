using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

public class ApplicationDbContext : DbContext
{
    public DbSet<ScheduledTask> ScheduledTasks { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var dbHost = Environment.GetEnvironmentVariable("DB_HOST");
        var dbPort = Environment.GetEnvironmentVariable("DB_PORT");
        var dbUser = Environment.GetEnvironmentVariable("DB_USER");
        var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");
        var dbName = Environment.GetEnvironmentVariable("DB_NAME");

        var connectionString = $"Server={dbHost};Port={dbPort};User={dbUser};Password={dbPassword};Database={dbName};";
        
        optionsBuilder.UseMySQL(connectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ScheduledTask>()
            .ToTable("task")  // Specify the table name as 'task'
            .Property(p => p.Headers)
            .HasConversion(
                v => JsonConvert.SerializeObject(v),
                v => JsonConvert.DeserializeObject<Dictionary<string, string>>(v))
            .HasColumnType("json"); // Ensure correct SQL type
        
        modelBuilder.Entity<ScheduledTask>()
            .HasKey(t => t.TaskGuid); // Set TaskGuid as the primary key
    }
}
