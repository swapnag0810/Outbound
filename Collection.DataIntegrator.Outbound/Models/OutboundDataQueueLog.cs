using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Collection.DataIntegrator.Outbound.Models
{
    public class OutboundDataQueueLog
    {

        public int OutboundDataQueueLogId { get; set; }
        public int OutboundDataQueueId { get; set; }
        public int EventTypeId { get; set; }
        public string Description { get; set; }
        public Nullable<bool> Status { get; set; }
        public Nullable<DateTime> CreatedAt { get; set; }
        public Nullable<DateTime> UpdatedAt { get; set; }
    }
}

