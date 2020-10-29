using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using H5Serv.Models.DB;

namespace H5Serv.Data
{
    public class MyDbContext:DbContext
    {
        public DbSet<User> User { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Data Source = (localdb)\\MSSQLLocalDB; Initial Catalog = H5serv");
        }
        public MyDbContext(DbContextOptions<MyDbContext> options):base(options) { }
        public DbSet<H5Serv.Models.DB.TodoItem> TodoItem { get; set; }
    }
}
