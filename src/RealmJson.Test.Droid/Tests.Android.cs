using Android.App;
using NUnit.Framework;
using Realms;
using SushiHangover.RealmJson;

namespace RealmJson.Test
{
	[TestFixture]
	public partial class Tests
	{

#if !PERF

		[Test]
		public void CreateAllFromJson_From_AndroidAssetStream()
		{
			using (var theRealm = Realm.GetInstance(RealmDBTempPath()))
			using (var assetStream = Application.Context.Assets.Open("States.json"))
			{
				theRealm.CreateAllFromJson<State>(assetStream);
				Assert.AreEqual(59, theRealm.All<State>().Count());
			}
		}

#endif

		public void Log(string text)
		{
			Android.Util.Log.Debug("REALM", text);
		}

		public ulong ConsumedMemory()
		{
			// HACK: Quick mem available, should also be looking at Cached, etc...
			// https://developer.android.com/reference/android/app/ActivityManager.MemoryInfo.html
			var mi = new ActivityManager.MemoryInfo();
			var activityManager = (ActivityManager)Application.Context.GetSystemService(Application.ActivityService);
			activityManager.GetMemoryInfo(mi);
			return (ulong)(mi.TotalMem - mi.AvailMem);
		}
	}
}
