using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Experiment.Common.DataModel;

namespace Experiment.ImplementationI.App.DataModel.Mongo
{
   public class MDepartment : IHasName
   {
      public int Id { get; set; }
      public string Name { get; set; }
      public DateTime DateAdded { get; set; }      
   }
}
