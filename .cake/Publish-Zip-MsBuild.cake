#load "Configuration.cake"

Task("Publish:Zip:MsBuild")
    .Description("Creates a zip archive from each found project")
    .IsDependeeOf("Publish")
    .WithCriteria<Configuration>((ctx, config) => config.Solution.LibraryProjects.Any())
    .Does<Configuration>(config => 
{
    foreach(var project in config.Solution.LibraryProjects) {
        var assemblyName = config.Solution.GetProjectName(project);
        var projectArtifactDirectory = config.Artifacts.GetRootFor(ArtifactTypeOption.Zip);
        var publishDirectory = project.OutputPaths.FirstOrDefault();
        var artifactZipName = $"{assemblyName}.zip";
        var artifactZipFullPath = $"{projectArtifactDirectory}/{artifactZipName}";

        if(DirectoryExists(publishDirectory))
        {
            Information($"Publishing {assemblyName} from {publishDirectory} to {artifactZipFullPath}");

            Zip(publishDirectory, artifactZipFullPath);
            config.Artifacts.Add(ArtifactTypeOption.Zip, artifactZipName, artifactZipFullPath);
        }
        else 
        {
            Warning($"Unable to find anything to publish for {assemblyName}");
        }
    }
});
