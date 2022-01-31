using Conflux.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Conflux.Database.Model
{
    // ===========================================================================
    //
    // These tables contain the connections between entities.
    // - Version 1.0
    //   This version contains the index for the related entities and the connection
    //
    // ===========================================================================

    [Table("cfx_links")]
    public class DxEntityLink
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [StringLength(200)]
        public string FromClass { get; set; }
        public long FromId { get; set; }
        public string FromCode { get; set; }

        [StringLength(200)]
        public string ToClass { get; set; }
        public long ToId { get; set; }
        public string ToCode { get; set; }
        public EntityRelationship Relationship { get; set; }
        public EntityLinkType LinkType { get; set; }
    }
}
