using System.Collections.Generic;
using System.Reflection.Emit;
using Microsoft.EntityFrameworkCore;
using Subscription.Models;

namespace Subscription.Data
{
    public class SubscriptionContext : DbContext
    {
        public SubscriptionContext(DbContextOptions<SubscriptionContext> options) : base(options) { }
        public DbSet<SubscriptionModel> Subscriptions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SubscriptionModel>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.Property(s => s.Domain).IsRequired();
                entity.Property(s => s.ValidUntil).IsRequired();
                entity.Property(s => s.SubscriptionEnum).IsRequired();
            });
        }
    }
}
