using Conflux.Database;
using Conflux.Helpers;
using Conflux.Management;
using COS2.Core.Data.Model.WRF;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace GlobalData.Agent.Acquisition.Providers
{
    public abstract class GlobalDataProvider
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        // This class manages a GlobalDataProvider for NCEP GFS HiRes Data
        public const string ProviderDatasetName = "Generic";

        // Operational parameters
        public ConfluxDatabase main;
        public AcquisitionAgentConfiguration mainConfig;
        public DatasetAcquisitionConfiguration datasetConfig;
        public GlobalDataDomain domain;

        public bool requestNew;
        public DateTime waitUntil;
        public GlobalDataDownload lastDownload;

        public GlobalDataProvider()
        {
            main = ConfluxManager.ObtainDatabase();
            mainConfig = (ConfluxManager.ObtainConfiguration() as AcquisitionAgentConfiguration);
        }

        public virtual void Initialize()
        {
            // Generic initialization
        }

        public virtual void Iterate()
        {
            // Generic Iteration
        }

        public virtual void Cleanup()
        {
            // Generic Iteration
        }





































        // ==========================================================================
        // Main Overridden methods
        // ==========================================================================

        public virtual GlobalDataDownload DownloadDataset(DateTime cDate)
        {
            // This routine will be overloaded in actual providers
            return null;
        }

































        // ==========================================================================
        // External Processing
        // ==========================================================================

        public void PackDownload(string downloadFolder)
        {
            if (ConfluxOperatingSystem.IsWindows())
            {
                // We don't post-process in Windows, at least not at this time

                // This routine packs *sub files, ready for processing via COS2.
                using (StreamWriter sw = new StreamWriter(new FileStream(downloadFolder + @"/pack.sh", FileMode.Create)))
                {
                    sw.WriteLine("cd " + downloadFolder);
                    sw.WriteLine("tar cfvz wrfpack.tar.gz *.sub > tar.lst");
                    sw.WriteLine("rm tar.lst *.sub");
                }
            }
            else
            {
                // TO-DO : This routine uses an old processing method.  We need to migrate to new version
                // This routine packs *sub files, ready for processing via COS2.
                using (StreamWriter sw = new StreamWriter(new FileStream(downloadFolder + @"/pack.sh", FileMode.Create)))
                {
                    sw.WriteLine("cd " + downloadFolder);
                    sw.WriteLine("tar cfvz wrfpack.tar.gz *.sub > tar.lst");
                    sw.WriteLine("rm tar.lst *.sub");
                }

                // This routine post-processes the domain file
                try
                {
                    // wgrib2 gfs.t00z.pgrb2.0p25.f001 -small_grib -100:-30 -70:0 gfs.t00z.pgrb2.0p25.f001.sub
                    Process proc = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "/bin/sh",
                            Arguments = "pack.sh",
                            WorkingDirectory = downloadFolder,
                            UseShellExecute = false,
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
                    logger.Error(ex, "PackGeoDomain  : could not pack the .sub files.");
                }
            }
        }

























        // ==========================================================================
        // General Download Processing Routines
        // ==========================================================================



















        // ==========================================================================
        // Date / Filename Methods
        // ==========================================================================




        public string GetFilename(DateTime cDate, int forecastHour)
        {
            // http://nomads.ncep.noaa.gov/pub/data/nccf/com/gfs/prod/gfs.[DATE]/gfs.t[FCHR]z.pgrb2.0p25.f[RUNHR]
            // http://nomads.ncep.noaa.gov/pub/data/nccf/com/gfs/prod/gfs.20180715/12/gfs.t12z.pgrb2b.0p25.f080 

            // New date format for GFS v15
            string sdate = cDate.ToString("yyyyMMdd") + "/" + cDate.Hour.ToString("D2");

            return datasetConfig.URLTemplate.Replace("[DATE]", sdate)
                                            .Replace("[FCHR]", cDate.Hour.ToString("D2"))
                                            .Replace("[RUNHR]", forecastHour.ToString("D3"));
        }

        public string GetFilenameOnly(DateTime cDate, int forecastHour)
        {
            // http://nomads.ncep.noaa.gov/pub/data/nccf/com/gfs/prod/gfs.[DATE]/gfs.t[FCHR]z.pgrb2.0p25.f[RUNHR]
            // http://nomads.ncep.noaa.gov/pub/data/nccf/com/gfs/prod/gfs.2018071512/gfs.t12z.pgrb2b.0p25.f080 

            string s = GetFilename(cDate, forecastHour);
            int i = s.IndexOf("/gfs.t");
            s = s.Substring(i + 1);

            return s;
        }












        // ==========================================================================
        // Filesystem Utility Methods
        // ==========================================================================

        public bool PrepareLocalDir(string sdir)
        {
            bool isOk = false;
            try
            {
                if (!Directory.Exists(sdir))
                    Directory.CreateDirectory(sdir);
                isOk = true;
            }
            catch (Exception ex)
            {
                string errorMsg = "Could not create local data storage folder";
                logger.Error(ex, errorMsg);
            }
            return isOk;
        }

        public bool ClearLocalDir(string sdir, bool catchError = true)
        {
            bool isOk = !catchError;
            try
            {
                if (Directory.Exists(sdir))
                    Directory.Delete(sdir, true);
                isOk = true;
            }
            catch (Exception ex)
            {
                if (catchError)
                {
                    string errorMsg = "Could not delete local data storage folder";
                    logger.Error(ex, errorMsg);
                }
            }
            return isOk;
        }
    }
}
