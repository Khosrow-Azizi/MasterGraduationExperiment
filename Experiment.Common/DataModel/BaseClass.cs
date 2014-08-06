using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Experiment.Common.DataModel
{
   public interface IExperimentClass
   {
      int Id { get; set; }
      DateTime DateAdded { get; set; }
   }

   public interface IHasName : IExperimentClass
   {
      string Name { get; set; }
   }

   public interface IHasFirstName : IExperimentClass
   {
      string FirstName { get; set; }
   }  
}
