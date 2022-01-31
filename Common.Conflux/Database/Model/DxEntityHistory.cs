using Conflux.Constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Conflux.Database.Model
{
    /// <summary>
    /// Entity storage history, manages changes to entities throughout the project life
    /// </summary>
    [Table("cfx_entityhistory")]
    public class DxEntityHistory : DxBasePersist
    {
        /// <summary>
        /// Reason record was generated
        /// </summary>
        public EntityHistoryRecordType RecordType { get; set; }
        /// <summary>
        /// Id of the entity being tracked. 
        /// </summary>
        public long EntityId { get; set; }
        /// <summary>
        /// The class name of the entity being stored.  Obtained using typeof(T).Name
        /// </summary>
        [StringLength(200)]
        public string Class { get; set; }
        [StringLength(50)]
        /// <summary>
        /// Alternate index for the entity, may be non-unique.  Can store code changes for entity.
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// JSON object representing the entity
        /// </summary>
        [Column(TypeName = "json")]
        public string Json { get; set; }
    }
}
