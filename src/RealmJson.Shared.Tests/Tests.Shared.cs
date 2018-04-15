using System;
using System.Collections.Generic;
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

		public string RealmDBTempPath()
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
				Assert.IsTrue(testObject.Abbreviation == "AL" & testObject.Name == "Alabama");
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
				Assert.IsTrue(testObject.Abbreviation == "AL" & testObject.Name == "Alabama");
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
				Assert.IsTrue(theRealm.Find<StateUnique>("AL") == null);
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
				Assert.IsTrue(testObject.Abbreviation == "AL" & testObject.Name == "Alabama");
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
				Assert.IsTrue(realmObject.Abbreviation == "NW" & realmObject.Name == "NEW");
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
				Assert.IsTrue(realmObject.Name == "UPDATED");
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
				Assert.IsTrue(theRealm.All<StateUnique>().Where((StateUnique s) => s.Abbreviation == "AL").Any());
				Assert.IsTrue(theRealm.All<StateUnique>().Last().Abbreviation == "WY");
			}
		}

		//[Test]
		//public void CreateAllFromJsonViaAutoMapperFromJson__NewRecords_Stream()
		//{
		//    byte[] byteArray = Encoding.UTF8.GetBytes(jsonStringTwoState);
		//    using (var stream = new MemoryStream(byteArray))
		//    using (var theRealm = Realm.GetInstance(RealmDBTempPath()))
		//    {
		//        theRealm.CreateAllFromJsonViaAutoMapper<StateUnique>(stream);
		//        Assert.AreEqual(2, theRealm.All<StateUnique>().Count());

		//        // Did the records get AutoMapped properly from the steam?
		//        Assert.IsTrue(theRealm.All<StateUnique>().Where((StateUnique s) => s.Abbreviation == "AL").Any());
		//        Assert.IsTrue(theRealm.All<StateUnique>().Last().Abbreviation == "WY");
		//    }
		//}

		//[Test]
		//public void CreateAllFromJsonViaAutoMapperFromJson__UpdateRecords_Stream()
		//{
		//    var dbFile = CreateTestDBFromJson(jsonStringTwoState);
		//    byte[] byteArray = Encoding.UTF8.GetBytes(jsonStringThreeState);
		//    using (var stream = new MemoryStream(byteArray))
		//    using (var theRealm = Realm.GetInstance(dbFile))
		//    {
		//        Assert.AreEqual(2, theRealm.All<StateUnique>().Count());
		//        theRealm.CreateAllFromJsonViaAutoMapper<StateUnique>(stream);
		//        Assert.AreEqual(3, theRealm.All<StateUnique>().Count());

		//        // Did the "new" record get AutoMapped properly from the steam?
		//        Assert.IsTrue(theRealm.All<StateUnique>().Where((StateUnique s) => s.Abbreviation == "OK").Any());
		//    }
		//}

		//[Test]
		//public void CreateAllFromJsonViaAutoMapperFromJson__UpdateRecords__InTrans_Stream()
		//{
		//    var dbFile = CreateTestDBFromJson(jsonStringTwoState);
		//    byte[] byteArray = Encoding.UTF8.GetBytes(jsonStringThreeState);
		//    using (var stream = new MemoryStream(byteArray))
		//    using (var theRealm = Realm.GetInstance(dbFile))
		//    {
		//        Assert.AreEqual(2, theRealm.All<StateUnique>().Count());
		//        using (var transaction = theRealm.BeginWrite())
		//        {
		//            theRealm.CreateAllFromJsonViaAutoMapper<StateUnique>(stream, true);
		//            transaction.Commit();
		//        }
		//        Assert.AreEqual(3, theRealm.All<StateUnique>().Count());
		//        // Did the "new" record get AutoMapped properly from the steam?
		//        Assert.IsTrue(theRealm.All<StateUnique>().Where((StateUnique s) => s.Abbreviation == "OK").Any());
		//    }
		//}

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

		[Test]
		public void NonManagedCollectionCopyFromNonManagedGenericType()
		{
			var nonManagedCollection = CreateStateTestCollection();
			var nonManagedCollectionCopy = nonManagedCollection.NonManagedCopy();
			Assert.False(nonManagedCollectionCopy.All(element => element.IsManaged));
			Assert.True(EnumerablesAreEqual(nonManagedCollectionCopy, nonManagedCollection));
		}

		[Test]
		public void NonManagedCollectionCopyFromManagedGenericType()
		{
			using (var theRealm = Realm.GetInstance(RealmDBTempPath()))
			{
				theRealm.Write(() =>
				{
					var nonManagedCollection = CreateStateTestCollection();
					foreach (var item in nonManagedCollection)
					{
						theRealm.Add(item);
					}
				});

				var managedCollection = theRealm.All<State>();

				foreach (var managedObject in managedCollection)
				{
					Assert.True(managedObject.IsManaged);
				}

				var nonManagedCollectionCopy = managedCollection.NonManagedCopy();

				foreach (var managedObject in nonManagedCollectionCopy)
				{
					Assert.False(managedObject.IsManaged);
				}

				Assert.True(EnumerablesAreEqual(managedCollection, nonManagedCollectionCopy));
			}
		}

		[Test]
		public void ManagedToNonManagedWithIList()
		{
			using (var theRealm = Realm.GetInstance(RealmDBTempPath()))
			{
				theRealm.Write(() =>
				{
					var nonManagedCollection = CreateStateCityTestCollection();
					foreach (var item in nonManagedCollection)
					{
							 //foreach (var city in item.Cities)
							 //{
							 //    theRealm.Add(city, true);
							 //}
							 var state = theRealm.Add(item);
					}
				});
				var managedCollection = theRealm.All<StatesWithCities>();

				var count = 0;
				foreach (var managedObject in managedCollection)
				{
					var nonMananged = managedObject.NonManagedCopy<StatesWithCities>();
					count++;
					Assert.False(nonMananged.IsManaged);
					Assert.True(nonMananged.Cities.Count() == count);
				}
			}
		}

		private IList<State> CreateStateTestCollection()
		{
			var testObject1 = new State { Name = "XX", Abbreviation = "X" };
			var testObject2 = new State { Name = "YY", Abbreviation = "Y" };
			var testObject3 = new State { Name = "ZZ", Abbreviation = "Z" };
			var testCollection = new[] { testObject1, testObject2, testObject3 };
			return testCollection;
		}

		private IList<StatesWithCities> CreateStateCityTestCollection()
		{
			var testCity1 = new City { Name = "AA", Abbreviation = "A" };
			var testCity2 = new City { Name = "BB", Abbreviation = "B" };
			var testCity3 = new City { Name = "CC", Abbreviation = "C" };
			var testCityCollection = new[] { testCity1, testCity2, testCity3 };

			var testObject1 = new StatesWithCities { Name = "XX", Abbreviation = "X" };
			testObject1.Cities.Add(testCityCollection[0]);

			var testObject2 = new StatesWithCities { Name = "YY", Abbreviation = "Y" };
			testObject2.Cities.Add(testCityCollection[0]);
			testObject2.Cities.Add(testCityCollection[1]);

			var testObject3 = new StatesWithCities { Name = "ZZ", Abbreviation = "Z" };
			testObject3.Cities.Add(testCityCollection[0]);
			testObject3.Cities.Add(testCityCollection[1]);
			testObject3.Cities.Add(testCityCollection[2]);

			var testCollection = new[] { testObject1, testObject2, testObject3 };
			return testCollection;
		}

		private bool EnumerablesAreEqual(IEnumerable<State> enumerable1, IEnumerable<State> enumerable2)
		{
			var elementCount = enumerable1.Count();
			if (elementCount != enumerable2.Count()) return false;
			for (var i = 0; i < elementCount; ++i)
			{
				var element1 = enumerable1.ElementAt(i);
				var element2 = enumerable2.ElementAt(i);
				if (element1.Name != element2.Name) return false;
				if (element1.Abbreviation != element2.Abbreviation) return false;
			}
			return true;
		}

		[Test]
		public void NonManagedCopyFromNonManagedGenericType()
		{
			var obj1 = new State
			{
				Name = "ZZ",
				Abbreviation = "Z"
			};
			var obj2 = obj1.NonManagedCopy<State>();
			Assert.False(obj2.IsManaged);
			// Non-managed objects are never equivalate, this RealmObject.Equals can not be used
			Assert.AreEqual(obj1.Abbreviation, obj2.Abbreviation);
			Assert.AreEqual(obj1.Name, obj2.Name);
		}

		[Test]
		public void NonManagedCopyFromNonManagedRealmObjectType()
		{
			var obj1 = new State
			{
				Name = "ZZ",
				Abbreviation = "Z"
			};
			var obj2 = obj1.NonManagedCopy<State>();
			// Non-managed objects are never equivalate, this RealmObject.Equals can not be used
			Assert.False(obj2.IsManaged);
			Assert.AreEqual(obj1.Abbreviation, obj2.Abbreviation);
			Assert.AreEqual(obj1.Name, obj2.Name);
		}

		//[Test]
		//public void NonManagedCopyFromManagedGenericType()
		//{
		//    using (var theRealm = Realm.GetInstance(RealmDBTempPath()))
		//    {
		//        State obj1 = null;
		//        theRealm.Write(() =>
		//        {
		//            var tmp = new State
		//            {
		//                Name = "ZZ",
		//                Abbreviation = "Z"
		//            };
		//            obj1 = theRealm.Add(tmp);
		//        });
		//        Assert.True(obj1.IsManaged);
		//        var obj2 = obj1.NonManagedCopy<State>();
		//        // Non-managed objects are never equivalate, this RealmObject.Equals can not be used
		//        Assert.False(obj2.IsManaged);
		//        Assert.AreEqual(obj1.Abbreviation, obj2.Abbreviation);
		//        Assert.AreEqual(obj1.Name, obj2.Name);
		//    }
		//}

		[Test]
		public void NonManagedCopyFromManagedRealmObjectType()
		{
			using (var theRealm = Realm.GetInstance(RealmDBTempPath()))
			{
				State obj1 = null;
				theRealm.Write(() =>
				{
					var tmp = new State
					{
						Name = "ZZ",
						Abbreviation = "Z"
					};
					obj1 = theRealm.Add(tmp);
				});
				Assert.True(obj1.IsManaged);
				var obj2 = obj1.NonManagedCopy<State>();
				// Non-managed objects are never equivalate, this RealmObject.Equals can not be used
				Assert.False(obj2.IsManaged);
				Assert.AreEqual(obj1.Abbreviation, obj2.Abbreviation);
				Assert.AreEqual(obj1.Name, obj2.Name);
			}
		}

#endif

	}
}