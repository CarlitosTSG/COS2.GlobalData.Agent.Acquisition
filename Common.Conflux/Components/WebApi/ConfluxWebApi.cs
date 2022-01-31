using Conflux.Management;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Conflux.Components.WebApi
{
    public class ConfluxWebApi
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        // =============================================================================
        // Common parameters
        // =============================================================================

        public string ecosystem;
        public string subsystem;
        public string component;

        public string webapiName;
        public ConfluxWebApiConfigurationInfo webapiConfiguration;

        public ConfluxWebApi(string ecosystem, string subsystem, string component)
        {
            // Setup the full ComponentName
            this.ecosystem = ecosystem;
            this.subsystem = subsystem;
            this.component = component;

            // =============================================================================
            // Initialize the ConfluxManager            
            // =============================================================================
            ConfluxManager.Initialize(this.ecosystem, this.subsystem, this.component);
        }

        public virtual void Configure(string webapiName)
        {
            // Cast the already obtained configuration as a generic webapi configuration to obtain
            // general information for this API
            var config = ConfluxManager.ObtainConfiguration() as ConfluxWebApiConfiguration;

            // It's assumed each WebApi will perform it's own initialization     
            try 
            {
                webapiConfiguration = config.WebApis.ToList().First(x => x.WebApiName == webapiName);

                // Store this configuration in the current ConfluxManager
                ConfluxManager.SetWebApiInfo(webapiConfiguration); 
            }
            catch(Exception ex)
            {
                // Possible configuration error, let's see how we manage it
                logger.Error(ex, "There was an error reading the appsettings.json file. Could not load the webapi specific configuration for : "+ webapiName);
                ConfluxManager.AbortOperations();
            }
        }

        // ===================================================================================
        // API Startup
        // ===================================================================================
        public virtual void Start()
        {
            Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseUrls(@"http://*:"+webapiConfiguration.Port.ToString());
                    webBuilder.UseStartup<ConfluxWebApiStartup>();
                }).Build().Run();
        }

    }
}
