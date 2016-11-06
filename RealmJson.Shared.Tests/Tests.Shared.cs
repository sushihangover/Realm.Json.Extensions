using System;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Realms;
using SushiHangover.RealmJson;

namespace RealmJson.Test
{
	[TestFixture]
	public partial class Tests
	{
		const string jsonStringSingleState = @"
{
    ""name"": ""Alabama"",
    ""abbreviation"": ""AL""
}
";

		const string jsonStringTwoState = @"
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

		const string jsonStringThreeState = @"
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

#if !PERF

		[Test]
		public void CreateObjectFromJson_String()
		{
			using (var theRealm = Realm.GetInstance(RealmDBTempPath()))
			{
				var testObject = theRealm.CreateObjectFromJson<StateUnique>(jsonStringSingleState);
				Assert.IsTrue(testObject.abbreviation == "AL" & testObject.name == "Alabama");
			}
		}

		[Test]
		public void CreateObjectFromJson_InTransaction_Stream()
		{
			byte[] byteArray = Encoding.UTF8.GetBytes(jsonStringSingleState);
			using (var stream = new MemoryStream(byteArray))
			using (var theRealm = Realm.GetInstance(RealmDBTempPath()))
			{
				StateUnique testObject;
				using (var transaction = theRealm.BeginWrite())
				{
					testObject = theRealm.CreateObjectFromJson<StateUnique>(stream, inTransaction: true);
					transaction.Commit();
				}
				Assert.IsTrue(testObject.abbreviation == "AL" & testObject.name == "Alabama");
			}
		}

		[Test]
		public void CreateObjectFromJson_InTransaction_RollBack_Stream()
		{
			byte[] byteArray = Encoding.UTF8.GetBytes(jsonStringSingleState);
			using (var stream = new MemoryStream(byteArray))
			using (var theRealm = Realm.GetInstance(RealmDBTempPath()))
			{
				using (var transaction = theRealm.BeginWrite())
				{
					theRealm.CreateObjectFromJson<StateUnique>(stream, inTransaction: true);
					transaction.Rollback();
				}
				Assert.IsTrue(theRealm.ObjectForPrimaryKey<StateUnique>("AL") == null);
			}
		}

		[Test]
		public void CreateObjectFromJson_Stream()
		{
			byte[] byteArray = Encoding.UTF8.GetBytes(jsonStringSingleState);
			using (var stream = new MemoryStream(byteArray))
			using (var theRealm = Realm.GetInstance(RealmDBTempPath()))
			{
				var testObject = theRealm.CreateObjectFromJson<StateUnique>(stream);
				Assert.IsTrue(testObject.abbreviation == "AL" & testObject.name == "Alabama");
			}
		}

		public string CreateTestDBFromJson(string jsonString)
		{
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
			var dbFile = CreateTestDBFromJson(jsonStringTwoState);

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

			var dbFile = CreateTestDBFromJson(jsonStringThreeState);

			using (var theRealm = Realm.GetInstance(dbFile))
			{
				var realmObject = theRealm.CreateOrUpdateObjectFromJson<StateUnique>(jsonString);
				Assert.IsTrue(realmObject.name == "UPDATED");
			}
		}

		[Test]
		public void CreateAllFromJson_String()
		{
			using (var theRealm = Realm.GetInstance(RealmDBTempPath()))
			{
				theRealm.CreateAllFromJson<StateUnique>(jsonStringThreeState);
				Assert.AreEqual(3, theRealm.All<StateUnique>().Count());
			}
		}

		[Test]
		public void CreateAllFromJson_Stream()
		{
			byte[] byteArray = Encoding.UTF8.GetBytes(jsonStringTwoState);
			using (var stream = new MemoryStream(byteArray))
			using (var theRealm = Realm.GetInstance(RealmDBTempPath()))
			{
				theRealm.CreateAllFromJson<StateUnique>(stream);
				Assert.AreEqual(2, theRealm.All<StateUnique>().Count());

				// Did the records get created properly from the steam?
				Assert.IsTrue(theRealm.All<StateUnique>().Where((StateUnique s) => s.abbreviation == "AL").Any());
				Assert.IsTrue(theRealm.All<StateUnique>().Last().abbreviation == "WY");
			}
		}

		[Test]
		public void CreateAllFromJsonViaAutoMapperFromJson__NewRecords_Stream()
		{
			byte[] byteArray = Encoding.UTF8.GetBytes(jsonStringTwoState);
			using (var stream = new MemoryStream(byteArray))
			using (var theRealm = Realm.GetInstance(RealmDBTempPath()))
			{
				theRealm.CreateAllFromJsonViaAutoMapper<StateUnique>(stream);
				Assert.AreEqual(2, theRealm.All<StateUnique>().Count());

				// Did the records get AutoMapped properly from the steam?
				Assert.IsTrue(theRealm.All<StateUnique>().Where((StateUnique s) => s.abbreviation == "AL").Any());
				Assert.IsTrue(theRealm.All<StateUnique>().Last().abbreviation == "WY");
			}
		}

		[Test]
		public void CreateAllFromJsonViaAutoMapperFromJson__UpdateRecords_Stream()
		{
			var dbFile = CreateTestDBFromJson(jsonStringTwoState);
			byte[] byteArray = Encoding.UTF8.GetBytes(jsonStringThreeState);
			using (var stream = new MemoryStream(byteArray))
			using (var theRealm = Realm.GetInstance(dbFile))
			{
				Assert.AreEqual(2, theRealm.All<StateUnique>().Count());
				theRealm.CreateAllFromJsonViaAutoMapper<StateUnique>(stream);
				Assert.AreEqual(3, theRealm.All<StateUnique>().Count());

				// Did the "new" record get AutoMapped properly from the steam?
				Assert.IsTrue(theRealm.All<StateUnique>().Where((StateUnique s) => s.abbreviation == "OK").Any());
			}
		}

		[Test]
		public void CreateAllFromJsonViaAutoMapperFromJson__UpdateRecords__InTrans_Stream()
		{
			var dbFile = CreateTestDBFromJson(jsonStringTwoState);
			byte[] byteArray = Encoding.UTF8.GetBytes(jsonStringThreeState);
			using (var stream = new MemoryStream(byteArray))
			using (var theRealm = Realm.GetInstance(dbFile))
			{
				Assert.AreEqual(2, theRealm.All<StateUnique>().Count());
				using (var transaction = theRealm.BeginWrite())
				{
					theRealm.CreateAllFromJsonViaAutoMapper<StateUnique>(stream, true);
					transaction.Commit();
				}
				Assert.AreEqual(3, theRealm.All<StateUnique>().Count());
				// Did the "new" record get AutoMapped properly from the steam?
				Assert.IsTrue(theRealm.All<StateUnique>().Where((StateUnique s) => s.abbreviation == "OK").Any());
			}
		}

		[Test]
		public void CreateAllFromJson_InvalidStream()
		{
			var jsonStringInvalid = @"";
			byte[] byteArray = Encoding.UTF8.GetBytes(jsonStringInvalid);
			using (var stream = new MemoryStream(byteArray))
			using (var theRealm = Realm.GetInstance(RealmDBTempPath()))
			{
				Assert.That(() => theRealm.CreateAllFromJson<StateUnique>(stream),
					Throws.Exception
						.TypeOf<Exception>()
						.With.Property("Message")
						.EqualTo(RealmDoesJson.ExMalFormeJsonMessage)
	           );
			}
		}

#endif

	}
}