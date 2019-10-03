#tool "nuget:?package=NUnit.Runners&version=2.6.4"
#load "Configuration.cake"

Task("Test:NUnit")    
    .IsDependentOn("Build")
    .IsDependeeOf("Test")
    .WithCriteria<Configuration>((ctx, config) => config.Solution.TestProjects.Any(p => p.IsNUnitTestProject()))
    .Does<Configuration>(config => 
{
    CreateDirectory($"{config.Artifacts.Root}/test-results");

    var shouldFail = false;
    foreach(var testProject in config.Solution.TestProjects.Where(p => p.IsNUnitTestProject())) {
        var assemblyName = config.Solution.GetProjectName(testProject);
        var testResultsRoot = $"{config.Artifacts.Root}/test-results";
        var testResultsXml = $"{testResultsRoot}/{assemblyName}.xml";
        var testAssembly = $"{testProject.OutputPaths.FirstOrDefault().ToString()}/{assemblyName}.dll";
        
        try 
        {
            var settings = new NUnitSettings() {
                NoLogo = true,
                ResultsFile = testResultsXml,
                OutputFile = $"{testResultsRoot}/{assemblyName}.log",
            };

            NUnit(testAssembly, settings);
        } 
        catch
        {
            shouldFail = true;
        }
    }

    Information("Publishing Test results from {0}", config.Artifacts.Root);
    var testResults = GetFiles($"{config.Artifacts.Root}/test-results/**/*.xml").ToArray();
    if(testResults.Any()) 
    {
        if(BuildSystem.IsRunningOnAzurePipelinesHosted || TFBuild.IsRunningOnAzurePipelines) 
        {
            TFBuild.Commands.PublishTestResults(new TFBuildPublishTestResultsData() {
                Configuration = config.Solution.BuildConfiguration,
                MergeTestResults = true,
                TestResultsFiles = testResults,
                TestRunner = TFTestRunnerType.VSTest
            });    
        }
    }

    if(shouldFail)
    {
        throw new Exception("Tests have failed");
    }
});
