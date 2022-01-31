using Dapper;
using Microsoft.EntityFrameworkCore;
using System;

using Conflux.Management;
using Dapper.Contrib.Extensions;

namespace Conflux.Database.Context
{
    public static class ConfluxContextOperations
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        // TO-DO: Refactor all these routines into the ConfluxContext
        public static bool ResetAutoIncrement(ConfluxContext context, ConfluxSession session, string tableName)
        {
            bool processOk = true;

            var connection = context.Database.GetDbConnection();

            try
            {
                logger.Info("Resetting AutoIncrement on Table : " + tableName);
                var sql = "alter table " + tableName+" auto_increment = 1";
                connection.Execute(sql);
            }
            catch (Exception ex)
            {
                string errorMsg = "An error occurred while resetting auto-increment on table : " + tableName;

                // Log the error locally
                logger.Error(ex, errorMsg);

                // Store the error in the Manager
                session.SetErrorDatabase(ex, errorMsg);

                // Set process result
                processOk = false;
            }

            return processOk;
        }

        public static bool Truncate(ConfluxContext context, ConfluxSession session, string tableName)
        {
            bool processOk = true;

            var connection = context.Database.GetDbConnection();

            try
            {
                logger.Info("Truncating table : " + tableName);
                var sql = "truncate table " + tableName;
                connection.Execute(sql);
            }
            catch(Exception ex)
            {
                string errorMsg = "An error occurred while truncating table : " + tableName;

                // Log the error locally
                logger.Error(ex, errorMsg);

                // Store the error in the Manager
                session.SetErrorDatabase(ex, errorMsg);

                // Set process result
                processOk = false;
            }

            return processOk;
        }

        public static bool DeleteAll(ConfluxContext context, ConfluxSession session, string tableName)
        {
            bool processOk = true;

            var connection = context.Database.GetDbConnection();

            try
            {
                logger.Info("Deleting all rows from table : " + tableName);
                var sql = "delete from " + tableName;
                var ndel = connection.Execute(sql);
                logger.Info("Deleted : " + ndel + " rows");
            }
            catch (Exception ex)
            {
                string errorMsg = "An error occurred while deleting all rows from table : " + tableName;

                // Log the error locally
                logger.Error(ex, errorMsg);

                // Store the error in the Manager
                session.SetErrorDatabase(ex, errorMsg);

                // Set process result
                processOk = false;
            }

            return processOk;
        }



    }
}
