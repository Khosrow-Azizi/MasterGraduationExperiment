using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Experiment.Common.DataRecorder.Db;
using Experiment.Common.Auxiliary;

namespace Experiment.Common.DataRecorder
{
   public interface IRecorder
   {
      void Record(PerformanceResult performanceResult);
      void Record(IEnumerable<PerformanceResult> performanceResults);
      void Delete(PartIResult partIResult);
      PartIResult GetLatestResult(TestCaseEnums testCase, TestScenarioEnums scenario);
      void DeleteAllResults();
   }
}
