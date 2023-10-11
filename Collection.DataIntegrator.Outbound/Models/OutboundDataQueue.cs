using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Collection.DataIntegrator.Outbound.Models
{
    public class OutboundDataQueue
    {

        public int OutboundDataQueueId { get; set; }
        public string Content { get; set; }
        public Nullable<bool> Status { get; set; }
        public Nullable<DateTime> CreatedAt { get; set; }
        public Nullable<DateTime> UpdatedAt { get; set; }

        public int EventTypeId { get; set; }

    }
}

