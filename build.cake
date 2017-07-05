#tool "nuget:?package=coveralls.net"
#tool "nuget:https://nuget.org/api/v2/?package=ReportGenerator"

#addin "nuget:https://nuget.org/api/v2/?package=Cake.Coveralls"
#addin "nuget:https://nuget.org/api/v2/?package=Cake.Incubator"

var target = Argument("target", "default");
var configuration = Argument("configuration", "Release");
var testFramework = Argument("testFramework", "");
var framework = Argument("framework", "");
var runCoverage = Argument<bool>("runCoverage", true);

var testBinDir = Directory("./test/Stubble.Extensions.JsonNet.Tests/bin");
var buildDir = Directory("./src/Stubble.Extensions.JsonNet/bin") + Directory(configuration);
var testBuildDir = testBinDir + Directory(configuration);

var artifactsDir = Directory("./artifacts/");

Task("Clean")
    .Does(() =>
{
    CleanDirectory(buildDir);
    CleanDirectory(testBuildDir);
    CleanDirectory("./artifacts");
    CleanDirectory("./coverage-results");
    CleanDirectory("./coverage-report");
    CleanDirectory("./test/Stubble.Extensions.JsonNet.Tests/TestResults");
});

Task("Restore")
    .IsDependentOn("Clean")
    .Does(() =>
{
    DotNetCoreRestore("./Stubble.Extensions.JsonNet.sln");
});

Task("Build")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore")
    .Does(() =>
{
    var setting = new DotNetCoreBuildSettings {
        Configuration = configuration,
        ArgumentCustomization = args => args.Append("/property:WarningLevel=0") // Until Warnings are fixed in StyleCop
    };

    if(!string.IsNullOrEmpty(framework))
    {
        setting.Framework = framework;
    }

    var testSetting = new DotNetCoreBuildSettings {
        Configuration = configuration
    };

    if(!string.IsNullOrEmpty(testFramework))
    {
        testSetting.Framework = testFramework;
    }

    DotNetCoreBuild("./src/Stubble.Extensions.JsonNet/", setting);
    DotNetCoreBuild("./test/Stubble.Extensions.JsonNet.Tests/", testSetting);
});

Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
{
    Action<ICakeContext> testAction = tool => {
        var xunitSettings = new XUnit2Settings {
            ShadowCopy = true
        };

        tool.DotNetCoreTest(new DotNetCoreTestSettings {
            Configuration = configuration,
            NoBuild = true,
            Verbose = false,
            Framework = testFramework,
            ArgumentCustomization = args =>
                args.Append("--logger:trx")
        },
        "./test/Stubble.Extensions.JsonNet.Tests/Stubble.Extensions.JsonNet.Tests.csproj",
        xunitSettings);
    };

    if(runCoverage || AppVeyor.IsRunningOnAppVeyor)
    {
        var path = new FilePath("./OpenCover-Experimental/OpenCover.Console.exe").MakeAbsolute(Context.Environment);

        Information(path.ToString());

        CreateDirectory("./coverage-results/");
        OpenCover(
            testAction,
            new FilePath("./coverage-results/results.xml"),
            new OpenCoverSettings {
              Register = "user",
              SkipAutoProps = true,
              OldStyle = true,
              ToolPath = path,
              ReturnTargetCodeOffset = 0
            }.WithFilter("+[Stubble.Extensions.JsonNet]*")
        );
    } else {
        testAction(Context);
    }
});

Task("Pack")
    .IsDependentOn("Test")
    .Does(() =>
{
    var settings = new DotNetCorePackSettings
    {
        OutputDirectory = artifactsDir,
        NoBuild = true,
        Configuration = configuration,
    };

    DotNetCorePack("./src/Stubble.Extensions.JsonNet/Stubble.Extensions.JsonNet.csproj", settings);
});

Task("Coveralls")
    .IsDependentOn("Pack")
    .Does(() =>
{
    if (!AppVeyor.IsRunningOnAppVeyor) return;

    var token = EnvironmentVariable("COVERALLS_REPO_TOKEN");

    CoverallsNet("./coverage-results/results.xml", CoverallsNetReportType.OpenCover, new CoverallsNetSettings()
    {
        RepoToken = token,
        CommitId = EnvironmentVariable("APPVEYOR_REPO_COMMIT"),
        CommitBranch = EnvironmentVariable("APPVEYOR_REPO_BRANCH"),
        CommitAuthor = EnvironmentVariable("APPVEYOR_REPO_COMMIT_AUTHOR"),
        CommitEmail = EnvironmentVariable("APPVEYOR_REPO_COMMIT_AUTHOR_EMAIL"),
        CommitMessage = EnvironmentVariable("APPVEYOR_REPO_COMMIT_MESSAGE")
    });
});

Task("CoverageReport")
    .IsDependentOn("Test")
    .Does(() =>
{
    ReportGenerator("./coverage-results/*.xml", "./coverage-report/");
});

Task("AppVeyor")
    .IsDependentOn("Coveralls");

Task("Travis")
    .IsDependentOn("Test");

Task("Default")
    .IsDependentOn("Pack");

RunTarget(target);