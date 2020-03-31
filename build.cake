#tool "nuget:?package=OctopusTools&Version=6.7.0"
#addin "nuget:?package=Cake.ArgumentHelpers"
#addin "Cake.Npm"&version=0.8.0
#addin "Cake.FileHelpers"&version=3.2.1
#addin nuget:?package=Cake.SemVer
#addin nuget:?package=semver&version=2.0.4
#module "nuget:?package=Cake.BuildSystems.Module&version=0.3.2"

using System;
using System.Net.Http;
using Newtonsoft.Json;
using System.IO;


string FilePath = @"FilePath.txt";
var target= Argument("Argument","Default");
var BuildNumber = ArgumentOrEnvironmentVariable("build.number", "", "0.0.1-local.0");
var buildoutputpath= "D:/Output_build/" ;
var octopkgpath= "D:/OctoPackages/";
var sourcepath="Application_3.sln";
var octopusApiKey=ArgumentOrEnvironmentVariable("OctopusDeployApiKey","");
string BranchName = null;
string publishDir = "D:/Publish_Package/";

var octopusServerUrl="http://localhost:83";

Task("Restore")
    .Does(() =>
	     {
			NuGetRestore("Application_3.sln");
	     });

Task("Build")
	.IsDependentOn("Restore")
	.IsDependentOn("Version")
    .Does(() => 
    {
        MSBuild(sourcepath, new MSBuildSettings()
              .WithProperty("OutDir", buildoutputpath)
                );

    });


	
Task("Publish")
    .IsDependentOn("Build")
	.Does(()=>
	{
		
                string[] data = File.ReadLines(FilePath);

				foreach (var File in data)
					{
						if (File.Contains(".csproj"))
						{
							var publishSettings = new DotNetCorePublishSettings
							{
							Configuration = "Release",
							OutputDirectory = publishDir,
							Runtime = "win-x64"
							};

							DotNetCorePublish(File, publishSettings);
						}
						
					}
			
  });
	

Task("OctoPush")
	.IsDependentOn("Publish")
	.Does(()=>
	{	
       var octoPushSettings = new OctopusPushSettings()
    {        
        ReplaceExisting =true
    };
    
    OctoPush(octopusServerUrl, 
        octopusApiKey, 
        GetFiles("D:/Publish_Package/*.*"), 
        octoPushSettings);
	});

Task("Version")
  .Does(() =>
{
	GitVersionSettings buildServerSettings = new GitVersionSettings {
		OutputType = GitVersionOutput.BuildServer,
        UpdateAssemblyInfo = true
    };

	GitVersion(buildServerSettings);

	SetGitVersionPath(buildServerSettings);

	GitVersionSettings localSettings = new GitVersionSettings();

	var versionResult = GitVersion(localSettings);

	SetGitVersionPath(localSettings);

	BuildNumber = versionResult.SemVer;
	BranchName = versionResult.BranchName;
});

public void SetGitVersionPath(GitVersionSettings settings)
{
	if (TeamCity.IsRunningOnTeamCity)
	{
		Information("Using shared GitVersion");

		settings.ToolPath = "C:\\Users\\vaishnavn\\.nuget\\packages\\gitversion.commandline\\4.0.0\\tools\\gitversion.exe";
	}
}


Task("Default")  
    .IsDependentOn("OctoPush"); 

RunTarget(target);
