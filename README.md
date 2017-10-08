<img style="float: right;" src="https://github.com/sushihangover/Realm.Json.Extensions/raw/master/media/SushiHangover.RealmJson.png">

## SushiHangover.RealmJson.Extensions

**Extension Methods for adding JSON APIs to a Realm Instance**

## Current project status:

[![Build status](https://ci.appveyor.com/api/projects/status/ronof3ruyjpl1c4v/branch/master?svg=true)](https://ci.appveyor.com/project/sushihangover/realm-json-extensions/branch/master)

## Nuget:

<div class="nuget-badge">
<p>
<code>
PM> Install-Package RealmJson.Extensions
</code>
</p>
</div>

Ref: [https://www.nuget.org/packages/RealmJson.Extensions](https://www.nuget.org/packages/RealmJson.Extensions)

## Issues?

###Post issues on [GitHub](https://github.com/sushihangover/Realm.Json.Extensions/issues)

## Need Help?

Post on [StackOverflow](http://stackoverflow.com/questions/tagged/xamarin+realm) with the tags: **`[XAMARIN]`** **`[REALM]`** **`[JSON]`**

## API Reference:

[https://sushihangover.github.io/Realm.Json.Extensions/](https://sushihangover.github.io/Realm.Json.Extensions/)

## Extension API:

* A Realm Instance:
	* .CreateAllFromJson\<T\>(string)
	* .CreateAllFromJson\<T\>(Stream)
 	* .CreateAllFromJsonViaAutoMapper\<T\>(Stream)
	* .CreateObjectFromJson\<T\>(string)
	* .CreateObjectFromJson\<T\>(Stream)
	* .CreateOrUpdateObjectFromJson\<T\>(string)
 	* .CreateOrUpdateObjectFromJson\<T\>(Stream)
 	* .CreateOrUpdateObjectFromJson\<T\>(Stream)

* A RealmObject Instance:
 	* .NonManagedCopy\<T\>()
 	* .NonManagedCopy()

* A RealmResult Instance (IQueryable): 
	* .NonManagedCopy<T>

## Github Repo:

[https://github.com/sushihangover/Realm.Json.Extensions](https://github.com/sushihangover/Realm.Json.Extensions)

## Usage / Examples:
	
### Single RealmObject from Json-based Strings:
	
	using (var theRealm = Realm.GetInstance(RealmDBTempPath()))
	{
		var realmObject = theRealm.CreateObjectFromJson<StateUnique>(jsonString);
	}

### Single RealmObject from a Stream:

	using (var stream = new MemoryStream(byteArray))
	using (var theRealm = Realm.GetInstance(RealmDBTempPath()))
	{
		var testObject = theRealm.CreateObjectFromJson<StateUnique>(stream);
	}


### Using Json Arrays from a Android Asset Stream:

	using (var theRealm = Realm.GetInstance(RealmDBTempPath()))
	using (var assetStream = Application.Context.Assets.Open("States.json"))
	{
		theRealm.CreateAllFromJson<State>(assetStream);
	}

### Using Json Arrays from a iOS Bundled Resource Stream:

	using (var theRealm = Realm.GetInstance(RealmDBTempPath()))
	using (var fileStream = new FileStream("./Data/States.json", FileMode.Open, FileAccess.Read))
	{
		theRealm.CreateAllFromJson<State>(fileStream);
	}

**Note:** Consult the Unit Tests for more usage details

### <sup>(1)</sup>`Xamarin.Forms` Usage

`AutoMapper` does not support PCL `Profile 259`[<sup>(2)</sup>)](https://github.com/AutoMapper/AutoMapper/issues/1531) and thus adding this Nuget package will fail if applied to a default `Xamarin.Form` project. 

You can change your Xamarin.From project to `Profile 
111`, then **retarget** the `Xamarin.Forms` package, and you will be able to add the Nuget package.

See the `Xamarin.Forms`-based (`Nuget.Test`) project in the code repo as an example.

Note: Once Xamarin has full support for `.NET Standard` and AutoMapper releases v5.2 [<sup>(3)</sup>](https://github.com/AutoMapper/AutoMapper/issues/1532), this mess should go away.

<sup>(2)</sup> https://github.com/AutoMapper/AutoMapper/issues/1531

<sup>(3)</sup> https://github.com/AutoMapper/AutoMapper/issues/1532

## Build Documention:

API Reference documention is built via the great <a href="http://www.doxygen.org/index.html">
<img src="http://www.stack.nl/~dimitri/doxygen/doxygen.png" alt="doxygen"/>
</a>

<div class="code">
doxygen Doxygen/realmthread.config
</div>

## Building:

### `xbuild` or `msbuild` based:

	xbuild /p:SolutionDir=./ /target:Clean /p:Configuration=Release   RealmJson.Extensions/RealmJson.Extensions.csproj
	xbuild /p:SolutionDir=./ /target:Build /p:Configuration=Release RealmJson.Extensions/RealmJson.Extensions.csproj


## Testing / NUnitLite Automation:

### `Xamarin.Android`

	adb shell am instrument -w RealmJson.Test.Droid/app.tests.TestInstrumentation


Ref: [Automate Android Nunit Test](https://developer.xamarin.com/guides/android/troubleshooting/questions/automate-android-nunit-test/)

Ref: [Issue 32005](https://bugzilla.xamarin.com/show_bug.cgi?id=32005)


### `Xamarin.iOS`
	
	xbuild RealmJson.Test.iOS/RealmJson.Test.iOS.csproj /p:SolutionDir=~/
	xcrun simctl list
	open -a Simulator --args -CurrentDeviceUDID 360D3BC6-4A6D-4B7E-A899-5C7651EC2107
	xcrun simctl install booted  ./RealmJson.Test.iOS/bin/iPhoneSimulator/Debug/RealmJson.Test.iOS.app
	xcrun simctl launch booted com.sushihangover.realmjson-test-ios
	cat ~/Library/Logs/CoreSimulator/360D3BC6-4A6D-4B7E-A899-5C7651EC2107/system.log

## Performance Testing:

There is a `#if PERF` within `Tests.Shared.Perf.Streaming.cs` that contains multple tests to evaluate streaming performance and memory overhead. 

These test methods **try** to evaluate:

1. `Realm.Manage` vs. `AutoMapper` performance/effeciency
1. Upper limits of the number of `RealmObject`s can be used in a Realm `Transaction` on a mobile device.

See [Perf.md](https://github.com/sushihangover/Realm.Json.Extensions/blob/master/Perf.md) for details.

## License

Consult [LICENSE](https://github.com/sushihangover/Realm.Json.Extensions/blob/master/LICENSE)

