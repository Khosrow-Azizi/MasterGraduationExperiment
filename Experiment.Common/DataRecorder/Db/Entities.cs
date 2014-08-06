using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Experiment.Common.DataRecorder.Db
{
   public partial class Entities :  DbContext
   {
      public Entities()
         : base(Experiment.Common.Auxiliary.Configurations.ResultDbEFConnectionString)
      {
      }
   }
}
