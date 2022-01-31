using CliWrap;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Conflux.Helpers
{
    public static class ConfluxOperatingSystem
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public static bool IsWindows() =>
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        public static bool IsMacOS() =>
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        public static bool IsLinux() =>
            RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

        public static string ObtainIP()
        {
            var ipString = "nodata";

            try
            {
                var stdOutBuffer = new StringBuilder();
                var cmd = Cli.Wrap("hostname")
                             .WithArguments("-I")
                             .WithValidation(CommandResultValidation.None) | stdOutBuffer;
                
                cmd.ExecuteAsync().GetAwaiter().GetResult();
                var w = stdOutBuffer.ToString().Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                ipString = w[0];
                logger.Info("OperatingSystem : Obtained IP : " + ipString);
            }
            catch(Exception ex)
            {
                string errorMsg = "Conflux was unable to obtain the IP address from the operating system";
                logger.Error(ex, errorMsg);
            }

            return ipString;
        }
    }
}
