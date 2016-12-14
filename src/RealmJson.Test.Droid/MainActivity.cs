using System.Reflection;
using Android.App;
using Android.OS;
using Xamarin.Android.NUnitLite;

namespace RealmJson.Test.Droid
{
	[Activity(Label = "RealmJson.Test.Droid", MainLauncher = true)]
	public class MainActivity : TestSuiteActivity
	{
		protected override void OnCreate(Bundle bundle)
		{
			AddTest(Assembly.GetExecutingAssembly());
			//AddTest(typeof(RealmJson.PCL.Tests.Tests).Assembly);
			//GCAfterEachFixture = true;

			base.OnCreate(bundle);
		}
	}
}
