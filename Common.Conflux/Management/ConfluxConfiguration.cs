using System;
using System.Collections.Generic;
using System.Text;

namespace Conflux.Management
{
    public class ConfluxStatisticsConfiguration
    {
        public bool CalculateStatistics { get; set; }
        public int AverageEvery { get; set; }
        public bool PostToMonitor { get; set; }
        public bool PostDaily { get; set; }
    }

    public class ConfluxPersistenceConfiguration
    {
        public string ConnectionString { get; set; }
        public bool ClearData { get; set; }
    }

    public class ConfluxConfiguration
    {
        public ConfluxPersistenceConfiguration Databases { get; set; }
        public ConfluxStatisticsConfiguration Statistics { get; set; }
        public string LoggingLevel { get; set; }
        public string ServerOwner { get; set; }
    }
}
