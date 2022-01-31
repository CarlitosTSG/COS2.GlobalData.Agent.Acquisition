using Conflux.Components.WebApi.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Conflux.Components.WebApi
{
    public static class ConfluxWebUserManagement
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public static WxLoginInfo PIN_GenerateUser(string token)
        {
            logger.Debug("JWT:" + token);

            // This simple class generates a generic logininfo object for PIN based logins
            return new WxLoginInfo()
            {
                Email = "",
                FullName = "Generic PIN User",
                Guid = "",
                Id = 0,
                Role = "PIN",
                ShortName = "Generic",
                UserClass = "",
                Token = token,
            };
        }
    }
}
