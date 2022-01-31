using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Conflux.Database.Model
{
    /// <summary>
    /// Entity storage class
    /// </summary>
    [Table("cfx_entities")]
    public class DxEntity : DxBasePersist
    {
        /// <summary>
        /// The class name of the entity being stored.  Obtained using typeof(T).Name
        /// </summary>
        [StringLength(200)]
        public string Class { get; set; }
        /// <summary>
        /// Alternate index for the entity, may be non-unique.
        /// </summary>
        [StringLength(50)]
        public string Code { get; set; }
        /// <summary>
        /// JSON object representing the entity
        /// </summary>
        [Column(TypeName = "json")]
        public string Json { get; set; }
    }
}
