using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace CloudDrive.Data.MongoDb
{
	public class CloudUserDataSource : ICloudUserDataSource
	{
		protected string DBConn { get; set; }
		protected string DatabaseName { get; set; }
		protected MongoClient DBClient { get; set; }
		protected MongoDatabase Database { get; set; }
		public CloudUserDataSource(string dbConn, string database)
		{
			this.DBConn = dbConn;
			this.DatabaseName = database;
			this.DBClient = new MongoClient(DBConn);			
			this.Database = DBClient.GetServer().GetDatabase(DatabaseName);
		}

		public CloudUser Get(string userId)
		{
			var collection = Database.GetCollection<CloudUser>("cloudusers"); 
			var query = Query<CloudUser>.EQ(e => e.UniqueName, userId);
			return collection.FindOne(query);
		}

		public void Set(CloudUser cloudUser)
		{
			var collection = Database.GetCollection<CloudUser>("cloudusers"); 
			collection.Save(cloudUser);
		}
	}
}
