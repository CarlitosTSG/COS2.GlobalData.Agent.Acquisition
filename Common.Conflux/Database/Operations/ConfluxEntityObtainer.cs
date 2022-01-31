using Conflux.Database.Model;
using Conflux.Management;
using Dapper.Contrib.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Conflux.Database.Operations
{
    public class ConfluxEntityObtainer : ConfluxEntityOperation
    {
        // ===========================================================================
        // Base Properties
        // ===========================================================================
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


        // ===========================================================================
        // Instantiation & Initial setup routines
        // ===========================================================================        

        public ConfluxEntityObtainer(ConfluxSession asession) : base(asession)
        {
            // Where AND parts
            whereAndparts = new List<string>();
            orderBy = "";
            limitTo = -1;
        }

        // ===========================================================================
        // Single Entity Obtainer
        // ===========================================================================

        public ConfluxEntityObtainer Get<T>() where T : DxBaseVirtualPersist
        {
            // Begin a single entity acquisition chain
            vType = typeof(T);

            // Add the class selector
            return this.IsClass(vType.Name);
        }

        public ConfluxEntityObtainer GetSingleFromOperator(ConfluxEntityOperation fromOP)
        {
            dxEntity = fromOP.dxEntity;
            vEntity = fromOP.vEntity;
            vType = fromOP.vType;

            vEntityJson = dxEntity.Json;

            return this.IsClass(vType.Name);
        }

        public ConfluxEntityObtainer Obtain()
        {
            // This routine executes a valid single entity query (firstordefault) and stores all loaded
            // information
            if (BuildSelectAll())
            {
                dxEntity = SingleSelectQuery<DxEntity>(querySQL);
                if(dxEntity!=null)
                {
                    logger.Debug("SingleQuery / DxEntity Obtained : " + dxEntity.Class + " / " + dxEntity.Id + " / " + dxEntity.Code);
                    // We've successfully created the entity.
                    // Deserialize back to a proper VirtualPersist class
                    ToVirtualEntity();
                }
                else
                {
                    dxEntity = null;
                    vEntity = null;

                    // Log warning, this isn't necessarily an error
                    string warnMsg = "No dxEntity was found matching the query.";
                    logger.Warn(warnMsg);
                }
            }
            else
            {
                string errorMsg = "An error occurred while obtaining a DxEntity : Where clause is badly formed";
                logger.Error(errorMsg);
                session.SetErrorBusinessLogic(errorMsg);
            }

            return this;
        }



        public dynamic ObtainVirtual()
        {
            // This routine executes a valid single entity query (firstordefault) and stores all loaded
            // information
            if (BuildSelectAll())
            {
                dxEntity = SingleSelectQuery<DxEntity>(querySQL);
                if (dxEntity != null)
                {
                    logger.Debug("SingleQuery / DxEntity Obtained : " + dxEntity.Class + " / " + dxEntity.Id + " / " + dxEntity.Code);
                    // We've successfully created the entity.
                    // Deserialize back to a proper VirtualPersist class
                    ToVirtualEntity();
                }
                else
                {
                    dxEntity = null;
                    vEntity = null;
                    
                    // Log warning, this isn't necessarily an error
                    string warnMsg = "No dxEntity was found matching the query.";
                    logger.Warn(warnMsg);                    
                }
            }
            else
            {
                string errorMsg = "An error occurred while obtaining a DxEntity : Where clause is badly formed";
                logger.Error(errorMsg);
                session.SetErrorBusinessLogic(errorMsg);
            }

            return vEntity;
        }

        public long ObtainId()
        {
            long id = 0;
            // This routine executes a valid single entity query (firstordefault) and stores all loaded
            // information
            if (BuildSelectId())
            {                
                id = SingleSelectQuery<long>(querySQL);
                logger.Debug("SingleQuery / Id Obtained : " + id);
            }
            else
            {
                string errorMsg = "An error occurred while obtaining the Id of a DxEntity : Where clause is badly formed";
                logger.Error(errorMsg);
                session.SetErrorBusinessLogic(errorMsg);
            }

            return id;
        }

        public string ObtainCode()
        {
            string code = null;
            // This routine executes a valid single entity query (firstordefault) and stores all loaded
            // information
            if (BuildSelectCode())
            {
                code = SingleSelectQuery<string>(querySQL);
                logger.Debug("SingleQuery / Code Obtained : " + code);
            }
            else
            {
                string errorMsg = "An error occurred while obtaining the Code of a DxEntity : Where clause is badly formed";
                logger.Error(errorMsg);
                session.SetErrorBusinessLogic(errorMsg);
            }

            return code;
        }

        public bool Exists()
        {
            return (ObtainId() != INVALID_ID);
        }



















        // ===========================================================================
        // Multiple Entity Obtainer
        // ===========================================================================

        public List<dynamic> ListObtainVirtual()
        {
            // This routine executes a valid single entity query (firstordefault) and stores all loaded
            // information
            if (BuildSelectAll())
            {
                dxEntityList = MultipleSelectQuery<DxEntity>(querySQL);

                if (dxEntityList != null)
                {
                    ToVirtualEntities();
                    logger.Debug("MultiQuerySelect : "+vType.Name+" : Obtained "+dxEntityList.Count+" records");
                }
                else
                {
                    dxEntityList = null;
                    vEntityList = null;
                    // Log results
                    string errorMsg = "An error executing an ObtainList DxEntity chain.";
                    logger.Error(errorMsg);
                    session.SetErrorBusinessLogic(errorMsg);
                }
            }
            else
            {
                dxEntityList = null;
                vEntityList = null;
                string errorMsg = "An error occurred while obtaining a DxEntity list : sql clause is badly formed";
                logger.Error(errorMsg);
                session.SetErrorBusinessLogic(errorMsg);
            }

            return vEntityList;
        }

        public List<T> ListObtainVirtual<T>() where T : DxBaseVirtualPersist
        {
            List<T> vConvList = null;

            try
            {
                // This routine executes a valid single entity query (firstordefault) and stores all loaded
                // information
                List<dynamic> vList = ListObtainVirtual();

                if (vList != null)
                {
                    vConvList = new List<T>();
                    for (int i = 0; i < vList.Count; i++)
                    {
                        vConvList.Add(vList[i] as T);
                    }
                }
            }
            catch (Exception ex)
            {
                dxEntityList = null;
                vEntityList = null;
                string errorMsg = "An error occurred while obtaining a DxEntity list : count not cast to final classes";
                logger.Error(ex, errorMsg);
                session.SetErrorBusinessLogic(ex, errorMsg);
            }

            return vConvList;
        }





















        // ===========================================================================
        // Where clauses
        // ===========================================================================

        protected List<string> whereAndparts;
        protected string whereClause;

        public ConfluxEntityObtainer IsClass(string dxClass)
        {
            whereAndparts.Add("Class = '" + dxClass + "'");
            return this;
        }

        public ConfluxEntityObtainer HasCode(string code)
        {
            whereAndparts.Add("Code = '" + code + "'");
            return this;
        }

        public ConfluxEntityObtainer HasId(long id)
        {
            whereAndparts.Add("Id = " + id.ToString());
            return this;
        }

        public ConfluxEntityObtainer AddCustomWhereAnd(string whereAnd)
        {
            whereAndparts.Add(whereAnd);
            return this;
        }

        // ===========================================================================
        // Where Clause Creator
        // ===========================================================================

        protected bool BuildWhereClause()
        {
            bool hasWhere = false;
            whereClause = "";

            // First version of this routine.  We'll add the whereAndparts first

            foreach(var w in whereAndparts)
            {
                if (whereClause == "")
                    whereClause = w;
                else
                    whereClause = whereClause + " AND " + w;

                // Simple check for now
                hasWhere = true;
            }               

            return hasWhere;
        }










        // ===========================================================================
        // Order-By clauses
        // ===========================================================================

        protected string orderBy;

        public ConfluxEntityObtainer AddOrderBy(string sql)
        {
            orderBy = sql;
            return this;
        }








        // ===========================================================================
        // Limit to
        // ===========================================================================

        protected int limitTo;

        public ConfluxEntityObtainer AddLimitTo(int ilimit)
        {
            limitTo = ilimit;
            return this;
        }








        // ===========================================================================
        // Select SQL Creator
        // ===========================================================================

        protected string querySQL;

        public bool BuildSelect(string request)
        {
            bool queryOk = true;

            // In this routine, we prepare the SQL query used by our operator
            string qbase = "SELECT " + request + " FROM " + entityTable;

            if (BuildWhereClause())
            {
                querySQL = qbase + " WHERE " + whereClause;
            }
            else
                querySQL = qbase;

            // We add orderBy clause if needed
            if(!String.IsNullOrWhiteSpace(orderBy))
            {
                querySQL = querySQL + " ORDER BY " + orderBy;
            }

            // And limitTo
            if (limitTo>0)
            {
                querySQL = querySQL + " LIMIT " + limitTo.ToString();
            }

            return queryOk;
        }

        public bool BuildSelectHistory(string request)
        {
            bool queryOk = true;

            // In this routine, we prepare the SQL query used by our operator
            string qbase = "SELECT " + request + " FROM " + entityHistoryTable;

            if (BuildWhereClause())
            {
                querySQL = qbase + " WHERE " + whereClause + " AND RecordType = 2 ";
            }
            else
                querySQL = qbase + " WHERE RecordType = 2 ";


            // We add orderBy clause if needed
            if (!String.IsNullOrWhiteSpace(orderBy))
            {
                querySQL = querySQL + " ORDER BY " + orderBy;
            }

            // And limitTo
            if (limitTo > 0)
            {
                querySQL = querySQL + " LIMIT " + limitTo.ToString();
            }

            return queryOk;
        }

        public bool BuildSelectAll()
        {
            return BuildSelect("*");
        }

        public bool BuildSelectAllHistory()
        {
            return BuildSelectHistory("*");
        }

        public bool BuildSelectId()
        {
            return BuildSelect("Id");
        }

        public bool BuildSelectCode()
        {
            return BuildSelect("Code");
        }
    }
}
