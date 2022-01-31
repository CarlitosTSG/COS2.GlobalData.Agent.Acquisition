using Conflux.Constants;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Conflux.Database.Model
{
    [Table("cfx_transact")]
    public class DxJsonTransact : DxBaseTransact
    {
        // ===========================================================================
        //
        // This is the base transactional class.  This class' purpose is to store information
        // that can vary in volume rapidly, and is stored in ever increasing numbers.
        // This information is stored sequentially (it's defined by a timestamp), and
        // can further be subindexed by the following properties
        //
        // Most specific databases will inherit this class for specific transaction lists
        //
        // - Version 1.0
        //   Base transactional class
        //
        // ===========================================================================

        // JSON General Info
        [StringLength(200)]
        public string Class { get; set; }

        [StringLength(50)]
        public string Version { get; set; }

        [Column(TypeName = "json")]
        public string Json { get; set; }


        // Simple relationship
        public long LinkId { get; set; }

        [StringLength(200)]
        public string LinkClass { get; set; }
        public EntityRelationship Relationship { get; set; }
        public EntityLinkType LinkType { get; set; }
    }
}
