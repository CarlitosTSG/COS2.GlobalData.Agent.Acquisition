using Conflux.Constants;
using Conflux.Database.Model;
using Conflux.Management;
using Dapper.Contrib.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.Policy;
using System.Text;

namespace Conflux.Database.Operations
{
    public class ConfluxConfigurationStorage : ConfluxOperation
    {
        // ===========================================================================
        // Base Properties
        // ===========================================================================
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public static string configTable = "cfx_config";

        public static string mode_base = "base";
        public static string mode_prod = "production";
        public static string mode_test = "testing";
        public static string mode_dev = "dev";

        public static string dataType_any = "any";
        public static string dataType_int64 = "int64";
        public static string dataType_bool = "bool";
        public static string dataType_string = "string";
        public static string dataType_datetime = "datetime";
        public static string dataType_bytes = "bytes";
        public static string dataType_decimal = "decimal";

        public static long default_long = -999999;
        public static int default_int = -999999;
        public static string default_string = "_nodata_";
        public static DateTime default_datetime = new DateTime(1900, 1, 1);
        public static decimal default_decimal = -999999.999m;

        // ===========================================================================
        // Properties
        // ===========================================================================        
        private long _userId = 0;
        private long _subsystemId = 0;
        private string _mode = mode_base;
        private string _type = dataType_any;
        private string _key = "";
        private long _datalong = default_long;
        private string _datastring = default_string;
        private DateTime _datadatetime = default_datetime;
        private decimal _datadecimal = default_decimal;
        private byte[] _databytes = null;
        private string WhereClause = "";

        private DxConfig dxConfig;
        private DxConfig dxConfigDefault;

        // ===========================================================================
        // Instantiation & Initial setup routines
        // ===========================================================================        

        public ConfluxConfigurationStorage(ConfluxSession asession, string akey) : base(asession)
        {
            // We're set for key management
            dxConfig = null;
            if (!String.IsNullOrWhiteSpace(akey))
                _key = akey.Trim().ToLowerInvariant();
            else
                _key = "";

            // Setup the base default
            dxConfigDefault = new DxConfig();
            dxConfigDefault.Type = dataType_any;
            dxConfigDefault.ValueData = null;
            dxConfigDefault.ValueDate = default_datetime;
            dxConfigDefault.ValueDecimal = default_decimal;
            dxConfigDefault.ValueInt = default_long;
            dxConfigDefault.ValueString = default_string;
        }



        // ===========================================================================
        // WHERE Clause Selectors
        // ===========================================================================

        private bool BuildWhere()
        {
            WhereClause = "WHERE";
            bool whereOk = true;

            if (!String.IsNullOrWhiteSpace(_key))
            {
                WhereClause += " ConfigKey = '" + _key.Trim().ToLowerInvariant() + "' AND"+
                               " UserId = "+_userId.ToString()+" AND"+
                               " SubsystemId = "+_subsystemId.ToString()+" AND"+
                               " Mode = '"+_mode+"'";
            }
            else
                whereOk = false;

            return whereOk;
        }

        public ConfluxConfigurationStorage UserId(long id)
        {
            _userId = id;
            return this;
        }

        public ConfluxConfigurationStorage SubsystemId(long id)
        {
            _subsystemId = id;
            return this;
        }

        public ConfluxConfigurationStorage Mode(string mode)
        {
            if(String.IsNullOrWhiteSpace(mode))
            {
                logger.Warn("Empty or null mode requested " + GetSignature()+" : Mode not changed");
            }
            else
                _mode = mode.Trim().ToLowerInvariant();

            return this;
        }



        // ===========================================================================
        // Default Management
        // ===========================================================================

        public ConfluxConfigurationStorage Default(int idefault)
        {
            dxConfigDefault.ValueInt = idefault;
            dxConfigDefault.Type = dataType_int64;
            return this;
        }

        public ConfluxConfigurationStorage Default(long idefault)
        {
            dxConfigDefault.ValueInt = idefault;
            dxConfigDefault.Type = dataType_int64;
            return this;
        }

        public ConfluxConfigurationStorage Default(decimal ddefault)
        {
            dxConfigDefault.ValueDecimal = ddefault;
            dxConfigDefault.Type = dataType_decimal;
            return this;
        }

        public ConfluxConfigurationStorage Default(float fddefault)
        {
            dxConfigDefault.ValueDecimal = Convert.ToDecimal(fddefault);
            dxConfigDefault.Type = dataType_decimal;
            return this;
        }

        public ConfluxConfigurationStorage Default(double dddefault)
        {
            dxConfigDefault.ValueDecimal = Convert.ToDecimal(dddefault);
            dxConfigDefault.Type = dataType_decimal;
            return this;
        }

        public ConfluxConfigurationStorage Default(DateTime dtdefault)
        {
            dxConfigDefault.ValueDate = dtdefault;
            dxConfigDefault.Type = dataType_datetime;
            return this;
        }

        public ConfluxConfigurationStorage Default(bool bdefault)
        {
            dxConfigDefault.ValueInt = bdefault ? 1 : 0;
            dxConfigDefault.Type = dataType_bool;
            return this;
        }



        // ===========================================================================
        // Obtain Methods
        // ===========================================================================
        protected ConfluxConfigurationStorage Obtain(bool setLocal = true)
        {            
            if (BuildWhere())
            {
                var querySQL = "SELECT * FROM " + configTable + " " + WhereClause;
                dxConfig = SingleSelectQuery<DxConfig>(querySQL);
                if(dxConfig != null)
                {
                    // Set local values
                    if (setLocal)
                    {
                        _type = dxConfig.Type;
                        _mode = dxConfig.Mode;
                        _userId = dxConfig.UserId;
                        _subsystemId = dxConfig.SubsystemId;
                        _databytes = dxConfig.ValueData;
                        _datadatetime = dxConfig.ValueDate;
                        _datadecimal = dxConfig.ValueDecimal;
                        _datalong = dxConfig.ValueInt;
                        _datastring = dxConfig.ValueString;
                    }

                    logger.Debug("Config Obtained "+ GetSignature()+" SingleQuery / Id Obtained : " + dxConfig.Id);
                }
                else
                {
                    logger.Debug("Config Not Found "+GetSignature());
                }
            }
            else
            {
                string errorMsg = "An error occurred while generating the where clause of a Config query "+GetSignature();
                logger.Error(errorMsg);
                session.SetErrorBusinessLogic(errorMsg);
            }
            return this;
        }

        protected long ObtainId()
        {
            long id = INVALID_ID;
            if(BuildWhere())
            {
                var querySQL = "SELECT Id FROM " + configTable + " " + WhereClause;
                id = SingleSelectQuery<long>(querySQL);
                logger.Debug("Config Id Obtained "+GetSignature()+" SingleQuery / Id Obtained : " + id);                
            }
            else
            {
                string errorMsg = "An error occurred while generating the where clause of a Config query " + GetSignature();
                logger.Error(errorMsg);
                session.SetErrorBusinessLogic(errorMsg);
            }
            return id;
        }

        public bool Exists()
        {
            return (ObtainId() != INVALID_ID);
        }

        // ===========================================================================
        // Get Methods
        // ===========================================================================
        protected ConfluxConfigurationStorage Get()
        {
            Obtain();
            if (dxConfig == null)
            {
                logger.Warn("Configuration value requested, but record does not exist " + GetSignature());
                dxConfig = dxConfigDefault;
            }
            return this;
        }

        public long GetLong()
        {
            Get();

            long lresult = default_long;
            if (dxConfig.Type != dataType_any && dxConfig.Type != dataType_int64)
            {
                logger.Warn("Configuration value requested, but stored type is different from request " + 
                            GetSignature() + " Requested : "+ dataType_int64);
            }
            else
            {
                lresult = dxConfig.ValueInt;
            }
            return lresult;
        }

        public int GetInt()
        {
            Get();
            int iresult = default_int;
            if (dxConfig.Type != dataType_any && dxConfig.Type != dataType_int64)
            {
                logger.Warn("Configuration value requested, but stored type is different from request " + 
                            GetSignature() + " Requested : " + dataType_int64);
            }
            else
            {
                try
                {
                    iresult = Convert.ToInt32(dxConfig.ValueInt);
                }
                catch
                {
                    logger.Warn("Integer overflow detected (from long to int) " + 
                                GetSignature() + " : Value will be set to 0");
                }
            }
            return iresult;
        }

        public string GetString()
        {
            Get();

            string sresult = default_string;
            if (dxConfig.Type != dataType_any && dxConfig.Type != dataType_string)
            {
                logger.Warn("Configuration value requested, but stored type is different from request " + 
                            GetSignature() + " : Requested : " + dataType_string);
            }
            else
            {
                sresult = dxConfig.ValueString;
            }
            return sresult;
        }

        public DateTime GetDateTime()
        {
            Get();

            DateTime dtresult = default_datetime;
            if (dxConfig.Type != dataType_any && dxConfig.Type != dataType_datetime && dxConfig.Type != dataType_string)
            {
                logger.Warn("Configuration value requested, but stored type is different from request " +
                            GetSignature() + " : Requested : " + dataType_datetime);                
            }
            else if(dxConfig.Type == dataType_string)
            {
                try
                {
                    dtresult = DateTime.Parse(dxConfig.ValueString);
                }
                catch
                {
                    logger.Warn("Invalid string data found (from string to datetime) " +
                            GetSignature() + " : Value will be set to a default datetime");
                }
            }
            else
            {
                dtresult = dxConfig.ValueDate;
            }
            return dtresult;
        }

        public bool GetBool()
        {
            Get();

            bool bresult = false;
            if (dxConfig.Type != dataType_any && dxConfig.Type != dataType_bool)
            {
                logger.Warn("Configuration value requested, but stored type is different from request " +
                            GetSignature() + " : Requested : " + dataType_bool);
            }
            else
            { 
                bresult = dxConfig.ValueInt == 1 ? true : false;
            }
            return bresult;
        }

        // ===========================================================================
        // Set Methods
        // ===========================================================================

        protected void Set()
        {
            if(BuildWhere())
            {
                dxConfig = null;
                Obtain(false);
                if (dxConfig == null)
                {
                    // Let's setup a new dxConfig
                    dxConfig = new DxConfig()
                    {
                        ConfigKey = _key,
                        Mode = _mode,
                        UserId = _userId,
                        SubsystemId = _subsystemId,
                        Type = _type,
                        ValueData = _databytes,
                        ValueDate = _datadatetime,
                        ValueDecimal = _datadecimal,
                        ValueInt = _datalong,
                        ValueString = _datastring
                    };

                    bool processOk = ExecuteQuery("Create DxConfig(" + _key + ")",
                        (x => { return x.Insert(dxConfig); })) > INVALID_ID;

                    if (processOk)
                    {
                        // We've successfully created the configuration.
                        logger.Debug("New configuration record stored " +GetSignature());
                    }
                    else
                    {
                        string errorMsg = "An error executing a Create Config chain "+
                                          GetSignature()+" : DxConfig could not be inserted into database.";
                        logger.Error(errorMsg);
                        session.SetErrorBusinessLogic(errorMsg);
                    }
                }
                else
                {
                    // Let's modify the obtained value
                    // TO-DO : Review what happens when a configuration value's type is changed.  For now, just change it
                    dxConfig.Type = _type;
                    dxConfig.ValueData = _databytes;
                    dxConfig.ValueDate = _datadatetime;
                    dxConfig.ValueDecimal = _datadecimal;
                    dxConfig.ValueInt = _datalong;
                    dxConfig.ValueString = _datastring;

                    bool processOk = ExecuteQuery("Update DxConfig(" + _key + ")",
                        (x => { return x.Update(dxConfig) ? 1 : 0; })) > INVALID_ID;

                    if (processOk)
                    {
                        logger.Debug("Updated configuration record "+GetSignature());
                    }
                    else
                    {
                        string errorMsg = "An error executing an Update Config chain" +
                                          GetSignature() + " : DxConfig could not be updated in the database.";
                        logger.Error(errorMsg);
                        session.SetErrorBusinessLogic(errorMsg);
                    }

                }
            }
        }

        public void Set(long lValue)
        {
            _type = dataType_int64;
            _datalong = lValue;

            Set();
        }

        public void Set(int iValue)
        {
            _type = dataType_int64;
            _datalong = iValue;

            Set();
        }

        public void Set(bool bValue)
        {
            _type = dataType_bool;
            _datalong = bValue ? 1 : 0;

            Set();
        }

        public void Set(DateTime dtValue)
        {
            _type = dataType_datetime;
            _datadatetime = dtValue;

            Set();
        }

        public void Set(decimal decValue)
        {
            _type = dataType_decimal;
            _datadecimal = decValue;

            Set();
        }

        public void Set(double decValue)
        {
            _type = dataType_decimal;
            try
            {
                _datadecimal = Convert.ToDecimal(decValue);
                Set();
            }
            catch
            {
                logger.Warn("Configuration value could not be set " +
                            GetSignature() + " : Decimal value overflow from double");
            }
        }

        public void Set(float decValue)
        {
            _type = dataType_decimal;
            try
            {
                _datadecimal = Convert.ToDecimal(decValue);
                Set();
            }
            catch
            {
                logger.Warn("Configuration value could not be set " +
                            GetSignature() + " : Decimal value overflow from single");
            }
        }

        public void Set(string sValue)
        {
            _type = dataType_string;
            _datastring = sValue!=null ? sValue : "";

            Set();
        }






        // ===========================================================================
        // Helper / Utility Methods
        // ===========================================================================

        public string GetSignature()
        {
            return "[ConfigKey:"+_key+ @"/Type:" + _type+@"/UserId:"+_userId+@"/SubsysId:"+_subsystemId+ @"/Mode:" + _mode + "]";
        }
    }
}
