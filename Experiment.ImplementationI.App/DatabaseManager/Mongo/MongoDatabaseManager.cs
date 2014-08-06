using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoDB.Driver.Builders;
using Experiment.Common.DataModel;
using Experiment.Common.DatabaseManager;
using Experiment.ImplementationI.App.DataModel.Mongo;

namespace Experiment.ImplementationI.App.DatabaseManager.Mongo
{
   public class MongoDatabaseManager : IDatabaseManager
   {
      public MongoDatabaseManager(string connectionString, string databaseName)
      {
         this.connectionString = connectionString;
         this.databaseName = databaseName;
      }

      public long GetTotalDepartmentCount()
      {
         return GetTotalCount<MDepartment>();
      }

      public long GetTotalProjectCount()
      {
         return GetTotalCount<MProject>();
      }

      public long GetTotalUserCount()
      {
         return GetTotalCount<MUser>();
      }

      public int GetNewDepartmentId()
      {
         return GetNewId<MDepartment>();
      }

      public int GetNewUserId()
      {
         return GetNewId<MUser>();
      }

      public int GetNewProjectId()
      {
         return GetNewId<MProject>();
      }

      public int[] GetAllDepartmentKeys()
      {
         return GetAllKeys<MDepartment>();
      }

      public int[] GetAllUserKeys()
      {
         return GetAllKeys<MUser>();
      }

      public int[] GetAllProjectKeys()
      {
         return GetAllKeys<MProject>();
      }

      public Dictionary<int, string> GetDepartmentKeysAndNames(float portion)
      {
         return GetKeyAndNames<MDepartment>(portion);
      }

      public Dictionary<int, string> GetProjectKeysAndNames(float portion)
      {
         return GetKeyAndNames<MProject>(portion);
      }

      public int[] GetDepartmentKeys(float portion)
      {
         return GetKeys<MDepartment>(portion);
      }

      public int[] GetProjectKeys(float portion)
      {
         return GetKeys<MProject>(portion);
      }

      public void UpdateDepartment(Dictionary<int, string> keyAndNames)
      {
         UpdateNames<MDepartment>(keyAndNames);
      }

      public int GetRandomDepartmentKey()
      {
         return GetRandomKey<MDepartment>();
      }

      public int GetRandomProjectKey()
      {
         return GetRandomKey<MProject>();
      }

      public int GetRandomUserKey()
      {
         return GetRandomKey<MUser>();
      }

      public string GetRandomDepartmentName()
      {
         return GetRandomName<MDepartment>();
      }

      public string GetRandomProjectName()
      {
         return GetRandomName<MProject>();
      }

      public string GetRandomUserFirstName()
      {
         return GetRandomUserFirstName<MUser>();
      }

      public string GetLatestDepartmentName()
      {
         return GetLatestName<MDepartment>();
      }

      public string GetLatestProjectName()
      {
         return GetLatestName<MProject>();
      }

      public void DeleteAll()
      {
         GetCollection<MProject>().Drop();
         GetCollection<MUser>().Drop();
         GetCollection<MDepartment>().Drop();
      }

      private long GetTotalCount<T>() where T : IExperimentClass
      {
         return GetCollection<T>().FindAll().Count();
      }

      private int GetNewId<T>() where T : IExperimentClass
      {
         var collection = GetCollection<T>().AsQueryable<T>();
         if (collection.Any())
            return collection.Max(e => e.Id) + 1;
         return 1;
      }

      private int[] GetAllKeys<T>() where T : IExperimentClass
      {
         return GetCollection<T>().FindAll().Select(e => e.Id).ToArray();
      }

      private Dictionary<int, string> GetKeyAndNames<T>(float portion) where T : IHasName
      {
         return GetCollection<T>().FindAll()
            .SetLimit((int)ConvertToCount<T>(portion))
            .Select(e => new KeyValuePair<int, string>(e.Id, e.Name)).ToDictionary(k => k.Key, v => v.Value);
      }

      private int[] GetKeys<T>(float portion) where T : IExperimentClass
      {
         return GetCollection<T>().FindAll()
            .SetLimit((int)ConvertToCount<T>(portion))
            .Select(e => e.Id).ToArray();
      }

      private void UpdateNames<T>(Dictionary<int, string> keyAndNames) where T : IHasName
      {
         var collection = GetCollection<T>();
         foreach (var item in keyAndNames)
         {
            IMongoQuery query = Query<T>.EQ(e => e.Id, item.Key);
            IMongoUpdate command = Update<T>.Set(e => e.Name, item.Value);
            collection.Update(query, command);
         }
      }

      private int GetRandomKey<T>() where T : IExperimentClass
      {
         int key = -1;
         var collection = GetCollection<T>();
         if (collection.FindOne() != null)
         {
            Random randGen = new Random();
            key = randGen.Next(collection.FindAll().Min(e => e.Id), collection.FindAll().Max(e => e.Id));
         }
         return key;
      }

      private string GetRandomName<T>() where T : IHasName
      {
         string name = string.Empty;
         var collection = GetCollection<T>();
         T entity = collection.FindOne(Query<T>.EQ(e => e.Id, GetRandomKey<T>()));
         if (entity != null)
            name = entity.Name;
         return name;
      }

      private string GetRandomUserFirstName<T>() where T : IHasFirstName
      {
         string name = string.Empty;
         var collection = GetCollection<T>();
         T entity = collection.FindOne(Query<T>.EQ(e => e.Id, GetRandomKey<T>()));
         if (entity != null)
            name = entity.FirstName;
         return name;
      }

      private string GetLatestName<T>() where T : IHasName
      {
         var entity = GetCollection<T>().FindAll()
            .SetSortOrder(SortBy.Descending("_id")).FirstOrDefault();
         return entity == null ? string.Empty : entity.Name;
      }     

      private MongoCollection<T> GetCollection<T>() where T : IExperimentClass
      {
         MongoServer server = new MongoClient(connectionString).GetServer();
         MongoDatabase database = server.GetDatabase(databaseName);
         return database.GetCollection<T>(typeof(T).Name);
      }

      private long ConvertToCount<T>(float portion) where T : IExperimentClass
      {
         long count = GetTotalCount<T>();
         float ratio = portion;
         if (ratio < 0)
            ratio = 0;
         if (portion > 1)
            ratio = 1;
         var result = (ratio * count);
         if (result > 0 && result < 1)
            result = 1;
         return (long)result;
      }

      private readonly string connectionString;
      private readonly string databaseName;     
   }
}
