#load "Configuration.cake"
#addin nuget:?package=Newtonsoft.Json&version=12.0.1
using Newtonsoft.Json;

public class EfMigration 
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string SafeName { get; set; }
}

public class EfContext
{
    public string FullName { get; set; }
    public string SafeName { get; set; }
    public string Name { get; set; }
    public string AssemblyQualifiedName { get; set; }
}

IEnumerable<EfContext> GetAllDbContexts(DirectoryPath workingDirectory, string configuration) 
{
    var settings = new ProcessSettings() 
    { 
        WorkingDirectory = workingDirectory,
        RedirectStandardOutput = true
    };

    settings.Arguments = string.Format("ef dbcontext list --configuration {0} --json --prefix-output", configuration);
    var list = Enumerable.Empty<EfContext>();
    
    using(var process = StartAndReturnProcess("dotnet", settings))
    {
        process.WaitForExit();
        if(process.GetExitCode() == 0)
        {
            try 
            {
                var outputAsJson = string.Join(Environment.NewLine, process.GetStandardOutput().Where(l => l.StartsWith("data:")).Select(l => l.Replace("data:", "")));
                list = JsonConvert.DeserializeObject<List<EfContext>>(outputAsJson);
                Verbose("Found {0} Db contexts", list.Count());
            }
            catch(Exception exception)             
            {
                Error("Unable to determine db context's for {0} : {1}", workingDirectory, exception.Message);
            }
        }
    }
    return list.ToList();
}

IEnumerable<EfMigration> GetMigrationsForContext(string dbContext, DirectoryPath workingDirectory, string configuration) 
{
    var settings = new ProcessSettings() 
    { 
        WorkingDirectory = workingDirectory,
        RedirectStandardOutput = true
    };

    settings.Arguments = string.Format("ef migrations list --configuration {0} --context {1} --json --prefix-output", configuration, dbContext);

    var list = Enumerable.Empty<EfMigration>();
    using(var process = StartAndReturnProcess("dotnet", settings))
    {
        process.WaitForExit();
        if(process.GetExitCode() == 0)
        {
            try
            {
                var outputAsJson = string.Join(Environment.NewLine, process.GetStandardOutput().Where(l => l.StartsWith("data:")).Select(l => l.Replace("data:", "")));
                list = JsonConvert.DeserializeObject<List<EfMigration>>(outputAsJson);
            }
            catch(Exception exception)             
            {
                Error("Unable to determine db migration list for {0} : {1}", dbContext, exception.Message);
            }            
        }
    }
    return list;
}

public static Configuration IncludeAsEfDbContext(this Configuration configuration, Func<CustomProjectParserResult, bool> includeAsEFDbContext)
{
    var projects = new HashSet<CustomProjectParserResult>();
    if(configuration.TaskParameters.TryGetValue("Artifacts:DotNetCore:Ef:Migration-Script", out object value) && value is HashSet<CustomProjectParserResult>)
    {
        projects = value as HashSet<CustomProjectParserResult>;
    }
    else
    {
        configuration.TaskParameters.Add("Artifacts:DotNetCore:Ef:Migration-Script", projects);
    }

    var projectsToInclude = configuration.Solution.Projects.Where(includeAsEFDbContext).ToList();

    if(projectsToInclude != null && projectsToInclude.Any())
    {
        projects.UnionWith(projectsToInclude);
    }

    return configuration;
}

public static bool HasCustomEfDbContextTargets(this Configuration configuration)
{
    var projectList = configuration.GetEfDbContextTargets();
    return projectList?.Any() ?? false;
}

public static IEnumerable<CustomProjectParserResult> GetEfDbContextTargets(this Configuration configuration)
{
    var projects = new HashSet<CustomProjectParserResult>();
    if(configuration.TaskParameters.TryGetValue("Artifacts:DotNetCore:Ef:Migration-Script", out object value) && value is HashSet<CustomProjectParserResult>)
    {
        projects = value as HashSet<CustomProjectParserResult>;
    }    
    return projects;
}

Task("Artifacts:DotNetCore:Ef:Migration-Script")
    .IsDependentOn("Build")
    .IsDependeeOf("Publish")
    .Does<Configuration>(config => 
{
    var efProjects = (config.HasCustomEfDbContextTargets() ? config.GetEfDbContextTargets() : config.Solution.Projects).ToList();

    Information("Generating scripts for {0} projects", efProjects.Count());
    foreach(var project in efProjects) {
        var assemblyName = config.Solution.GetProjectName(project);
        var workingDirectory = project.ProjectFilePath.GetDirectory();
        var availableDbContexts = GetAllDbContexts(workingDirectory, config.Solution.BuildConfiguration).ToList();

        if(availableDbContexts.Any())
        {
            Information("Generating scripts for {0} containing {1} contexts", assemblyName, availableDbContexts.Count);
            foreach(var dbContext in availableDbContexts) 
            {
                Information("Generating Sql Script for {0}", dbContext.SafeName);
                var migrations = GetMigrationsForContext(dbContext.SafeName, workingDirectory, config.Solution.BuildConfiguration);
                
                var sqlScript = MakeAbsolute(File($"{config.Artifacts.Root}/sql/{dbContext.SafeName}.sql"));
                if(FileExists(sqlScript)) {
                    DeleteFile(sqlScript);
                }

                var settings = new ProcessSettings() 
                { 
                    WorkingDirectory = workingDirectory
                };

                settings.Arguments = string.Format("ef migrations script -i -o {0} --configuration {1} --context {2}", sqlScript, config.Solution.BuildConfiguration, dbContext.SafeName);

                using(var process = StartAndReturnProcess("dotnet", settings))
                {
                    process.WaitForExit();
                    Verbose("Exit code: {0}", process.GetExitCode());
                }

                config.Artifacts.Add(ArtifactTypeOption.Other, sqlScript.GetFilename().ToString(), sqlScript);
            }
        }
    }

});
