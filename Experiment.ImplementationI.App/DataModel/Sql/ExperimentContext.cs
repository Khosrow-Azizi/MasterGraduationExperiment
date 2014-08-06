using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Experiment.Common.Auxiliary;

namespace Experiment.ImplementationI.App.DataModel.Sql
{
   public partial class ExperimentContext : DbContext
   {
     public ExperimentContext() :
        base(Configurations.DbEFConnectionString) { }
   }
}
