using Microsoft.EntityFrameworkCore;
using userPanelOMR.model;

namespace userPanelOMR.Context
{

    // Normal Class me DbContext ko inherit karte hain from EF Core    
    public class JWTContext : DbContext
    {
        public JWTContext(DbContextOptions<JWTContext> options) : base(options)
        {
        }

        // dbSet se hum table ko intract karte hain jo humne model me banaye hain
        public DbSet<UserClass> UsersTab { get; set; }
        public DbSet<EmpClass> EmployeesTab { get; set; }
        public DbSet<DynamicForm> DynamicForm { get; set; }
        public DbSet<SingUps> singUps { get; set; }



        // dvb set me humne 2 model(table) di hai, hum primary key aise define karte hain.
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EmpClass>().HasKey(e => e.EmpId);       // Define EmpId as the primary key.
            modelBuilder.Entity<UserClass>().HasKey(u => u.CstId);      // Define UserId as the primary key.
            modelBuilder.Entity<DynamicForm>().HasKey(d => d.id);       // Define Id as the primary key.
            modelBuilder.Entity<SingUps>().HasKey(d => d.userId);            // Define Id as the primary key.
        }

    }
}
