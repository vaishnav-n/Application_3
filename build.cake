
#tool "nuget:http://localhost:82/httpAuth/app/nuget/feed/CakeTemplate/Cake_TemplateFeed/v2/?package=cake.template.1.0.0.nupkg""
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
string buildoutputpath="D:\";

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
		Version();
	});



RunTarget(target);