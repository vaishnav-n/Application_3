#r "C:\Users\vaishnavn\source\repos\CakePractice\Cake\Sample\bin\Debug\Sample.dll"
#addin "nuget:?package=Cake.ArgumentHelpers"
#tool "nuget:?package=GitVersion.CommandLine&Version=4.0.0"
#module "nuget:?package=Cake.BuildSystems.Module&version=0.3.2"

var target = Argument("target", "Build");
var BuildNumber = ArgumentOrEnvironmentVariable("build.number", "", "0.0.1-local.0");
var MsBuildLogger = ArgumentOrEnvironmentVariable("MsBuildLogger", "", "");
var DeploymentBranches = ArgumentOrEnvironmentVariable("DeploymentBranches", "", " ");
var TeamCityBuildAgentDirectory = ArgumentOrEnvironmentVariable("teamcity.agent.home.dir", "", "c:\\BuildAgent");

string BranchName = null;
string tenant = null;
string solutionfilepath="Application_3.sln";
string buildoutputpath="D:/Build_output/";

Task("Build")
   .IsDependentOn("Version")
   .IsDependentOn("Restore")
   .Does(() =>
   {
   Information("Building solution");
    TaskBuild(solutionfilepath,buildoutputpath);
   });
   
Task("Restore")
    .Does(() =>
	{
    Information("Restoring Packages");
		NuGetRestore(solutionfilepath);
	});

Task("Version")
    .IsDependentOn("Restore")
	.Does(()=>
	{
    Information("Staring Gitversion");

    var vermsg= Version();

    Information(vermsg);
	});

Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
    {
     var testAdapterPath = GetFiles("./**/vstest15/TeamCity.VSTest.TestAdapter.dll").First();

	Information("Test Adapter Path " + testAdapterPath);

	if (TeamCity.IsRunningOnTeamCity) 
	{
		settings.Logger = "teamcity";
		settings.TestAdapterPath = testAdapterPath.GetDirectory();
	}

    DotNetCoreTest(
        "./HHAExchange.VisitAuthorization.sln",
        settings);
  
    }

Task("Publish")
    .IsDepedentOn("Test")
    .Does(()=>
    {
      String JsonPath="./Lstpaths.json";

      PublishMultipleTasks(JsonPath)
    }

Task("OctoPush")
  .IsDependentOn("Pack")
  .IsDependentOn("Publish")
  .IsDependentOn("Version")
  .Does(() => 
{

	if (BuildNumber.Contains("-develop") || BuildNumber.Contains("-release") || IsFeatureBranchWithTenant())
	{
		Information("Push packages to Octopus");

		OctoPush(OctopusDeployUrl, OctopusDeployApiKey, GetFiles("./publishpackage/*.*"),
		  new OctopusPushSettings 
		  {
			ReplaceExisting = true
		  });
	}
});

RunTarget(target);