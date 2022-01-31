using Conflux.Management;
using System;
using System.Collections.Generic;
using System.Text;

namespace Conflux.Components.Agent
{
    public class ConfluxAgentConfigurationInfo
    {
        public string AgentName { get; set; }
        public int MainCycleSleepSecs { get; set; }
    }

    public class ConfluxAgentConfiguration : ConfluxConfiguration
    {
        public ConfluxAgentConfigurationInfo[] Agents { get; set; }
    }
}
