using Conflux.Components.WebApi.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Conflux.Components.WebApi
{
    public interface IConfluxWebServices
    {
        public WxResponse ProcessLogin(WxRequest loginRequest);
        public WxResponse ProcessExternal(WxRequest loginRequest);
        public WxResponse Process(WxRequest loginRequest);        
    }
}
