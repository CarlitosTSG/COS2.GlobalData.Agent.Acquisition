using Conflux.Constants;
using Conflux.Management;
using Conflux.Database.Model;

using Newtonsoft.Json;

using System;
using Microsoft.EntityFrameworkCore;
using Dapper.Contrib.Extensions;
using System.Data.Common;
using System.Runtime.CompilerServices;
using Dapper;
using NLog.LayoutRenderers.Wrappers;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using System.Threading;

namespace Conflux.Database
{
    // Abstract class for managing a Conflux Database
    // This class stores the entities and their versions
    // The idea is for Conflux to make managing and processing queries and entities
    // easy.
    // This is that platform

    public class ConfluxDatabase
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        // ===============================================================================
        // Operational properties
        // ===============================================================================
        public string DatabaseName { get; set; }


        public ConfluxDatabase(string aDatabase)
        {
            DatabaseName = aDatabase;
        }

        /// <summary>
        /// Creates a new administrative ConfluxSession (one not connected to a specific user)
        /// </summary>
        /// <returns>a new administrative ConfluxSession, active</returns>
        public ConfluxSession AcquireSession()
        {
            return new ConfluxSession(DatabaseName);
        }



































        //// ===============================================================================
        //// DxJsonTransact Routines
        //// ===============================================================================

        //private bool AddDxJsonTransact(ConfluxSession session, DxJsonTransact dxJTransact)
        //{
        //    return DoExecuteQuery(session, "Add DxJsonTransact(" + dxJTransact.Date.ToOADate() + "," + dxJTransact.Class + ")",
        //        (x => { return x.Insert(dxJTransact); })) > INVALID_ID;
        //}

        //public DxJsonTransact PrepareDxJsonTransact<T>(ConfluxSession session, string aClass, string aVersion,
        //    long aLinkId, string aLinkClass, EntityRelationship aRelationship, EntityLinkType aLinkType, 
        //    T transact) where T : DxBaseVirtualTransact
        //{
        //    DxJsonTransact dxJTransact = null;

        //    try
        //    {
        //        dxJTransact = new DxJsonTransact()
        //        {
        //            CreatedUserId = session.UserId,
        //            Date = transact.Date,
        //            Class = aClass,
        //            Version = aVersion,
        //            Json = JsonConvert.SerializeObject(transact),
        //            LinkId = aLinkId,
        //            LinkClass = aLinkClass,
        //            Relationship = aRelationship,
        //            LinkType = aLinkType
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        string errorMsg = "An error occurred while preparing a DxJsonTransact";

        //        // Log the error locally
        //        logger.Error(ex, errorMsg);
        //    }

        //    return dxJTransact;
        //}

        //public DxJsonTransact PrepareDxJsonTransact<T>(ConfluxSession session, string aClass, string aVersion, T transact) where T : DxBaseVirtualTransact
        //{
        //    return PrepareDxJsonTransact(session, aClass, aVersion, 0, "_none_", EntityRelationship.None, EntityLinkType.None, transact);
        //}

        //private void PrepareVirtualTransact(DxJsonTransact dxJsonTransact, DxBaseVirtualTransact transact)
        //{
        //    transact.Id = dxJsonTransact.Id;
        //    transact.Timestamp = dxJsonTransact.Timestamp;
        //    transact.CreatedUserId = dxJsonTransact.CreatedUserId;
        //    transact.Date = dxJsonTransact.Date;
        //}

        //public long AddTransaction<T>(ConfluxSession session, string aVersion,
        //                              long aLinkId, string aLinkClass, EntityRelationship aRelationship, EntityLinkType aLinkType, 
        //                              T transact) where T : DxBaseVirtualTransact
        //{
        //    var dxJsonTransact = PrepareDxJsonTransact(session, typeof(T).Name, aVersion, aLinkId, aLinkClass, aRelationship, aLinkType, transact);
        //    if (dxJsonTransact != null)
        //    {
        //        if (AddDxJsonTransact(session, dxJsonTransact))
        //        {
        //            PrepareVirtualTransact(dxJsonTransact, transact);
        //            return transact.Id;
        //        }
        //        else
        //            return INVALID_ID;
        //    }
        //    else
        //        return INVALID_ID;
        //}

        //public long AddTransaction<T>(ConfluxSession session, string aVersion, T transact) where T : DxBaseVirtualTransact
        //{
        //    return AddTransaction(session, aVersion, 0, "", EntityRelationship.None, EntityLinkType.None, transact);
        //}

        //public long AddTransaction<T>(ConfluxSession session, 
        //    long aLinkId, string aLinkClass, EntityRelationship aRelationship, EntityLinkType aLinkType, T transact) where T : DxBaseVirtualTransact
        //{
        //    return AddTransaction(session, "", aLinkId, aLinkClass, aRelationship, aLinkType, transact);
        //}

        //public long AddTransaction<T>(ConfluxSession session, T transact) where T : DxBaseVirtualTransact
        //{
        //    return AddTransaction(session, "", 0, "", EntityRelationship.None, EntityLinkType.None, transact);
        //}

        //public T GetTransactionWhere<T>(ConfluxSession session, string whereClause) where T : DxBaseVirtualTransact
        //{
        //    T transaction = null;
        //    try
        //    {
        //        string baseClassWhere = "Class = '" + typeof(T).Name + "' AND ";
        //        DxJsonTransact dxJsonTransact = DoSingleSelectTransactQuery<DxJsonTransact>(session, baseClassWhere + whereClause);
        //        if(dxJsonTransact!=null)
        //        {
        //            transaction = JsonConvert.DeserializeObject<T>(dxJsonTransact.Json);
        //            PrepareVirtualTransact(dxJsonTransact, transaction);
        //        }
        //        else
        //        {
        //            // This isn't necessarily an error, log it
        //            logger.Debug("No entity found on GetTransactionWhere [" + whereClause + "] : " + typeof(T).Name + " from the database");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        string errorMsg = "An error occurred while getting a single " + typeof(T).Name + " from the database";

        //        // Log the error locally
        //        logger.Error(ex, errorMsg);

        //        // This is an error that should cause the session to stop
        //        session.SetErrorBusinessLogic(errorMsg, ex);
        //    }
        //    return transaction;
        //}

        //public T GetTransaction<T>(ConfluxSession session, long id) where T : DxBaseVirtualTransact
        //{
        //    return GetTransactionWhere<T>(session, "Id = " + id.ToString());
        //}        

        //// DateTime.UtcNow.ToString("s", System.Globalization.CultureInfo.InvariantCulture)






















        //// ===============================================================================
        //// Generic Insert Operations
        //// ===============================================================================


        //private long AddDxEntityHistory(ConfluxSession session, DxEntityHistory dxEntity)
        //{
        //    return DoExecuteQuery(session, "Add DxBaseEntityHistory(" + dxEntity.Class + ")",
        //        (x => { return x.Insert(dxEntity); }));
        //}

        //private long AddDxEntityLink(ConfluxSession session, DxEntityLink dxLink)
        //{
        //    return DoExecuteQuery(session, "Add DxEntityLink(" + dxLink.Class+"->"+dxLink.LinkClass + ")",
        //        (x => { return x.Insert(dxLink); }));
        //}

        //private long AddDxConfig(ConfluxSession session, DxConfig dxConfig)
        //{
        //    return DoExecuteQuery(session, "Add DxConfig(" + dxConfig.Key + ")",
        //        (x => { return x.Insert(dxConfig); }));
        //}



        //private long AddDxLog(ConfluxSession session, DxLog dxLog)
        //{
        //    return DoExecuteQuery(session, "Add DxLog",
        //        (x => { return x.Insert(dxLog); }));
        //}

        //// ===============================================================================
        //// Generic Single Read Operations
        //// ===============================================================================































        // ===============================================================================
        // Preparation Routines
        // ===============================================================================





        

    }
}
