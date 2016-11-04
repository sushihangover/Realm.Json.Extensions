using System;
using System.Reflection;

using Android.App;
using Android.Content;
using Android.Runtime;

using Xamarin.Android.NUnitLite;
using Android.Util;

namespace RealmJson.Test.Droid
{

	[Instrumentation(Name = "app.tests.TestInstrumentation")]
	public class TestInstrumentation : TestSuiteInstrumentation
	{

		public TestInstrumentation(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer)
		{
		}

		protected override void AddTests()
		{
			AddTest(Assembly.GetExecutingAssembly());
			//GCAfterEachFixture = true;
		}

		public override void Finish(Result resultCode, Android.OS.Bundle results)
		{
			base.Finish(resultCode, results);
		}
	}
}
