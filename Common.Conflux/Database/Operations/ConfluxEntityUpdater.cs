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
    public class ConfluxEntityUpdater : ConfluxEntityOperation
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

        public ConfluxEntityUpdater(ConfluxSession asession, bool saveHistory = false) : base(asession)
        {
            // We're set for creation
            SaveHistory = saveHistory;
        }

        public ConfluxEntityUpdater Update<T>(T virtualEntity) where T : DxBaseVirtualPersist
        {
            // The virtualEntity should have everything needed to create a dxEntity

            dxEntity = PrepareDxEntityFromExisting(virtualEntity);
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

        private DxEntity PrepareDxEntityFromExisting<T>(T vEntity) where T : DxBaseVirtualPersist
        {
            DxEntity dxE = null;

            try
            {
                dxE = new DxEntity()
                {
                    Id = vEntity.Id,
                    CreatedDate = vEntity.CreatedDate,
                    CreatedUserId = vEntity.CreatedUserId,
                    ModifiedDate = DateTime.UtcNow,
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

        public ConfluxEntityUpdater Execute()
        {
            if(dxEntity!=null)
            {
                bool storeHistoryOk = false;
                if (SaveHistory)
                {
                    try
                    {
                        var op = new ConfluxEntityObtainer(session).GetSingleFromOperator(this).HasId(dxEntity.Id).Obtain();
                        if (op != null)
                        {
                            AddEntityHistory(EntityHistoryRecordType.Updated, op.dxEntity);
                            storeHistoryOk = true;
                        }
                        else
                        {
                            string errorMsg = "An error saving a DxEntity history on update.";
                            logger.Error(errorMsg);
                            session.SetErrorBusinessLogic(errorMsg);
                        }
                    }
                    catch (Exception ex)
                    {
                        string errorMsg = "An error saving a DxEntity history on update.";
                        logger.Error(ex, errorMsg);
                        session.SetErrorBusinessLogic(ex, errorMsg);
                    }
                }
                else
                    storeHistoryOk = true;

                if (storeHistoryOk)
                {
                    // Update DxEntity
                    bool processOk = ExecuteQuery("Update DxEntity(" + dxEntity.Class + " / " + dxEntity.Id + ")",
                        (x => { return x.Update(dxEntity) ? 1 : 0; })) > INVALID_ID;

                    if (processOk)
                    {
                        // We've successfully created the entity.
                        // Obtain updated info from the dxEntity but do not recreate virtual entity
                        // If we don't recreate it, we will be modifying the original external entity
                        // passed to Conflux, which will give the end user updated information and
                        // greater control.
                        ToVirtualEntity(false);
                    }
                    else
                    {
                        string errorMsg = "An error executing an Update Entity chain.  DxEntity could not be updated in the database.";
                        logger.Error(errorMsg);
                        session.SetErrorBusinessLogic(errorMsg);
                    }
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
