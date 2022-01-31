using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Conflux.Database.Model
{
    public class DxBasePersist
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }
        public long CreatedUserId { get; set; }

        [Required]
        public DateTime ModifiedDate { get; set; }
        public long ModifiedUserId { get; set; }

        public DxBasePersist()
        {
            CreatedDate = DateTime.UtcNow;
            ModifiedDate = DateTime.UtcNow;
        }
    }
}
