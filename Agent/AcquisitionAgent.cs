using Conflux.Components.Agent;
using Conflux.Database;
using Conflux.Management;
using GlobalData.Agent.Acquisition.Providers;
using MailKit.Net.Smtp;
using MimeKit;
using System;

namespace GlobalData.Agent.Acquisition
{
    public class AcquisitionAgent : ConfluxAgent
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        // ===================================================================================
        // Properties
        // ===================================================================================
        public ConfluxDatabase mainDatabase;
        public AcquisitionAgentConfiguration mainConfig;
        public GFSHiResProvider gfsProvider;



        // ===================================================================================
        // Agent Constructor
        // ===================================================================================
        public AcquisitionAgent(string ecosystem, string subsystem, string component) : 
            base(ecosystem, subsystem, component)
        {
            // Initialize ConfluxManager
        }


        // ===================================================================================
        // Agent Configuration
        // ===================================================================================
        public void Configure()
        {
            // Configure the current ConfluxManager with a specific configuration pattern
            ConfluxManager.Configure<AcquisitionAgentConfiguration>();

            // We now obtain the configuration "back" from the ConfluxManager and cast it
            // so we can use it locally.
            mainConfig = ConfluxManager.ObtainConfiguration() as AcquisitionAgentConfiguration;

            // Finally, perform the base configuration
            base.Configure("Acquisition");
        }


        // ===================================================================================
        // Agent Operations
        // ===================================================================================
        public void Start()
        {
            // 3.- PowerOn Monitor persistence & configuration
            ConfluxManager.PrepareMonitorDatabase();

            // 4.- PowerOn System Persistence
            ConfluxManager.PrepareDatabase();

            // 10.- Begin Conflux operations
            if (ConfluxManager.Operational)
            {
                ConfluxManager.BeginOperations();

                // Log AcquisitionAgent Start
                logger.Info("Starting Agent");

                // Execute the AcquisitionAgent
                Run();
            }
            else
            {
                ConfluxManager.AbortOperations();
            }
        }


        public void Run()
        {

            logger.Info("Agent Settings : Owner : " + mainConfig.ServerOwner);
            logger.Info("Agent Settings : Concurrency : " + mainConfig.Acquisition.Concurrency);

            // Mail
            StartingMail();

            // Setup
            logger.Info("Setting up NCEP GFS HiRes provider");


            // Do not run tests - used for internal development only
            //(new AcquisitionAgent_Tests()).DoTests();

            // Create our single provider
            // TO-DO : Add more providers
            gfsProvider = new GFSHiResProvider();

            // We initialize the gfsProvider
            gfsProvider.Initialize();
            // And perform an initial cleanup
            gfsProvider.Cleanup();

            // Main Cycle
            logger.Info("Starting main cycle");
            bool canOperate = true;
            while (canOperate)
            {
                Iterate();
            }

            logger.Info("Stopping Agent");
            // Shutdown
        }

        public void Iterate()
        {
            // This routine performs a single iteration of the Agent
            // Normally, a single iteration involves agent processing and thread management, heartbeat triggering and
            // operational checks

            IterateProviders();

            IterationSleep();
        }

        public void IterateProviders()
        {
            // This iteration step performs the normal agent processing steps done in an iteration loop
            gfsProvider.Iterate();
        }




        public void StartingMail()
        {
            try
            {
                // This routine sends out possible info for receptor alert r
                string FromAddress = mainConfig.ServerMail.SenderEmailAddress;
                string FromAdressTitle = mainConfig.ServerMail.SenderEmailName;

                #region Se aplica la propiedad TAG en el inicio del Global Data
                //To Address 
                string ToAddress = mainConfig.ServerMail.AdminInfoRecipients;
                string Subject = "COS2 : Inicio de Servidor : " + mainConfig.ServerOwner;
                string BodyContent =
                    "COS2 - Inicio de Servidor GlobalData "+ mainConfig.Acquisition.Tag + " \r\n" +
                    "\r\n" +
                    "Fecha     : " + DateTime.Now.ToString("yyyy-MM-dd HH:mm") + "\r\n" +
                    "\r\n" +
                    "TSG Environmental";
                #endregion  

                // Smtp Server 
                string SmtpServer = mainConfig.ServerMail.SMTPServer;
                int SmtpPortNumber = mainConfig.ServerMail.SMTPPort;

                var mimeMessage = new MimeMessage();
                mimeMessage.From.Add(new MailboxAddress(FromAdressTitle, FromAddress));

                char[] csplit = { ',', ';' };
                string[] adrs = ToAddress.Split(csplit, StringSplitOptions.RemoveEmptyEntries);

                foreach (string item in adrs)
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        logger.Info("Agent Settings : Admin Info - Sending Startup Email to : [" + item + "]");
                        mimeMessage.To.Add(new MailboxAddress(item, item));
                    }
                }

                //mimeMessage.To.Add(new MailboxAddress(ToAdressTitle, ));
                mimeMessage.Subject = Subject;
                mimeMessage.Body = new TextPart("plain")
                {
                    Text = BodyContent
                };

                using (var client = new SmtpClient())
                {
                    client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                    client.Connect(SmtpServer, SmtpPortNumber, false);
                    // Note: only needed if the SMTP server requires authentication 
                    // Error 5.5.1 Authentication  
                    client.Authenticate(mainConfig.ServerMail.SMTPUsername, mainConfig.ServerMail.SMTPPassword); // "adminplume@tsgenviro.com", "Admin.123"
                    client.Send(mimeMessage);
                    client.Disconnect(true);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Agent Settings : Admin Info - Sending Startup Email Error");
            }
        }
    }
}
