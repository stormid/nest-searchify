#load "Configuration.cake"

Task("Publish:MsBuild")
    .IsDependentOn("Build")
    .IsDependeeOf("Publish")
    .Does<Configuration>(config => 
{
    foreach(var webProject in config.Solution.WebProjects) {
        var assemblyName = config.Solution.GetProjectName(webProject);
        var projectArtifactDirectory = $"{config.Artifacts.GetRootFor(ArtifactTypeOption.WebDeploy)}/{assemblyName}";
        var artifactZipName = $"{assemblyName}.zip";
        var artifactZipFullPath = $"{projectArtifactDirectory}/{artifactZipName}";

        MSBuild(webProject.ProjectFilePath, c => c
            .SetConfiguration(config.Solution.BuildConfiguration)
            .SetVerbosity(Verbosity.Quiet)
            .UseToolVersion(MSBuildToolVersion.VS2017)
            .WithWarningsAsError()
            .WithTarget("Package")
            .WithProperty("PackageAsSingleFile", "true")
            .WithProperty("SkipInvalidConfigurations", "false")
            .WithProperty("AutoParameterizationWebConfigConnectionStrings", "false")
            .WithProperty("PackageLocation", artifactZipFullPath)
        );
        config.Artifacts.Add(ArtifactTypeOption.WebDeploy, assemblyName, $"{projectArtifactDirectory}");
    }
});