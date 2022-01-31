using System;
using System.Collections.Generic;
using System.Text;

namespace Conflux.Components.WebApi.Model
{
    public class WxLoginInfo
    {
        public string UserClass { get; set; }
        public long Id { get; set; }
        public string Guid { get; set; }
        public string ShortName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string Token { get; set; }
    }
}
