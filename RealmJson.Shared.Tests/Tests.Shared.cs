using System;
using System.IO;
using System.Text;
using System.Collections.Concurrent;
using NUnit.Framework;
using RealmJson.Extensions;
using Realms;
using System.Threading.Tasks;
using System.Threading;
#if __ANDROID__
using Android.App;
using System.Linq;
using Java.Security;
using Android.Util;
#endif
#if __IOS__
#endif

namespace RealmJson.Test
{
	[TestFixture]
	public partial class Tests
	{
		public void DeleteRealmDB(string realmDBFullPath)
		{
			if (File.Exists(realmDBFullPath))
			{
				File.Delete(realmDBFullPath);
			}
		}

		string RealmDBTempPath()
		{
			return Path.GetTempFileName();
		}

		[SetUp]
		public void Setup()
		{
		}

		[TearDown]
		public void Tear()
		{
		}

		public void Log(string text)
		{
#if __ANDROID__
			Android.Util.Log.Debug("REALM", text);
#endif

#if __IOS__
			Console.WriteLine("[REALM] " + text);
#endif
		}

#if false

		[Test]
		public void CreateObjectFromJson_String()
		{
			var jsonString = @"
{
    ""name"": ""Alabama"",
    ""abbreviation"": ""AL""
}
";
			using (var theRealm = Realm.GetInstance(RealmDBTempPath()))
			{
				var testObject = theRealm.CreateObjectFromJson<StateUnique>(jsonString);
				Assert.IsTrue(testObject.abbreviation == "AL" & testObject.name == "Alabama");
			}
		}

		[Test]
		public void CreateObjectFromJson_Stream()
		{
			var jsonString = @"
{
    ""name"": ""Alabama"",
    ""abbreviation"": ""AL""
}
";
			byte[] byteArray = Encoding.UTF8.GetBytes(jsonString);
			using (var stream = new MemoryStream(byteArray))
			using (var theRealm = Realm.GetInstance(RealmDBTempPath()))
			{
				var testObject = theRealm.CreateObjectFromJson<StateUnique>(stream);
				Assert.IsTrue(testObject.abbreviation == "AL" & testObject.name == "Alabama");
			}
		}


		public string CreateTestDBFromJson()
		{
			var jsonString = @"
[
    {
        ""name"": ""Alabama"",
        ""abbreviation"": ""AL""
    },
    {
        ""name"": ""Oklahoma"",
        ""abbreviation"": ""OK""
    },
    {
        ""name"": ""Wyoming"",
        ""abbreviation"": ""WY""
    }
]
";
			var dbFile = RealmDBTempPath();
			using (var theRealm = Realm.GetInstance(dbFile))
			{
				theRealm.CreateAllFromJson<StateUnique>(jsonString);
			}
			return dbFile;
		}

		[Test]
		public void CreateOrUpdateObjectFromJson_Create_PKisString_String()
		{
			var jsonString = @"
				{
				    ""name"": ""NEW"",
				    ""abbreviation"": ""NW""
				}";
			var dbFile = CreateTestDBFromJson();

			using (var theRealm = Realm.GetInstance(dbFile))
			{
				var realmObject = theRealm.CreateOrUpdateObjectFromJson<StateUnique>(jsonString);
				Assert.IsTrue(realmObject.abbreviation == "NW" & realmObject.name == "NEW");
			}
		}

		[Test]
		public void CreateOrUpdateObjectFromJson_Update_PKisString_String()
		{
			var jsonString = @"
				{
				    ""name"": ""UPDATED"",
				    ""abbreviation"": ""OK""
				}";

			var dbFile = CreateTestDBFromJson();

			using (var theRealm = Realm.GetInstance(dbFile))
			{
				var realmObject = theRealm.CreateOrUpdateObjectFromJson<StateUnique>(jsonString);
				Assert.IsTrue(realmObject.name == "UPDATED");
			}
		}

		[Test]
		public void CreateAllFromJson_String()
		{
			var jsonString = @"
[
    {
        ""name"": ""Alabama"",
        ""abbreviation"": ""AL""
    },
    {
        ""name"": ""Oklahoma"",
        ""abbreviation"": ""OK""
    },
    {
        ""name"": ""Wyoming"",
        ""abbreviation"": ""WY""
    }
]
";
			using (var theRealm = Realm.GetInstance(RealmDBTempPath()))
			{
				theRealm.CreateAllFromJson<StateUnique>(jsonString);
				Assert.AreEqual(3, theRealm.All<StateUnique>().Count());
			}
		}

		[Test]
		public void CreateAllFromJson_Stream()
		{
			var jsonString = @"
[
    {
        ""name"": ""Alabama"",
        ""abbreviation"": ""AL""
    },
{
        ""name"": ""Wyoming"",
        ""abbreviation"": ""WY""
    }
]
";
			byte[] byteArray = Encoding.UTF8.GetBytes(jsonString);
			using (var stream = new MemoryStream(byteArray))
			using (var theRealm = Realm.GetInstance(RealmDBTempPath()))
			{
				theRealm.CreateAllFromJson<StateUnique>(stream);
				Assert.AreEqual(2, theRealm.All<StateUnique>().Count());

				// Did the records get created properly from the steam?
				Assert.IsTrue(theRealm.All<StateUnique>().Where((State s) => s.abbreviation == "AL").Any());
				Assert.IsTrue(theRealm.All<StateUnique>().Last().abbreviation == "WY");
			}
		}

		//[Test]
		//public void CreateAllFromJson_InvalidStream()
		//{
		//	var jsonString = @"";
		//	byte[] byteArray = Encoding.UTF8.GetBytes(jsonString);
		//	using (var stream = new MemoryStream(byteArray))
		//	using (var theRealm = Realm.GetInstance(RealmDBTempPath()))
		//	{
		//		var ex = Assert.Throws<Newtonsoft.Json.JsonSerializationException>(() => theRealm.CreateAllFromJson<StateUnique>(stream));
		//		Assert.That(ex, Is.EqualTo(new Newtonsoft.Json.JsonSerializationException()));
		//	}
		//}

#if __ANDROID__

		[Test]
		public void CreateAllFromJson_From_AndroidAssetStream()
		{
			using (var theRealm = Realm.GetInstance(RealmDBTempPath()))
			using (var assetStream = Application.Context.Assets.Open("States.json"))
			{
				theRealm.CreateAllFromJson<StateUnique>(assetStream);
				Assert.AreEqual(59, theRealm.All<StateUnique>().Count());
			}
		}

#endif

#if __IOS__

		[Test]
		public void CreateAllFromJson_iOSBundleResourceStream()
		{
			using (var theRealm = Realm.GetInstance(RealmDBTempPath()))
			using (var fileStream = new FileStream("./Data/States.json", FileMode.Open, FileAccess.Read))
			{
				theRealm.CreateAllFromJson<StateUnique>(fileStream);
				Assert.AreEqual(59, theRealm.All<StateUnique>().Count());
			}
		}

#endif

#endif

	}
}