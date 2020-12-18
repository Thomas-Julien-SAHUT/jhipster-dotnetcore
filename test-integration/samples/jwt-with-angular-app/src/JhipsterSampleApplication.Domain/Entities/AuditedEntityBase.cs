using System;
using JhipsterSampleApplication.Domain.Interfaces;

namespace JhipsterSampleApplication.Domain {
    public abstract class AuditedEntityBase : IAuditedEntityBase {
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string LastModifiedBy { get; set; }
        public DateTime LastModifiedDate { get; set; }
    }
}
