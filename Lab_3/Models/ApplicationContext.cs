using System.Data.Entity;

namespace Lab_3.Models
{
    public class ApplicationContext : DbContext
    {
        public DbSet<Student> Students { get; set; }

        public ApplicationContext() : base("name=ApplicationContext")
        {
        }
    }
}