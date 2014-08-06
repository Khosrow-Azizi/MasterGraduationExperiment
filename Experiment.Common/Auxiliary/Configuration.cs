using System;
using System.Collections.Generic;
using System.Data.EntityClient;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Experiment.Common.Properties;

namespace Experiment.Common.Auxiliary
{
   public class Configurations
   {
      public static string SqlConnectionString
      {
         get
         {
            return Settings.Default.RunLocal ?
                @Settings.Default.LocalSqlConnectionString :
                @Settings.Default.RemoteSqlConnectionString;
         }
      }

      public static string MongoConnectionString
      {
         get
         {
            return Settings.Default.RunLocal ?
                @Settings.Default.LocalMongoConnectionString :
                @Settings.Default.RemoteMongoConnectionString;
         }
      }

      public static string DatabaseName
      {
         get
         {
            return Settings.Default.DatabaseName;
         }
      }

      public static string ResultDbEFConnectionString
      {
         get
         {
            return new EntityConnectionStringBuilder
            {
               Metadata = "res://*",
               Provider = "System.Data.SqlClient",
               ProviderConnectionString = new SqlConnectionStringBuilder
               {
                  InitialCatalog = Settings.Default.ResultDatabaseName,
                  DataSource = Settings.Default.RunLocal ?
                  @Settings.Default.LocalResultDataSource :
                  @Settings.Default.RemoteResultDataSource,
                  IntegratedSecurity = Settings.Default.RunLocal ? true : false,
                  PersistSecurityInfo = Settings.Default.RunLocal ?  false : true,
                  UserID = "MasterExperiment",
                  Password = "master",
               }.ConnectionString
            }.ConnectionString;
         }
      }

      public static string DbEFConnectionString
      {
         get
         {
            return new EntityConnectionStringBuilder
            {
               Metadata = "res://*",
               Provider = "System.Data.SqlClient",
               ProviderConnectionString = new SqlConnectionStringBuilder
               {
                  InitialCatalog = Settings.Default.DatabaseName,
                  DataSource = Settings.Default.RunLocal ?
                  @Settings.Default.LocalSqlDataSource :
                  @Settings.Default.RemoteSqlDataSource,
                  IntegratedSecurity = Settings.Default.RunLocal ? true : false,
                  PersistSecurityInfo = Settings.Default.RunLocal ? false : true,
                  UserID = "MasterExperiment",
                  Password = "master",
               }.ConnectionString
            }.ConnectionString;
         }
      }

      public static string ResultDbSqlConnectionString
      {
         get
         {
            return new SqlConnectionStringBuilder
              {
                 InitialCatalog = Settings.Default.ResultDatabaseName,
                 DataSource = Settings.Default.RunLocal ?
                 @Settings.Default.LocalResultDataSource :
                 @Settings.Default.RemoteResultDataSource,
                 IntegratedSecurity = Settings.Default.RunLocal ? true : false,
                 PersistSecurityInfo = Settings.Default.RunLocal ? false : true,
                 UserID = "MasterExperiment",
                 Password = "master",
              }.ConnectionString;
         }
      }
   }
}
