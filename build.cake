#tool "nuget:?package=ReportGenerator"

#tool nuget:?package=Codecov
#addin nuget:?package=Cake.Codecov
#addin nuget:?package=Cake.Coverlet

var target = Argument("target", "default");
var configuration = Argument("configuration", "Release");
var testFramework = Argument("testFramework", "");
var framework = Argument("framework", "");
var runCoverage = Argument<bool>("runCoverage", true);

var testBinDir = Directory("./test/Stubble.Extensions.JsonNet.Tests/bin");
var buildDir = Directory("./src/Stubble.Extensions.JsonNet/bin") + Directory(configuration);
var testBuildDir = testBinDir + Directory(configuration);
var coverageDirectory = Directory("./coverage-results");

var artifactsDir = Directory("./artifacts/");

Task("Clean")
    .Does(() =>
{
    CleanDirectory(buildDir);
    CleanDirectory(testBuildDir);
    CleanDirectory("./artifacts");
    CleanDirectory(coverageDirectory);
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
    var testSettings = new DotNetCoreTestSettings {
        Configuration = configuration,
        NoBuild = true,
        Verbosity = DotNetCoreVerbosity.Quiet,
        Framework = testFramework,
        ArgumentCustomization = args =>
            args.Append("--logger:trx")
    };

    var coverletSettings = new CoverletSettings {
        CollectCoverage = runCoverage,
        CoverletOutputFormat = CoverletOutputFormat.opencover | CoverletOutputFormat.cobertura,
        CoverletOutputDirectory = coverageDirectory,
        CoverletOutputName = $"results-{DateTime.UtcNow:dd-MM-yyyy-HH-mm-ss-FFF}"
    };

    DotNetCoreTest("./test/Stubble.Extensions.JsonNet.Tests/Stubble.Extensions.JsonNet.Tests.csproj", testSettings, coverletSettings);

    if (AppVeyor.IsRunningOnAppVeyor)
    {
        foreach(var file in GetFiles("./test/Stubble.Extensions.JsonNet.Tests/TestResults/*"))
        {
            AppVeyor.UploadTestResults(file, AppVeyorTestResultsType.MSTest);
            AppVeyor.UploadArtifact(file);
        }
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

Task("CodeCov")
    .IsDependentOn("Pack")
    .WithCriteria(BuildSystem.IsRunningOnAzurePipelinesHosted && IsRunningOnWindows())
    .Does(() =>
{
    var coverageFiles = GetFiles("./coverage-results/*.opencover.xml")
        .Select(f => f.FullPath)
        .ToArray();

    var settings = new CodecovSettings();
    var token = EnvironmentVariable("CODECOV_REPO_TOKEN");
    settings.Token = token;

    foreach(var file in coverageFiles)
    {
        settings.Files = new [] { file };

        // Upload coverage reports.
        Codecov(settings);
    }
});

Task("CoverageReport")
    .IsDependentOn("Test")
    .Does(() =>
{
    ReportGenerator("./coverage-results/*.xml", "./coverage-report/");
});

Task("AppVeyor")
    .IsDependentOn("CodeCov");

Task("Travis")
    .IsDependentOn("Test");

Task("Default")
    .IsDependentOn("CodeCov");

RunTarget(target);