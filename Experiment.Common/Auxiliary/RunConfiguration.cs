using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Experiment.Common.Auxiliary
{
   public class RunConfiguration
   {
      public int NumberOfInsertRuns { get; private set; }
      public int NumberOfSelectRuns { get; private set; }
      public int NumberOfUpdateRuns { get; private set; }
      public int TotalNumberOfDepartments { get; private set; }
      public int TotalNumberOfUsers { get; private set; }
      public int TotalNumberOfProjects { get; private set; }
      public TestCaseEnums TestCase { get; private set; }

      private RunConfiguration () 	{	}

      public static RunConfiguration GetConfigurations(TestCaseEnums testCase)
      {
         switch (testCase)
         {
            case TestCaseEnums.Initialize:
               return new RunConfiguration
               {
                  NumberOfInsertRuns = 10,
                  NumberOfSelectRuns = 10,
                  NumberOfUpdateRuns = 10,
                  TotalNumberOfDepartments = 1,
                  TotalNumberOfUsers = 1,
                  TotalNumberOfProjects = 1,
                  TestCase = testCase,
               };
            case TestCaseEnums.TestCase1:
               return new RunConfiguration
               {
                  NumberOfInsertRuns = 100,
                  NumberOfSelectRuns = 100,
                  NumberOfUpdateRuns = 100,
                  TotalNumberOfDepartments = 4,
                  TotalNumberOfUsers = 128,
                  TotalNumberOfProjects = 16,
                  TestCase = testCase,
               };
            case TestCaseEnums.TestCase2:
               return new RunConfiguration
               {
                  NumberOfInsertRuns = 100,
                  NumberOfSelectRuns = 200,
                  NumberOfUpdateRuns = 100,
                  TotalNumberOfDepartments = 8,
                  TotalNumberOfUsers = 256,
                  TotalNumberOfProjects = 64,
                  TestCase = testCase,
               };
            case TestCaseEnums.TestCase3:
               return new RunConfiguration
               {
                  NumberOfInsertRuns = 100,
                  NumberOfSelectRuns = 100,
                  NumberOfUpdateRuns = 100,
                  TotalNumberOfDepartments = 16,
                  TotalNumberOfUsers = 1024,
                  TotalNumberOfProjects = 512,
                  TestCase = testCase,
               };
            case TestCaseEnums.TestCase4:
               return new RunConfiguration
               {
                  NumberOfInsertRuns = 100,
                  NumberOfSelectRuns = 200,
                  NumberOfUpdateRuns = 100,
                  TotalNumberOfDepartments = 128,
                  TotalNumberOfUsers = 4096,
                  TotalNumberOfProjects = 8192,
                  TestCase = testCase,
               };
            case TestCaseEnums.TestCase5:
               return new RunConfiguration
               {
                  NumberOfInsertRuns = 100,
                  NumberOfSelectRuns = 100,
                  NumberOfUpdateRuns = 100,
                  TotalNumberOfDepartments = 1128,
                  TotalNumberOfUsers = 5096,
                  TotalNumberOfProjects = 9192,
                  TestCase = testCase,
               };
            case TestCaseEnums.TestCase6:
               return new RunConfiguration
               {
                  NumberOfInsertRuns = 100,
                  NumberOfSelectRuns = 100,
                  NumberOfUpdateRuns = 100,
                  TotalNumberOfDepartments = 2128,
                  TotalNumberOfUsers = 6096,
                  TotalNumberOfProjects = 10192,
                  TestCase = testCase,
               };
            case TestCaseEnums.TestCase7:
               return new RunConfiguration
               {
                  NumberOfInsertRuns = 100,
                  NumberOfSelectRuns = 100,
                  NumberOfUpdateRuns = 100,
                  TotalNumberOfDepartments = 3128,
                  TotalNumberOfUsers = 7096,
                  TotalNumberOfProjects = 11192,
                  TestCase = testCase,
               };
            case TestCaseEnums.TestCase8:
               return new RunConfiguration
               {
                  NumberOfInsertRuns = 100,
                  NumberOfSelectRuns = 100,
                  NumberOfUpdateRuns = 100,
                  TotalNumberOfDepartments = 4128,
                  TotalNumberOfUsers = 8096,
                  TotalNumberOfProjects = 12192,
                  TestCase = testCase,
               };
            default:
               return new RunConfiguration
               {
                  NumberOfInsertRuns = 0,
                  TotalNumberOfDepartments = 0,
                  TotalNumberOfUsers = 0,
                  TotalNumberOfProjects = 0,
                  TestCase = testCase,
               };
         }
      }
   }
}
