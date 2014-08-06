using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Experiment.Common.DatabaseManager;
using Experiment.Common.DataModel;
using Experiment.ImplementationI.App.DataModel.Sql;

namespace Experiment.ImplementationI.App.DatabaseManager.Sql
{
   public class SqlDatabaseManager : IDatabaseManager
   {
      public SqlDatabaseManager(string connectionString)
      {
         this.connectionString = connectionString;
      }

      public long GetTotalDepartmentCount()
      {
         return GetTotalCount<Department>();
      }

      public long GetTotalProjectCount()
      {
         return GetTotalCount<Project>();
      }

      public long GetTotalUserCount()
      {
         return GetTotalCount<User>();
      }

      public int GetNewDepartmentId()
      {
         return GetNewId<Department>();
      }

      public int GetNewUserId()
      {
         return GetNewId<User>();
      }

      public int GetNewProjectId()
      {
         return GetNewId<Project>();
      }

      public int[] GetAllDepartmentKeys()
      {
         return GetAllKeys<Department>();
      }

      public int[] GetAllUserKeys()
      {
         return GetAllKeys<User>();
      }

      public int[] GetAllProjectKeys()
      {
         return GetAllKeys<Project>();
      }

      public Dictionary<int, string> GetDepartmentKeysAndNames(float portion)
      {
         return GetKeyAndNames<Department>(portion);
      }

      public Dictionary<int, string> GetProjectKeysAndNames(float portion)
      {
         return GetKeyAndNames<Project>(portion);
      }

      public int[] GetDepartmentKeys(float portion)
      {
         return GetKeys<Department>(portion);
      }

      public int[] GetProjectKeys(float portion)
      {
         return GetKeys<Project>(portion);
      }

      private int[] GetKeys<T>(float portion) where T : IExperimentClass
      {
         long amount = ConvertToCount<T>(portion);
         List<int> keys = new List<int>();
         using (SqlConnection sqlConnection = new SqlConnection(connectionString))
         {
            string query = string.Format(
               "SELECT TOP {0} Id FROM [dbo].[{1}]",
               amount, typeof(T).Name);
            sqlConnection.Open();
            using (var rdr = new SqlCommand(query, sqlConnection).ExecuteReader())
            {
               if (rdr.HasRows)
               {
                  while (rdr.Read())
                  {
                     keys.Add(Convert.ToInt32(rdr["Id"]));
                  }
               }
               rdr.Close();
            }
            sqlConnection.Close();
         }
         return keys.ToArray();
      }

      public void UpdateDepartment(Dictionary<int, string> keyAndNames)
      {
         UpdateName<Department>(keyAndNames);
      }

      public int GetRandomDepartmentKey()
      {
         return GetRandomKey<Department>();
      }

      public int GetRandomProjectKey()
      {
         return GetRandomKey<Project>();
      }

      public int GetRandomUserKey()
      {
         return GetRandomKey<User>();
      }

      public string GetRandomDepartmentName()
      {
         return GetRandomName<Department>();
      }

      public string GetRandomProjectName()
      {
         return GetRandomName<Project>();
      }

      public string GetRandomUserFirstName()
      {
         return GetRandomUserFirstName<User>();
      }

      public string GetLatestDepartmentName()
      {
         return GetRandomName<Department>();
      }

      public string GetLatestProjectName()
      {
         return GetRandomName<Project>();
      }

      public void DeleteAll()
      {
         using (SqlConnection sqlConnection = new SqlConnection(connectionString))
         {
            sqlConnection.Open();
            var query = string.Format("{0} {1} {2} {3}",
               "Delete [dbo].[ProjectUser]",
               "Delete [dbo].[Project]",
               "Delete [dbo].[User]",
               "Delete [dbo].[Department]");
            new SqlCommand(query, sqlConnection).ExecuteNonQuery();
            sqlConnection.Close();
         }
      }

      private long GetTotalCount<T>() where T : IExperimentClass
      {
         long count = 0;
         using (SqlConnection sqlConnection = new SqlConnection(connectionString))
         {
            string query = string.Format(
               "SELECT COUNT(*) AS Total From [dbo].[{0}]",
               typeof(T).Name);
            sqlConnection.Open();
            using (var rdr = new SqlCommand(query, sqlConnection).ExecuteReader())
            {
               if (rdr.HasRows)
               {
                  while (rdr.Read())
                  {
                     count = Convert.ToInt32(rdr["Total"]);
                  }
               }
               rdr.Close();
            }
            sqlConnection.Close();
         }
         return count;
      }

      private int GetNewId<T>() where T : IExperimentClass
      {
         int id = 1;
         using (SqlConnection sqlConnection = new SqlConnection(connectionString))
         {
            sqlConnection.Open();
            string query = string.Format("SELECT Max(Id) AS MaxId FROM [dbo].[{0}]", typeof(T).Name);
            using (var rdr = new SqlCommand(query, sqlConnection).ExecuteReader())
            {
               if (rdr.HasRows)
               {
                  while (rdr.Read())
                  {
                     id = DBNull.Value.Equals(rdr["MaxId"]) ? id : Convert.ToInt32(rdr["MaxId"]) + 1;
                  }
               }
               rdr.Close();
            }
            sqlConnection.Close();
         }
         return id;
      }

      private int[] GetAllKeys<T>() where T : IExperimentClass
      {
         List<int> ids = new List<int>();
         using (SqlConnection sqlConnection = new SqlConnection(connectionString))
         {
            sqlConnection.Open();
            string query = string.Format("SELECT Id FROM [dbo].[{0}]", typeof(T).Name);
            using (var rdr = new SqlCommand(query, sqlConnection).ExecuteReader())
            {
               if (rdr.HasRows)
               {
                  while (rdr.Read())
                  {
                     ids.Add(Convert.ToInt32(rdr["Id"]));
                  }
               }
               rdr.Close();
            }
            sqlConnection.Close();
         }
         return ids.ToArray();
      }

      private Dictionary<int, string> GetKeyAndNames<T>(float portion) where T : IExperimentClass
      {
         long amount = ConvertToCount<T>(portion);
         Dictionary<int, string> keyAndNames = new Dictionary<int, string>();
         using (SqlConnection sqlConnection = new SqlConnection(connectionString))
         {
            string query = string.Format(
               "SELECT TOP {0} Id, Name FROM [dbo].[{1}]",
               amount, typeof(T).Name);
            sqlConnection.Open();
            using (var rdr = new SqlCommand(query, sqlConnection).ExecuteReader())
            {
               if (rdr.HasRows)
               {
                  while (rdr.Read())
                  {
                     keyAndNames.Add(Convert.ToInt32(rdr["Id"]), Convert.ToString(rdr["Name"]));
                  }
               }
               rdr.Close();
            }
            sqlConnection.Close();
         }
         return keyAndNames;
      }

      private void UpdateName<T>(Dictionary<int, string> keyAndNames) where T : IHasName
      {
         string query = "";
         using (SqlConnection sqlConnection = new SqlConnection(connectionString))
         {
            sqlConnection.Open();
            foreach (var item in keyAndNames)
            {
               query = string.Format("UPDATE [dbo].[{0}] SET Name = '{1}' WHERE Id = {2}",
               typeof(T).Name, item.Value, item.Key);
               var reader = new SqlCommand(query, sqlConnection).ExecuteReader();
               reader.Close();
               reader.Dispose();
            }
         }
      }

      private int GetRandomKey<T>() where T : IExperimentClass
      {
         int key = -1;
         using (SqlConnection sqlConnection = new SqlConnection(connectionString))
         {
            string query = string.Format(
               "SELECT TOP 1 Id FROM [dbo].[{0}] ORDER BY NEWID()", typeof(T).Name);
            sqlConnection.Open();
            using (var rdr = new SqlCommand(query, sqlConnection).ExecuteReader())
            {
               if (rdr.HasRows)
               {
                  while (rdr.Read())
                  {
                     key = Convert.ToInt32(rdr["Id"]);
                  }
               }
               rdr.Close();
            }
            sqlConnection.Close();
         }
         return key;
      }

      private string GetRandomName<T>() where T : IHasName
      {
         string name = string.Empty;
         using (SqlConnection sqlConnection = new SqlConnection(connectionString))
         {
            string query = string.Format(
               "SELECT TOP 1 Name FROM [dbo].[{0}] ORDER BY NEWID()", typeof(T).Name);
            sqlConnection.Open();
            using (var rdr = new SqlCommand(query, sqlConnection).ExecuteReader())
            {
               if (rdr.HasRows)
               {
                  while (rdr.Read())
                  {
                     name = Convert.ToString(rdr["Name"]);
                  }
               }
               rdr.Close();
            }
            sqlConnection.Close();
         }
         return name;
      }

      private string GetRandomUserFirstName<T>() where T : IHasFirstName
      {
         string firstName = string.Empty;
         using (SqlConnection sqlConnection = new SqlConnection(connectionString))
         {
            string query = string.Format("SELECT TOP 1 FirstName FROM [dbo].[{0}] ORDER BY NEWID()", typeof(T).Name);
            sqlConnection.Open();
            using (var rdr = new SqlCommand(query, sqlConnection).ExecuteReader())
            {
               if (rdr.HasRows)
               {
                  while (rdr.Read())
                  {
                     firstName = Convert.ToString(rdr["FirstName"]);
                  }
               }
               rdr.Close();
            }
            sqlConnection.Close();
         }
         return firstName;
      }

      private string GetLatestName<T>() where T : IHasName
      {
         string name = string.Empty;
         using (SqlConnection sqlConnection = new SqlConnection(connectionString))
         {
            sqlConnection.Open();
            string query = string.Format("SELECT TOP 1 Name FROM [dbo].[{0}] ORDER BY Id DESC", typeof(T).Name);
            using (var rdr = new SqlCommand(query, sqlConnection).ExecuteReader())
            {
               if (rdr.HasRows)
               {
                  while (rdr.Read())
                  {
                     name = Convert.ToString(rdr["Name"]);
                  }
               }
               rdr.Close();
            }
            sqlConnection.Close();
         }
         return name;
      }

      private long ConvertToCount<T>(float portion) where T : IExperimentClass
      {
         long count = GetTotalCount<T>();
         float ratio = portion;
         if (ratio < 0)
            ratio = 0;
         if (portion > 1)
            ratio = 1;
         var result = (ratio * count);
         if (result > 0 && result < 1)
            result = 1;
         return (long)result;
      }

      private readonly string connectionString;
   }
}
