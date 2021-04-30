using System;

namespace EMT.Common.Entities
{
    /// <summary>
    /// Classes that need sync fields or common fields inherit from this class.
    /// Useful for changes that affects many classes.
    /// </summary>
    public class AuditableEntityBase
    {
        // Audit and sync
        public uint CreatedBy { get; set; }  // UserId (uint)
        public uint UpdatedBy { get; set; }  // UserId (uint)
        public bool IsDeleted { get; set; }  // bit, not null
        public long CreatedAt { get; set; }  // bigint, not null
        public long UpdatedAt { get; set; }  // bigint, not null
    }
}
