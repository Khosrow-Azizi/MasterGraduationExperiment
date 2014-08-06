using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Experiment.Common.DataModel;

namespace Experiment.Common.DatabaseManager
{
   public interface IDatabaseManager
   {
      long GetTotalDepartmentCount();
      long GetTotalProjectCount();
      long GetTotalUserCount();
      int GetNewDepartmentId();
      int GetNewUserId();
      int GetNewProjectId();
      int[] GetAllDepartmentKeys();
      int[] GetAllUserKeys();
      int[] GetAllProjectKeys();
      int[] GetDepartmentKeys(float portion);
      int[] GetProjectKeys(float portion);
      Dictionary<int, string> GetDepartmentKeysAndNames(float portion);
      Dictionary<int, string> GetProjectKeysAndNames(float portion);
      void UpdateDepartment(Dictionary<int, string> keyAndNames);
      int GetRandomDepartmentKey();
      int GetRandomProjectKey();
      int GetRandomUserKey();
      string GetRandomDepartmentName();
      string GetRandomProjectName();
      string GetRandomUserFirstName();
      string GetLatestDepartmentName();
      string GetLatestProjectName();
      void DeleteAll();
   }
}
