using System;
using System.ComponentModel.DataAnnotations;

namespace EMT.Common.Entities
{
    // POCO Class for table: AppParameter	
    public class AppParameter : AuditableEntityBase
    {
        [Key]
        [Required]
        public uint AppParameterId { get; set; } //  PK / int / 0 /  Not Nullable
        [Required]
        public int GroupId { get; set; }    // int / 0 /  Not Nullable
        public string GroupCode { get; set; }   // varchar / 150 / Nullable
        [Required]
        public bool IsEditableGroup { get; set; }   // bit / 0 /  Not Nullable
        [Required]
        public int PId { get; set; }    // int / 0 /  Not Nullable
        public int? OrdIndex { get; set; }  // int / 0 / Nullable
        [Required]
        public string Value1 { get; set; }  // nvarchar / 250 / Nullable
        public string Value2 { get; set; }  // nvarchar / 250 / Nullable
        public string Notes { get; set; }   // nvarchar / -1 / Nullable
    }

    // POCO Class for table: note	
    public class MyNote : AuditableEntityBase
    {
        [Key]
        [Required]
        public uint NoteId { get; set; }  //  PK / varchar / 0 /  Not Nullable
        [Required]
        [StringLength(300)]
        public string Title { get; set; }   // varchar / 0 /  Not Nullable
        public string NoteBody { get; set; }    // mediumtext / 0 / Nullable
    }

    // Entity Class for table: MyTask	
    public class MyTask : AuditableEntityBase
    {
        [Key]
        [Required]
        public uint MyTaskId { get; set; } //  PK / int / 0 /  Not Nullable
        [Required]
        public int Y_TaskStatus_PId { get; set; }   // int / 0 /  Not Nullable
        [Required]
        [StringLength(250)]
        public string Description { get; set; } // varchar / 0 /  Not Nullable
        public string Notes { get; set; }   // mediumtext / 0 / Nullable
    }

    // POCO Class for table: AppUserRole
    public class AppUserRole : AuditableEntityBase
    {
        [Required]public uint AppUserRoleId { get; set; }  // AppUserRoleId (PK, int, not null) 
        [Required][StringLength(150)]public string Name { get; set; }  // Name (varchar(150), not null) 
        [Required]public bool IsActive { get; set; }  // IsActive (bit, not null) 
        [Required]public bool IsSuperAdmin { get; set; }  // IsSuperAdmin (bit, not null) 
        public string Notes { get; set; }  // Notes (nvarchar(max), null) 
    }

    public class AppUser : AuditableEntityBase
    {
        [Required]
        public uint AppUserId { get; set; }  // AppUserId (PK, varchar(50), not null) 
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string UserName { get; set; }  // UserName (varchar(200), not null) 
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string HashedPassword { get; set; }  // HashedPassword (varchar(50), not null) 
        [Required]
        [StringLength(200, MinimumLength = 2)]
        public string FirstName { get; set; }  // FirstName (nvarchar(100), not null) 
        [Required]
        [StringLength(200, MinimumLength = 2)]
        public string LastName1 { get; set; }  // LastName1 (nvarchar(150), not null) 
        public string LastName2 { get; set; }  // LastName2 (nvarchar(150), null) 
        public string FN { get; set; }  // FN (nvarchar(400), null) 
        [Required]
        [StringLength(250)]
        public string Email { get; set; }  // Email (varchar(200), null) 
        [Required]
        public bool IsActive { get; set; }  // IsActive (bit, not null) 
        [Required]
        public int Y_Role_AppRoleId { get; set; }  // Y_Role_AppRoleId (int, not null) 
        public string Notes { get; set; }  // Notes (nvarchar(max), null) 
    }    

}