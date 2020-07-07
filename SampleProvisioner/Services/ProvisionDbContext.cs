using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace SampleProvisioner.Services
{
    class ProvisionDbContext : DbContext
    {
        private readonly string connectionString;

        public ProvisionDbContext(String connectionString)
        {
            this.connectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(connectionString);
            }

            base.OnConfiguring(optionsBuilder);
        }
    }
}
