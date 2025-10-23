using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using se347_be.Database.DbEntity;

namespace se347_be.Database
{
    public class MyAppDbContext : DbContext
    {
        public MyAppDbContext(DbContextOptions<MyAppDbContext> options) : base(options) { }
        
        public DbSet<TestEntity> TestEntities { get; set; }
    }
}