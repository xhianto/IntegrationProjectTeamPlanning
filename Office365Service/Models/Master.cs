using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace Office365Service.Models
{
    public partial class Master
    {
        public int Id { get; set; }
        public byte[] Uuid { get; set; }
        public string SourceEntityId { get; set; }
        public string EntityType { get; set; }
        public int EntityVersion { get; set; }
        public string Source { get; set; }
    }
}
