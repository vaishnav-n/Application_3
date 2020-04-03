
#addin "nuget:?package=cake.template"
#addin "nuget:?package=Cake.ArgumentHelprs"
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
    TaskBuild(solutionfilepath,buildoutputpath);
   });
   
Task("Restore")
    .Does(() =>
	{
		NuGetRestore(solutionfilepath);
	});

Task("Version")
    .IsDependentOn("Restore")
	.Does(()=>
	{
		Version();
	}



RunTarget(target);