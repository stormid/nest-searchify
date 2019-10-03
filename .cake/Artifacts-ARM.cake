Task("Artifacts:Copy:ARM")
    .WithCriteria<Configuration>((ctx, config) => ctx.DirectoryExists("./src/arm") && ctx.GetSubDirectories("./src/arm").Any())
    .IsDependentOn("Build")
    .IsDependeeOf("Publish")
    .Does<Configuration>(config => 
{
    var artifacts = $"{config.Artifacts.Root}/arm";
    EnsureDirectoryExists(artifacts);
    foreach(var directory in GetSubDirectories("./src/arm")) 
    {
        if(DirectoryExists(directory)) {
            var copyFrom = directory;
            var copyTo = $"{artifacts}/{directory.GetDirectoryName()}";
            Information("{0} -> {1}", copyFrom, copyTo);
            EnsureDirectoryExists(copyTo);
            CopyDirectory(directory, copyTo);
            config.Artifacts.Add(ArtifactTypeOption.Other, directory.GetDirectoryName(), directory.FullPath);
        }
    }
});