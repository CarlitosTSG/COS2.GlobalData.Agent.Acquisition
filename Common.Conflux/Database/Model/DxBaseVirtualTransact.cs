using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Conflux.Database.Model
{
    public class DxBaseVirtualTransact
    {
        public long Id { get; set; }
        public DateTime Timestamp { get; set; }
        public long CreatedUserId { get; set; }
        public DateTime Date { get; set; }
    }
}
