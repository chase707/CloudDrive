
namespace CloudDrive.Data
{
	public interface ICloudUserDataSource
	{
		CloudUser Get();
		void Set(CloudUser cloudUser);
	}
}
