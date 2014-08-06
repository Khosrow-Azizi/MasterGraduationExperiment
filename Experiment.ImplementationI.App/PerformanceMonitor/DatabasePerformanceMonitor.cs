using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Experiment.ImplementationI.App.PerformanceMonitor
{
   public abstract class DatabasePerformanceMonitor : IDatabasePerformanceMonitor
   {
      public abstract double Insert(IEnumerable<DataModel.Sql.Department> departments, Stopwatch stopwatch);
      public abstract double Insert(IEnumerable<DataModel.Sql.Project> projects, Stopwatch stopwatch);
      public abstract double Insert(IEnumerable<DataModel.Sql.User> users, Stopwatch stopwatch);
      public abstract double UpdateDepartmentNameByKeys(int[] keys, string newName, Stopwatch stopwatch);
      public abstract double UpdateProjectNameByKeys(int[] keys, string newName, Stopwatch stopwatch);
      public abstract double UpdateUserLastName(string firstNamePattern, string newLastName, Stopwatch stopwatch);
      public abstract double SelectDepartmentByKey(int key, Stopwatch stopwatch);
      public abstract double SelectUserByKey(int key, Stopwatch stopwatch);
      public abstract double SelectDepartmentByName(string name, Stopwatch stopwatch);
      public abstract double SelectUsersByFirstName(string firstName, Stopwatch stopwatch);
      public abstract double SelectDepartmentsByUser(string userFirstName, Stopwatch stopwatch);
      public abstract double SelectUsersByProjects(int[] projectKeys, Stopwatch stopwatch);
      public abstract double SelectAverageAgeByProjectsMethodI(int[] projectKeys, Stopwatch stopwatch);
      public abstract double SelectAverageAgeByProjectsMethodII(int[] projectKeys, Stopwatch stopwatch);

      protected double GetTimeElapsed(Stopwatch stopwatch)
      {
         return ((double)stopwatch.ElapsedTicks / (double)System.Diagnostics.Stopwatch.Frequency) * 1000;
      }
   }
}
