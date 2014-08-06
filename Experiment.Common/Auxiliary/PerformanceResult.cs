using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Experiment.Common.Auxiliary
{
   public class PerformanceResult
   {
      public TestCaseEnums TestCase { get; set; }
      public TestScenarioEnums TestScenario { get; set; }
      public double ExecutionTime { get; set; }
   }
}
