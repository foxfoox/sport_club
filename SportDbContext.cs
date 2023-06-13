using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

public class SportDBContext : DbContext
{
    public SportDBContext(DbContextOptions<SportDBContext> options): base(options){}

    public DbSet<SportService> Services => Set<SportService>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();


    

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasMany(u => u.Subscriptions)
            .WithOne(s => s.User)
            .HasForeignKey(s => s.UserId)
            .IsRequired();
        
        modelBuilder.Entity<SportService>()
            .HasMany(sr => sr.Subscriptions)
            .WithOne(s => s.Service)
            .HasForeignKey(s => s.ServiceId)
            .IsRequired();
    }

    public String? Token
    {
        get
        {
            return _configs.Find("TOKEN")?.value;
        }
        set
        {
            _configs.Add(new _ConfigEntity(){key = "TOKEN", value = value});
            SaveChanges();
        }
    }

///////////////////////////////////CONFIGURATION///////////////////////////////////////////
    private DbSet<_ConfigEntity> _configs => Set<_ConfigEntity>();

    public bool IsConfigured
    {
        get
        {
            return _configs.Any();
        }
    }

    private class _ConfigEntity
    {
        [Key]
        public string  key   {get; set;}
        public string? value {get; set;}
    }

///////////////////////////////////CONFIGURATION///////////////////////////////////////////

//////////////////////////////////INITIALIZATION//////////////////////////////////////////

    public bool Initialized
    {
        get
        {
            return _configs.Find("INIT")?.value is String value && Boolean.Parse(value);
        }
        set
        {
            _configs.Add(new _ConfigEntity(){key = "INIT", value = value.ToString()});
            SaveChanges();
        }
    }

    public void Initialize()
    {
        Services.Add(new SportService()
        {
            Name   = "Personal Training",
            Period = 30,
            Price  = 1000,
            Time   = ServiceTime.NOT_SPECIFIED 
        });

        Services.Add(new SportService()
        {
            Name   = "Personal Training",
            Period = 30,
            Price  = 400,
            Time   = ServiceTime.MORNING 
        });

        Services.Add(new SportService()
        {
            Name   = "Personal Training",
            Period = 60,
            Price  = 1500,
            Time   = ServiceTime.NOT_SPECIFIED 
        });

        Services.Add(new SportService()
        {
            Name   = "Gym Membership",
            Period = 30,
            Price  = 2000,
            Time   = ServiceTime.NOT_SPECIFIED 
        });

        Users.Add(new User()
        {
            Name     = "Mr.Me",
            Email    = "xxxxx@me.com",
            IsAdmin  = true,
            Password = "PaSsWoRd123"
        });

        SaveChanges();

        Initialized = true;
    }
//////////////////////////////////INITIALIZATION//////////////////////////////////////////

}