using Conflux.Helpers;
using Conflux.Database;
using Conflux.Database.Context;

using Newtonsoft.Json;
using NLog;
using System;
using System.IO;
using Conflux.Components.WebApi;
using System.Collections.Generic;

namespace Conflux.Management
{
    public class ConfluxManager
    {
        // -------------------------------------------------------------------
        // Singleton Management
        // -------------------------------------------------------------------
        private static readonly ConfluxManager instance = new ConfluxManager();

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static ConfluxManager()
        {

        }


        public static ConfluxManager Live
        {
            get
            {
                return instance;
            }
        }




        // -------------------------------------------------------------------
        // Properties
        // -------------------------------------------------------------------     
        public Logger Logger { get; set; }




        // -------------------------------------------------------------------
        // Properties
        // -------------------------------------------------------------------     
        public string EcosystemName { get; set; }
        public string SubsystemName { get; set; }
        public string ComponentName { get; set; }
        public bool NoDatabases { get; set; }
        public bool HasChildDBs { get; set; }
        public string BaseOperationsDirectory { get; set; }
        public string ConfigurationDirectory { get; set; }
        public string LoggingDirectory { get; set; }
        public string StorageDirectory { get; set; }
        public string InstanceConnectionString { get; set; }
        public bool InstanceIsPortable { get; set; }
        public string PortableDirectory { get; set; }

        // -------------------------------------------------------------------
        // Operational Live Values
        // -------------------------------------------------------------------     
        public Dictionary<string,string> OperationalInfo { get; set; }


        // -------------------------------------------------------------------
        // Generic Properties
        // -------------------------------------------------------------------     
        public ConfluxConfiguration Configuration { get; set; }

        // -------------------------------------------------------------------
        // WebApi Properties
        // -------------------------------------------------------------------     
        public ConfluxWebApiConfigurationInfo CurrentWebApiInfo { get; set; }




        // -------------------------------------------------------------------
        // Monitor Database
        // -------------------------------------------------------------------     

        public ConfluxDatabase MonitorDatabase { get; set; }






        // -------------------------------------------------------------------
        // State
        // -------------------------------------------------------------------     
        public string State { get; set; }
        public bool CanOperate { get; set; }
        public bool StopOnSessionError { get; set; }

















        // -------------------------------------------------------------------
        // Initialization code
        // -------------------------------------------------------------------       
        private ConfluxManager()
        {
            // SetState
            State = "PreInit";

            // Unless we know otherwise, we can operate            
            CanOperate = true;
            NoDatabases = true;

            // Operational Info
            OperationalInfo = new Dictionary<string, string>();
        }

        private void InstanceInitialize(string aecosystem, string asubsystem, string acomponent)
        {
            InstanceIsPortable = false;
            EcosystemName = aecosystem;
            SubsystemName = asubsystem;
            ComponentName = acomponent;

            // We'll reconfigure these parameters in the main Conflux configuration
            HasChildDBs = false;
            NoDatabases = true;
            StopOnSessionError = false;

            // Set Directories
            if(CanOperate)
                SetDirectories();

            // Set Logging
            if (CanOperate)
                InitLogging();

            // SetState
            if (CanOperate)
            {
                State = "Initialized";

                // Log start of Manager                     
                Logger.Info("================================================================================");
                Logger.Info("ConfluxManager Initialization : " + EcosystemName + " / " + SubsystemName + " / " + ComponentName);
                Logger.Info("================================================================================");

            }
        }

        private void InstanceInitializePortable(string aecosystem, string asubsystem, string acomponent, string aportableDir)
        {
            InstanceIsPortable = true;
            PortableDirectory = aportableDir;
            EcosystemName = aecosystem;
            SubsystemName = asubsystem;
            ComponentName = acomponent;

            // We'll reconfigure these parameters in the main Conflux configuration
            HasChildDBs = false;
            NoDatabases = true;
            StopOnSessionError = false;

            // Set Directories
            if (CanOperate)
                SetPortableDirectories();

            // Set Logging
            if (CanOperate)
                InitLogging();

            // SetState
            if (CanOperate)
            {
                State = "Initialized";

                // Log start of Manager                     
                Logger.Info("================================================================================");
                Logger.Info("ConfluxManager Initialization : " + EcosystemName + " / " + SubsystemName + " / " + ComponentName);
                Logger.Info("================================================================================");
                Logger.Info("Portable Instance : "+PortableDirectory);
                Logger.Info("================================================================================");

            }
        }


        // Static version for external singleton access
        public static void Initialize(string aecosystem, string acomponent, string asubsystem)
        {
            Live.InstanceInitialize(aecosystem, acomponent, asubsystem);
        }

        public static void InitializePortable(string aecosystem, string acomponent, string asubsystem, string portableBaseDir)
        {
            Live.InstanceInitializePortable(aecosystem, acomponent, asubsystem, portableBaseDir);
        }





        public void Stop()
        {
            // Finish Operations
            CanOperate = false;
            State = "Stopped";

            // Log end of Manager     
            Logger.Info("================================================================================");
            Logger.Info("ConfluxManager : Manager Stop");
            Logger.Info("================================================================================");

            LogManager.Flush();
        }





        private void SetDirectories()
        {
            if (CanOperate)
            {
                // If we're not running in Linux, we need to work from the base OF directory in Windows
                if (ConfluxOperatingSystem.IsWindows())
                {
                    BaseOperationsDirectory = @"c:\emRoot\" + EcosystemName + @"\";
                    // Secondary directories
                    ConfigurationDirectory = BaseOperationsDirectory + @"config\";
                    LoggingDirectory = BaseOperationsDirectory + @"logs\";
                    StorageDirectory = BaseOperationsDirectory + @"storage\";
                }
                else
                {
                    BaseOperationsDirectory = @"/emRoot/" + EcosystemName + @"/";
                    ConfigurationDirectory = BaseOperationsDirectory + @"config/";
                    LoggingDirectory = BaseOperationsDirectory + @"logs/";
                    StorageDirectory = BaseOperationsDirectory + @"storage/";
                }
            }
        }

        private void SetPortableDirectories()
        {
            if (CanOperate)
            {
                // If we're not running in Linux, we need to work from the base OF directory in Windows
                if (ConfluxOperatingSystem.IsWindows())
                {
                    BaseOperationsDirectory = PortableDirectory+@"\";
                    // Secondary directories
                    ConfigurationDirectory = BaseOperationsDirectory + @"config\";
                    LoggingDirectory = BaseOperationsDirectory + @"logs\";
                    StorageDirectory = BaseOperationsDirectory + @"storage\";
                }
                else
                {
                    BaseOperationsDirectory = PortableDirectory + @"/";
                    ConfigurationDirectory = BaseOperationsDirectory + @"config/";
                    LoggingDirectory = BaseOperationsDirectory + @"logs/";
                    StorageDirectory = BaseOperationsDirectory + @"storage/";
                }
            }
        }


        private void InitLogging()
        {
            if (CanOperate)
            {
                try
                {
                    LogManager.Configuration = new NLog.Config.XmlLoggingConfiguration(
                        ConfigurationDirectory + @"nlog.config");

                    GlobalDiagnosticsContext.Set("loggingDirectory", LoggingDirectory);
                    GlobalDiagnosticsContext.Set("componentName", ComponentName.ToLowerInvariant());

                    LogManager.Configuration = LogManager.Configuration?.Reload();

                    Logger = LogManager.GetCurrentClassLogger();
                }
                catch
                {
                    // Can't operate
                    CanOperate = false;

                    // Log externally
                    System.Console.WriteLine("ConfluxManager Error : " + EcosystemName + " / " + ComponentName + " : Could not initialize logging");
                }
            }
        }

        public static string GetStorageDirectory()
        {
            return Live.StorageDirectory;
        }

        public static string GetBaseDirectory()
        {
            return Live.BaseOperationsDirectory;
        }

        public static bool IsPortable()
        {
            return Live.InstanceIsPortable;
        }
















        // -------------------------------------------------------------------
        // Configuration Management
        // -------------------------------------------------------------------       
        public void InstanceConfigure<T>() where T : ConfluxConfiguration
        {
            if (CanOperate)
            {
                // Load the configuration as the registered class
                try
                {
                    string json = File.ReadAllText(ConfigurationDirectory + "appsettings.json");
                    Configuration = JsonConvert.DeserializeObject<T>(json);

                    Logger.Info("ConfluxManager Configuration : " + Configuration.LoggingLevel);
                    Logger.Info("================================================================================");

                    InstanceConnectionString = Configuration.Databases.ConnectionString;

                    // SetState
                    State = "Configured";
                }
                catch (Exception e)
                {
                    // Possible configuration error, let's see how we manage it
                    CanOperate = false;
                    Logger.Error(e, "There was an error reading the appsettings.json file. Could not load the configuration");
                }
            }
        }

        public void InstanceConfigure()
        {
            InstanceConfigure<ConfluxConfiguration>();
        }

        public static void Configure<T>() where T : ConfluxConfiguration
        {
            Live.InstanceConfigure<T>();
        }

        public static void Configure()
        {
            Live.InstanceConfigure<ConfluxConfiguration>();
        }

        public static ConfluxConfiguration ObtainConfiguration()
        {
            return Live.Configuration;
        }































        // -------------------------------------------------------------------
        // Database Management for Standard Conflux databases
        // -------------------------------------------------------------------       

        public string GetDatabaseName(string subsystem)
        {
            return "cfx." + EcosystemName.ToLowerInvariant() + "." + subsystem.ToLowerInvariant();
        }

        public void PrepareBaseDatabase(string subsystem, bool clear = false)
        {
            // This routine sets the system up as client-only, without local databases (no monitor)

            if (NoDatabases)
            {
                NoDatabases = false;
                Logger.Info("ConfluxManager : Setup for work with local/remote databases");
            }

            if (CanOperate)
            {

                Logger.Info("ConfluxManager Database Preparation : " + subsystem);

                var c = ConfluxContext.Acquire(GetDatabaseName(subsystem), clear);

                Logger.Info("ConfluxManager Database Context Acquired : " + subsystem);

                if (c != null)
                {
                    Logger.Info("ConfluxManager Database Validating/Creating/Migrating : " + subsystem);
                    if (c.CheckInitDB(clear))
                    {
                        Logger.Info("ConfluxManager Database Check/Init Complete : " + subsystem);
                    }
                    else
                    {
                        Logger.Info("ConfluxManager Database Check/Init Incomplete : " + subsystem);
                        CanOperate = false;
                    }
                }
                else
                {
                    CanOperate = false;
                    Logger.Error("ConfluxManager Error : The [" + subsystem + "] database could not be initialized");
                }
            }
        }

        public ConfluxDatabase ObtainDatabase(string subsystem)
        {

            ConfluxDatabase xdb = null;
            if (CanOperate && !NoDatabases)
            {
                try
                {
                    xdb = new ConfluxDatabase(GetDatabaseName(subsystem));
                    if (xdb == null)
                    {
                        CanOperate = false;
                        Logger.Error("ConfluxManager Error : The [" + subsystem + "] database could not be obtained");
                    }
                }
                catch (Exception ex)
                {
                    CanOperate = false;
                    Logger.Error(ex, "ConfluxManager Error : The [" + subsystem + "] database could not be obtained");
                }
            }
            return xdb;
        }

        public void Instance_RunWithoutPersistence()
        {
            // This routine sets the system up as client-only, without local databases (no monitor)
            NoDatabases = true;
            Logger.Info("ConfluxManager : Setup for work without local/remote databases");
        }

        public static void PrepareDatabase(string subsystem, bool clear = false)
        {
            Live.PrepareBaseDatabase(subsystem);
        }

        public static void PrepareMonitorDatabase(bool clear = false)
        {
            Live.PrepareBaseDatabase("monitor");
        }

        public static void PrepareDatabase(bool clear = false)
        {
            Live.PrepareBaseDatabase("main");
        }

        public static ConfluxDatabase ObtainDatabase()
        {
            return Live.ObtainDatabase("main");
        }

        public static void RunWithoutPersistence()
        {
            Live.Instance_RunWithoutPersistence();
        }





























        // -------------------------------------------------------------------
        // Operational Info
        // -------------------------------------------------------------------       

        public static bool HasOperationalInfo(string key)
        {
            return Live.OperationalInfo.ContainsKey(key);
        }

        public static string GetOperationalInfo(string key)
        {
            return Live.OperationalInfo[key];
        }

        public static void SetOperationalInfo(string key, string value)
        {
            Live.OperationalInfo[key] = value;
        }





















        // -------------------------------------------------------------------
        // Operations Management
        // -------------------------------------------------------------------       

        public void InstanceBeginOperations()
        {
            if (CanOperate)
            {
                // Set State
                State = "Operational";

                // Log end of Manager     
                Logger.Info("================================================================================");
                Logger.Info("ConfluxManager : Begin Operations");
                Logger.Info("================================================================================");

                // All database initializations have taken place, and now we need to create the handlers
                // for the default databases
                if(!NoDatabases)
                    MonitorDatabase = new ConfluxDatabase("monitor");                
            }
        }

        public static void BeginOperations()
        {
            Live.InstanceBeginOperations();
        }

        public void InstanceAbortOperations()
        {
            // Log end of Manager     
            Logger.Info("================================================================================");
            Logger.Info("ConfluxManager : Aborting Operations");
            Logger.Info("================================================================================");

            CanOperate = false;

            // Close all references
            Stop();
        }

        public static void AbortOperations()
        {
            Live.InstanceAbortOperations();
        }














        // -------------------------------------------------------------------
        // Agent Utilities
        // -------------------------------------------------------------------       










        // -------------------------------------------------------------------
        // WebApi Utilities
        // -------------------------------------------------------------------       

        public ConfluxWebApiConfigurationInfo Instance_GetWebApiInfo()
        {
            return CurrentWebApiInfo;
        }

        public void Instance_SetWebApiInfo(ConfluxWebApiConfigurationInfo currentInfo)
        {
            CurrentWebApiInfo = currentInfo;
        }

        public static ConfluxWebApiConfigurationInfo ObtainWebApiInfo()
        {
            return Live.Instance_GetWebApiInfo();
        }

        public static void SetWebApiInfo(ConfluxWebApiConfigurationInfo currentInfo)
        {
            Live.Instance_SetWebApiInfo(currentInfo);
        }













        // -------------------------------------------------------------------
        // Session & Monitor Messaging
        // -------------------------------------------------------------------       

        public static void ReportSessionError(ConfluxSession session)
        {
            // See if we need to stop
            if (Live.StopOnSessionError)
                Live.Stop();
        }














        // -------------------------------------------------------------------
        // Helper Code
        // -------------------------------------------------------------------       

        public static string ConnectionString
        {
            get
            {
                return Live.InstanceConnectionString;
            }
        }

        public static bool Operational
        {
            get
            {
                return Live.CanOperate;
            }
        }

    }
}
