using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Conflux.Database.Model
{
    [Table("cfx_storage")]
    public class DxStorage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        [Required]
        public DateTime CreatedDate { get; set; }
        [StringLength(200)]
        public string LinkClass { get; set; }
        public long LinkId { get; set; }
        [StringLength(50)]
        public string LinkCode { get; set; }
        [StringLength(50)]
        public string StorageCode { get; set; }
        [StringLength(1000)]
        public string StorageFilename { get; set; }
        [StringLength(1000)]
        public string StorageURL { get; set; }
        public long Size { get; set; }
        public byte[] Data { get; set; }

        public DxStorage()
        {
            CreatedDate = DateTime.UtcNow;
        }

    }
}
