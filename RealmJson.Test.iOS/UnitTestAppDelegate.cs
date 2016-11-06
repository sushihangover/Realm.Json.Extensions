using Foundation;
using MonoTouch.NUnit.UI;
using UIKit;

namespace RealmJson.Test.iOS
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the 
	// User Interface of the application, as well as listening (and optionally responding) to 
	// application events from iOS.
	[Register("UnitTestAppDelegate")]
	public partial class UnitTestAppDelegate : UIApplicationDelegate
	{
		// class-level declarations
		UIWindow window;
		TouchRunner runner;

		//
		// This method is invoked when the application has loaded and is ready to run. In this 
		// method you should instantiate the window, load the UI into it and then make the window
		// visible.
		//
		// You have 17 seconds to return from this method, or iOS will terminate your application.
		//
		public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
		{
			// create a new window instance based on the screen size
			window = new UIWindow(UIScreen.MainScreen.Bounds);
			runner = new TouchRunner(window);

			// register every tests included in the main application/assembly
			runner.Add(System.Reflection.Assembly.GetExecutingAssembly());
			// start running the test suites as soon as the application is loaded
			runner.AutoStart = true;
			// crash the application (to ensure it's ended) and return to springboard
			runner.TerminateAfterExecution = true;

			window.RootViewController = new UINavigationController(runner.GetViewController());

			// make the window visible
			window.MakeKeyAndVisible();

			return true;
		}
	}
}
