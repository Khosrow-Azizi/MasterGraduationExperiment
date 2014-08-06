using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Experiment.Common.DataRecorder.Db;
using Experiment.Common.Auxiliary;

namespace Experiment.Common.DataRecorder
{
   public class Recorder : IRecorder
   {
      public Recorder(DataBaseTypeEnums dataBaseType)
      {
         this.dataBaseType = dataBaseType;
      }

      public void Record(PerformanceResult performanceResult)
      {
         using (var ctx = new Entities())
         {
            ctx.PartIResult.Add(new PartIResult
               {
                  TestNumber = GenerateNewTestNumber(ctx, performanceResult),
                  DataBaseType = (int)dataBaseType,
                  DateTimeAdded = DateTime.Now,
                  TestCase = (int)performanceResult.TestCase,
                  TestScenario = (int)performanceResult.TestScenario,
                  ExecutionTime = performanceResult.ExecutionTime,
               });
            ctx.SaveChanges();
         }
      }

      public void Record(IEnumerable<PerformanceResult> performanceResults)
      {
         int testNumber = 0;
         using (var ctx = new Entities())
         {
            testNumber = GenerateNewTestNumber(ctx, performanceResults.First());
         }

         var bundles = GetInsertBundles(performanceResults.ToArray());
         using (SqlConnection sqlConnection = new SqlConnection(Configurations.ResultDbSqlConnectionString))
         {
            sqlConnection.Open();
            foreach (var bundle in bundles)
            {
               using (SqlCommand command = new SqlCommand())
               {
                  StringBuilder sb = new StringBuilder();
                  sb.Append("INSERT INTO [dbo].[PartIResult] (TestNumber, DataBaseType, DateTimeAdded, TestCase, TestScenario, ExecutionTime) VALUES ");
                  int paramNumber = 0;
                  List<string> paramTuples = new List<string>();
                  foreach (var result in bundle)
                  {
                     string p0 = "@p0" + paramNumber;
                     string p1 = "@p1" + paramNumber;
                     string p2 = "@p2" + paramNumber;
                     string p3 = "@p3" + paramNumber;
                     string p4 = "@p4" + paramNumber;
                     string p5 = "@p5" + paramNumber;
                     paramTuples.Add(string.Format("({0},{1},{2},{3},{4},{5})", p0, p1, p2, p3, p4, p5));
                     command.Parameters.AddWithValue(p0, testNumber);
                     command.Parameters.AddWithValue(p1, (int)dataBaseType);
                     command.Parameters.AddWithValue(p2, DateTime.Now);
                     command.Parameters.AddWithValue(p3, (int)result.TestCase);
                     command.Parameters.AddWithValue(p4, (int)result.TestScenario);
                     command.Parameters.AddWithValue(p5, result.ExecutionTime);
                     paramNumber++;
                     testNumber++;
                  }
                  sb.Append(string.Join(", ", paramTuples));
                  command.CommandText = sb.ToString();
                  command.Connection = sqlConnection;
                  command.ExecuteNonQuery();
               }
            }
            sqlConnection.Close();
         }
      }

      private const int MaxAllowedSqlParameters = 2000;
      private List<PerformanceResult[]> GetInsertBundles(PerformanceResult[] performanceResults)
      {
         long totalCount = performanceResults.LongCount();
         var maxAmountAllowed = MaxAllowedSqlParameters / 6;
         List<PerformanceResult[]> bundles = new List<PerformanceResult[]>();
         if (totalCount > maxAmountAllowed)
         {
            long numberOfBundles = totalCount / maxAmountAllowed;
            int numberOfItemsBundled = 0;
            for (int i = 0; i < numberOfBundles; i++)
            {
               bundles.Add(performanceResults.Skip(i * maxAmountAllowed).Take(maxAmountAllowed).ToArray());
               numberOfItemsBundled += maxAmountAllowed;
            }
            if (numberOfItemsBundled < totalCount)
               bundles.Add(performanceResults.Skip(numberOfItemsBundled).Take(MaxAllowedSqlParameters).ToArray());
         }
         else
         {
            bundles.Add(performanceResults);
         }
         return bundles;
      }

      public PartIResult GetLatestResult(TestCaseEnums testCase, TestScenarioEnums scenario)
      {
         using (Entities ctx = new Entities())
         {
            return ctx.PartIResult.OrderByDescending(tr => tr.TestNumber).FirstOrDefault(r => r.DataBaseType == (int)dataBaseType
               && r.TestCase == (int)testCase && r.TestScenario == (int)scenario);
         }
      }

      public void Delete(PartIResult partIResult)
      {
         using (Entities ctx = new Entities())
         {
            PartIResult resultInDb = ctx.PartIResult.FirstOrDefault(r => r.DataBaseType == (int)dataBaseType &&
               r.TestNumber == partIResult.TestNumber &&
                 r.TestCase == (int)partIResult.TestCase && r.TestScenario == (int)partIResult.TestScenario);
            if (resultInDb == null)
               return;
            ctx.PartIResult.Remove(resultInDb);
            ctx.SaveChanges();
         }
      }

      public void DeleteAllResults()
      {
         using (SqlConnection sqlConnection = new SqlConnection(Configurations.ResultDbSqlConnectionString))
         {
            var query = "Delete [dbo].[PartIResult] WHERE DataBaseType = " + (int)dataBaseType;
            sqlConnection.Open();
            using (var command = new SqlCommand(query, sqlConnection))
            {
               command.ExecuteNonQuery();
            }
            sqlConnection.Close();
         }
      }

      private int GenerateNewTestNumber(Entities ctx, PerformanceResult performanceResult)
      {
         var testResults = ctx.PartIResult
              .Where(r => r.DataBaseType == (int)dataBaseType &&
                 r.TestCase == (int)performanceResult.TestCase && r.TestScenario == (int)performanceResult.TestScenario);
         if (testResults.Any())
            return testResults.Max(r => r.TestNumber) + 1;
         return 1;
      }

      private readonly DataBaseTypeEnums dataBaseType;
   }
}
