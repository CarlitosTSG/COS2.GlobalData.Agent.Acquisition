using Conflux.Components.WebApi.Model;
using Conflux.Constants;
using Conflux.Database.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Conflux.Components.WebApi.Services
{
    public class ConfluxWebServices : IConfluxWebServices
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public virtual WxResponse Process(WxRequest loginRequest)
        {
            // Abstract class - this should be overriden
            return new WxResponse();
        }

        public virtual WxResponse ProcessExternal(WxRequest loginRequest)
        {
            // Abstract class - this should be overriden
            return new WxResponse();
        }

        public virtual WxResponse ProcessLogin(WxRequest loginRequest)
        {
            // Abstract class - this should be overriden
            return new WxResponse();
        }












        // =================================================================================
        // Utility Request Checks
        // =================================================================================

        public bool RequestCheck_ApiKey(WxRequest request, WxResponse response, string apikey)
        {
            try
            {
                if ( (response.Type != WebResponseType.SystemError && response.Type != WebResponseType.AppError) &&
                    ((request.ApiKey == null) ||  
                     (request.ApiKey.Trim() != apikey.Trim())) )
                {
                    SetSystemError(response, "Error de Sistema", "La llamada API es incorrecta o incompleta");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "There was an error in the WebApi filter processing routines");
                SetSystemError(response, "Error de Sistema ", "Error operacional WebApi.  Por favor contactarse con EnviroModeling");
            }

            return ((response.Type != WebResponseType.SystemError) || (response.Type != WebResponseType.AppError));
        }

        public bool RequestCheck_Type(WxRequest request, WxResponse response, WebRequestType wtype)
        {
            if ((response.Type != WebResponseType.SystemError && response.Type != WebResponseType.AppError) &&
                (request.RequestType != wtype))
            {
                SetSystemError(response, "Error de Sistema", "La llamada API es incompatible");
            }

            return (response.Type != WebResponseType.SystemError) || (response.Type != WebResponseType.AppError);
        }

        public bool RequestCheck_Request(WxRequest request, WxResponse response, string requestText)
        {
            if ((response.Type != WebResponseType.SystemError && response.Type != WebResponseType.AppError) &&
                (request.Request.Trim().ToLower() != requestText.Trim().ToLower()))
            {
                SetSystemError(response, "Error de Sistema", "La llamada API es incompatible");
            }

            return (response.Type != WebResponseType.SystemError) || (response.Type != WebResponseType.AppError);
        }


        public bool RequestCheck_FilterCount(WxRequest request, WxResponse response, int minfilter, int maxfilter)
        {
            try
            {
                if ((response.Type != WebResponseType.SystemError && response.Type != WebResponseType.AppError))
                {
                    bool filtersOk = true;

                    if (request.Filters == null)
                    {
                        if (minfilter > 0 || maxfilter > 0)
                            filtersOk = false;
                    }
                    else
                    {
                        if (minfilter > request.Filters.Length)
                            filtersOk = false;
                        if (maxfilter < request.Filters.Length && maxfilter != -1)
                            filtersOk = false;
                    }

                    if (!filtersOk)
                        SetSystemError(response, "Error de Sistema", "La llamada API no tiene la información necesaria");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "There was an error in the WebApi filter processing routines");
                SetSystemError(response, "Error de Sistema ", "Error operacional WebApi.  Por favor contactarse con EnviroModeling");
            }

            return (response.Type != WebResponseType.SystemError) || (response.Type != WebResponseType.AppError);
        }

        public bool RequestCheck_FilterType(WxRequest request, WxResponse response, int filteridx, string key)
        {
            try
            {
                if ((response.Type != WebResponseType.SystemError && response.Type != WebResponseType.AppError) &&
                    (request.Filters[filteridx - 1].Key.Trim().ToLower() != key.Trim().ToLower()))
                {
                    SetSystemError(response, "Error de Sistema", "La llamada API no tiene la información correcta");
                }
            }
            catch(Exception ex)
            {
                logger.Error(ex, "There was an error in the WebApi filter processing routines");
                SetSystemError(response, "Error de Sistema ", "Error operacional WebApi.  Por favor contactarse con EnviroModeling");
            }

            return (response.Type != WebResponseType.SystemError) || (response.Type != WebResponseType.AppError);
        }


        // =================================================================================
        // Filter processing routines
        // =================================================================================

        public bool Request_GetFilterValue(WxRequest request, WxResponse response,int filteridx, out string value)
        {
            value = null;
            try
            {
                if ( (response.Type != WebResponseType.SystemError && response.Type != WebResponseType.AppError) )
                {
                    value = request.Filters[filteridx - 1].Value;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "There was an error in the WebApi filter processing routines");

                // Generally, all errors from Conflux that go to a web api are in Spanish.  We could change that
                SetSystemError(response, "Error de Sistema", "Error operacional WebApi.  Por favor contactarse con EnviroModeling");
            }

            return (response.Type != WebResponseType.SystemError) || (response.Type != WebResponseType.AppError);
        }


































        // =================================================================================
        // Utility Routines for Responses
        // =================================================================================

        public T JsonDeserializeEntity<T>(WxResponse response, string key, string json) where T : DxBaseVirtualPersist
        {
            T vEntity = null;

            try
            {
                vEntity = JsonConvert.DeserializeObject<T>(json);
            }
            catch(Exception ex)
            {
                logger.Error(ex, "There was an error in the WebApi filter processing routines. JSON Invalid for : "+key);

                // Generally, all errors from Conflux that go to a web api are in Spanish.  We could change that
                SetSystemError(response, "Error de Sistema", "Error operacional WebApi.  Por favor contactarse con EnviroModeling");
            }

            return vEntity;
        }

        

        public void SetSystemError(WxResponse response, string title, string info, string additional)
        {
            response.Type = WebResponseType.SystemError;
            response.MessageTitle = title;
            response.MessageInfo = info;
            response.MessageAdditional = additional;
            response.Json = null;
        }
        public void SetSystemError(WxResponse response, string title, string info)
        {
            SetSystemError(response, title, info, null);
        }

        public void SetAppError(WxResponse response, string title, string info, string additional)
        {
            response.Type = WebResponseType.AppError;
            response.MessageTitle = title;
            response.MessageInfo = info;
            response.MessageAdditional = additional;
            response.Json = null;
        }
        public void SetAppError(WxResponse response, string title, string info)
        {
            SetAppError(response, title, info, "");
        }

















        // =================================================================================
        // Static Utility Routines
        // =================================================================================

        public static string JsonConvertCamel(object o)
        {
            DefaultContractResolver contractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            };

            if (o != null)
                return JsonConvert.SerializeObject(o, new JsonSerializerSettings
                {
                    ContractResolver = contractResolver,
                    Formatting = Formatting.None
                });
            else
                return null;
        }


    }
}
