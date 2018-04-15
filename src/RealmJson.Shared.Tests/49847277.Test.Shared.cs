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
	public partial class Tests
	{

		const string jsonBacklinks = @"
{
  ""$id"": ""1"",
  ""Id"": ""979e7341-0d16-4ba4-b91b-31ec81bb18ad"",
  ""BList"": [
    {
      ""$id"": ""2"",
      ""Id"": ""dbb35317-eae0-4978-9675-e0246805fc34"",
      ""CList"": [
        {
          ""$id"": ""3"",
          ""Id"": ""2da5ac92-bc73-4f80-8a27-051bbf4e5e66"",

		  },
        {
          ""$id"": ""4"",
          ""Id"": ""a40f7f12-7eee-47ee-845f-4481b72c0109"",
        },
        {
          ""$id"": ""5"",
          ""Id"": ""37606fc1-74a0-4a9e-a802-587076429edc"",
        }
      ]
    }
  ]
}
";

		[Test]
		[Ignore("Not currently supported")]
		public void LoadBackLinks()
		{
			using (var theRealm = Realm.GetInstance(RealmDBTempPath()))
			{
				theRealm.CreateObjectFromJson<A>(jsonBacklinks);
				Assert.AreEqual(1, theRealm.All<A>().Count());
				var z = theRealm.All<A>().First().BList;
				Assert.AreEqual(1, theRealm.All<A>().First().BList.Count());
				Assert.AreEqual(3, theRealm.All<B>().First().CList.Count());
			}
		}
	}
}
