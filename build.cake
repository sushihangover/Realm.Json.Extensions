using System.Threading;
//////////////////////////////////////////////////////////////////////
// ADDINS
//////////////////////////////////////////////////////////////////////

#addin "Cake.FileHelpers"
#addin "Cake.AppleSimulator.SushiHangover"

//////////////////////////////////////////////////////////////////////
// TOOLS
//////////////////////////////////////////////////////////////////////

#tool "nuget:?package=Cake.AppleSimulator.SushiHangover"
#tool "nuget:?package=SushiHangover.RealmThread"
#tool "nuget:?package=RealmJson.Extensions"
#tool "nuget:?package=xunit.runner.console"
#tool "nuget:?package=GitVersion.CommandLine"
#tool "GitReleaseManager"
#tool "GitLink"

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
if (string.IsNullOrWhiteSpace(target))
{
    target = "Default";
}

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

// Should MSBuild & GitLink treat any errors as warnings?
var treatWarningsAsErrors = false;

// Build configuration
var local = BuildSystem.IsLocalBuild;
var isRunningOnMacOS = IsRunningOnUnix(); //(Context.Environment.Platform.Family == PlatformFamily.OSX); // Still broken
var isRunningOnUnix = IsRunningOnUnix();
var isRunningOnWindows = IsRunningOnWindows();

var githubOwner = "sushihangover";
var githubRepository = "Realm.Json.Extensions";
var githubUrl = string.Format("https://github.com/{0}/{1}", githubOwner, githubRepository);

var isRunningOnAppVeyor = AppVeyor.IsRunningOnAppVeyor;
var isPullRequest = AppVeyor.Environment.PullRequest.IsPullRequest;
var isRepository = StringComparer.OrdinalIgnoreCase.Equals("sushihangover/Realm.Json.Extensions", AppVeyor.Environment.Repository.Name);

var isReleaseBranch = StringComparer.OrdinalIgnoreCase.Equals("master", AppVeyor.Environment.Repository.Branch);
var isTagged = AppVeyor.Environment.Repository.Tag.IsTag;

// Version
var gitVersion = GitVersion();
var majorMinorPatch = gitVersion.MajorMinorPatch;
var semVersion = gitVersion.SemVer;
var informationalVersion = gitVersion.InformationalVersion;
var nugetVersion = gitVersion.NuGetVersion;
var buildVersion = gitVersion.FullBuildMetaData;

// Artifacts
var artifactDirectory = "./artifacts/";
var packageWhitelist = new[] { "RealmJson.Extensions" };

// Macros
Action Abort = () => { throw new Exception("A non-recoverable fatal error occurred."); };
Action TestFailuresAbort = () => { throw new Exception("Testing revealed failed unit tests"); };
Action NonMacOSAbort = () => { throw new Exception("Running on platforms other macOS is not supported."); };

Action<string> RestorePackages = (solution) =>
{
    NuGetRestore(solution);
};

Action<string, string> Package = (nuspec, basePath) =>
{
    CreateDirectory(artifactDirectory);

    Information("Packaging {0} using {1} as the BasePath.", nuspec, basePath);

    NuGetPack(nuspec, new NuGetPackSettings {
        Authors                  = new [] { "SushiHangover/RobertN" },
        Owners                   = new [] { "sushihangover" },

        ProjectUrl               = new Uri(githubUrl),
        IconUrl                  = new Uri("https://raw.githubusercontent.com/sushihangover/Realm.Json.Extensions/master/media/icon.png"),
        LicenseUrl               = new Uri("https://opensource.org/licenses/MIT"),
        Copyright                = "Copyright (c) SushiHangover/RobertN",
        RequireLicenseAcceptance = false,

        Version                  = nugetVersion,
        Tags                     = new [] {  "Json", "Realm", "Xamarin", "Netwonsoft", "SushiHangover" },
        ReleaseNotes             = new [] { string.Format("{0}/releases", githubUrl) },

        Symbols                  = true,
        Verbosity                = NuGetVerbosity.Detailed,
        OutputDirectory          = artifactDirectory,
        BasePath                 = basePath,
    });
};

Action<string> SourceLink = (solutionFileName) =>
{
    GitLink("./", new GitLinkSettings() {
        RepositoryUrl = githubUrl,
        SolutionFileName = solutionFileName,
        ErrorsAsWarnings = false, 
    });
};

Action<string, string, string> buildThisApp = (p,c,t) =>
{
    Information("{0}:{1}:{2}", t,c,p);
    if (isRunningOnMacOS)
    {
        var settings = new XBuildSettings()
            .WithProperty("SolutionDir", new string[] { @"./" })
            .WithProperty("OutputPath", new string[] { @"../../artifacts/" })
            .SetConfiguration(c)
            .SetVerbosity(Verbosity.Quiet)
            .WithTarget(t);
        XBuild(p, settings);
    };
};

Action<string, string> unitTestApp = (bundleId, appPath) =>
{
    Information("Shutdown");
    ShutdownAllAppleSimulators();

    var setting = new SimCtlSettings() { ToolPath = FindXCodeTool("simctl") };
    var simulators = ListAppleSimulators(setting);
    var device = simulators.First(x => x.Name == "xUnit Runner" & x.Runtime == "iOS 10.2");
    // Information(string.Format($"Name={device.Name}, UDID={device.UDID}, Runtime={device.Runtime}, Availability={device.Availability}"));

    Information("LaunchAppleSimulator");
    LaunchAppleSimulator(device.UDID);
    Thread.Sleep(60 * 1000);

    Information("UninstalliOSApplication");
    UninstalliOSApplication(
        device.UDID, 
        bundleId,
        setting);
	Thread.Sleep(5 * 1000);

    Information("InstalliOSApplication");
    InstalliOSApplication(
        device.UDID,
        appPath,
        setting);
	// Delay to allow simctl install to finish, otherwise you can receive the following error:
	// The request was denied by service delegate (SBMainWorkspace) for reason: 
	// Busy ("Application "cake.applesimulator.test-xunit" is installing or uninstalling, and cannot be launched").     
	Thread.Sleep(5 * 1000);

    Information("TestiOSApplication");
    var testResults = TestiOSApplication(
        device.UDID, 
        bundleId,
        setting);
    Information("Test Results:");
    // Information(string.Format($"Tests Run:{testResults.Run} Passed:{testResults.Passed} Failed:{testResults.Failed} Skipped:{testResults.Skipped} Inconclusive:{testResults.Inconclusive}"));    

    Information("UninstalliOSApplication");
    UninstalliOSApplication(
        device.UDID, 
        bundleId,
        setting);

    Information("Shutdown");
    ShutdownAllAppleSimulators();

    if (testResults.Run > 0 && testResults.Failed > 0) 
    {
	    // Information(string.Format($"Tests Run:{testResults.Run} Passed:{testResults.Passed} Failed:{testResults.Failed} Skipped:{testResults.Skipped} Inconclusive:{testResults.Inconclusive}"));    
		TestFailuresAbort();
    }
};

/////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
/////////////////////////////////////////////////////////////////////////////

Setup((context) =>
{
    Information("Building version {0} of SushiHangover.RealmJson.Extensions. (isTagged: {1})", informationalVersion, isTagged);
});

Teardown((context) =>
{
    // Executed AFTER the last task.
});

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Build")
    .IsDependentOn("RestorePackages")
    .IsDependentOn("UpdateAssemblyInfo")
    .Does (() =>
{
    Action<string> build = (filename) =>
    {
        var solution = System.IO.Path.Combine("./src/", filename);

        Information("Building {0}", solution);

        if (isRunningOnMacOS)
        {
            var settings = new XBuildSettings()
                .SetConfiguration("Release")
                .SetVerbosity(Verbosity.Quiet)
                .WithTarget("Clean");
            XBuild(solution, settings);
            settings = new XBuildSettings()
                .SetConfiguration("Release")
                .SetVerbosity(Verbosity.Quiet)
                .WithTarget("Build");
            XBuild(solution, settings);
        }
        if (isRunningOnWindows)
        {
            MSBuild(solution, new MSBuildSettings()
                .SetConfiguration("Release")
                .SetVerbosity(Verbosity.Minimal)
                .SetNodeReuse(false));
        }
    };

    build("RealmJsonExtensions.sln");
});

Task("UpdateAppVeyorBuildNumber")
    .WithCriteria(() => isRunningOnAppVeyor)
    .Does(() =>
{
    Information("{0}", buildVersion);
    AppVeyor.UpdateBuildVersion(buildVersion);
});

Task("UpdateAssemblyInfo")
    .IsDependentOn("UpdateAppVeyorBuildNumber")
    .Does (() =>
{
    var file = "./src/CommonAssemblyInfo.cs";

    CreateAssemblyInfo(file, new AssemblyInfoSettings {
        Product = "SushiHangover.RealmJson.Extensions",
        Version = majorMinorPatch,
        FileVersion = majorMinorPatch,
        InformationalVersion = informationalVersion,
        Copyright = "Copyright (c) SushiHangover/RobertN"
    });
});

Task("RestorePackages").Does (() =>
{
    RestorePackages("./src/RealmJsonExtensions.sln");
});

Task("RunUnitTests")
    .IsDependentOn("Build")
    .Does(() =>
{
    //XUnit2("./src/RealmThread.Tests/bin/x64/Release/RealmThread.Tests.dll", new XUnit2Settings {
    //    OutputDirectory = artifactDirectory,
    //    XmlReportV1 = false,
    //    NoAppDomain = false // RealmThread.Tests requires AppDomain otherwise it does not resolve System.Reactive.*
    //});
});

Task("Package")
    .IsDependentOn("RunUnitTests")
    .Does (() =>
{
    Package("./src/RealmJson.Extensions.nuspec", "./src");
});

Task("PublishPackages")
    .IsDependentOn("Package")
    .WithCriteria(() => !local)
    .WithCriteria(() => !isPullRequest)
    .WithCriteria(() => isReleaseBranch)
    .WithCriteria(() => isRepository)
    .WithCriteria(() => isTagged)
    .Does (() =>
{
    if (isReleaseBranch && !isTagged)
    {
        Information("Packages will not be published as this release has not been tagged.");
        return;
    }

    // Resolve the API key.
    var apiKey = EnvironmentVariable("NUGET_APIKEY");
    if (string.IsNullOrEmpty(apiKey))
    {
        throw new Exception("The NUGET_APIKEY environment variable is not defined.");
    }

    var source = EnvironmentVariable("NUGET_SOURCE");
    if (string.IsNullOrEmpty(source))
    {
        throw new Exception("The NUGET_SOURCE environment variable is not defined.");
    }

    // only push whitelisted packages.
    foreach(var package in packageWhitelist)
    {
        // only push the package which was created during this build run.
        var packagePath = artifactDirectory + File(string.Concat(package, ".", nugetVersion, ".nupkg"));

        // Push the package.
        NuGetPush(packagePath, new NuGetPushSettings {
            Source = source,
            ApiKey = apiKey
        });
    }
});

Task("CreateRelease")
    .IsDependentOn("Package")
    .WithCriteria(() => !local)
    .WithCriteria(() => !isPullRequest)
    .WithCriteria(() => isRepository)
    .WithCriteria(() => isReleaseBranch)
    .WithCriteria(() => isTagged)
    .Does (() =>
{
    var username = EnvironmentVariable("GITHUB_USERNAME");
    if (string.IsNullOrEmpty(username))
    {
        throw new Exception("The GITHUB_USERNAME environment variable is not defined.");
    }

    var token = EnvironmentVariable("GITHUB_TOKEN");
    if (string.IsNullOrEmpty(token))
    {
        throw new Exception("The GITHUB_TOKEN environment variable is not defined.");
    }

    GitReleaseManagerCreate(username, token, githubOwner, githubRepository, new GitReleaseManagerCreateSettings {
        Milestone         = majorMinorPatch,
        Name              = majorMinorPatch,
        Prerelease        = true,
        TargetCommitish   = "master"
    });
});

Task("PublishRelease")
    .IsDependentOn("RunUnitTests")
    .IsDependentOn("Package")
    .WithCriteria(() => !local)
    .WithCriteria(() => !isPullRequest)
    .WithCriteria(() => isRepository)
    .WithCriteria(() => isReleaseBranch)
    .WithCriteria(() => isTagged)
    .Does (() =>
{
    var username = EnvironmentVariable("GITHUB_USERNAME");
    if (string.IsNullOrEmpty(username))
    {
        throw new Exception("The GITHUB_USERNAME environment variable is not defined.");
    }

    var token = EnvironmentVariable("GITHUB_TOKEN");
    if (string.IsNullOrEmpty(token))
    {
        throw new Exception("The GITHUB_TOKEN environment variable is not defined.");
    }

    // only push whitelisted packages.
    foreach(var package in packageWhitelist)
    {
        // only push the package which was created during this build run.
        var packagePath = artifactDirectory + File(string.Concat(package, ".", nugetVersion, ".nupkg"));

        GitReleaseManagerAddAssets(username, token, githubOwner, githubRepository, majorMinorPatch, packagePath);
    }

    GitReleaseManagerClose(username, token, githubOwner, githubRepository, majorMinorPatch);
});

Task("Documentation")
    .WithCriteria(() => local)
    .Does (() =>
{
    Information("Documentation");
    using(var process = StartAndReturnProcess("doxygen", new ProcessSettings{ Arguments = "Doxygen/realmthread.config" }))
    {
        process.WaitForExit();
        Information("Exit code: {0}", process.GetExitCode());
    }
});

Task("UnitTestiOSApp")
    .WithCriteria(() => isRunningOnMacOS)
    .IsDependentOn("RestorePackages")
    .Does (() =>
{
    buildThisApp(
        "./src/RealmJson.Test.iOS/RealmJson.Test.iOS.csproj",
        "UnitTesting",
        "Rebuild"    
    );
    unitTestApp(
        "com.sushihangover.realmjson-test-ios",
        "./artifacts/RealmJson.Test.iOS.app"
    );
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("CreateRelease")
    .IsDependentOn("PublishPackages")
    .IsDependentOn("PublishRelease")
    .Does (() =>
{
});

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
