using Conflux.Constants;
using Conflux.Database.Model;
using Conflux.Management;
using Dapper.Contrib.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Conflux.Database.Operations
{
    public class ConfluxVirtualKeyGenerator : ConfluxOperation
    {
        // ===========================================================================
        // Base Properties
        // ===========================================================================
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public static string vkeyTable = "cfx_vkeys";
        
        public ConfluxVirtualKeyGenerator(ConfluxSession asession) : base(asession)
        {

        }

        // ===========================================================================
        // VirtualKey Generation
        // ===========================================================================

        public long Generate(string fromClass, long fromId, string toClass)
        {
            var dxVKey = new DxVirtualKey() { FromClass = fromClass, FromId = fromId, ToClass = toClass };

            long id = ExecuteQuery("Obtain Virtual Key(" + fromClass + " / " + fromId + " > " + toClass + ")" ,
                (x => { return x.Insert(dxVKey); }));

            if (id == INVALID_ID)
            {
                string errorMsg = "An error executing a Create Virtual Key quert.  DxVirtualKey could not be inserted into database.";
                logger.Error(errorMsg);
                session.SetErrorBusinessLogic(errorMsg);
            }
            return id;
        }
    }
}
