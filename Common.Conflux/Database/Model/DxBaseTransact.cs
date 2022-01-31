using Conflux.Constants;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Conflux.Database.Model
{    
    public class DxBaseTransact
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

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        public DateTime Timestamp { get; set; }
        public long CreatedUserId { get; set; }

        public DateTime Date { get; set; }


        public DxBaseTransact()
        {
            Timestamp = DateTime.UtcNow;            
        }
    }
}
