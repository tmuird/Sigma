using Microsoft.EntityFrameworkCore;
using SigmaApp.Models;

namespace SigmaApp.Data
{
    public partial class LocalContext : DbContext
    {
        

        public LocalContext()
        {
            SQLitePCL.Batteries_V2.Init();

            this.Database.EnsureCreated();
        }

        public DbSet<User> Users { get; set; }

        public DbSet<Conversation> Conversations { get; set; }

        public DbSet<Message> Messages { get; set; }


  

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            try
            {

                
                string dbPath = Path.Combine(FileSystem.AppDataDirectory, "sigmalocal.sqlite");
                Console.WriteLine(dbPath);
                optionsBuilder
              .UseSqlite($"Filename={dbPath}");
            }
            catch (Exception)
            {

                throw;
            }
            
          
            
        }
    }
}