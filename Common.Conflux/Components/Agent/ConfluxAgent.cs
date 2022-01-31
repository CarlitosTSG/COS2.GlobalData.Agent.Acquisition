using Conflux.Helpers;
using Conflux.Management;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Conflux.Components.Agent
{
    public class ConfluxAgent
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public bool isPortable;

        public string ecosystem;
        public string subsystem;
        public string component;

        public string agentName;       
        public ConfluxAgentConfigurationInfo agentConfiguration;

        public ConfluxAgent(string ecosystem, string subsystem, string component)
        {
            // Setup the full ComponentName
            this.ecosystem = ecosystem;
            this.subsystem = subsystem;
            this.component = component;

            // =============================================================================
            // Check to see if this is a portable setup
            // =============================================================================
            // In order to do this, obtain the current working directory
            string currentDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            if(File.Exists(currentDir+@"/../conflux.portable.json"))
            {
                // =============================================================================
                // Initialize the ConfluxManager            
                // =============================================================================
                // We're in a portable setting, define as such
                if (ConfluxOperatingSystem.IsWindows())
                    ConfluxManager.InitializePortable(this.ecosystem, this.subsystem, this.component, currentDir + @"\..");
                else
                    ConfluxManager.InitializePortable(this.ecosystem, this.subsystem, this.component, currentDir + @"/..");
            }
            else
            {
                // =============================================================================
                // Initialize the ConfluxManager            
                // =============================================================================
                ConfluxManager.Initialize(this.ecosystem, this.subsystem, this.component);
            }

            // Initialize the generic agent configuration
            agentConfiguration = null;
        }

        public virtual void Configure(string agentName)
        {
            // Cast the already obtained configuration as a generic webapi configuration to obtain
            // general information for this API
            var config = ConfluxManager.ObtainConfiguration() as ConfluxAgentConfiguration;

            // It's assumed each WebApi will perform it's own initialization     
            try
            {
                agentConfiguration = config.Agents.ToList().First(x => x.AgentName == agentName);
            }
            catch(Exception ex)
            {
                // Possible configuration error, let's see how we manage it
                logger.Error(ex, "There was an error reading the appsettings.json file. Could not load the agent specific configuration for : "+agentName);
                ConfluxManager.AbortOperations();
            }
        }

        public void IterationSleep()
        {
            if(agentConfiguration!=null)
            {
                Thread.Sleep(agentConfiguration.MainCycleSleepSecs);
            }            
        }

    }
}
