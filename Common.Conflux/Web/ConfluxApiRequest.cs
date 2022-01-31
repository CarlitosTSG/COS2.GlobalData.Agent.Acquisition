using Conflux.Components.WebApi.Model;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Conflux.Constants;
using Conflux.Database.Model;
using Conflux.Components.WebApi.Services;
using System.Linq;

namespace Common.Conflux.Web
{
    public class ConfluxApiRequest
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public string Hostname { get; set; }        
        public List<WxFilter> Filters { get; set; }
        public WxRequest request { get; set; }

        public ConfluxApiRequest(string hostname, string apiKey, WebRequestType requestType, string requestString)
        {
            // Hostname includes http or https
            Hostname = hostname;
            Filters = new List<WxFilter>();
            request = new WxRequest()
            {
                ApiKey = apiKey,
                RequestType = requestType,
                Request = requestString,
            };

        }

        public WxResponse SendRequest()
        {
            var response = new WxResponse();

            try
            {
                
                if (Filters.Count == 0)
                    request.Filters = null;
                else
                    request.Filters = Filters.ToArray();

                using var client = new HttpClient();
                client.BaseAddress = new Uri(Hostname + "/conflux/api/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var stringContent = new StringContent(JsonConvert.SerializeObject(request),Encoding.UTF8, "application/json");

                logger.Debug("ConfluxApiRequest : Sending request to : " + Hostname + " : " + request.RequestType + " : " + request.Request);

                HttpResponseMessage httpResponse = client.PostAsync("external", stringContent).Result;

                if(httpResponse.IsSuccessStatusCode)
                {
                    var json = httpResponse.Content.ReadAsStringAsync().Result;
                    response = JsonConvert.DeserializeObject<WxResponse>(json);
                    logger.Debug("ConfluxApiRequest : Response : " + response.Type+" : "+response.MessageTitle+" : "+response.MessageInfo);
                }
                else
                {
                    logger.Debug("ConfluxApiRequest : Response Error : " + httpResponse.StatusCode + " : " + httpResponse.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                string errorMsg = "Could not call an external Conflux API";
                logger.Error(ex, errorMsg);
            }


            return response;
        }

        public void AddValueToRequest(string entityKey, string value)
        {
            var w = new WxFilter()
            {
                Key = entityKey,
                Value = value,
            };
            Filters.Add(w);
        }


        public void AddEntityToRequest<T>(string entityKey, T entity) where T : DxBaseVirtualPersist
        {
            var w = new WxFilter()
            {
                Key = entityKey,
                Value = ConfluxWebServices.JsonConvertCamel(entity)
            };
            Filters.Add(w);
        }

    }
}
