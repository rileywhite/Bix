using System;

namespace Bix.Core
{
    public class ModelMetadata
    {
        public DateTime? CreatedAt { get; set; }
        public decimal? CreatedById { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public decimal? LastUpdatedById { get; set; }
    }
}
