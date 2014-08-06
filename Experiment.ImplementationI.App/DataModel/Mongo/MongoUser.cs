using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Experiment.Common.DataModel;

namespace Experiment.ImplementationI.App.DataModel.Mongo
{
   public class MUser : IHasFirstName
   {
      public int Id { get; set; }
      public string FirstName { get; set; }
      public string LastName { get; set; }
      public int Age { get; set; }
      public int DepartmentId { get; set; }
      public DateTime DateAdded { get; set; }
   }
}
