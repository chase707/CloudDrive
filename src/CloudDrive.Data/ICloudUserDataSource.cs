
namespace CloudDrive.Data
{
	public interface ICloudUserDataSource
	{
		CloudUser Get(string userId);
		void Set(CloudUser cloudUser);
	}
}
