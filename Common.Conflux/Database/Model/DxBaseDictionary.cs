using System;

namespace Conflux.Database.Model
{
    public class DxBaseDictionary : DxBasePersist
    {
        public string ConfigKey { get; set; }
        public string Type { get; set; }
        public DateTime ValueDate { get; set; }
        public string ValueString { get; set; }
        public decimal ValueDecimal { get; set; }
        public long ValueInt { get; set; }
        public byte[] ValueData { get; set; }
    }
}
