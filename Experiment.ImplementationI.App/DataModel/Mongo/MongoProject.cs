using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Experiment.Common.DataModel;

namespace Experiment.ImplementationI.App.DataModel.Mongo
{
   public class MProject : IHasName
   {
      public MProject()
      {
         this.Users = new int[0];
      }
      public int Id { get; set; }      
      public string Name { get; set; }
      public int ManagerId { get; set; }
      public int DepartmentId { get; set; }
      public DateTime DateAdded { get; set; }
      public int[] Users { get; set; }
   }
}
