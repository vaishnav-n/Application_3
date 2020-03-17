#addin "nuget:?package=Cake.ArgumentHelpers"
#tool "nuget:?package=OctopusTools&Version=6.7.0"
#addin "Cake.Npm"&version=0.8.0
#addin nuget:?package=Cake.SemVer
#addin nuget:?package=semver&version=2.0.4
#module "nuget:?package=Cake.BuildSystems.Module&version=0.3.2"

using System;
using System.Net.Http;
using Newtonsoft.Json;
using System.IO;

var target= Argument("Argument","Default");
var buildoutputpath= "D:/Output_build/" ;
var octopkgpath= "D:/OctoPackages/";
var packageId = "app_2";
var semVer = CreateSemVer(1,0,0);
var sourcepath= "Application_3.sln";
var octopusApiKey="API-FNLJSUPLFWUEDSKTFIZBHUWPAM";

var octopusServerUrl="http://localhost:83";

Task("Restore")
    .Does(()=>
    {
      NuGetRestore("Application_3.sln");
      DotNetCoreRestore("Application_3.sln");
    });

Task("Build")
    .IsDependentOn("Restore")
    .Does(() => 
    {
        MSBuild(sourcepath, new MSBuildSettings()
              .WithProperty("OutDir", buildoutputpath)
                );

    });

Task("OctoPack")
	.IsDependentOn("Build")
	.Does(()=>
	{    
		var octoPackSettings = new OctopusPackSettings()
		{
			BasePath = buildoutputpath,
			OutFolder = octopkgpath,
			Overwrite = true,
			Version = semVer.ToString()
		};    

    OctoPack(packageId,octoPackSettings);
	});

Task("OctoPush")
	.IsDependentOn("OctoPack")
	.Does(()=>
	{	
       var octoPushSettings = new OctopusPushSettings()
    {        
        ReplaceExisting =true
    };
    
    OctoPush(octopusServerUrl, 
        octopusApiKey, 
        GetFiles("D:/OctoPackages/*.*"),
        octoPushSettings);
	});

Task("OctoDeployRelease")
	.IsDependentOn("OctoPush")
	.Does(()=>
	{
		var createReleaseSettings = new CreateReleaseSettings
		{
			Server = octopusServerUrl,
			ApiKey = octopusApiKey,
			DeploymentProgress = true,
			Channel = "Develop",
			DeployTo = "Test",
			Packages = new Dictionary<string, string>
			{
				{packageId, semVer.ToString()}
			}
       		 };
	
    OctoDeployRelease("app_2",createReleaseSettings);
  
	});

Task("Default")  
    .IsDependentOn("OctoDeployRelease"); 

RunTarget(target);
