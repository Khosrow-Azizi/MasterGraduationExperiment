using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Experiment.Common.Auxiliary;
using Experiment.Common.DatabaseManager;
using Experiment.Common.DataRecorder;
using Experiment.Common.DataRecorder.Db;
using Experiment.ImplementationI.App.DatabaseManager.Mongo;
using Experiment.ImplementationI.App.DatabaseManager.Sql;
using Experiment.ImplementationI.App.DataModel.Mongo;
using Experiment.ImplementationI.App.DataModel.Sql;
using Experiment.ImplementationI.App.PerformanceMonitor;

namespace Experiment.ImplementationI.App
{
   public class ExperimentRunner
   {
      public static void Main(string[] args)
      {
         Process proc = Process.GetCurrentProcess();
         ProcessThread thread = proc.Threads[0];
         thread.IdealProcessor = 1;
         thread.ProcessorAffinity = (IntPtr)2;

         ExperimentRunner expRunner = null;
         bool waitForUser = true;
         while (waitForUser)
         {
            Console.WriteLine("***** Program: Experiment Implementation I *****");
            Console.WriteLine("Which part of the Experiment would you like to run?");
            Console.WriteLine("     Press 1 for SQL Server");
            Console.WriteLine("     Press 2 for MongoDB");
            Console.WriteLine("     Press 6 to stop the program");

            int answer;
            int.TryParse(Console.ReadLine(), out answer);
            switch (answer)
            {
               case -1:
                  {
                     expRunner = new ExperimentRunner(DataBaseTypeEnums.UnitTest);
                     waitForUser = false;
                  }
                  break;
               case 1:
                  {
                     expRunner = new ExperimentRunner(DataBaseTypeEnums.SqlServerImplementationI);
                     waitForUser = false;
                  }
                  break;
               case 2:
                  {
                     expRunner = new ExperimentRunner(DataBaseTypeEnums.MongoDbImplementationI);
                     waitForUser = false;
                  }
                  break;
               case 6:
                  waitForUser = false;
                  break;
               default:
                  Console.Beep();
                  Console.WriteLine("Please select a number from the options list..");
                  break;
            }
         }

         if (expRunner != null)
            expRunner.RunAll();

         Console.Write("\nStopping the program...");
         int count = 4;
         while (count > 0)
         {
            Thread.Sleep(1000);
            Console.Write("...");
            count--;
         }
         Console.WriteLine("\nPress any key to close the program.");
         Console.ReadLine();
      }

      public ExperimentRunner(DataBaseTypeEnums databaseType)
      {
         switch (databaseType)
         {
            case DataBaseTypeEnums.UnitTest:
            case DataBaseTypeEnums.SqlServerImplementationI:
               {
                  this.dbPerformanceMonitor = new SqlPerformanceMonitor(Configurations.SqlConnectionString);
                  this.dbManager = new SqlDatabaseManager(Configurations.SqlConnectionString);
               }
               break;
            case DataBaseTypeEnums.MongoDbImplementationI:
               {
                  this.dbPerformanceMonitor = new MongoPerformanceMonitor(Configurations.MongoConnectionString, Configurations.DatabaseName);
                  this.dbManager = new MongoDatabaseManager(Configurations.MongoConnectionString, Configurations.DatabaseName);
               }
               break;
            default:
               Console.WriteLine("There is no performance monitor for the given database type.");
               return;
         }

         this.databaseType = databaseType;
         this.recorder = new Recorder(databaseType);
         this.stopwatch = new Stopwatch();
         this.results = new List<PerformanceResult>();
      }

      public void RunAll()
      {
         Console.WriteLine(string.Format("Running the experiment with database type '{0}'..", databaseType.ToString()));
         Console.WriteLine("Start time: " + DateTime.Now);
         if (!FlushDatabases())
            return;
         Thread.Sleep(10000);
         if (Run(TestCaseEnums.Initialize))
            if (Run(TestCaseEnums.TestCase1))
               if (Run(TestCaseEnums.TestCase2))
                  if (Run(TestCaseEnums.TestCase3))
                     if (Run(TestCaseEnums.TestCase4))
                        if (Run(TestCaseEnums.TestCase5))
                           if (Run(TestCaseEnums.TestCase6))
                              Run(TestCaseEnums.TestCase7);

         SaveRandomData();
         Console.WriteLine("The experiment finished successfully.");
         Console.WriteLine("End time: " + DateTime.Now);
      }

      public bool Run(TestCaseEnums testCase)
      {
         Console.WriteLine(string.Format("Test case '{0}' has started..", testCase.ToString()));
         RunConfiguration config = RunConfiguration.GetConfigurations(testCase);

         stopwatch.Reset();
         results.Clear();

         if (!RunInserts(config, stopwatch, results))
            return false;

         if (!RunUpdateDepartmentName(config, stopwatch, results))
            return false;

         // restore department names to their original values
         if (!RestoreDepartmentsUniqueness())
            return false;

         if (!RunUpdateUserLastName(config, stopwatch, results))
            return false;

         if (!RunUpdateProjectName(config, stopwatch, results))
            return false;

         if (!RunSelectDepartmentByKey(config, stopwatch, results))
            return false;

         if (!RunSelectDepartmentByRandomName(config, stopwatch, results))
            return false;

         if (!RunSelectUserByKey(config, stopwatch, results))
            return false;

         if (!RunSelectUserByRandomFirstName(config, stopwatch, results))
            return false;

         if (!RunSelectDepartmentByRandomUser(config, stopwatch, results))
            return false;

         if (!RunSelectUserByRandomProject(config, stopwatch, results))
            return false;

         if (!RunSelectAvgUserAgeByProjectsMethodI(config, stopwatch, results))
            return false;

         if (!RunSelectAvgUserAgeByProjectsMethodII(config, stopwatch, results))
            return false;

         Console.WriteLine("Saving result data in the database..");
         recorder.Record(results);
         Console.WriteLine("Saving result data succeeded.");

         results.Clear();
         stopwatch.Stop();
         Console.WriteLine(string.Format("Test case '{0}' successfully completed.", testCase.ToString()));
         Console.WriteLine("--------------------------------------------------------------------------");
         Thread.Sleep(10000);
         return true;
      }

      private bool RunInserts(RunConfiguration config, Stopwatch sw, List<PerformanceResult> resultsTobeRecorded)
      {
         Console.WriteLine("Inserts tests has started..");
         HashSet<Department> departments = new HashSet<Department>();
         HashSet<User> users = new HashSet<User>();
         HashSet<Project> projects = new HashSet<Project>();
         Random randomGeneratore = new Random();

         try
         {
            for (int r = 0; r < config.NumberOfInsertRuns; r++)
            {
               departments.Clear();
               users.Clear();
               projects.Clear();
               Console.WriteLine("Run count: " + r);
               Console.WriteLine("Inserting departments..");

               int namePostfix = GetPostfix(dbManager.GetLatestDepartmentName());
               int depId = dbManager.GetNewDepartmentId();
               for (int i = 0; i < config.TotalNumberOfDepartments; i++)
               {
                  departments.Add(new Department
                  {
                     Id = depId,
                     Name = DepartmentNamePrefix + (++namePostfix),
                     DateAdded = DateTime.Now,
                  });
                  depId++;
               }
               resultsTobeRecorded.Add(new PerformanceResult
               {
                  ExecutionTime = dbPerformanceMonitor.Insert(departments, sw),
                  TestCase = config.TestCase,
                  TestScenario = TestScenarioEnums.InsertDepartment,
               });

               Console.WriteLine("Inserting departments succeeded.");

               // insert users with random departments
               Console.WriteLine("Inserting users..");
               int userId = dbManager.GetNewUserId();
               for (int i = 0; i < config.TotalNumberOfUsers; i++)
               {
                  users.Add(new User
                  {
                     Id = userId,
                     FirstName = GenerateRandomFirstName(),
                     LastName = LastNamePrefix + randomGeneratore.Next(i, MaxNamePostfix),
                     Age = randomGeneratore.Next(20, 67),
                     DepartmentId = departments.ElementAt(randomGeneratore.Next(0, departments.Count - 1)).Id,
                     DateAdded = DateTime.Now,
                  });
                  userId++;
               }
               resultsTobeRecorded.Add(new PerformanceResult
               {
                  ExecutionTime = dbPerformanceMonitor.Insert(users, sw),
                  TestCase = config.TestCase,
                  TestScenario = TestScenarioEnums.InsertUser,
               });
               Console.WriteLine("Inserting users succeeded.");

               // insert projects
               Console.WriteLine("Inserting projects..");
               namePostfix = GetPostfix(dbManager.GetLatestProjectName());
               int projectId = dbManager.GetNewProjectId();
               for (int i = 0; i < config.TotalNumberOfProjects; i++)
               {
                  Project project = new Project
                   {
                      Id = projectId,
                      Name = ProjectNamePrefix + (++namePostfix),
                      DepartmentId = departments.ElementAt(randomGeneratore.Next(0, departments.Count - 1)).Id,
                      ManagerId = users.ElementAt(randomGeneratore.Next(0, users.Count - 1)).Id,
                      DateAdded = DateTime.Now,
                   };
                  // add random users to project              
                  if (users.Count < MaxUserPerProject)
                  {
                     while (project.User1.Count < users.Count)
                        project.User1.Add(users.ElementAt(randomGeneratore.Next(0, users.Count - 1)));
                  }
                  else
                  {
                     while (project.User1.Count < MaxUserPerProject)
                        project.User1.Add(users.ElementAt(randomGeneratore.Next(0, users.Count - 1)));
                  }
                  projects.Add(project);
                  projectId++;
               }
               resultsTobeRecorded.Add(new PerformanceResult
               {
                  ExecutionTime = dbPerformanceMonitor.Insert(projects, sw),
                  TestCase = config.TestCase,
                  TestScenario = TestScenarioEnums.InsertProject,
               });
               Console.WriteLine("Inserting projects succeeded.");
            }
            Console.WriteLine("Insert tests completed.");
            return true;
         }
         catch (Exception ex)
         {
            Console.WriteLine("Failed to complete Insert tests. Error: " + ex.Message);
            return false;
         }
      }

      private bool RunUpdateDepartmentName(RunConfiguration config, Stopwatch sw, List<PerformanceResult> resultsTobeRecorded)
      {
         Console.WriteLine(string.Format("Update tests with scenario '{0}' has started..",
           TestScenarioEnums.UpdateDepartmentNameByKeys.ToString()));

         try
         {
            // get 1/10 of the departments
            if (uniqueDepartmentKeyAndNames == null)
               uniqueDepartmentKeyAndNames = dbManager.GetDepartmentKeysAndNames((float)0.1);
            var keys = uniqueDepartmentKeyAndNames.Keys.ToArray();
            // bundle keys in batches in case the number of keys exceeds the maximum sql parameters
            var keysBundles = GetKeyBundles(keys);
            for (int i = 0; i < config.NumberOfUpdateRuns; i++)
            {
               Console.WriteLine("Run count: " + i);
               foreach (var bundle in keysBundles)
               {
                  resultsTobeRecorded.Add(new PerformanceResult
                  {
                     ExecutionTime = dbPerformanceMonitor.UpdateDepartmentNameByKeys(bundle, NewDepartmentName, sw),
                     TestCase = config.TestCase,
                     TestScenario = TestScenarioEnums.UpdateDepartmentNameByKeys,
                  });
               }
            }
            Console.WriteLine("Upadte test completed.");
            return true;
         }
         catch (Exception ex)
         {
            Console.WriteLine("Failed to complete Update tests. Error: " + ex.Message);
            return false;
         }
      }

      private bool RunUpdateUserLastName(RunConfiguration config, Stopwatch sw, List<PerformanceResult> resultsTobeRecorded)
      {
         Console.WriteLine(string.Format("Update tests with scenario '{0}' has started..",
          TestScenarioEnums.UpdateUserLastNameByFirstName.ToString()));
         try
         {
            for (int i = 0; i < config.NumberOfUpdateRuns; i++)
            {
               Console.WriteLine("Run count: " + i);
               if (RandomUserFirsftNames.Count <= i)
               {
                  // make a mixture of existing and possibly not existing firstnames
                  if (i / 2 == 0)
                     RandomUserFirsftNames.Add(dbManager.GetRandomUserFirstName());
                  else
                     RandomUserFirsftNames.Add(GenerateRandomFirstName());
               }

               resultsTobeRecorded.Add(new PerformanceResult
               {
                  ExecutionTime = dbPerformanceMonitor.UpdateUserLastName(RandomUserFirsftNames[i], NewUserLastNamePrefix, sw),
                  TestCase = config.TestCase,
                  TestScenario = TestScenarioEnums.UpdateUserLastNameByFirstName,
               });
            }
            Console.WriteLine("Upadte test completed.");
            return true;
         }
         catch (Exception ex)
         {
            Console.WriteLine("Failed to complete Update tests. Error: " + ex.Message);
            return false;
         }
      }

      private bool RunUpdateProjectName(RunConfiguration config, Stopwatch sw, List<PerformanceResult> resultsTobeRecorded)
      {
         Console.WriteLine(string.Format("Update tests with scenario '{0}' has started..",
          TestScenarioEnums.UpdateProjectNameByKeys.ToString()));
         string newName = string.Empty;
         try
         {
            // get 1/4 of the projects
            int[] projectKeys = dbManager.GetProjectKeys((float)0.25);

            // bundle keys in batches in case the number of keys exceeds the maximum sql parameters
            var projectKeysBundles = GetKeyBundles(projectKeys);
            for (int i = 0; i < config.NumberOfUpdateRuns; i++)
            {
               Console.WriteLine("Run number: " + i);
               newName = NewProjectNamePrefix + i;
               foreach (var bundle in projectKeysBundles)
               {
                  resultsTobeRecorded.Add(new PerformanceResult
                  {
                     ExecutionTime = dbPerformanceMonitor.UpdateProjectNameByKeys(bundle, newName, sw),
                     TestCase = config.TestCase,
                     TestScenario = TestScenarioEnums.UpdateProjectNameByKeys,
                  });
               }
            }
            Console.WriteLine("Upadte test completed.");
            return true;
         }
         catch (Exception ex)
         {
            Console.WriteLine("Failed to complete Update tests. Error: " + ex.Message);
            return false;
         }
      }

      private bool RunSelectDepartmentByKey(RunConfiguration config, Stopwatch sw, List<PerformanceResult> resultsTobeRecorded)
      {
         Console.WriteLine(string.Format("Select tests with scenario '{0}' has started..",
          TestScenarioEnums.SelectDepartmentByKey.ToString()));
         try
         {
            for (int i = 0; i < config.NumberOfSelectRuns; i++)
            {
               Console.WriteLine("Run number: " + i);
               if (RandomDepartmentIds.Count <= i)
                  RandomDepartmentIds.Add(dbManager.GetRandomDepartmentKey().ToString());

               resultsTobeRecorded.Add(new PerformanceResult
               {
                  ExecutionTime = dbPerformanceMonitor.SelectDepartmentByKey(int.Parse(RandomDepartmentIds[i]), sw),
                  TestCase = config.TestCase,
                  TestScenario = TestScenarioEnums.SelectDepartmentByKey,
               });
            }
            Console.WriteLine("Select test completed.");
            return true;
         }
         catch (Exception ex)
         {
            Console.WriteLine("Failed to complete Select tests. Error: " + ex.Message);
            return false;
         }
      }

      private bool RunSelectDepartmentByRandomName(RunConfiguration config, Stopwatch sw, List<PerformanceResult> resultsTobeRecorded)
      {
         Console.WriteLine(string.Format("Select tests with scenario '{0}' has started..",
          TestScenarioEnums.SelectDepartmentByRandomName.ToString()));
         try
         {
            for (int i = 0; i < config.NumberOfSelectRuns; i++)
            {
               Console.WriteLine("Run number: " + i);
               if (RandomDepartmentNames.Count <= i)
                  RandomDepartmentNames.Add(dbManager.GetRandomDepartmentName());

               resultsTobeRecorded.Add(new PerformanceResult
               {
                  ExecutionTime = dbPerformanceMonitor.SelectDepartmentByName(RandomDepartmentNames[i], sw),
                  TestCase = config.TestCase,
                  TestScenario = TestScenarioEnums.SelectDepartmentByRandomName,
               });
            }
            Console.WriteLine("Select test completed.");
            return true;
         }
         catch (Exception ex)
         {
            Console.WriteLine("Failed to complete Select tests. " + ex.Message);
            return false;
         }
      }

      private bool RunSelectUserByKey(RunConfiguration config, Stopwatch sw, List<PerformanceResult> resultsTobeRecorded)
      {
         Console.WriteLine(string.Format("Select tests with scenario '{0}' has started..",
         TestScenarioEnums.SelectUserByKey.ToString()));
         try
         {
            for (int i = 0; i < config.NumberOfSelectRuns; i++)
            {
               Console.WriteLine("Run number: " + i);
               if (RandomUserIds.Count <= i)
                  RandomUserIds.Add(dbManager.GetRandomUserKey().ToString());

               resultsTobeRecorded.Add(new PerformanceResult
               {
                  ExecutionTime = dbPerformanceMonitor.SelectUserByKey(int.Parse(RandomUserIds[i]), sw),
                  TestCase = config.TestCase,
                  TestScenario = TestScenarioEnums.SelectUserByKey,
               });
            }
            Console.WriteLine("Select test completed.");
            return true;
         }
         catch (Exception ex)
         {
            Console.WriteLine("Failed to complete Select tests. Error: " + ex.Message);
            return false;
         }
      }

      private bool RunSelectUserByRandomFirstName(RunConfiguration config, Stopwatch sw, List<PerformanceResult> resultsTobeRecorded)
      {
         Console.WriteLine(string.Format("Select tests with scenario '{0}' has started..",
            TestScenarioEnums.SelectUsersByRandomFirstName.ToString()));
         try
         {
            for (int i = 0; i < config.NumberOfSelectRuns; i++)
            {
               Console.WriteLine("Run number: " + i);
               if (RandomUserFirsftNames.Count <= i)
               {
                  // make a mixture of existing and possibly not existing firstnames
                  if (i / 2 == 0)
                     RandomUserFirsftNames.Add(dbManager.GetRandomUserFirstName());
                  else
                     RandomUserFirsftNames.Add(GenerateRandomFirstName());
               }
               resultsTobeRecorded.Add(new PerformanceResult
               {
                  ExecutionTime = dbPerformanceMonitor.SelectUsersByFirstName(RandomUserFirsftNames[i], sw),
                  TestCase = config.TestCase,
                  TestScenario = TestScenarioEnums.SelectUsersByRandomFirstName,
               });
            }
            Console.WriteLine("Select test completed.");
            return true;
         }
         catch (Exception ex)
         {
            Console.WriteLine("Failed to complete Select tests. Error: " + ex.Message);
            return false;
         }
      }

      private bool RunSelectDepartmentByRandomUser(RunConfiguration config, Stopwatch sw, List<PerformanceResult> resultsTobeRecorded)
      {
         Console.WriteLine(string.Format("Select tests with scenario '{0}' has started..",
           TestScenarioEnums.SelectDepartmentByRandomUserFirstName.ToString()));
         try
         {
            for (int i = 0; i < config.NumberOfUpdateRuns; i++)
            {
               Console.WriteLine("Run count: " + i);
               if (RandomUserFirsftNames.Count <= i)
               {
                  // make a mixture of existing and possibly not existing firstnames
                  if (i / 2 == 0)
                     RandomUserFirsftNames.Add(dbManager.GetRandomUserFirstName());
                  else
                     RandomUserFirsftNames.Add(GenerateRandomFirstName());
               }
               resultsTobeRecorded.Add(new PerformanceResult
               {
                  ExecutionTime = dbPerformanceMonitor.SelectDepartmentsByUser(RandomUserFirsftNames[i], sw),
                  TestCase = config.TestCase,
                  TestScenario = TestScenarioEnums.SelectDepartmentByRandomUserFirstName,
               });
            }
            Console.WriteLine("Select test completed.");
            return true;
         }
         catch (Exception ex)
         {
            Console.WriteLine("Failed to complete Select tests. Error: " + ex.Message);
            return false;
         }
      }

      private bool RunSelectUserByRandomProject(RunConfiguration config, Stopwatch sw, List<PerformanceResult> resultsTobeRecorded)
      {
         Console.WriteLine(string.Format("Select tests with scenario '{0}' has started..",
           TestScenarioEnums.SelectUsersByRandomProjectKeys.ToString()));
         HashSet<int> projectKeys = new HashSet<int>();
         try
         {
            for (int i = 0; i < config.NumberOfSelectRuns; i++)
            {
               Console.WriteLine("Run number: " + i);
               projectKeys.Clear();
               if (RandomProjectIds.Count <= i)
               {
                  if (config.TestCase == TestCaseEnums.Initialize)
                     projectKeys.Add(dbManager.GetRandomProjectKey());
                  else
                  {
                     // get three random projects
                     while (projectKeys.Count < 3)
                        projectKeys.Add(dbManager.GetRandomProjectKey());
                  }
                  RandomProjectIds.Add(i, projectKeys.ToArray());
               }
               resultsTobeRecorded.Add(new PerformanceResult
               {
                  ExecutionTime = dbPerformanceMonitor.SelectUsersByProjects(RandomProjectIds[i], sw),
                  TestCase = config.TestCase,
                  TestScenario = TestScenarioEnums.SelectUsersByRandomProjectKeys,
               });
            }
            Console.WriteLine("Select test completed.");
            return true;
         }
         catch (Exception ex)
         {
            Console.WriteLine("Failed to complete Select tests. Error: " + ex.Message);
            return false;
         }
      }

      private bool RunSelectAvgUserAgeByProjectsMethodI(RunConfiguration config, Stopwatch sw, List<PerformanceResult> resultsTobeRecorded)
      {
         Console.WriteLine(string.Format("Select tests with scenario '{0}' has started..",
           TestScenarioEnums.SelectAverageAgeByRandomProjectKeysMI.ToString()));
         HashSet<int> projectKeys = new HashSet<int>();
         try
         {
            long totalProjectsInDb = dbManager.GetTotalProjectCount();
            for (int i = 0; i < config.NumberOfSelectRuns; i++)
            {
               Console.WriteLine("Run number: " + i);
               projectKeys.Clear();
               if (RandomProjectIds.Count <= i)
               {
                  if (totalProjectsInDb < 3)
                     projectKeys.Add(dbManager.GetRandomProjectKey());
                  else
                  {
                     // get three random projects
                     while (projectKeys.Count < 3)
                        projectKeys.Add(dbManager.GetRandomProjectKey());
                  }
                  RandomProjectIds.Add(i, projectKeys.ToArray());
               }
               resultsTobeRecorded.Add(new PerformanceResult
               {
                  ExecutionTime = dbPerformanceMonitor.SelectAverageAgeByProjectsMethodI(RandomProjectIds[i], sw),
                  TestCase = config.TestCase,
                  TestScenario = TestScenarioEnums.SelectAverageAgeByRandomProjectKeysMI,
               });
            }
            Console.WriteLine("Select test completed.");
            return true;
         }
         catch (Exception ex)
         {
            Console.WriteLine("Failed to complete Select tests. Error: " + ex.Message);
            return false;
         }
      }

      private bool RunSelectAvgUserAgeByProjectsMethodII(RunConfiguration config, Stopwatch sw, List<PerformanceResult> resultsTobeRecorded)
      {
         Console.WriteLine(string.Format("Select tests with scenario '{0}' has started..",
           TestScenarioEnums.SelectAverageAgeByRandomProjectKeysMII.ToString()));
         HashSet<int> projectKeys = new HashSet<int>();
         try
         {
            long totalProjectsInDb = dbManager.GetTotalProjectCount();
            for (int i = 0; i < config.NumberOfSelectRuns; i++)
            {
               Console.WriteLine("Run number: " + i);
               projectKeys.Clear();
               if (RandomProjectIds.Count <= i)
               {
                  if (totalProjectsInDb < 3)
                     projectKeys.Add(dbManager.GetRandomProjectKey());
                  else
                  {
                     // get three random projects
                     while (projectKeys.Count < 3)
                        projectKeys.Add(dbManager.GetRandomProjectKey());
                  }
                  RandomProjectIds.Add(i, projectKeys.ToArray());
               }
               resultsTobeRecorded.Add(new PerformanceResult
               {
                  ExecutionTime = dbPerformanceMonitor.SelectAverageAgeByProjectsMethodII(RandomProjectIds[i], sw),
                  TestCase = config.TestCase,
                  TestScenario = TestScenarioEnums.SelectAverageAgeByRandomProjectKeysMII,
               });
            }
            Console.WriteLine("Select test completed.");
            return true;
         }
         catch (Exception ex)
         {
            Console.WriteLine("Failed to complete Select tests. Error: " + ex.Message);
            return false;
         }
      }

      private void SaveRandomData()
      {
         Console.WriteLine("Saving the random data..");
         try
         {
            using (SqlConnection sqlConnection = new SqlConnection(Configurations.ResultDbSqlConnectionString))
            {
               var query = "Delete [dbo].[PartIRandomData]";
               sqlConnection.Open();
               using (var command = new SqlCommand(query, sqlConnection))
               {
                  command.ExecuteNonQuery();
               }
               sqlConnection.Close();
            }

            using (Entities ctx = new Entities())
            {
               foreach (string id in RandomDepartmentIds)
                  ctx.PartIRandomData.Add(new PartIRandomData { Type = (int)RandomDataTypeEnums.DepartmentId, Value = id });

               foreach (string id in RandomUserIds)
                  ctx.PartIRandomData.Add(new PartIRandomData { Type = (int)RandomDataTypeEnums.UserId, Value = id });

               foreach (var bundle in RandomProjectIds)
               {
                  foreach (int id in bundle.Value)
                     ctx.PartIRandomData.Add(new PartIRandomData { Type = (int)RandomDataTypeEnums.ProjectId, Value = id.ToString() });
               }

               foreach (string name in RandomDepartmentNames)
                  ctx.PartIRandomData.Add(new PartIRandomData { Type = (int)RandomDataTypeEnums.DepartmentName, Value = name });

               foreach (string name in RandomUserFirsftNames)
                  ctx.PartIRandomData.Add(new PartIRandomData { Type = (int)RandomDataTypeEnums.UserFirstName, Value = name });

               ctx.SaveChanges();
            }
            Console.WriteLine("Saving the random data succeeded.");
         }
         catch (Exception ex)
         {
            Console.WriteLine("Saving the random data Failed. Error: " + ex.Message);
         }
      }

      private List<int[]> GetKeyBundles(int[] allKeys)
      {
         long totalKeys = allKeys.LongCount();
         List<int[]> projectKeysBundles = new List<int[]>();
         if (totalKeys > MaxAllowedSqlParameters)
         {
            long numberOfBundles = totalKeys / MaxAllowedSqlParameters;
            int numberOfKeysBundled = 0;
            for (int i = 0; i < numberOfBundles; i++)
            {
               projectKeysBundles.Add(allKeys.Skip(i * MaxAllowedSqlParameters).Take(MaxAllowedSqlParameters).ToArray());
               numberOfKeysBundled += MaxAllowedSqlParameters;
            }
            if (numberOfKeysBundled < totalKeys)
               projectKeysBundles.Add(allKeys.Skip(numberOfKeysBundled).Take(MaxAllowedSqlParameters).ToArray());
         }
         else
         {
            projectKeysBundles.Add(allKeys);
         }
         return projectKeysBundles;
      }

      private string GenerateRandomFirstName()
      {
         return FirstNamePrefix + new Random().Next(1, MaxNamePostfix);
      }

      public bool FlushDatabases()
      {
         Console.WriteLine("Flushing the database..");
         try
         {
            dbManager.DeleteAll();
            recorder.DeleteAllResults();
            Console.WriteLine("Flushing the database succeeded.");
            return true;
         }
         catch (Exception ex)
         {
            Console.WriteLine("Failed to flush the database. Error: " + ex.Message);
            return false;
         }
      }

      private static List<string> randomUserFirstNames;
      protected static List<string> RandomUserFirsftNames
      {
         get
         {
            if (randomUserFirstNames == null)
            {
               using (Entities ctx = new Entities())
               {
                  randomUserFirstNames = ctx.PartIRandomData
                        .Where(d => d.Type == (int)RandomDataTypeEnums.UserFirstName)
                        .Select(dv => dv.Value).ToList();
               }
            }
            return randomUserFirstNames;
         }
      }

      private static List<string> randomDepartmentNames;
      protected static List<string> RandomDepartmentNames
      {
         get
         {
            if (randomDepartmentNames == null)
            {
               using (Entities ctx = new Entities())
               {
                  randomDepartmentNames = ctx.PartIRandomData
                     .Where(d => d.Type == (int)RandomDataTypeEnums.DepartmentName)
                     .Select(dv => dv.Value).ToList();
               }
            }
            return randomDepartmentNames;
         }
      }

      private static List<string> randomDepartmentIds;
      protected static List<string> RandomDepartmentIds
      {
         get
         {
            if (randomDepartmentIds == null)
            {
               using (Entities ctx = new Entities())
               {
                  randomDepartmentIds = ctx.PartIRandomData
                           .Where(d => d.Type == (int)RandomDataTypeEnums.DepartmentId)
                           .Select(dv => dv.Value).ToList();
               }
            }
            return randomDepartmentIds;
         }
      }

      private static List<string> randomUserIds;
      protected static List<string> RandomUserIds
      {
         get
         {
            if (randomUserIds == null)
            {
               using (Entities ctx = new Entities())
               {
                  randomUserIds = ctx.PartIRandomData
                              .Where(d => d.Type == (int)RandomDataTypeEnums.UserId)
                              .Select(dv => dv.Value).ToList();
               }
            }
            return randomUserIds;
         }
      }

      private static Dictionary<int, int[]> randomProjectIds;
      protected static Dictionary<int, int[]> RandomProjectIds
      {
         get
         {
            if (randomProjectIds == null)
            {
               using (Entities ctx = new Entities())
               {
                  randomProjectIds = new Dictionary<int, int[]>();
                  Queue<int> projectIds = new Queue<int>();
                  foreach (var id in ctx.PartIRandomData.Where(d => d.Type == (int)RandomDataTypeEnums.ProjectId).ToArray())
                  {
                     projectIds.Enqueue(int.Parse(id.Value));
                  }
                  int index = 0;
                  List<int> keyTriples = null;
                  while (projectIds.Count > 0)
                  {
                     keyTriples = new List<int>();
                     for (int i = 0; i < 3; i++)
                     {
                        if (projectIds.Count > 0)
                           keyTriples.Add(projectIds.Dequeue());
                     }
                     randomProjectIds.Add(index, keyTriples.ToArray());
                     index++;
                  }
               }
            }
            return randomProjectIds;
         }
      }

      private int GetPostfix(string name)
      {
         if (string.IsNullOrEmpty(name))
            return 0;
         string number = "";
         for (int i = 0; i < name.Length; i++)
         {
            if (char.IsNumber(name[i]))
               number += name[i];
         }
         return string.IsNullOrEmpty(number) ? 0 : int.Parse(number);
      }

      private bool RestoreDepartmentsUniqueness()
      {
         try
         {
            Console.WriteLine("Restoring department names uniqueness..");
            dbManager.UpdateDepartment(uniqueDepartmentKeyAndNames);
            Console.WriteLine("Restoring department names succeeded.");
            return true;
         }
         catch (Exception ex)
         {
            Console.WriteLine("Failed to restore departments. Error: " + ex.Message);
            return false;
         }
      }

      private Dictionary<int, string> uniqueDepartmentKeyAndNames;
      private readonly DataBaseTypeEnums databaseType;
      private readonly IDatabasePerformanceMonitor dbPerformanceMonitor;
      private readonly IDatabaseManager dbManager;
      private readonly IRecorder recorder;
      private readonly List<PerformanceResult> results;
      private readonly Stopwatch stopwatch;
      private const string DepartmentNamePrefix = "Department ";
      private const string FirstNamePrefix = "First name ";
      private const string LastNamePrefix = "Last name ";
      private const string ProjectNamePrefix = "Project ";
      private const string NewDepartmentName = "New Dapartment Name ";
      private const string NewUserLastNamePrefix = "New Last Name ";
      private const string NewProjectNamePrefix = "New Project Name ";
      private const int MaxUserPerProject = 3;
      private const int MaxNamePostfix = 1000000;
      private const int MaxAllowedSqlParameters = 2000;
   }
}
