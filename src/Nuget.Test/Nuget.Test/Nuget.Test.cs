using System;
using Xamarin.Forms;
using System.Runtime.InteropServices;
using System.Reflection;
using System.IO;
using Realms;
using SushiHangover.RealmJson;
using System.Linq;
using D = System.Diagnostics.Debug;

namespace Nuget.Test
{
	public class App : Application
	{
		Label label;
		void Button_Clicked(object sender, EventArgs e)
		{
			// Note: Using embedded resources is a REALLY bad thing for mobile performance, do not do it this with for 
			// production code. Use a dependancy service to obtain a stream from the Asset or BundleResource

			//var assembly = typeof(App).GetTypeInfo().Assembly;
			//var resourceName = "Nuget.Test.States.json";
			using (var theRealm = Realm.GetInstance())
			using (Stream stream = typeof(App).GetTypeInfo().Assembly.GetManifestResourceStream("Nuget.Test.States.json"))
			using (StreamReader reader = new StreamReader(stream))
			{
				theRealm.CreateAllFromJson<State>(stream);
				foreach (var state in theRealm.All<State>())
				{
					D.WriteLine(state.abbreviation);
					label.Text += state.abbreviation + "\n";
				}
			}
		}

		public App()
		{
			// The root page of your application
			var button = new Button()
			{
				Text = "Bulk Load Json"
			};
			label = new Label()
			{
				LineBreakMode = LineBreakMode.WordWrap,
				VerticalOptions = LayoutOptions.FillAndExpand,
				HorizontalOptions = LayoutOptions.FillAndExpand,
			};
			var content = new ContentPage
			{
				Title = "SushiHangover.RealmJson.Extensions",
				Content = new StackLayout
				{
					VerticalOptions = LayoutOptions.Center,
					Children = {
						new Label {
							HorizontalTextAlignment = TextAlignment.Center,
							Text = "Welcome to Xamarin Realm does Json"
						},
						button,
						new ScrollView {
							Content = label
						}
					}
				}
			};

			button.Clicked += Button_Clicked;

			MainPage = new NavigationPage(content);
		}

		protected override void OnStart()
		{
			// Handle when your app starts
		}

		protected override void OnSleep()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume()
		{
			// Handle when your app resumes
		}
	}
}
