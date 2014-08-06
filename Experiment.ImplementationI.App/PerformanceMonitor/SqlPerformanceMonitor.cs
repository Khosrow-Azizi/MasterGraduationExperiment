using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Experiment.ImplementationI.App.DataModel.Sql;


namespace Experiment.ImplementationI.App.PerformanceMonitor
{
   public class SqlPerformanceMonitor : DatabasePerformanceMonitor
   {
      public SqlPerformanceMonitor(string connectionString)
      {
         this.connectionString = connectionString;
      }

      public override double Insert(IEnumerable<Department> departments, Stopwatch stopwatch)
      {
         using (var ctx = new ExperimentContext())
         {
            ctx.Configuration.AutoDetectChangesEnabled = false;
            ctx.Configuration.ValidateOnSaveEnabled = false;            
            foreach (var department in departments)
               ctx.Department.Add(department);
            stopwatch.Restart();
            ctx.SaveChanges();
            stopwatch.Stop();
         }
         return GetTimeElapsed(stopwatch);
      }

      public override double Insert(IEnumerable<User> users, Stopwatch stopwatch)
      {
         using (var ctx = new ExperimentContext())
         {
            ctx.Configuration.AutoDetectChangesEnabled = false;
            ctx.Configuration.ValidateOnSaveEnabled = false;
            foreach (var user in users)
               ctx.User.Add(user);
            stopwatch.Restart();
            ctx.SaveChanges();
            stopwatch.Stop();
         }
         return GetTimeElapsed(stopwatch);
      }

      public override double Insert(IEnumerable<Project> projects, Stopwatch stopwatch)
      {
         using (var ctx = new ExperimentContext())
         {
            ctx.Configuration.AutoDetectChangesEnabled = false;
            ctx.Configuration.ValidateOnSaveEnabled = false;
            foreach (var project in projects)
            {
               foreach (var user in project.User1)
                  ctx.User.Attach(user);
               ctx.Project.Add(project);
            }
            stopwatch.Restart();
            ctx.SaveChanges();
            stopwatch.Stop();
         }
         return GetTimeElapsed(stopwatch);
      }

      public override double UpdateDepartmentNameByKeys(int[] keys, string newName, Stopwatch stopwatch)
      {
         if (!keys.Any())
            throw new ArgumentException("Keys array should contain at least one item.");

         List<string> paramIndexes = new List<string>();
         int keysCount = keys.Length;
         StringBuilder sb = new StringBuilder();
         using (SqlConnection sqlConnection = new SqlConnection(connectionString))
         {
            sqlConnection.Open();
            using (SqlCommand command = new SqlCommand())
            {
               command.Connection = sqlConnection;
               command.CommandType = CommandType.Text;
               sb.Append("UPDATE [dbo].[Department] SET Name = @name WHERE Id IN (");
               command.Parameters.AddWithValue("@name", newName);
               for (int i = 0; i < keysCount; i++)
               {
                  string paramIndex = "@p" + i;
                  paramIndexes.Add(paramIndex);
                  command.Parameters.AddWithValue(paramIndex, keys[i]);
               }
               sb.Append(string.Join(", ", paramIndexes) + ")");
               command.CommandText = sb.ToString();

               stopwatch.Restart();
               command.ExecuteNonQuery();
               stopwatch.Stop();
            }
            sqlConnection.Close();
         }
         return GetTimeElapsed(stopwatch);
      }

      public override double UpdateProjectNameByKeys(int[] keys, string newName, Stopwatch stopwatch)
      {
         List<string> paramNames = new List<string>();
         int keysCount = keys.Length;
         StringBuilder sb = new StringBuilder();
         using (SqlConnection sqlConnection = new SqlConnection(connectionString))
         {
            sqlConnection.Open();
            using (SqlCommand command = new SqlCommand())
            {
               command.Connection = sqlConnection;
               command.CommandType = CommandType.Text;
               sb.Append("UPDATE [dbo].[Project] SET Name = @name WHERE Id IN (");
               command.Parameters.AddWithValue("@name", newName);
               for (int i = 0; i < keysCount; i++)
               {
                  string paramIndex = "@p" + i;
                  paramNames.Add(paramIndex);
                  command.Parameters.AddWithValue(paramIndex, keys[i]);
               }
               sb.Append(string.Join(", ", paramNames) + ")");
               command.CommandText = sb.ToString();

               stopwatch.Restart();
               command.ExecuteNonQuery();
               stopwatch.Stop();
            }
            sqlConnection.Close();
         }
         return GetTimeElapsed(stopwatch);
      }

      public override double UpdateUserLastName(string firstNamePattern, string newLastName, Stopwatch stopwatch)
      {
         using (SqlConnection sqlConnection = new SqlConnection(connectionString))
         {
            sqlConnection.Open();
            using (SqlCommand command = new SqlCommand())
            {
               command.Connection = sqlConnection;
               command.CommandType = CommandType.Text;
               command.CommandText = "UPDATE [dbo].[User] SET Lastname = @newLastName WHERE FirstName LIKE @firstNamePattern";
               command.Parameters.AddWithValue("@newLastName", newLastName);
               command.Parameters.AddWithValue("@firstNamePattern", firstNamePattern);

               stopwatch.Restart();
               command.ExecuteNonQuery();
               stopwatch.Stop();
            }
            sqlConnection.Close();
         }
         return GetTimeElapsed(stopwatch);
      }

      public override double SelectDepartmentByKey(int key, Stopwatch stopwatch)
      {
         using (SqlConnection sqlConnection = new SqlConnection(connectionString))
         {
            sqlConnection.Open();
            using (SqlCommand command = new SqlCommand())
            {
               command.Connection = sqlConnection;
               command.CommandType = CommandType.Text;
               command.CommandText = "SELECT Id, Name, DateAdded FROM [dbo].[Department] WHERE Id = @id";
               command.Parameters.AddWithValue("@id", key);

               stopwatch.Restart();
               using (SqlDataReader rdr = command.ExecuteReader())
               {
                  if (rdr.HasRows)
                  {
                     while (rdr.Read())
                     {
                        Department department = new Department
                        {
                           Id = rdr.GetInt32(0),
                           Name = rdr.GetString(1),
                           DateAdded = rdr.GetDateTime(2),
                        };
                     }
                  }
               }
               stopwatch.Stop();
            }
            sqlConnection.Close();
         }
         return GetTimeElapsed(stopwatch);
      }

      public override double SelectUserByKey(int key, Stopwatch stopwatch)
      {
         using (SqlConnection sqlConnection = new SqlConnection(connectionString))
         {
            sqlConnection.Open();
            using (SqlCommand command = new SqlCommand())
            {
               command.Connection = sqlConnection;
               command.CommandType = CommandType.Text;
               command.CommandText = "SELECT Id, FirstName, LastName, Age, DepartmentId, DateAdded FROM [dbo].[User] WHERE Id = @id";
               command.Parameters.AddWithValue("@id", key);

               stopwatch.Restart();
               using (SqlDataReader rdr = command.ExecuteReader())
               {
                  if (rdr.HasRows)
                  {
                     while (rdr.Read())
                     {
                        User user = new User
                        {
                           Id = rdr.GetInt32(0),
                           FirstName = rdr.GetString(1),
                           LastName = rdr.GetString(2),
                           Age = rdr.GetInt32(3),
                           DepartmentId = rdr.GetInt32(4),
                           DateAdded = rdr.GetDateTime(5),
                        };
                     }
                  }
                  stopwatch.Stop();
                  rdr.Close();
               }
            }
            sqlConnection.Close();
         }
         return GetTimeElapsed(stopwatch);
      }

      public override double SelectDepartmentByName(string name, Stopwatch stopwatch)
      {
         using (SqlConnection sqlConnection = new SqlConnection(connectionString))
         {
            sqlConnection.Open();
            using (SqlCommand command = new SqlCommand())
            {
               command.Connection = sqlConnection;
               command.CommandType = CommandType.Text;
               command.CommandText = "SELECT Id, Name, DateAdded FROM [dbo].[Department] WHERE Name LIKE @name";
               command.Parameters.AddWithValue("@name", name);

               stopwatch.Restart();
               using (SqlDataReader rdr = command.ExecuteReader())
               {
                  if (rdr.HasRows)
                  {
                     while (rdr.Read())
                     {
                        Department department = new Department
                        {
                           Id = rdr.GetInt32(0),
                           Name = rdr.GetString(1),
                           DateAdded = rdr.GetDateTime(2),
                        };
                     }
                  }
                  stopwatch.Stop();
                  rdr.Close();
               }
            }
            sqlConnection.Close();
         }
         return GetTimeElapsed(stopwatch);
      }

      public override double SelectUsersByFirstName(string firstName, Stopwatch stopwatch)
      {
         List<User> users = new List<User>();
         using (SqlConnection sqlConnection = new SqlConnection(connectionString))
         {
            sqlConnection.Open();
            using (SqlCommand command = new SqlCommand())
            {
               command.Connection = sqlConnection;
               command.CommandType = CommandType.Text;
               command.CommandText = "SELECT Id, FirstName, LastName, Age, DepartmentId, DateAdded FROM [dbo].[User] WHERE FirstName LIKE @firstName";
               command.Parameters.AddWithValue("@firstName", firstName);

               stopwatch.Restart();
               using (SqlDataReader rdr = command.ExecuteReader())
               {
                  if (rdr.HasRows)
                  {
                     while (rdr.Read())
                     {
                        users.Add(new User
                        {
                           Id = rdr.GetInt32(0),
                           FirstName = rdr.GetString(1),
                           LastName = rdr.GetString(2),
                           Age = rdr.GetInt32(3),
                           DepartmentId = rdr.GetInt32(4),
                           DateAdded = rdr.GetDateTime(5),
                        });
                     }
                  }
                  stopwatch.Stop();
                  rdr.Close();
               }
            }
            sqlConnection.Close();
         }
         return GetTimeElapsed(stopwatch);
      }

      public override double SelectDepartmentsByUser(string userFirstName, Stopwatch stopwatch)
      {
         StringBuilder sb = new StringBuilder();
         List<Department> departments = new List<Department>();
         using (SqlConnection sqlConnection = new SqlConnection(connectionString))
         {
            sqlConnection.Open();
            using (SqlCommand command = new SqlCommand())
            {
               command.Connection = sqlConnection;
               command.CommandType = CommandType.Text;
               sb.Append("SELECT [dbo].[Department].Id, [dbo].[Department].Name, [dbo].[Department].DateAdded FROM [dbo].[Department] ");
               sb.Append("INNER JOIN [dbo].[User] ON [dbo].[Department].[Id] = [dbo].[User].[DepartmentId] ");
               sb.Append("WHERE [dbo].[User].[FirstName] LIKE @userFirstName");
               command.CommandText = sb.ToString();
               command.Parameters.AddWithValue("@userFirstName", userFirstName);

               stopwatch.Restart();
               using (SqlDataReader rdr = command.ExecuteReader())
               {
                  if (rdr.HasRows)
                  {
                     while (rdr.Read())
                     {
                        departments.Add(new Department
                        {
                           Id = rdr.GetInt32(0),
                           Name = rdr.GetString(1),
                           DateAdded = rdr.GetDateTime(2),
                        });
                     }
                  }
                  stopwatch.Stop();
                  rdr.Close();
               }
            }
            sqlConnection.Close();
         }
         return GetTimeElapsed(stopwatch);
      }

      public override double SelectUsersByProjects(int[] projectKeys, Stopwatch stopwatch)
      {
         if (!projectKeys.Any())
            throw new ArgumentException("ProjectKeys array should contain at least one item.");
         StringBuilder sb = new StringBuilder();
         List<User> users = new List<User>();
         using (SqlConnection sqlConnection = new SqlConnection(connectionString))
         {
            sqlConnection.Open();
            using (SqlCommand command = new SqlCommand())
            {
               command.Connection = sqlConnection;
               command.CommandType = CommandType.Text;
               sb.Append("SELECT DISTINCT [dbo].[User].Id, [dbo].[User].FirstName, [dbo].[User].LastName, ");
               sb.Append("[dbo].[User].Age, [dbo].[User].DepartmentId, [dbo].[User].DateAdded FROM [dbo].[User] ");
               sb.Append("INNER JOIN [dbo].[ProjectUser] ON [dbo].[User].[Id] = [dbo].[ProjectUser].[UserId] ");
               sb.Append("WHERE [dbo].[ProjectUser].[ProjectId] IN (");
               List<string> paramNames = new List<string>();
               int keysCount = projectKeys.Length;
               for (int i = 0; i < keysCount; i++)
               {
                  string paramIndex = "@p" + i;
                  paramNames.Add(paramIndex);
                  command.Parameters.AddWithValue(paramIndex, projectKeys[i]);
               }
               sb.Append(string.Join(", ", paramNames) + ")");
               command.CommandText = sb.ToString();

               stopwatch.Restart();
               using (SqlDataReader rdr = command.ExecuteReader())
               {
                  if (rdr.HasRows)
                  {
                     while (rdr.Read())
                     {
                        users.Add(new User
                        {
                           Id = rdr.GetInt32(0),
                           FirstName = rdr.GetString(1),
                           LastName = rdr.GetString(2),
                           Age = rdr.GetInt32(3),
                           DepartmentId = rdr.GetInt32(4),
                           DateAdded = rdr.GetDateTime(5),
                        });
                     }
                  }
                  stopwatch.Stop();
                  rdr.Close();
               }
            }
            sqlConnection.Close();
         }
         return GetTimeElapsed(stopwatch);
      }

      public override double SelectAverageAgeByProjectsMethodI(int[] projectKeys, Stopwatch stopwatch)
      {
         if (!projectKeys.Any())
            throw new ArgumentException("ProjectKeys array should contain at least one item.");
         StringBuilder sb = new StringBuilder();
         using (SqlConnection sqlConnection = new SqlConnection(connectionString))
         {
            sqlConnection.Open();
            using (SqlCommand command = new SqlCommand())
            {
               command.Connection = sqlConnection;
               command.CommandType = CommandType.Text;

               sb.Append("SELECT AVG([dbo].[User].Age) AS AverageAge FROM [dbo].[User] ");
               sb.Append("INNER JOIN [dbo].[ProjectUser] ON [dbo].[User].[Id] = [dbo].[ProjectUser].[UserId] ");
               sb.Append("WHERE [dbo].[ProjectUser].[ProjectId] IN (");
               List<string> paramNames = new List<string>();
               int keysCount = projectKeys.Length;
               for (int i = 0; i < keysCount; i++)
               {
                  string paramIndex = "@p" + i;
                  paramNames.Add(paramIndex);
                  command.Parameters.AddWithValue(paramIndex, projectKeys[i]);
               }
               sb.Append(string.Join(", ", paramNames) + ")");
               command.CommandText = sb.ToString();

               stopwatch.Restart();
               using (SqlDataReader rdr = command.ExecuteReader())
               {
                  if (rdr.HasRows)
                  {
                     while (rdr.Read())
                     {
                        var averageAge = rdr[0];
                     }
                  }
                  stopwatch.Stop();
                  rdr.Close();
               }
            }
            sqlConnection.Close();
         }
         return GetTimeElapsed(stopwatch);
      }

      public override double SelectAverageAgeByProjectsMethodII(int[] projectKeys, Stopwatch stopwatch)
      {
         return -1;
      }

      private readonly string connectionString;
   }
}
