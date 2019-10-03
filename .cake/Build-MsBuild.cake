#load "Configuration.cake"

Task("Build:MsBuild")
    .IsDependentOn("Restore")
    .IsDependeeOf("Build")
    .Does<Configuration>(config =>
{
    MSBuild(config.Solution.Path.ToString(), c => c
        .SetConfiguration(config.Solution.BuildConfiguration)
        .SetVerbosity(Verbosity.Minimal)
        .UseToolVersion(MSBuildToolVersion.VS2017)
        .WithWarningsAsError()
        .WithTarget("Build")
    );
});