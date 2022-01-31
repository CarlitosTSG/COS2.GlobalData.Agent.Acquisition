using Conflux.Constants;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Conflux.Database.Model
{
    [Table("cfx_log")]
    public class DxLog : DxBaseTransact
    {
        [StringLength(200)]
        public string Class { get; set; }

        [StringLength(50)]
        public string Level { get; set; }

        [StringLength(2000)]
        public string Message { get; set; }
    }
}
