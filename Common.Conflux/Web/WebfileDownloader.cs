using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Conflux.Web
{
    public class WebfileDownloadPart
    {
        public int index { get; set; }
        public long start { get; set; }
        public long end { get; set; }
        public int downStatus { get; set; }
        public string fileurl { get; set; }
        public string fileurlshort { get; set; }
        public string filename { get; set; }
        public DateTime dtStart { get; set; }
        public Thread t { get; set; }
    }

    public class WebfileDownloadResult
    {
        public bool Error { get; set; }
        public long Size { get; set; }
        public String FilePath { get; set; }
        public TimeSpan TimeTaken { get; set; }
        public int ParallelDownloads { get; set; }
    }

    public static class WebfileDownloader
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        static WebfileDownloader()
        {
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.DefaultConnectionLimit = 100;
            ServicePointManager.MaxServicePointIdleTime = 1000;
        }

        public static bool PrepareLocalDir(string sdir)
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

                // Log the error locally
                logger.Error(ex, errorMsg);                
            }
            return isOk;
        }

        public static bool FileAvailable(string fileUrl, string fileUrlShort, bool displayMessage = true)
        {
            bool isAvail = false;

            Task.Run(async () =>
            {

                using (var client = new HttpClient())
                {
                    try
                    {
                        client.Timeout = new TimeSpan(0, 0, 15);
                        HttpRequestMessage request =
                           new HttpRequestMessage(HttpMethod.Head, new Uri(fileUrl));

                        var r = await client.SendAsync(request);

                        var totalValueAsString = r.Content.Headers.SingleOrDefault(h => h.Key.Equals("Content-Length")).Value.First();

                        if (r.StatusCode == HttpStatusCode.OK)
                        {
                            isAvail = true;
                            if (displayMessage)
                                logger.Info("File availability check : " + fileUrlShort + " : file available / Size : " + totalValueAsString);
                        }
                        else
                        {
                            isAvail = false;
                            if (displayMessage)
                                logger.Info("File availability check : " + fileUrlShort + " : file missing or unavailable : " + r.StatusCode);
                        }
                    }
                    catch
                    {
                        isAvail = false;
                        if (displayMessage)
                            logger.Info("File availability check : " + fileUrlShort + " : file missing or unavailable");
                    }
                }


            }).GetAwaiter().GetResult();

            return isAvail;
        }

        public static WebfileDownloadResult Downloadv2(String fileUrl, String fileUrlShort, String destinationFolderPath, int numParts = 0, bool validateSSL = false)
        {
            Uri uri = new Uri(fileUrl);
            String destinationFilePath = Path.Combine(destinationFolderPath, uri.Segments.Last());
            WebfileDownloadResult result = new WebfileDownloadResult() { FilePath = destinationFilePath, Error = false };

            logger.Info("Download : File : " + fileUrlShort + " : Start");

            // Default - pretty awesome too
            if (numParts <= 0)
            {
                numParts = Environment.ProcessorCount;
            }

            // Obtain the file size
            long size = 0;

            Task.Run(async () =>
            {
                using (var client = new HttpClient())
                {
                    try
                    {
                        client.Timeout = new TimeSpan(0, 0, 15);
                        HttpRequestMessage request =
                           new HttpRequestMessage(HttpMethod.Head, new Uri(fileUrl));

                        var r = await client.SendAsync(request);
                        size = long.Parse(r.Content.Headers.SingleOrDefault(h => h.Key.Equals("Content-Length")).Value.First());

                        if (r.StatusCode == HttpStatusCode.OK)
                        {
                            logger.Info("Download : File : " + fileUrlShort + " : Size : " + (size / (1024.0 * 1024.0)).ToString("F4") + " Mb");
                        }
                        else
                        {
                            result.Error = true;
                            logger.Info("Download : File : " + fileUrlShort + " : file missing or unavailable : " + r.StatusCode);
                        }
                    }
                    catch
                    {
                        // In this case we don't do anything
                        result.Error = true;
                        logger.Info("Download : Network Error : Could not check file size");
                    }
                }
            }).GetAwaiter().GetResult();







            // Setup the download runs needed
            if (!result.Error)
            {
                result.Size = size;
                if (File.Exists(destinationFilePath))
                {
                    File.Delete(destinationFilePath);
                }

                // Create the download objects and pass on the relevant information
                List<WebfileDownloadPart> parts = new List<WebfileDownloadPart>();
                long chunkSize = size / numParts;
                for (int chunk = 0; chunk < numParts - 1; chunk++)
                {
                    var range = new WebfileDownloadPart()
                    {
                        index = chunk,
                        start = chunk * chunkSize,
                        end = (chunk + 1) * chunkSize - 1,
                        downStatus = 0, // Not downloading
                        fileurl = fileUrl,
                        fileurlshort = fileUrlShort,
                        filename = "",
                        t = null,
                    };
                    parts.Add(range);
                }
                parts.Add(new WebfileDownloadPart()
                {
                    index = numParts - 1,
                    start = parts.Any() ? parts.Last().end + 1 : 0,
                    end = size - 1,
                    downStatus = 0, // Not downloading
                    fileurl = fileUrl,
                    fileurlshort = fileUrlShort,
                    filename = "",
                    t = null,
                });


                // Do the main download loop
                int nErrors = 0;
                int nDownOk = 0;
                DateTime startTime = DateTime.Now;

                logger.Debug("Download : File : " + fileUrlShort + " : Ready to download in " + numParts + " parts");

                while (nDownOk < numParts && nErrors < numParts / 2)
                {
                    // Check to see if there is a download part that's in new state
                    // 0 : Ready to download
                    // 1 : Downloading
                    // 2 : Done
                    // 3 : Error
                    // 4 : Complete

                    int idx = -1;
                    for (int i = 0; i < parts.Count && idx < 0; i++)
                        if (parts[i].downStatus == 0)
                            idx = i;

                    // We have a new part to download
                    if (idx > -1)
                    {
                        var p = parts[idx];
                        var t = new Thread(() => DoDownloadPart(p));
                        parts[idx].t = t;
                        parts[idx].downStatus = 1;
                        parts[idx].dtStart = DateTime.Now;
                        logger.Debug("Download : File : " + fileUrlShort + " : Preparing to download part # " + (idx + 1));
                        t.Start();
                    }

                    // Check to see if there is a download part that's in an error state
                    idx = -1;
                    for (int i = 0; i < parts.Count && idx < 0; i++)
                        if (parts[i].downStatus == 3)
                            idx = i;

                    // We need to re-start this part
                    if (idx > -1)
                    {
                        nErrors++;
                        if (nErrors < numParts) // We still try again
                        {
                            var p = parts[idx];
                            var t = new Thread(() => DoDownloadPart(p));

                            parts[idx].t = t;
                            parts[idx].downStatus = 1;
                            parts[idx].dtStart = DateTime.Now;
                            logger.Debug("Download : File : " + fileUrlShort + " : Preparing to re-download part # " + (idx + 1));
                            t.Start();
                        }
                    }

                    // Check to see if there is a download part that's downloaded
                    idx = -1;
                    for (int i = 0; i < parts.Count && idx < 0; i++)
                        if (parts[i].downStatus == 2)
                            idx = i;

                    // We're done with this part
                    if (idx > -1)
                    {
                        parts[idx].downStatus = 4;
                        nDownOk++;
                        logger.Debug("Download : File : " + fileUrlShort + " : Part # " + (idx + 1) + " flagged as complete " + nDownOk + " / " + numParts);
                    }

                    // We also check to see if there are downloading threads
                    for (int i = 0; i < parts.Count && idx < 0; i++)
                        if (parts[i].downStatus == 1)
                        {
                            // Check the timing
                            if (DateTime.Now.Subtract(parts[i].dtStart).TotalMinutes > 15.0)
                            {
                                logger.Info("Download : File : " + fileUrlShort + " : Part # " + (i + 1) + " is taking too long, aborting");

                                parts[i].t.Abort();
                                parts[i].t = null;
                                parts[i].downStatus = 3;
                            }
                        }

                    // Sleep for 2 seconds
                    System.Threading.Thread.Sleep(1000);
                }





                if (nDownOk == numParts)
                {
                    result.TimeTaken = DateTime.Now.Subtract(startTime);
                    logger.Info("Download : File : " + fileUrlShort + " : Download Complete : " + result.TimeTaken.TotalSeconds.ToString("F2") + " seconds");
                    using (FileStream destinationStream = new FileStream(destinationFilePath, FileMode.Append))
                    {
                        for (int i = 0; i < parts.Count; i++)
                        {
                            byte[] tempFileBytes = File.ReadAllBytes(parts[i].filename);
                            destinationStream.Write(tempFileBytes, 0, tempFileBytes.Length);
                            File.Delete(parts[i].filename);
                        }
                    }
                    logger.Info("Download : File : " + fileUrlShort + " : Merge Complete");
                }
                else
                {
                    // We have a problem
                    logger.Info("Download : Network Error : File : " + fileUrlShort + " : Download Incomplete : " + result.TimeTaken.TotalSeconds.ToString("F2") + " seconds");
                    for (int i = 0; i < parts.Count; i++)
                    {
                        try
                        {
                            File.Delete(parts[i].filename);
                        }
                        finally
                        {
                            // Nothing to do
                        }
                    }
                    result.Error = true;
                }
            }
            return result;
        }






        public static void DoDownloadPart(WebfileDownloadPart d)
        {
            // Download the part D, we're inside a thread already
            logger.Debug("Download : File : " + d.fileurlshort + " : Download part # " + (d.index + 1));

            d.filename = Path.GetTempFileName();
            logger.Debug("Download : File : " + d.fileurlshort + " : DP # " + (d.index + 1) + " Temp File : " + d.filename);

            try
            {
                Task.Run(async () =>
                {
                    using (var client = new HttpClient())
                    {
                        try
                        {
                            client.Timeout = new TimeSpan(0, 0, 15);
                            client.DefaultRequestHeaders.Range = new RangeHeaderValue(d.start, d.end);

                            logger.Debug("Download : File : " + d.fileurlshort + " : DP # " + (d.index + 1) + " Block : " + d.start + " - " + d.end);

                            using (HttpResponseMessage response = await client.GetAsync(d.fileurl, HttpCompletionOption.ResponseHeadersRead))
                            {
                                logger.Debug("Download : File : " + d.fileurlshort + " : DP # " + (d.index + 1) + " Response Code : " + response.StatusCode);
                                using (Stream streamToReadFrom = await response.Content.ReadAsStreamAsync())
                                {
                                    string fileToWriteTo = d.filename;
                                    using (Stream streamToWriteTo = File.Open(fileToWriteTo, FileMode.Create))
                                    {
                                        await streamToReadFrom.CopyToAsync(streamToWriteTo);
                                    }
                                }
                                d.downStatus = 2;
                                logger.Debug("Download : File : " + d.fileurlshort + " : Download part # " + (d.index + 1) + " : Complete");
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Info("Download : Network Error : File : " + d.fileurlshort + " : Download part # " + (d.index + 1) + " : Download Error : " + ex.Message);
                            d.downStatus = 3;
                        }
                    }
                }).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                logger.Info("Download : Network Error : File : " + d.fileurlshort + " : Download part # " + (d.index + 1) + " : Download Error : " + ex.Message);
                d.downStatus = 3;
            }
        }

    }

}
