using System;
using System.Collections.Generic;
using System.Text;

namespace Collection.DataIntegrator.Outbound.Models
{
    public class EventType
    {
        public int EventTypeId { get; set; }
        public string EventName { get; set; }
        public bool IsActive { get; set; }
        public string Handler { get; set; }
        public string FileName { get; set; }
        public string Type { get; set; }
        public Nullable<DateTime> CreatedAt { get; set; }
        public Nullable<DateTime> UpdatedAt { get; set; }
        public string SFTPOutboundFolderName { get; set; }
    }
}

