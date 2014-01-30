using System.IO;
using System.Runtime.Serialization;
using System.Xml.Linq;
using SkyNet.Model;

namespace CloudDrive.Service.SkyDrive
{
    public class PersistantAccessToken
    {
		const string FileName = "AccessToken.xml";
		string AccessTokenFolder { get; set; }
		private UserToken _AccessToken;
		public UserToken AccessToken
		{
			get
			{
				return _AccessToken;
			}
			set
			{
				_AccessToken = value;
				Persist();
			}
		}

		public PersistantAccessToken(string folder)
		{
			AccessTokenFolder = folder;
			Refresh();
		}

		void Persist()
		{
			var doc = Serialize();
			doc.Save(GetFullPath(), SaveOptions.None);
		}

		void Refresh()
		{
			try
			{
				var configurationFile = XDocument.Load(GetFullPath());
				if (configurationFile != null)
				{
					this.AccessToken = Deserialize(configurationFile);
				}
			}
			catch (System.Xml.XmlException)
			{
				
			}
		}

		XDocument Serialize()
		{
			XDocument doc = new XDocument();
			using (var writer = doc.CreateWriter())
			{
				// write xml into the writer
				var serializer = new DataContractSerializer(this._AccessToken.GetType());
				serializer.WriteObject(writer,  this._AccessToken);
			}

			return doc;
		}

		UserToken Deserialize(XDocument document)
		{
			using (var reader = document.CreateReader())
			{
				var serializer = new DataContractSerializer(typeof(UserToken));
				return (UserToken)serializer.ReadObject(reader);
			}
		}

		string GetFullPath()
		{
			return Path.Combine(this.AccessTokenFolder, FileName);
		}
	}
}

