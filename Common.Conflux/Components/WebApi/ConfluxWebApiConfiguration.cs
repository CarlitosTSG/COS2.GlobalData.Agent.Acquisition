using Conflux.Management;
using System;
using System.Collections.Generic;
using System.Text;

namespace Conflux.Components.WebApi
{
    public class ConfluxWebApiConfigurationInfo
    {
        public string LoginType { get; set; }
        public string DefaultPIN { get; set; }
        public string DefaultUser { get; set; }
        public string DefaultPass { get; set; }
        public string WebApiName { get; set; }
        public int Port { get; set; }       
        public string ApiKey { get; set; }
    }

    public class ConfluxWebApiConfiguration : ConfluxConfiguration
    {
        public ConfluxWebApiConfigurationInfo[] WebApis { get; set; }
        public string SecretKey { get; set; }
    }
}
