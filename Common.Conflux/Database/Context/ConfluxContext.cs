using Conflux.Management;
using Conflux.Database.Model;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

using Newtonsoft.Json;

using System;
using System.IO;

namespace Conflux.Database.Context
{
    public class ConfluxContext : IdentityDbContext<DxWebUser>
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        // ==================================================================================
        // ConfluxContext Properties
        // ==================================================================================
        public string DatabaseName { get; set; }

        // ==================================================================================
        // Standard ConfluxContext Tables
        // ==================================================================================
        public DbSet<DxEntity> Entities { get; set; }
        public DbSet<DxEntityHistory> EntityHistory { get; set; }
        public DbSet<DxEntityLink> Links { get; set; }
        public DbSet<DxVirtualKey> VirtualKeys { get; set; }
        public DbSet<DxConfig> Config { get; set; }
        public DbSet<DxJsonTransact> JsonTransactions { get; set; }
        public DbSet<DxLog> Logs { get; set; }
        public DbSet<DxStorage> Storage { get; set; }
        public DbSet<DxStorageHistory> StorageHistory { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (modelBuilder != null)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.Entity<DxEntity>()
                            .HasIndex(b => b.Code);

                modelBuilder.Entity<DxEntityLink>()
                            .HasIndex(b => new { b.FromClass, b.FromId });
                modelBuilder.Entity<DxEntityLink>()
                            .HasIndex(b => new { b.FromClass, b.FromCode });
                modelBuilder.Entity<DxEntityLink>()
                            .HasIndex(b => new { b.ToClass, b.ToId, b.Relationship });
                modelBuilder.Entity<DxEntityLink>()
                            .HasIndex(b => new { b.ToClass, b.ToCode, b.Relationship });

                modelBuilder.Entity<DxLog>()
                            .HasIndex(b => b.Timestamp);

                modelBuilder.Entity<DxJsonTransact>()
                            .HasIndex(b => b.Timestamp);
                modelBuilder.Entity<DxJsonTransact>()
                            .HasIndex(b => b.Date);
                modelBuilder.Entity<DxJsonTransact>()
                            .HasIndex(b => new { b.LinkClass, b.Relationship, b.LinkId });

                modelBuilder.Entity<DxStorage>()
                            .HasIndex(b => new { b.LinkClass, b.LinkId });
                modelBuilder.Entity<DxStorage>()
                            .HasIndex(b => new { b.LinkClass, b.LinkCode });
            }
        }

        public ConfluxContext(DbContextOptions<ConfluxContext> options)
            : base(options)
        {
            // Standard Constructor
            DatabaseName = "default";
        }

        public ConfluxContext(DbContextOptions<ConfluxContext> options, string dbname)
            : base(options)
        {
            // Conflux Constructor
            DatabaseName = dbname;
        }










        // ==============================================================================================
        // Static Context Generation
        // ==============================================================================================

        public static ConfluxContext Acquire(string name, bool isReadOnly = true)
        {
            ConfluxContext c = null;
            try
            {
                var connString = ConfluxManager.ConnectionString.
                    Replace("[database]", name, StringComparison.InvariantCulture);  

                var optionsBuilder = new DbContextOptionsBuilder<ConfluxContext>();
                optionsBuilder.UseMySql(connString);

                c = new ConfluxContext(optionsBuilder.Options, name);
                if (isReadOnly)
                    c.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
                else
                    c.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;
            }
            catch (Exception ex)
            {
                string errorMsg = "An error occurred while creating a Conflux database context";

                // Log the error locally
                logger.Error(ex, errorMsg);
            }

            return c;
        }

        // ==============================================================================================
        // Database Migration
        // ==============================================================================================

        public bool CheckInitDB(bool clear = false)
        {
            bool processOk = false;
            try
            {                
                logger.Info("Creating/Migrating Conflux Database");
                Database.Migrate();

                if (clear)
                {
                    // We need to clean all the users and roles first
                    logger.Info("Clearing Data");

                    // For this, we need a work session
                    using var session = new ConfluxSession(DatabaseName);
                    ConfluxContextOperations.Truncate(this, session, "cfx_index");
                    ConfluxContextOperations.Truncate(this, session, "cfx_entities");
                    ConfluxContextOperations.Truncate(this, session, "cfx_entityhistory");
                    ConfluxContextOperations.Truncate(this, session, "cfx_config");
                    ConfluxContextOperations.Truncate(this, session, "cfx_transact");
                    ConfluxContextOperations.Truncate(this, session, "cfx_log");

                    ConfluxContextOperations.ResetAutoIncrement(this, session, "cfx_index");
                    ConfluxContextOperations.ResetAutoIncrement(this, session, "cfx_entities");
                    ConfluxContextOperations.ResetAutoIncrement(this, session, "cfx_entityhistory");
                    ConfluxContextOperations.ResetAutoIncrement(this, session, "cfx_config");
                    ConfluxContextOperations.ResetAutoIncrement(this, session, "cfx_transact");
                    ConfluxContextOperations.ResetAutoIncrement(this, session, "cfx_log");

                    ConfluxContextOperations.DeleteAll(this, session, "aspnetusertokens");
                    ConfluxContextOperations.DeleteAll(this, session, "aspnetuserroles");
                    ConfluxContextOperations.DeleteAll(this, session, "aspnetuserlogins");
                    ConfluxContextOperations.DeleteAll(this, session, "aspnetuserclaims");
                    ConfluxContextOperations.DeleteAll(this, session, "aspnetroleclaims");
                    ConfluxContextOperations.DeleteAll(this, session, "aspnetroles");
                    ConfluxContextOperations.DeleteAll(this, session, "aspnetusers");
                }

                processOk = true;
                logger.Info("Check/Initialization Complete");
            }
            catch (Exception ex)
            {
                string errorMsg = "An error occurred while migrating database";

                // Log the error locally
                logger.Error(ex, errorMsg);
            }
            return processOk;
        }

    }


    // =================================================================================================
    // Context Factory, Used for Migrations
    // =================================================================================================

    public class ConfluxContextFactory : IDesignTimeDbContextFactory<ConfluxContext>
    {
        public ConfluxContext CreateDbContext(string[] args)
        {
            // Manually we'll obtain the context
            // We only do this locally, if we're doing migrations, but this should only be done in a
            // specific development machine.
            // TO-DO : Change COS2 to a migration common ecosystem
            string json = File.ReadAllText(@"c:\emRoot\COS2\config\appsettings.json");
            var configuration = JsonConvert.DeserializeObject<ConfluxConfiguration>(json);
            var ConnectionString = configuration.Databases.ConnectionString;

            var builder = new DbContextOptionsBuilder<ConfluxContext>()
                .UseMySql(ConnectionString);

            return new ConfluxContext(builder.Options);
        }
    }
}
