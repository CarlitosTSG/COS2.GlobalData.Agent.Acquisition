using Conflux.Database;
using Conflux.Helpers;
using Conflux.Management;
using Conflux.Web;
using COS2.Core.Business;
using COS2.Core.Constants;
using COS2.Core.Data.Model.WRF;
using FluentFTP;
using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace GlobalData.Agent.Acquisition.Providers
{

    public enum GlobalAgentState { Normal, InitialStop, Wait8amStop, Wait8pmStop};

    public class GFSHiResProvider : GlobalDataProvider
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        // This class manages a GlobalDataProvider for NCEP GFS HiRes Data
        public new const string ProviderDatasetName = "GFS_HiRes";

        // Here we maintain state in order to review download status and message that information forward
        //public DateTime stateDatePrevious;
        //public bool stateDownloaded;
        //public bool stateSentMessage;
        //public bool stateFirst;
        public GlobalAgentState agentState = GlobalAgentState.Normal;
        public DateTime stateLastChange;

        // FluentFTP download flags
        bool fluentFtpFlagDownloadError;
        int fluentFtpTotalFiles;
        int fluentFtpCurrentFile;
        double fluentFtpMaxSpeed;
        long fluentFtpTotalSize;
        List<string> fluentFtpFiles;
        int contadorFtp = 0;

        public GFSHiResProvider() : base()
        {
            // TO-DO : Validate if the datasets don't include the provider
            datasetConfig = mainConfig.Acquisition.Datasets.ToList().First(x => x.Name == ProviderDatasetName);
            logger.Info("Provider : " + ProviderDatasetName + " : NumHours : " + datasetConfig.NumHours);

            // TO-DO : Define several domains and pass them to providers
            using var session = main.AcquireSession();
            domain = session.Get<GlobalDataDomain>().HasCode("GFS.Hi.SouthAmerica").ObtainVirtual();

            if (domain == null)
            {
                domain = new GlobalDataDomain()
                {
                    Code = "GFS.Hi.SouthAmerica",
                    Provider = "GFS HiRes [NCEP-GFS/0.25deg]",
                    GeoDomain = "SouthAmerica",
                    Active = true
                };

                session.Create(domain).Execute();
            }

            // Initialize counters and state flags
            stateLastChange = DateTime.UtcNow;
        }

        public override void Initialize()
        {
            // Execute the base initialization
            base.Initialize();

            // Do the first check
            using (var session = main.AcquireSession())
            {
                lastDownload = session.Get<GlobalDataDownload>().GlobalData_LastCompleteDownload(domain.Id).ObtainVirtual();
            }

            waitUntil = DateTime.UtcNow.AddHours(-24.0); // Let's set the last request a long while ago, so we get to it quick

            // Log last data set downloaded
            if (lastDownload == null)
            {
                logger.Info("Provider : " + ProviderDatasetName + " : No Last Download Found");
            }
            else
            {
                logger.Info("Provider : " + ProviderDatasetName + " : Last UTC Date Downloaded : " + lastDownload.DateUTC.ToString("yyyy-MM-dd HH"));
            }

            requestNew = true;
        }
        public override void Iterate()
        {
            // Setup base state/email sending variables parameters

            // Perform the base iteration
            base.Iterate();

            // This routine checks to see if a new dataset should be downloaded
            if (DateTime.UtcNow.CompareTo(waitUntil) > 0.0)
            {
                // Let's check to see if the last download is valid
                // For Windows, let's request data 6 hours earlier for downloads
                var cRequestDate = GlobalDataServices.GetCurrentDownloadDate(ConfluxOperatingSystem.IsWindows());

                if (lastDownload == null || cRequestDate.CompareTo(lastDownload.DateUTC) != 0)
                {

                    //if(!stateFirst)
                    //{
                    //    if(cRequestDate.CompareTo(stateDatePrevious) != 0)
                    //    {
                    //        if(!stateDownloaded)
                    //        {
                    //            if(!stateSentMessage)
                    //            {
                    //                // We send the email if there was a problem with the dataset and this particular timeframe
                    //                DownloadDatasetError_SendEmail(stateDatePrevious);
                    //                stateSentMessage = true;
                    //            }
                    //        }
                    //        stateDatePrevious = cRequestDate;
                    //    }
                    //}

                    // Let's check how many hours have passed since the request date changed and we don't have data

                    if (requestNew)
                    {
                        logger.Info("Provider : " + ProviderDatasetName + " : Current Dataset To Download : " + cRequestDate.ToString("yyyy-MM-dd HH"));
                        requestNew = false;
                        stateLastChange = cRequestDate;                        
                    }


                    // We check to see if the download is available.  If so, we go into download mode
                    GlobalDataDownload descarga = new GlobalDataDownload();

                    var respuestaDescargaFTP = DownloadDatasetFTP(cRequestDate);
                    var cSet = respuestaDescargaFTP.Item1;//almaceno el valor de la descarga, ya sea null o el objeto
                    try
                    {
                        if (cSet != null && ( respuestaDescargaFTP.Item2 == true || contadorFtp < 3 ))
                            //si es que tengo la respuesta null
                            //y tuve un error en la descarga. intentare descargar por https
                            //o si es que consulto mas de 3 veces por el servidor ftp 
                            cSet = DownloadDataset(cRequestDate);
                    }
                    catch(Exception ex)
                    {
                        cSet = null;
                        logger.Error(ex, "Error al intentar Hacer la descarga por FTP y HTTPS.");
                    }
                    
                    if (cSet == null)
                    {
                        if (DateTime.UtcNow.Subtract(stateLastChange).TotalHours > 6.0 && agentState == GlobalAgentState.Normal)
                        {
                            // We now don't have data, we need to change state and send an email.
                            agentState = GlobalAgentState.InitialStop;
                            DownloadDatasetError_SendEmail();
                        }
                        else if (agentState == GlobalAgentState.InitialStop)
                        {
                            if (DateTime.UtcNow.Hour > 12)
                                agentState = GlobalAgentState.Wait8pmStop;
                            else
                                agentState = GlobalAgentState.Wait8amStop;
                        }
                        else if (agentState == GlobalAgentState.Wait8pmStop)
                        {
                            if (DateTime.UtcNow.Hour < 12)
                            {
                                DownloadDatasetError_SendEmail();
                                agentState = GlobalAgentState.Wait8amStop;
                            }
                        }
                        else if (agentState == GlobalAgentState.Wait8amStop)
                        {
                            if (DateTime.UtcNow.Hour > 11)
                            {
                                DownloadDatasetError_SendEmail();
                                agentState = GlobalAgentState.Wait8pmStop;
                            }
                        }

                        // If not, then we begin checking every 5 minutes.
                        int waitMin = 5; // 5-Minute default wait kick time
                        if (lastDownload != null)
                        {
                            int deltaMin = Convert.ToInt32(DateTime.UtcNow.Subtract(lastDownload.DownloadStart).TotalMinutes);
                            if (deltaMin < 3 * 60)
                                waitMin = 10;
                            else
                                waitMin = 5;
                        }
                        waitUntil = DateTime.UtcNow.AddMinutes(waitMin);
                    }
                    else
                    {
                        contadorFtp = 0;
                        logger.Info("Descarga realizada, contador de error ftp reiniciado");
                        lastDownload = cSet;

                        if (agentState != GlobalAgentState.Normal)
                        {
                            agentState = GlobalAgentState.Normal;
                            DownloadDatasetError_SendEmail();
                            stateLastChange = cRequestDate;
                        }

                        int waitMin = 60; // Let's wait 60 minutes before asking again
                        waitUntil = DateTime.UtcNow.AddMinutes(waitMin);
                        requestNew = true;
                    }

                }

            }
        }
        public void DownloadDatasetError_SendEmail()
        {
            try
            {
                string FromAddress = mainConfig.ServerMail.SenderEmailAddress;
                string FromAdressTitle = mainConfig.ServerMail.SenderEmailName;

                #region Se aplica la propiedad TAG en el correo de error de descarga.

                //double percent = 100.0 * fluentFtpCurrentFile / (fluentFtpTotalFiles * 1.0);

                //To Address 
                string ToAddress = mainConfig.ServerMail.AdminErrorRecipients;

                string Subject = "";
                string BodyContent = "";

                if(agentState == GlobalAgentState.InitialStop)
                {
                    Subject = "COS2 : GlobalData " + mainConfig.Acquisition.Tag + " : Error de Bajada de Datos WRF (Alerta Inicial) : " + stateLastChange.ToString("yyyy-MM-dd HH");
                    BodyContent =
                        "COS2 : GlobalData " + mainConfig.Acquisition.Tag + " : Error de Bajada de Datos WRF (Alerta Inicial)\r\n" +
                        "\r\n" +
                        "-------------------------------------------------------------------------------- \r\n" +
                        "Proveedor de Datos         : " + domain.Provider + "\r\n" +
                        "Fecha Datos Faltantes(UTC) : " + stateLastChange.ToString("yyyy-MM-dd HH") + "\r\n" +
                        "Tiempo sin Datos (h)       : " + DateTime.UtcNow.Subtract(stateLastChange).TotalHours.ToString("F1") + "\r\n" +
                        "-------------------------------------------------------------------------------- \r\n" +
                        "\r\n" +
                        "TSG Environmental";
                }
                else if (agentState == GlobalAgentState.Wait8amStop)
                {
                    Subject = "COS2 : GlobalData " + mainConfig.Acquisition.Tag + " : Error de Bajada de Datos WRF (Alerta 8am) : " + stateLastChange.ToString("yyyy-MM-dd HH");
                    BodyContent =
                        "COS2 : GlobalData " + mainConfig.Acquisition.Tag + " : Error de Bajada de Datos WRF (Alerta 8am)\r\n" +
                        "\r\n" +
                        "-------------------------------------------------------------------------------- \r\n" +
                        "Proveedor de Datos         : " + domain.Provider + "\r\n" +
                        "Fecha Datos Faltantes(UTC) : " + stateLastChange.ToString("yyyy-MM-dd HH") + "\r\n" +
                        "Tiempo sin Datos (h)       : " + DateTime.UtcNow.Subtract(stateLastChange).TotalHours.ToString("F1") + "\r\n" +
                        "-------------------------------------------------------------------------------- \r\n" +
                        "\r\n" +
                        "TSG Environmental";
                }
                else if (agentState == GlobalAgentState.Wait8pmStop)
                {
                    Subject = "COS2 : GlobalData " + mainConfig.Acquisition.Tag + " : Error de Bajada de Datos WRF (Alerta 8pm) : " + stateLastChange.ToString("yyyy-MM-dd HH");
                    BodyContent =
                        "COS2 : GlobalData " + mainConfig.Acquisition.Tag + " : Error de Bajada de Datos WRF (Alerta 8pm)\r\n" +
                        "\r\n" +
                        "-------------------------------------------------------------------------------- \r\n" +
                        "Proveedor de Datos         : " + domain.Provider + "\r\n" +
                        "Fecha Datos Faltantes(UTC) : " + stateLastChange.ToString("yyyy-MM-dd HH") + "\r\n" +
                        "Tiempo sin Datos (h)       : " + DateTime.UtcNow.Subtract(stateLastChange).TotalHours.ToString("F1") + "\r\n" +
                        "-------------------------------------------------------------------------------- \r\n" +
                        "\r\n" +
                        "TSG Environmental";
                }
                else if (agentState == GlobalAgentState.Normal)
                {
                    Subject = "COS2 : GlobalData " + mainConfig.Acquisition.Tag + " : Datos WRF Restaurados : " + lastDownload.DateUTC.ToString("yyyy-MM-dd HH");
                    BodyContent =
                        "COS2 : GlobalData " + mainConfig.Acquisition.Tag + " : Reanudación Datos WRF\r\n" +
                        "\r\n" +
                        "-------------------------------------------------------------------------------- \r\n" +
                        "Proveedor de Datos       : " + domain.Provider + "\r\n" +
                        "Fecha Datos Nuevos (UTC) : " + stateLastChange.ToString("yyyy-MM-dd HH") + "\r\n" +
                        "Tiempo sin Datos (h)     : " + DateTime.UtcNow.Subtract(stateLastChange).TotalHours.ToString("F1") + "\r\n" +
                        "-------------------------------------------------------------------------------- \r\n" +
                        "\r\n" +
                        "TSG Environmental";
                }

                #endregion

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
                        logger.Info("Dataset : SendEmail : ", "WRF Data Download Error - Sending mail to : [" + item + "]");
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
                logger.Error(ex, "Dataset : SendEmail : Could not send email");
            }

        }
        public override void Cleanup()
        {            
            // Perform the cleanup after a successful download
            // This routine obtains all records that don't have FilesDeleted = true, and skips all record for the last 4 days
            logger.Info("Provider : " + ProviderDatasetName + " : Clearing unused datasets from local filesystem");

            using var session = main.AcquireSession();
            // Obtain all downloads that have files still
            var dynList = session.Get<GlobalDataDownload>().GlobalData_CompletedDownloadWithFiles(domain.Id).ListObtainVirtual();

            // Obtain the current operational date, and subtract 2 days.  Anything older is useless to Modeler subsystems.
            var cValidDate = GlobalDataServices.GetCurrentDownloadDate(ConfluxOperatingSystem.IsWindows()).AddDays(-2);

            // Go through the list and see if any of the records should be purged (if their date is too old)
            foreach (var dyn in dynList)
            {
                var d = (GlobalDataDownload)dyn;

                if (d.DateUTC.CompareTo(cValidDate) < 1)
                {
                    // This dataset is too old, we can safely delete the file data
                    // First, update the record
                    logger.Info("Provider : " + ProviderDatasetName + " : Clearing files for : " + d.DateUTC.ToString("yyyy-MM-dd HH"));
                    d.FilesDeleted = true;
                    session.UpdateNoHistory(d).Execute();

                    // And now we delete the associated folder data
                    ClearLocalDir(GetBaseFolderForDate(d.DateUTC));
                }
            }
        }
        public (GlobalDataDownload, bool) DownloadDatasetFTP(DateTime cDate)
        {
            //informaremos un intento de descarga a traves de ftp.
            logger.Info("----Comenzando intento de descarga a traves de FTP.---");
            // cDate is a UTC Date containing the download requested.
            // This routine does all that's needed to download a set.  If the set doesn't exist yet, it returns null.
            // If the set is partially there, this routine does it's own waiting and only exits once the download is complete
            // or has failed spectacularly.

            bool descargaFallida = false;

            GlobalDataDownload downloadSet = new GlobalDataDownload()
            {
                DateUTC = cDate,
                YearUTC = cDate.Year,
                MonthUTC = cDate.Month,
                DayUTC = cDate.Day,
                HourUTC = cDate.Hour,
                Code = "",
                DeltaHours = datasetConfig.NumHours,
                GlobalDomainId = domain.Id,
                DownloadedFiles = datasetConfig.NumHours+1,
                DownloadStart = DateTime.UtcNow,
                DownloadSize = 0,
                DownloadSpeed = 0.0,
                PercentDone = 100.0,
                ExpectedFiles = datasetConfig.NumHours+1,
                Status = DownloadStatus.Downloading,
                FilesDeleted = false,
            };
            // First, we check to see if there is a download available



            try
            {
                logger.Info("Dataset : " + cDate.ToString("yyyy-MM-dd HH") + " : Checking Files ");

                // This version uses FluentFTP to actively perform a connection (when needed) and test folder/filenames
                // We prepare the local full and trim folders right away in this version

                // First, we check to see if files are available
                if (FluentFilesReady(cDate))
                {
                    // First, clear the last download if needed
                    ClearDownload(cDate);

                    // Now create the work folders
                    string sfolderOrig = GetBaseFolderForDate(cDate) + @"/orig";                    
                    string sfolderTrim = GetBaseFolderForDate(cDate) + domain.GeoDomain;
                    if (PrepareLocalDir(sfolderOrig) && PrepareLocalDir(sfolderTrim))
                    {
                        // In this version, we add the record once we're done                    
                        if (FluentDownloadFiles(cDate, sfolderOrig))
                        {
                            // Trim files
                            logger.Info("Dataset : " + cDate.ToString("yyyy-MM-dd HH") + " : Downloaded Successfully");
                            for (int i = 0; i <= datasetConfig.NumHours; i++)
                            {
                                logger.Info("Dataset : " + cDate.ToString("yyyy-MM-dd HH") + " : Trimming File : " + i.ToString());
                                PostProcessGeoDomain(sfolderTrim, fluentFtpFiles[i]);
                            }

                            logger.Info("Dataset : " + cDate.ToString("yyyy-MM-dd HH") + " : Packing Files");
                            PackDownload(sfolderTrim);

                            downloadSet.Status = DownloadStatus.Complete;
                            downloadSet.PercentDone = 100;
                            downloadSet.DownloadEnd = DateTime.UtcNow;
                            downloadSet.DownloadSize = fluentFtpTotalSize;
                            downloadSet.DownloadSeconds = downloadSet.DownloadEnd.Subtract(downloadSet.DownloadStart).TotalSeconds;
                            downloadSet.DownloadSpeed = ((double)downloadSet.DownloadSize / downloadSet.DownloadSeconds) / (1024.0 * 1024.0);
                            downloadSet.FilesDeleted = false;

                            logger.Info("Dataset : " + cDate.ToString("yyyy-MM-dd HH") + " : Processed GeoDomain");


                            try
                            {
                                downloadSet.DownloadEnd = DateTime.UtcNow;
                                downloadSet.DownloadSeconds = downloadSet.DownloadEnd.Subtract(downloadSet.DownloadStart).TotalSeconds;

                                using (var session = main.AcquireSession())
                                {
                                    session.CreateNoHistory(downloadSet).Execute();
                                }

                                logger.Info("Dataset : " + cDate.ToString("yyyy-MM-dd HH") + " : Published");
                               //descargaFallida = false;
                                Cleanup();
                            }
                            catch (Exception ex)
                            {
                                logger.Error(ex, "Dataset : could not update download record to db.");
                            }
                        }
                        else
                        {

                            logger.Info("Dataset : " + cDate.ToString("yyyy-MM-dd HH") + " : Could not be Downloaded");

                            downloadSet.PercentDone = 100.0 * fluentFtpCurrentFile / (fluentFtpTotalFiles * 1.0);

                            logger.Info("Dataset : " + cDate.ToString("yyyy-MM-dd HH") + " : Download errors, will retry shortly");
                            logger.Info("Dataset : " + cDate.ToString("yyyy-MM-dd HH") + " : Removing partial download record");


                            // Delete download completely
                            ClearDownload(cDate);
                            descargaFallida = true;
                        }
                    }
                    else
                    {
                        logger.Error("Dataset : Could not prepare output folders for downloading");
                        downloadSet = null;
                    }
                }
                else
                {
                    downloadSet = null;
                }
            }
            catch (Exception ex)
            {
                downloadSet = null;
                logger.Error(ex, "FluentFtp : Severe Error : Could not operate or download");
                fluentFtpFlagDownloadError = true;
            }
            return (downloadSet, descargaFallida);
        }
        public bool FluentFilesReady(DateTime cdate)
        {
            bool filesReady = false;

            fluentFtpFlagDownloadError = false;
            fluentFtpCurrentFile = 0;
            fluentFtpTotalFiles = datasetConfig.NumHours+1;
            fluentFtpFiles = new List<string>();

            try
            {
                string sdate = cdate.ToString("yyyyMMdd");
                string shour = cdate.Hour.ToString("D2");
                string spath = @"/pub/data/nccf/com/gfs/prod/gfs." + sdate + @"/" + shour + @"/atmos/";
                string baseUrl = mainConfig.Acquisition.FtpServer; // @"ftp.ncep.noaa.gov";
                FtpClient client = new FtpClient(baseUrl);

                try
                {
                    client.Connect();

                    // ftp.ncep.noaa.gov
                    // /pub/data/nccf/com/gfs/prod/gfs.20210504/06/atmos/
                    // /pub/data/nccf/com/gfs/prod/gfs.[DATE]/[HOUR]/atmos/
                    // gfs.t06z.pgrb2.0p25.f082                    

                    if (client.DirectoryExists(spath))//aqui sabre si hay archivos o carpetas relacionadas a ese timestap
                    {
                        // Check to see if the first and last file exist
                        client.DataConnectionType = FtpDataConnectionType.AutoPassive;
                        client.SetWorkingDirectory(spath);

                        bool filesExist = true;
                        int nfiles = 0;
                        for (int i = 0; i <= datasetConfig.NumHours; i++)
                        {
                            fluentFtpFiles.Add("gfs.t" + shour + "z.pgrb2.0p25.f" + i.ToString("D3"));
                            if (!client.FileExists(fluentFtpFiles[i]))
                                filesExist = false;
                            else
                            {
                                nfiles++;
                                Thread.Sleep(1000);
                            }
                        }
                        if (filesExist)
                        {
                            filesReady = true;
                            logger.Info("FluentFtp Dataset : " + cdate.ToString("yyyy-MM-dd HH") + " : Files Ready for Download");
                        }
                        else
                        {
                            logger.Info("FluentFtp Dataset : " + cdate.ToString("yyyy-MM-dd HH") + " : Files Not Ready : (Only " + nfiles + " available)");
                        }
                    }
                    else
                    {
                        logger.Info("FluentFtp Dataset : " + cdate.ToString("yyyy-MM-dd HH") + " : Directory not available yet");
                        client.Disconnect();
                    }
                }
                catch (Exception ex)
                {
                    logger.Warn("FluentFtp : Could not verify DataSet : " + sdate + " " + shour);
                    logger.Warn($"FluentFtp : intento numero: {contadorFtp} fallo al descargar por ftp");
                    fluentFtpFlagDownloadError = true;//aqui es porque no puede descargar ftp
                    contadorFtp++;
                }

                if (client.IsConnected)
                    client.Disconnect();
                client.Dispose();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "FluentFtp : Severe Error : Could not operate");
                fluentFtpFlagDownloadError = true;
            }

            return filesReady;
        }
        public bool FluentDownloadFiles(DateTime cdate, string outpath)
        {
            bool downloadOk = false;
            fluentFtpFlagDownloadError = false;
            fluentFtpTotalFiles = datasetConfig.NumHours+1;
            fluentFtpCurrentFile = 0;
            fluentFtpMaxSpeed = 0.0;

            try
            {
                string sdate = cdate.ToString("yyyyMMdd");
                string shour = cdate.Hour.ToString("D2");
                string spath = @"/pub/data/nccf/com/gfs/prod/gfs." + sdate + @"/" + shour + @"/atmos/";
                string baseUrl = mainConfig.Acquisition.FtpServer; // @"ftp.ncep.noaa.gov";
                FtpClient client = new FtpClient(baseUrl);

                try
                {
                    client.Connect();
                    // ftp.ncep.noaa.gov
                    // /pub/data/nccf/com/gfs/prod/gfs.20210504/06/atmos/
                    // /pub/data/nccf/com/gfs/prod/gfs.[DATE]/[HOUR]/atmos/
                    // gfs.t06z.pgrb2.0p25.f082

                    // Check to see if the first and last file exist
                    client.DataConnectionType = FtpDataConnectionType.AutoPassive;
                    client.SetWorkingDirectory(spath);
                    fluentFtpTotalSize = 0;

                    bool downOk = true;
                    for (int i = 0; i < fluentFtpFiles.Count && downOk; i++)
                    {
                        bool fileOk = false;
                        int ntries = 5;
                        while (ntries > 0 && !fileOk)
                        {
                            try
                            {
                                if (!client.IsConnected)
                                {
                                    client.Connect();
                                    client.DataConnectionType = FtpDataConnectionType.AutoPassive;
                                    client.SetWorkingDirectory(spath);
                                }
                                var status = client.DownloadFile(outpath + @"/" + fluentFtpFiles[i], fluentFtpFiles[i], FtpLocalExists.Append, FtpVerify.Retry);
                                if (status == FtpStatus.Success)
                                {
                                    fluentFtpTotalSize += (new FileInfo(outpath + @"/" + fluentFtpFiles[i])).Length;
                                    fileOk = true;
                                    fluentFtpCurrentFile++;
                                    logger.Info("FluentFTP : Download Complete : " + fluentFtpFiles[i]);
                                }
                                else
                                {
                                    logger.Info("FluentFTP : Download Error, Retrying : " + fluentFtpFiles[i]);
                                    ntries--;
                                    Thread.Sleep(2000);
                                }
                            }
                            catch (Exception ex)
                            {
                                logger.Error(ex, "FluentFtp : Download Error, Retrying : " + fluentFtpFiles[i]);
                                ntries--;
                                Thread.Sleep(2000);
                            }
                        }
                        if (!fileOk)
                            downOk = false;
                    }
                    if (client.IsConnected)
                        client.Disconnect();
                    if (downOk)
                    {
                        // We process the last file                        
                        logger.Info("FluentFTP : Download Complete");

                        // We're successful         
                        downloadOk = true;
                    }
                    else
                    {
                        logger.Info("FluentFTP : Download Incomplete");
                        fluentFtpFlagDownloadError = true;
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "FluentFtp : Could not download DataSet : " + sdate + " " + shour);
                    fluentFtpFlagDownloadError = true;
                }

                if (client.IsConnected)
                    client.Disconnect();
                client.Dispose();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "FluentFtp : Severe Error : Could not operate");
                fluentFtpFlagDownloadError = true;
            }
            return downloadOk;
        }
        public override GlobalDataDownload DownloadDataset(DateTime cDate)
        {
            //avisaremos a la consola que estamos comenzando un intento de descarga por https
            logger.Info("---Comenzando intento de descarga a traves de metodo https.---");
            // cDate is a UTC Date containing the download requested.
            // This routine does all that's needed to download a set.  If the set doesn't exist yet, it returns null.
            // If the set is partially there, this routine does it's own waiting and only exits once the download is complete
            // or has failed spectacularly.

            GlobalDataDownload downloadSet = null;

            // First, we check to see if there is a download available

            try
            {
                if (WebfileDownloader.FileAvailable(GetFilename(cDate, 0), GetFilenameOnly(cDate, 0)))
                {
                    // Jackpot, we have data available
                    // 1 : Approach 1.0
                    //   : We attempt to download each file one by one.  If file download is not successful,
                    //   : we try to download the parts that weren't properly loaded again.

                    // First, setup the download folder
                    // Local storage folder setup

                    string sfolderOrig = GetBaseFolderForDate(cDate) + @"/orig";
                    string sfolderTrim = GetBaseFolderForDate(cDate) + domain.GeoDomain;

                    // First, clear the last download if needed
                    ClearDownload(cDate);

                    // Now create the work folders
                    if (PrepareLocalDir(sfolderOrig) && PrepareLocalDir(sfolderTrim))
                    {
                        // And let's add the current download
                        downloadSet = new GlobalDataDownload()
                        {
                            DateUTC = cDate,
                            YearUTC = cDate.Year,
                            MonthUTC = cDate.Month,
                            DayUTC = cDate.Day,
                            HourUTC = cDate.Hour,
                            Code = "",
                            DeltaHours = datasetConfig.NumHours,
                            GlobalDomainId = domain.Id,
                            DownloadedFiles = 0,
                            DownloadStart = DateTime.UtcNow,
                            DownloadSize = 0,
                            DownloadSpeed = 0.0,
                            PercentDone = 0.0,
                            ExpectedFiles = datasetConfig.NumHours + 1,
                            Status = DownloadStatus.Downloading,
                            FilesDeleted = false,
                        };

                        using (var session = main.AcquireSession())
                        {
                            session.CreateNoHistory(downloadSet).Execute();
                        }

                        // Second, iterate through the numhours
                        logger.Info("Dataset : " + cDate.ToString("yyyy-MM-dd HH") + " : Downloading " + downloadSet.ExpectedFiles + " files");

                        bool errorSet = false;
                        for (int i = 0; i <= datasetConfig.NumHours && !errorSet; i++)
                        {
                            // We define a set amount of retries per each file
                            // If we pass those retries, then we have an error in the complete set and need to retry later
                            int nRetries = 0;
                            int totRetries = 10;
                            bool fileOk = false;
                            WebfileDownloadResult res = null;

                            while (!fileOk && nRetries < totRetries)
                            {
                                // Check file availability
                                if (WebfileDownloader.FileAvailable(GetFilename(cDate, i + 1), GetFilenameOnly(cDate, i + 1), false))
                                {
                                    string sfilenm = GetFilename(cDate, i);
                                    string sfilenmShort = GetFilenameOnly(cDate, i);
                                    res = WebfileDownloader.Downloadv2(sfilenm, sfilenmShort, sfolderOrig + @"/", mainConfig.Acquisition.Concurrency, false);
                                    if (!res.Error)
                                    {
                                        fileOk = true;
                                    }
                                    else
                                    {
                                        logger.Warn("Dataset : Download Failed.  Waiting one minute for retry");
                                    }
                                }

                                if (!fileOk)
                                {
                                    nRetries++;
                                    if (nRetries < totRetries)
                                        Thread.Sleep(60000);
                                }
                            }

                            errorSet = errorSet || !fileOk;
                            if (!errorSet)
                            {
                                // Post process the file
                                PostProcessGeoDomain(sfolderTrim, GetFilenameOnly(cDate, i));

                                // Update the database record
                                try
                                {
                                    downloadSet.PercentDone = 100.0 * ((i + 1) * 1.0 / (datasetConfig.NumHours + 1) * 1.0);
                                    downloadSet.DownloadedFiles += 1;
                                    downloadSet.DownloadSize += res.Size;
                                    downloadSet.DownloadSeconds = DateTime.UtcNow.Subtract(downloadSet.DownloadStart).TotalSeconds;
                                    downloadSet.DownloadSpeed = (downloadSet.DownloadSize / (1024.0 * 1024.0)) / (DateTime.UtcNow.Subtract(downloadSet.DownloadStart).TotalSeconds);
                                    downloadSet.Status = DownloadStatus.Downloading;
                                    downloadSet.DownloadEnd = DateTime.UtcNow;

                                    using var session = main.AcquireSession();
                                    session.UpdateNoHistory(downloadSet).Execute();
                                }
                                catch (Exception ex)
                                {
                                    logger.Error(ex, "Dataset : could not update download record in db.");
                                    errorSet = true;
                                }
                            }
                            else
                            {
                                errorSet = true;
                            }
                        }

                        if (!errorSet)
                        {
                            logger.Info("Dataset : " + cDate.ToString("yyyy-MM-dd HH") + " : Downloaded Successfully");
                            try
                            {
                                logger.Info("Dataset : " + cDate.ToString("yyyy-MM-dd HH") + " : Packing Files");
                                PackDownload(sfolderTrim);

                                downloadSet.Status = DownloadStatus.Complete;
                                downloadSet.DownloadEnd = DateTime.UtcNow;
                                downloadSet.DownloadSeconds = downloadSet.DownloadEnd.Subtract(downloadSet.DownloadStart).TotalSeconds;
                                downloadSet.DownloadSpeed = ((double)downloadSet.DownloadSize / downloadSet.DownloadSeconds) / (1024.0 * 1024.0);
                                downloadSet.FilesDeleted = false;

                                using (var session = main.AcquireSession())
                                {
                                    session.UpdateNoHistory(downloadSet).Execute();
                                }

                                logger.Info("Dataset : " + cDate.ToString("yyyy-MM-dd HH") + " : Published");
                                downloadSet = null;
                                Cleanup();
                            }
                            catch (Exception ex)
                            {
                                logger.Error(ex, "Dataset : could not update download record to db.");
                            }
                        }
                        else
                        {
                            logger.Info("Dataset : " + cDate.ToString("yyyy-MM-dd HH") + " : Could not be Downloaded");

                            DownloadDatasetError_SendEmail();

                            logger.Info("Dataset : " + cDate.ToString("yyyy-MM-dd HH") + " : Download errors, will retry shortly");
                            logger.Info("Dataset : " + cDate.ToString("yyyy-MM-dd HH") + " : Removing partial download record");


                            // Delete download completely
                            ClearDownload(cDate);
                            downloadSet = null;
                        }


                        // Save the dataset                        
                    }
                    else
                    {
                        logger.Error("Dataset : Could not prepare output folders for downloading");
                    }
                }
            }
            catch
            {

            }

            return downloadSet;
        }
        public void PostProcessGeoDomain(string folderdomain, string filenm)
        {
            if (ConfluxOperatingSystem.IsWindows())
            {
                // We don't post-process in Windows, at least not at this time

                // wgrib2 gfs.t00z.pgrb2.0p25.f001 -small_grib -100:-30 -70:0 gfs.t00z.pgrb2.0p25.f001.sub
                using StreamWriter sw = new StreamWriter(new FileStream(folderdomain + @"/trim.sh", FileMode.Create));
                sw.WriteLine("cd " + folderdomain);
                sw.WriteLine("/usr/bin/wgrib2 ../orig/" + filenm + " -small_grib -100:-30 -70:0 " + filenm + ".sub > wgrib2.lst");
                sw.WriteLine("rm ../orig/" + filenm + " wgrib2.lst");
            }
            else
            {
                // TO-DO : This routine uses an old processing method.  We need to migrate to new version

                // wgrib2 gfs.t00z.pgrb2.0p25.f001 -small_grib -100:-30 -70:0 gfs.t00z.pgrb2.0p25.f001.sub
                using (StreamWriter sw = new StreamWriter(new FileStream(folderdomain + @"/trim.sh", FileMode.Create)))
                {
                    sw.WriteLine("cd " + folderdomain);
                    sw.WriteLine("/usr/bin/wgrib2 ../orig/" + filenm + " -small_grib -100:-30 -70:0 " + filenm + ".sub > wgrib2.lst");
                    sw.WriteLine("rm ../orig/" + filenm + " wgrib2.lst");
                }

                try
                {
                    Process proc = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "chmod",
                            Arguments = "oug+rx trim.sh",
                            WorkingDirectory = folderdomain,
                            //UseShellExecute = true,
                            RedirectStandardOutput = false,
                            RedirectStandardError = false,
                            CreateNoWindow = true
                        }
                    };

                    proc.Start();
                    proc.WaitForExit();
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "PostProcessGeoDomain : could not run chmod on trim.sh.");
                }

                // Print and get current working directory
                string currdir = Directory.GetCurrentDirectory();
                Directory.SetCurrentDirectory(folderdomain);

                try
                {
                    Process proc = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "/bin/sh",
                            Arguments = "trim.sh",
                            WorkingDirectory = folderdomain,
                            UseShellExecute = false,
                            RedirectStandardOutput = false,
                            RedirectStandardError = false,
                            CreateNoWindow = true,
                        }
                    };
                    proc.Start();
                    proc.WaitForExit();
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "PostProcessGeoDomain : could not run wgrib2 on the end file.");
                }

                Directory.SetCurrentDirectory(currdir);
            }
        }
        // ==========================================================================
        // Date / Filename Methods
        // ==========================================================================
        public void ClearDownload(DateTime cDate)
        {
            using var session = main.AcquireSession();

            var down = session.Get<GlobalDataDownload>().GlobalData_DownloadForDate(domain.Id, cDate).ObtainVirtual();
            if (down != null)
            {
                session.DeleteNoHistory(down).Execute();
                ClearLocalDir(GetBaseFolderForDate(cDate));
                logger.Info("Existing Dataset : " + cDate.ToString("yyyy-MM-dd HH") + " : Deleted Successfully");
            }
            else
            {
                ClearLocalDir(GetBaseFolderForDate(cDate), false);
            }
        }

        public string GetBaseFolderForDate(DateTime cDate)
        {
            return ConfluxManager.GetStorageDirectory() + @"GlobalData/" + ProviderDatasetName
                        + @"/" + cDate.ToString("yyyyMMdd_HH") + @"/";
        }
    }
}
