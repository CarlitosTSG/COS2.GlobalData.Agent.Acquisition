using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Conflux.Database.Model
{
    // ===========================================================================
    //
    // This table contains configuration dictionaries for the component/system
    // - Version 1.0
    //   Indexes to Users, Subsystems and possible operational modes included
    //   These are optional
    //
    // ===========================================================================

    [Table("cfx_config")]
    public class DxConfig : DxBaseDictionary
    {        
        public long UserId { get; set; }
        public long SubsystemId { get; set; }

        [StringLength(50)]
        public string Mode { get; set; }
    }
}
