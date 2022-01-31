using Conflux.Components.Agent;
using Conflux.Management;
using System;
using System.Collections.Generic;
using System.Text;

namespace GlobalData.Agent.Acquisition
{
    public class ServerMailConfiguration
    {
        public string SMTPServer { get; set; }
        public int SMTPPort { get; set; }
        public string SMTPUsername { get; set; }
        public string SMTPPassword { get; set; }

        public string SenderEmailAddress { get; set; }
        public string SenderEmailName { get; set; }

        public string AdminInfoRecipients { get; set; }
        public string AdminErrorRecipients { get; set; }
    }


    public class DatasetAcquisitionConfiguration
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string URLTemplate { get; set; }
        public int NumHours { get; set; }
        public bool Active { get; set; }
    }

    #region Entidad que contiene la propiedad TAG

    public class AcquisitionAgentConfigurationInfo
    {
        public string Tag { get; set; }
        public int Concurrency { get; set; }
        public int RetainDatasets { get; set; }
        public string FtpServer { get; set; }
        public DatasetAcquisitionConfiguration[] Datasets { get; set; }
    }

    #endregion

    public class AcquisitionAgentConfiguration : ConfluxAgentConfiguration
    {
        public AcquisitionAgentConfigurationInfo Acquisition { get; set; }
        public ServerMailConfiguration ServerMail { get; set; }
    }
}

