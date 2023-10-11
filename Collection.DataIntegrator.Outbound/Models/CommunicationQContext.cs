using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Collection.DataIntegrator.Outbound.Models
{
    public class CommunicationQContext : DbContext
    {
        private readonly DbContextOptions _options;
        public CommunicationQContext(DbContextOptions<CommunicationQContext> options) : base(options)
        {
            _options = options;
        }

        public DbSet<OutboundDataQueue> OutboundDataQueue { get; set; }
        public DbSet<OutboundDataQueueLog> OutboundDataQueueLog { get; set; }

        public DbSet<EventType> EventType { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration<OutboundDataQueue>(new OutboundDataQueueMapper());
        }

        #region configure tables
        class OutboundDataQueueMapper : IEntityTypeConfiguration<OutboundDataQueue>
        {
            public void Configure(EntityTypeBuilder<OutboundDataQueue> builder)
            {
                //builder.ToTable("OutboundDataQueue");
                //builder.Property(t => t.OutboundDataQueueId).HasColumnName("OutboundDataQueueId").ValueGeneratedOnAdd();
                //builder.Property(t => t.EventTypeId).HasColumnName("EventTypeId");

                //builder.ToTable("OutboundDataQueueLog");
                //builder.Property(t => t.OutboundDataQueueId).HasColumnName("OutboundDataQueueLogId").ValueGeneratedOnAdd();
                //builder.Property(t => t.EventTypeId).HasColumnName("EventTypeId");
            }
        }
        #endregion
    }


}

