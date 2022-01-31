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
    public class ConfluxEntityCreator : ConfluxEntityOperation
    {
        // ===========================================================================
        // Base Properties
        // ===========================================================================
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        // ===========================================================================
        // Properties
        // ===========================================================================        
        protected bool SaveHistory = true;


        // ===========================================================================
        // Instantiation & Initial setup routines
        // ===========================================================================        

        public ConfluxEntityCreator(ConfluxSession asession, bool saveHistory = false) : base(asession)
        {
            // We're set for creation
            SaveHistory = saveHistory;
        }

        public ConfluxEntityCreator Create<T>(T virtualEntity) where T : DxBaseVirtualPersist
        {
            // Creates a DxEntity and prepares it for storage
            // This routine is not called directly, but it's called inside session.Create()
            dxEntity = PrepareDxEntity(virtualEntity);
            if (dxEntity != null)
            {
                vEntity = virtualEntity;
                vType = typeof(T);
            }
            else
            {
                // The error was already reported
                dxEntity = null;
                vEntity = null;
                vType = null;
            }

            // Fluent link return
            return this;
        }

        private DxEntity PrepareDxEntity<T>(T vEntity) where T : DxBaseVirtualPersist
        {
            DxEntity dxE = null;

            try
            {
                dxE = new DxEntity()
                {
                    CreatedUserId = session.UserId,
                    ModifiedUserId = session.UserId,
                    Class = typeof(T).Name,
                    Code = vEntity.Code,
                    Json = JsonConvert.SerializeObject(vEntity)
                };
            }
            catch (Exception ex)
            {
                string errorMsg = "An error occurred while preparing a DxEntity";
                logger.Error(ex, errorMsg);
                session.SetErrorDatabase(ex, errorMsg);
            }

            return dxE;
        }






        // ===========================================================================
        // Internal Worker Methods
        // ===========================================================================

        public ConfluxEntityCreator Execute()
        {
            if(dxEntity!=null)
            {
                bool processOk = ExecuteQuery("Create DxEntity(" + dxEntity.Class + ")",
                    (x => { return x.Insert(dxEntity); })) > INVALID_ID;

                if(processOk)
                {
                    // We've successfully created the entity.
                    // Obtain updated info from the dxEntity but do not recreate virtual entity
                    // If we don't recreate it, we will be modifying the original external entity
                    // passed to Conflux, which will give the end user updated information and
                    // greater control.
                    ToVirtualEntity(false);

                    // Store history
                    if(SaveHistory)
                        AddEntityHistory(EntityHistoryRecordType.Added);
                }
                else
                {
                    string errorMsg = "An error executing a Create Entity chain.  DxEntity could not be inserted into database.";
                    logger.Error(errorMsg);
                    session.SetErrorBusinessLogic(errorMsg);
                }
            }
            else
            {
                string errorMsg = "An error executing a Create Entity chain.  DxEntity is null";
                logger.Error(errorMsg);
                session.SetErrorBusinessLogic(errorMsg);
            }
            return this;
        }

    }
}
