using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Experiment.ImplementationI.App.DataModel.Sql;

namespace Experiment.ImplementationI.App.PerformanceMonitor
{
   public interface IDatabasePerformanceMonitor
   {
      double Insert(IEnumerable<Department> departments, Stopwatch stopwatch);
      double Insert(IEnumerable<Project> projects, Stopwatch stopwatch);
      double Insert(IEnumerable<User> users, Stopwatch stopwatch);
      double UpdateDepartmentNameByKeys(int[] keys, string newName, Stopwatch stopwatch);
      double UpdateProjectNameByKeys(int[] keys, string newName, Stopwatch stopwatch);
      double UpdateUserLastName(string firstNamePattern, string newLastName, Stopwatch stopwatch);
      double SelectDepartmentByKey(int key, Stopwatch stopwatch);
      double SelectUserByKey(int key, Stopwatch stopwatch);
      double SelectDepartmentByName(string name, Stopwatch stopwatch);
      double SelectUsersByFirstName(string firstName, Stopwatch stopwatch);
      double SelectDepartmentsByUser(string userFirstName, Stopwatch stopwatch);
      double SelectUsersByProjects(int[] projectKeys, Stopwatch stopwatch);
      double SelectAverageAgeByProjectsMethodI(int[] projectKeys, Stopwatch stopwatch);
      double SelectAverageAgeByProjectsMethodII(int[] projectKeys, Stopwatch stopwatch);
   }
}
