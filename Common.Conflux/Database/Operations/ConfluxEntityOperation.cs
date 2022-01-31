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
    public class ConfluxEntityOperation : ConfluxOperation
    {
        // ===========================================================================
        // Base Properties
        // ===========================================================================
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public static string entityTable = "cfx_entities";
        public static string entityHistoryTable = "cfx_entityhistory";
        public static string linkTable = "cfx_links";
        

        // ===========================================================================
        // Properties
        // ===========================================================================        
        public DxEntity dxEntity;
        public dynamic vEntity;
        public string vEntityJson;
        public List<DxEntity> dxEntityList;
        public List<dynamic> vEntityList;
        public Type vType;

        public ConfluxEntityOperation(ConfluxSession asession) : base(asession)
        {
            // We make sure our virtualEntity is null
            dxEntity = null;
            vEntity = null;
            dxEntityList = null;
            vEntityList = null;
            vType = null;
        }

        private void PrepareVirtualEntity(DxEntity dxEntity, DxBaseVirtualPersist entity)
        {
            entity.Id = dxEntity.Id;
            entity.Code = dxEntity.Code;
            entity.CreatedDate = dxEntity.CreatedDate;
            entity.CreatedUserId = dxEntity.CreatedUserId;
            entity.ModifiedDate = dxEntity.ModifiedDate;
            entity.ModifiedUserId = dxEntity.ModifiedUserId;
        }


        // ===========================================================================
        // Entity Serialization / Deserialization
        // ===========================================================================

        protected void FromVirtualEntity<T>(T virtualEntity)
        {
            try
            {
                vEntity = virtualEntity;
                vType = typeof(T);
                vEntityJson = JsonConvert.SerializeObject(virtualEntity);
            }
            catch(Exception ex)
            {
                string errorMsg = "An error occurred while serializing : "+typeof(T).Name;
                logger.Error(ex, errorMsg);
                session.SetErrorBusinessLogic(ex, errorMsg);
            }
        }

        protected void ToVirtualEntity(bool create = true)
        {            
            try
            {
                if(dxEntity!=null)
                {
                    vEntityJson = dxEntity.Json;

                    // If we are creating a new entity, the vEntity was already passed 
                    // and we don't want to delete it.  If we're obtaining, we create it.
                    if (create)
                        vEntity = JsonConvert.DeserializeObject(vEntityJson, vType);

                    vEntity.Id = dxEntity.Id;
                    vEntity.Code = dxEntity.Code;
                    vEntity.CreatedDate = dxEntity.CreatedDate;
                    vEntity.CreatedUserId = dxEntity.CreatedUserId;
                    vEntity.ModifiedDate = dxEntity.ModifiedDate;
                    vEntity.ModifiedUserId = dxEntity.ModifiedUserId;
                }
            }
            catch (Exception ex)
            {
                string errorMsg = "An error occurred while serializing : " + vType.Name;
                logger.Error(ex, errorMsg);
                session.SetErrorBusinessLogic(ex, errorMsg);
            }
        }



        protected void ToVirtualEntities()
        {
            // This routine can only be called from a SELECT to List
            try
            {
                if (dxEntityList != null)
                {
                    // We create the dynamic list, empty
                    vEntityList = new List<dynamic>();

                    foreach(var dxe in dxEntityList)
                    {
                        var vJson = dxe.Json;
                        dynamic v = JsonConvert.DeserializeObject(vJson, vType);

                        v.Id = dxe.Id;
                        v.Code = dxe.Code;
                        v.CreatedDate = dxe.CreatedDate;
                        v.CreatedUserId = dxe.CreatedUserId;
                        v.ModifiedDate = dxe.ModifiedDate;
                        v.ModifiedUserId = dxe.ModifiedUserId;

                        vEntityList.Add(v);
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMsg = "An error occurred while serializing : " + vType.Name;
                logger.Error(ex, errorMsg);
                session.SetErrorBusinessLogic(ex, errorMsg);
            }
        }


        // ===========================================================================
        // EntityHistory Methods
        // ===========================================================================

        // This method is called for a dxEntity that's just been read from the database
        // but hasn't been updated yet.  It assumes that the dxEntity exists and writes
        // it exactly as loaded into the history table.
        protected void AddEntityHistory(EntityHistoryRecordType recordType, DxEntity dxSend = null)
        {
            DxEntity dxOrig = dxSend;
            if (dxOrig == null)
                dxOrig = dxEntity;

            if(dxOrig!=null)
            {
                // We create the record based on the values in dxEntity
                var dxHistory = new DxEntityHistory()
                {
                    Class = dxOrig.Class,
                    Code = dxOrig.Code,
                    EntityId = dxOrig.Id,
                    Json = dxOrig.Json,
                    RecordType = recordType
                };

                bool processOk = ExecuteQuery("Create DxEntityHistory for DxEntity (" + dxOrig.Id + " / "+ dxOrig.Class + ")",
                    (x => { return x.Insert(dxHistory); })) > INVALID_ID;

                if (!processOk)
                {
                    string errorMsg = "An error storing an Entity History record.";
                    logger.Error(errorMsg);
                    session.SetErrorBusinessLogic(errorMsg);
                }
            }
            else
            {
                // This is a logic error and needs to be logged
                string errorMsg = "An error occurred while adding entity history.  The dxEntity is empty";
                logger.Error(errorMsg);
                session.SetErrorBusinessLogic(errorMsg);

            }
        }







        //public long AddEntity<T>(ConfluxSession session, string aCode, string aVersion, T entity) where T : DxBaseVirtualPersist
        //{
        //    var dxEntity = PrepareDxEntity(session, typeof(T).Name, aCode, aVersion, entity);
        //    if (dxEntity != null)
        //    {
        //        if (AddDxEntity(session, dxEntity))
        //        {
        //            PrepareVirtualEntity(dxEntity, entity);
        //            return entity.Id;
        //        }
        //        else
        //            return INVALID_ID;
        //    }
        //    else
        //        return INVALID_ID;
        //}

        //public long AddEntity<T>(ConfluxSession session, T entity) where T : DxBaseVirtualPersist
        //{
        //    return AddEntity(session, "", "", entity);
        //}

        //public T GetEntityWhere<T>(ConfluxSession session, string whereClause, bool notDeleted = true) where T : DxBaseVirtualPersist
        //{
        //    T entity = null;
        //    try
        //    {
        //        string baseClassWhere = "Class = '" + typeof(T).Name + "' AND ";
        //        DxEntity dxEntity = DoSingleSelectPersistQuery<DxEntity>(session, baseClassWhere + whereClause, notDeleted);
        //        if (dxEntity != null)
        //        {
        //            entity = JsonConvert.DeserializeObject<T>(dxEntity.Json);
        //            PrepareVirtualEntity(dxEntity, entity);
        //        }
        //        else
        //        {
        //            // This isn't necessarily an error, log it
        //            logger.Debug("No entity found on GetEntityWhere [" + whereClause + "] : " + typeof(T).Name + " from the database");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        string errorMsg = "An error occurred while getting a single " + typeof(T).Name + " from the database";

        //        // Log the error locally
        //        logger.Error(ex, errorMsg);
        //    }
        //    return entity;
        //}

        //public T GetEntity<T>(ConfluxSession session, long id) where T : DxBaseVirtualPersist
        //{
        //    return GetEntityWhere<T>(session, "Id = " + id.ToString());
        //}

        //public T GetEntity<T>(ConfluxSession session, string code) where T : DxBaseVirtualPersist
        //{
        //    return GetEntityWhere<T>(session, "Code = '" + code + "'");
        //}






        // ===========================================================================
        // Fluent API
        // ===========================================================================



    }
}
