using Conflux.Database.Model;
using Conflux.Database.Context;
using Conflux.Management;
using Dapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Globalization;
using System.Linq;

namespace Conflux.Database.Operations
{
    public class ConfluxOperation
    {
        // TO-DO : Rebuild class into one managing objects more linq-y

        // ===========================================================================
        // Properties
        // ===========================================================================
        public const long INVALID_ID = 0;
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        // ===========================================================================
        // Properties
        // ===========================================================================
        protected ConfluxSession session;

        protected ConfluxOperation(ConfluxSession asession)
        {
            session = asession;
        }

        protected long ExecuteQuery(string queryInfo, Func<DbConnection, long> action)
        {
            long queryResult = INVALID_ID;

            try
            {
                using (var c = ConfluxContext.Acquire(session.DatabaseName))
                {
                    using (var connection = c.Database.GetDbConnection())
                    {
                        logger.Debug("ExecuteQuery : " + queryInfo);
                        queryResult = action(connection);
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMsg = "An error occurred while executing ExecuteQuery [" + queryInfo +
                                  "] in database [" + session.DatabaseName + "]";
                logger.Error(ex, errorMsg);
                session.SetErrorDatabase(ex, errorMsg);
            }

            return queryResult;
        }

        protected T SingleSelectQuery<T>(string sql)
        {
            T entity = default;

            try
            {
                using (var c = ConfluxContext.Acquire(session.DatabaseName))
                {
                    using (var connection = c.Database.GetDbConnection())
                    {
                        logger.Debug("SingleSelectQuery : [" + sql + "]");
                        entity = connection.QueryFirstOrDefault<T>(sql);
                    }
                }
            }
            catch (Exception ex)
            {
                entity = default;
                string errorMsg = "An error occurred while executing SingleSelectQuery [" + sql +
                                    "] in database [" + session.DatabaseName + "]";
                logger.Error(ex, errorMsg);
                session.SetErrorDatabase(ex, errorMsg);
            }

            return entity;
        }

        protected List<T> MultipleSelectQuery<T>(string sql)
        {
            List<T> entities = default;

            try
            {
                using (var c = ConfluxContext.Acquire(session.DatabaseName))
                {
                    using (var connection = c.Database.GetDbConnection())
                    {
                        logger.Debug("MultipleSelectQuery : [" + sql + "]");

                        using (var multi = connection.QueryMultiple(sql))
                        {                            
                            entities = multi.Read<T>().ToList();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                entities = null;
                string errorMsg = "An error occurred while executing MultipleSelectQuery [" + sql +
                                    "] in database [" + session.DatabaseName + "]";
                logger.Error(ex, errorMsg);
                session.SetErrorDatabase(ex, errorMsg);
            }

            return entities;
        }

        protected List<T> MultipleSelectQueryHistory<T>(string sql)
        {
            List<T> entities = default;

            try
            {
                using (var c = ConfluxContext.Acquire(session.DatabaseName))
                {
                    using (var connection = c.Database.GetDbConnection())
                    {
                        logger.Debug("MultipleSelectQuery : [" + sql + "]");

                        using (var multi = connection.QueryMultiple(sql))
                        {
                            entities = multi.Read<T>().ToList();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                entities = null;
                string errorMsg = "An error occurred while executing MultipleSelectQuery [" + sql +
                                    "] in database [" + session.DatabaseName + "]";
                logger.Error(ex, errorMsg);
                session.SetErrorDatabase(ex, errorMsg);
            }

            return entities;
        }


        // ===========================================================================
        // Helper Utilities
        // ===========================================================================

        public static string DateToISO8601(DateTime dt)
        {
            return dt.ToString("s", CultureInfo.InvariantCulture)+"Z";
        }

        public static string JsonDate(string jsonPath)
        {
            return "STR_TO_DATE(JSON_VALUE(Json, '$." + jsonPath + "'), '%Y-%m-%dT%TZ')";
        }

        public static string Date(DateTime date)
        {
            return "STR_TO_DATE('" +DateToISO8601(date) + "', '%Y-%m-%dT%TZ')";
        }























        //private T DoSingleSelectPersistQuery<T>(string sql) where T : DxBasePersist
        //{
        //    T entity = null;

        //    try
        //    {                
        //        using (var c = ConfluxContext.Acquire(session.DatabaseName))
        //        {
        //            using (var connection = c.Database.GetDbConnection())
        //            {
        //                logger.Debug("SingleSelectPersistQuery : [" + sql + "]");
        //                entity = connection.QueryFirstOrDefault<T>(sql);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        entity = null;
        //        string errorMsg = "An error occurred while executing SingleSelectPersistQuery [" + sql +
        //                            "] in database [" + session.DatabaseName + "]";

        //        // Log the error locally
        //        logger.Error(ex, errorMsg);

        //        // Update the session
        //        session.SetErrorDatabase(ex, errorMsg);
        //    }

        //    return entity;
        //}



        //private T DoSingleSelectTransactQuery<T>(string sql) where T : DxBaseTransact
        //{
        //    T transact = null;

        //    try
        //    {                
        //        using (var c = ConfluxContext.Acquire(session.DatabaseName))
        //        {
        //            using (var connection = c.Database.GetDbConnection())
        //            {
        //                logger.Debug("SingleSelectTransactQuery : [" + sql + "]");
        //                transact = connection.QueryFirstOrDefault<T>(sql);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        transact = null;
        //        string errorMsg = "An error occurred while executing SingleSelectPersistQuery [" + sql +
        //                          "] in database [" + session.DatabaseName + "]";

        //        // Log the error locally
        //        logger.Error(ex, errorMsg);

        //        // Update the session
        //        session.SetErrorDatabase(ex, errorMsg);
        //    }

        //    return transact;
        //}

    }
}
