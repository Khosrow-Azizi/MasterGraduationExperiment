using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Experiment.Common.DataModel;
using Experiment.ImplementationI.App.DataModel.Mongo;
using Experiment.ImplementationI.App.DataModel.Sql;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace Experiment.ImplementationI.App.PerformanceMonitor
{
   public class MongoPerformanceMonitor : DatabasePerformanceMonitor
   {
      public MongoPerformanceMonitor(string connectionString, string databaseName)
      {
         if (!localDateTimeRegistered)
         {
            DateTimeSerializationOptions options = DateTimeSerializationOptions.LocalInstance;
            var serializer = new DateTimeSerializer(options);
            BsonSerializer.RegisterSerializer(typeof(DateTime), serializer);
            localDateTimeRegistered = true;
         }
         this.connectionString = connectionString;
         this.databaseName = databaseName;
         this.insertOptions = new MongoInsertOptions { WriteConcern = WriteConcern.Acknowledged };
         this.updateOptions = new MongoUpdateOptions { Flags = UpdateFlags.Multi, WriteConcern = WriteConcern.Acknowledged };
      }

      public override double Insert(IEnumerable<Department> departments, Stopwatch stopwatch)
      {
         stopwatch.Reset();
         var collection = GetCollection<MDepartment>();
         foreach (var dep in departments)
         {
            MDepartment mdep = dep.ToMongoDepartment();
            stopwatch.Start();
            collection.Insert<MDepartment>(mdep, insertOptions);
            stopwatch.Stop();
         }
         return GetTimeElapsed(stopwatch);
      }

      public override double Insert(IEnumerable<Project> projects, Stopwatch stopwatch)
      {
         stopwatch.Reset();
         var collection = GetCollection<MProject>();
         foreach (var proj in projects)
         {
            MProject mproj = proj.ToMongoProject();
            stopwatch.Start();
            collection.Insert<MProject>(mproj, insertOptions);
            stopwatch.Stop();
         }
         return GetTimeElapsed(stopwatch);
      }

      public override double Insert(IEnumerable<User> users, Stopwatch stopwatch)
      {
         stopwatch.Reset();
         var collection = GetCollection<MUser>();
         foreach (var user in users)
         {
            MUser muser = user.ToMongoUser();
            stopwatch.Start();
            collection.Insert<MUser>(muser, insertOptions);
            stopwatch.Stop();
         }
         return GetTimeElapsed(stopwatch);
      }

      public override double UpdateDepartmentNameByKeys(int[] keys, string newName, Stopwatch stopwatch)
      {
         var collection = GetCollection<MDepartment>();
         IMongoQuery query = Query<MDepartment>.In(e => e.Id, keys);
         IMongoUpdate command = Update<MDepartment>.Set(e => e.Name, newName);

         stopwatch.Restart();
         collection.Update(query, command, updateOptions);
         stopwatch.Stop();
         return GetTimeElapsed(stopwatch);
      }

      public override double UpdateProjectNameByKeys(int[] keys, string newName, Stopwatch stopwatch)
      {
         var collection = GetCollection<MProject>();
         IMongoQuery query = Query<MProject>.In(e => e.Id, keys);
         IMongoUpdate command = Update<MProject>.Set(e => e.Name, newName);

         stopwatch.Restart();
         collection.Update(query, command, updateOptions);
         stopwatch.Stop();
         return GetTimeElapsed(stopwatch);
      }

      public override double UpdateUserLastName(string firstNamePattern, string newLastName, Stopwatch stopwatch)
      {
         var collection = GetCollection<MUser>();
         IMongoQuery query = Query<MUser>.EQ(e => e.FirstName, firstNamePattern);
         IMongoUpdate command = Update<MUser>.Set(e => e.LastName, newLastName);

         stopwatch.Restart();
         collection.Update(query, command, updateOptions);
         stopwatch.Stop();
         return GetTimeElapsed(stopwatch);
      }

      public override double SelectDepartmentByKey(int key, Stopwatch stopwatch)
      {
         var collection = GetCollection<MDepartment>();
         IMongoQuery query = Query<MDepartment>.EQ(e => e.Id, key);

         stopwatch.Restart();
         MDepartment department = collection.Find(query).FirstOrDefault();
         stopwatch.Stop();
         return GetTimeElapsed(stopwatch);
      }

      public override double SelectUserByKey(int key, Stopwatch stopwatch)
      {
         var collection = GetCollection<MUser>();
         IMongoQuery query = Query<MUser>.EQ(e => e.Id, key);

         stopwatch.Restart();
         MUser user = collection.Find(query).FirstOrDefault();
         stopwatch.Stop();
         return GetTimeElapsed(stopwatch);
      }

      public override double SelectDepartmentByName(string name, Stopwatch stopwatch)
      {
         var collection = GetCollection<MDepartment>();
         IMongoQuery query = Query<MDepartment>.EQ(e => e.Name, name);

         stopwatch.Restart();
         MDepartment department = collection.Find(query).FirstOrDefault();
         stopwatch.Stop();
         return GetTimeElapsed(stopwatch);
      }

      public override double SelectUsersByFirstName(string firstName, Stopwatch stopwatch)
      {
         var collection = GetCollection<MUser>();
         IMongoQuery query = Query<MUser>.EQ(e => e.FirstName, firstName);

         stopwatch.Restart();
         IEnumerable<MUser> users = collection.Find(query).ToArray();
         stopwatch.Stop();
         return GetTimeElapsed(stopwatch);
      }

      public override double SelectDepartmentsByUser(string userFirstName, Stopwatch stopwatch)
      {
         var depCollection = GetCollection<MDepartment>();
         var userCollection = GetCollection<MUser>();

         IMongoQuery userQuery = Query<MUser>.EQ(u => u.FirstName, userFirstName);
         stopwatch.Restart();
         var departmentIds = userCollection.Find(userQuery).Select(u => u.DepartmentId);
         stopwatch.Stop();

         IMongoQuery depQuery = Query<MDepartment>.In(d => d.Id, departmentIds);
         stopwatch.Start();
         IEnumerable<MDepartment> departments = depCollection.Find(depQuery).ToArray();
         stopwatch.Stop();

         return GetTimeElapsed(stopwatch);
      }

      public override double SelectUsersByProjects(int[] projectKeys, Stopwatch stopwatch)
      {
         var userCollection = GetCollection<MUser>();
         var projectCollection = GetCollection<MProject>();

         IMongoQuery projectQuery = Query<MProject>.In(p => p.Id, projectKeys);
         stopwatch.Restart();
         var projectUserIds = projectCollection.Find(projectQuery).SelectMany(p => p.Users);
         stopwatch.Stop();

         IMongoQuery userQuery = Query<MUser>.In(u => u.Id, projectUserIds);
         stopwatch.Start();
         IEnumerable<MUser> users = userCollection.Find(userQuery).ToArray();
         stopwatch.Stop();

         return GetTimeElapsed(stopwatch);
      }

      /// <summary>
      /// Calculate average by map reduce
      /// </summary>
      /// <param name="projectKeys"></param>
      /// <param name="stopwatch"></param>
      /// <returns></returns>
      public override double SelectAverageAgeByProjectsMethodI(int[] projectKeys, Stopwatch stopwatch)
      {
         var userCollection = GetCollection<MUser>();
         var projectCollection = GetCollection<MProject>();
         IMongoQuery projectQuery = Query<MProject>.In(p => p.Id, projectKeys);

         string mapFunc =
          @"function() {
               var user = this;
               emit(null, { count: 1, age: user.Age });
            }";

         string reduceFunc =
            @"function(key, values) {
               var result = {count: 0, totalAge: 0 };
               values.forEach(function(value) {               
                  result.count += value.count;
                  result.totalAge += value.age; });
               return result;
             }";

         string finalizeFunc =
            @"function(key, value) {      
               value.average = value.totalAge / value.count;
               return value;
             }";

         MapReduceArgs mrArgs = new MapReduceArgs
         {
            Query = Query<MUser>.In(u => u.Id, projectCollection.Find(projectQuery).SelectMany(p => p.Users)),
            MapFunction = mapFunc,
            ReduceFunction = reduceFunc,
            FinalizeFunction = finalizeFunc,
            OutputMode = MapReduceOutputMode.Inline,
         };

         stopwatch.Restart();
         userCollection.MapReduce(mrArgs);
         stopwatch.Stop();
         return GetTimeElapsed(stopwatch);
      }

      /// <summary>
      /// Calculate average by pipe line
      /// </summary>
      /// <param name="projectKeys"></param>
      /// <param name="stopwatch"></param>
      /// <returns></returns>
      public override double SelectAverageAgeByProjectsMethodII(int[] projectKeys, Stopwatch stopwatch)
      {
         var userCollection = GetCollection<MUser>();
         var projectCollection = GetCollection<MProject>();
         IMongoQuery projectQuery = Query<MProject>.In(p => p.Id, projectKeys);

         stopwatch.Restart();
         var aggResult = userCollection.Aggregate(new AggregateArgs
         {
            Pipeline = new BsonDocument[]
               {
                  new BsonDocument //match stage 
                  { 
                     {
                        "$match", new BsonDocument 
                           { { "_id", new BsonDocument 
                              { { "$in", new BsonArray(projectCollection.Find(projectQuery).SelectMany(p => p.Users)) } } } }
                     } 
                  },
                  new BsonDocument //aggregation stage
                  { 
                     { 
                        "$group", new BsonDocument 
                                 { 
                                    { "_id", 0 }, 
                                    { "AverageAge", new BsonDocument { {"$avg", "$Age"} } } 
                                 }
                     }  
                  }
               },
            OutputMode = AggregateOutputMode.Inline,
         });
         stopwatch.Stop();
         return GetTimeElapsed(stopwatch); 
      }

      private MongoCollection<T> GetCollection<T>() where T : IExperimentClass
      {
         MongoServer server = new MongoClient(connectionString).GetServer();
         MongoDatabase database = server.GetDatabase(databaseName);
         return database.GetCollection<T>(typeof(T).Name);
      }

      private readonly string connectionString;
      private readonly string databaseName;
      private readonly MongoInsertOptions insertOptions;
      private readonly MongoUpdateOptions updateOptions;
      private static bool localDateTimeRegistered;
   }
}
