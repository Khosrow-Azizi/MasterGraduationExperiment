//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Experiment.ImplementationI.App.DataModel.Sql
{
    using System;
    using System.Collections.Generic;
    
    public partial class User
    {
        public User()
        {
            this.Project = new HashSet<Project>();
            this.Project1 = new HashSet<Project>();
        }
    
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
        public int DepartmentId { get; set; }
        public System.DateTime DateAdded { get; set; }
    
        public virtual Department Department { get; set; }
        public virtual ICollection<Project> Project { get; set; }
        public virtual ICollection<Project> Project1 { get; set; }
    }
}