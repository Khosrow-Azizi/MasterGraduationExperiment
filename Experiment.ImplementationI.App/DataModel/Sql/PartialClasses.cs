using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Experiment.Common.DataModel;
using Experiment.ImplementationI.App.DataModel.Mongo;

namespace Experiment.ImplementationI.App.DataModel.Sql
{
   public partial class Department : IHasName
   {
      public MDepartment ToMongoDepartment()
      {
         return new MDepartment
         {
            Id = this.Id,
            Name = this.Name,
            DateAdded = this.DateAdded,
         };
      }
   }

   public partial class Project : IHasName
   {
      public MProject ToMongoProject()
      {
         return new MProject
         {
            Id = this.Id,
            Name = this.Name,
            DateAdded = this.DateAdded,
            DepartmentId = this.DepartmentId,
            ManagerId = this.ManagerId,
            Users = this.User1.Select(u => u.Id).ToArray(),
         };
      }
   }

   public partial class User : IHasFirstName
   {
      public MUser ToMongoUser()
      {
         return new MUser
         {
            Id = this.Id,
            FirstName = this.FirstName,
            LastName = this.FirstName,
            Age = this.Age,
            DepartmentId = this.DepartmentId,
            DateAdded = this.DateAdded,
         };
      }
   }
}
