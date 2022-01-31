using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Conflux.Database.Model
{
    public class DxBaseVirtualPersist 
    {
        public long Id { get; set; }
        public string Code { get; set; }        
        public DateTime CreatedDate { get; set; }
        public long CreatedUserId { get; set; }
        public DateTime ModifiedDate { get; set; }
        public long ModifiedUserId { get; set; }
        // Flag to check whether this entity is part of another
        public bool IsChild { get; set; }
    }
}
