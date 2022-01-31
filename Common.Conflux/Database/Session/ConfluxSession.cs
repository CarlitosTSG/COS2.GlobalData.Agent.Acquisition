using Conflux.Database.Model;
using Conflux.Database.Operations;
using Conflux.Helpers;
using Microsoft.EntityFrameworkCore.Metadata;
using System;

namespace Conflux.Management
{
    public class ConfluxSession : IDisposable
    {
        // -------------------------------------------------------------------
        // Database
        // -------------------------------------------------------------------     
        public string DatabaseName;

        // -------------------------------------------------------------------
        // Web User
        // -------------------------------------------------------------------     
        public string WebUserGuid { get; set; } // For web user logins
        public string UserName { get; set; }

        // -------------------------------------------------------------------
        // User
        // -------------------------------------------------------------------     
        public int UserId { get; set; } // For secondary table managing user information
        public string FullName { get; set; }
        public string SessionSource { get; set; }
        public bool IsBot { get; set; } // If true, it's not a web user

        // -------------------------------------------------------------------
        // Admin/Subsystem ID (If the session corresponds to a subsystem)
        // -------------------------------------------------------------------     
        public int SubsystemId { get; set; }


        // -------------------------------------------------------------------
        // Session State
        // -------------------------------------------------------------------
        public DateTime SessionStart { get; set; }
        public DateTime SessionEnd { get; set; }
        public bool SessionError { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorDetail { get; set; }
        public bool SessionErrorDatabase { get; set; }
        public string ErrorDatabaseMessage { get; set; }
        public string ErrorDatabaseDetail { get; set; }
        public bool SessionErrorBusinessLogic { get; set; }
        public string ErrorBusinessLogicMessage { get; set; }
        public string ErrorBusinessLogicDetail { get; set; }


        // -------------------------------------------------------------------
        // Construction
        // -------------------------------------------------------------------     
        public ConfluxSession(string aDatabaseName)
        {
            // DatabaseName
            DatabaseName = aDatabaseName;

            // Internal Bot for admin operations
            // Bot users are not web users
            WebUserGuid = ""; // Not a web user
            UserName = "InternalBot";

            // Any entities created with this bot will have a user id of 0
            UserId = 0;
            FullName = "Internal Admin Bot";
            SessionSource = "Core";
            IsBot = true;

            // This is a main database constructor, not for subsystems/clientdbs
            SubsystemId = 0;

            StartSingleOperation();
        }

        // -------------------------------------------------------------------
        // Base Methods
        // -------------------------------------------------------------------     

        public void StartSingleOperation()
        {
            // Session Start Time
            SessionStart = DateTime.UtcNow;

            SessionError = false;
            SessionErrorDatabase = false;
            SessionErrorBusinessLogic = false;

            ErrorMessage = "";
            ErrorDetail = "";
            ErrorDatabaseMessage = "";
            ErrorDatabaseDetail = "";
            ErrorBusinessLogicMessage = "";
            ErrorBusinessLogicDetail = "";
        }

        public void EndSingleOperation()
        {
            // Here, we make decisions based on what happened with the session
            // For now, for this version of Conflux, any error within the session triggers a stop of operations
            // 
            // TO-DO: Manage different configurations for operational errors.
            if (SessionError)
            {
                ConfluxManager.ReportSessionError(this);
            }
        }

        public bool CanContinue
        {
            get
            {
                return !SessionError;
            }
        }

        public void SetErrorDatabase(Exception ex, string msg)
        {
            SetErrorDatabase(msg, "[Exception] " + ex.Message);
        }

        public void SetErrorDatabase(string msg, string detail)
        {
            SessionErrorDatabase = true;
            ErrorDatabaseMessage = msg;
            ErrorDatabaseDetail = detail;

            SessionError = true;
            ErrorMessage = ErrorMessage.IsEmpty() ? "Database : " + ErrorDatabaseMessage :
                                                    ErrorMessage + " / " +
                                                    "Database : " + ErrorDatabaseMessage;
            ErrorDetail = ErrorDetail.IsEmpty() ? "Database : " + ErrorDatabaseDetail :
                                                  ErrorDetail + System.Environment.NewLine +
                                                  "Database : " + ErrorDatabaseDetail;
        }

        public void SetErrorBusinessLogic(string msg)
        {
            SetErrorBusinessLogic(msg, "No Details");
        }

        public void SetErrorBusinessLogic(Exception ex, string msg)
        {
            SetErrorBusinessLogic(msg, "[Exception] " + ex.Message);
        }

        public void SetErrorBusinessLogic(string msg, string detail)
        {
            SessionErrorBusinessLogic = true;
            ErrorBusinessLogicMessage = msg;
            ErrorBusinessLogicDetail = detail;

            SessionError = true;
            ErrorMessage = ErrorMessage.IsEmpty() ? "Business Logic : " + ErrorBusinessLogicMessage :
                                                    ErrorMessage + " / " +
                                                    "Business Logic : " + ErrorBusinessLogicMessage;
            ErrorDetail = ErrorDetail.IsEmpty() ? "Business Logic : " + ErrorBusinessLogicDetail :
                                                  ErrorDetail + System.Environment.NewLine +
                                                  "Business Logic : " + ErrorBusinessLogicDetail;
        }




















        // ===============================================================
        // Conflux Database Operations
        // ===============================================================

        public ConfluxEntityCreator Create<T>(T virtualEntity) where T : DxBaseVirtualPersist
        {
            return new ConfluxEntityCreator(this).Create(virtualEntity);
        }
        public ConfluxEntityCreator CreateNoHistory<T>(T virtualEntity) where T : DxBaseVirtualPersist
        {
            return new ConfluxEntityCreator(this, false).Create(virtualEntity);
        }

        public ConfluxEntityObtainer Get<T>() where T : DxBaseVirtualPersist
        {
            return new ConfluxEntityObtainer(this).Get<T>();
        }

        public ConfluxEntityUpdater Update<T>(T virtualEntity) where T : DxBaseVirtualPersist
        {
            return new ConfluxEntityUpdater(this).Update(virtualEntity);
        }

        public ConfluxEntityUpdater UpdateNoHistory<T>(T virtualEntity) where T : DxBaseVirtualPersist
        {
            return new ConfluxEntityUpdater(this, false).Update(virtualEntity);
        }

        public ConfluxEntityDeleter Delete<T>(T virtualEntity) where T : DxBaseVirtualPersist
        {
            return new ConfluxEntityDeleter(this).Delete(virtualEntity);
        }

        public ConfluxEntityDeleter DeleteNoHistory<T>(T virtualEntity) where T : DxBaseVirtualPersist
        {
            return new ConfluxEntityDeleter(this, false).Delete(virtualEntity);
        }

        public ConfluxConfigurationStorage ConfigStorage(string key)
        {
            return new ConfluxConfigurationStorage(this, key);
        }
























        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                EndSingleOperation();

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ConfluxSession()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
