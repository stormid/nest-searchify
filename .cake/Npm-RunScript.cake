#addin "nuget:?package=Cake.Npm&version=0.15.0"

public partial class Configuration 
{
    public Configuration RunNpmScript(string scriptName, string workingDirectory = "./")
    {
        Npm = new NpmConfiguration(context, scriptName, workingDirectory);
        return this;
    }

    public NpmConfiguration Npm { get; private set; }
}

public class NpmConfiguration
{
    public string WorkingDirectory { get; } = "./";
    public string ScriptName { get; } = "ci";

    private readonly ICakeContext cakeContext;

    public NpmConfiguration(ICakeContext cakeContext, string scriptName, string workingDirectory)
    {
        this.cakeContext = cakeContext;
        ScriptName = scriptName;
        WorkingDirectory = workingDirectory ?? "./";
    }

    public bool CanExecuteNpm {
        get {
            return cakeContext.FileExists($"{WorkingDirectory.TrimEnd('/')}/package.json");
        }
    }
}

Task("Npm").IsDependeeOf("Restore").IsDependentOn("Npm:Install").IsDependentOn("Npm:Build");

Task("Npm:Install")
    .WithCriteria<Configuration>((ctx, config) => config.Npm.CanExecuteNpm)
    .Does<Configuration>(config => 
{
    var settings = new NpmInstallSettings();
    settings.WorkingDirectory = config.Npm.WorkingDirectory;
    settings.LogLevel = NpmLogLevel.Silent;
    settings.RedirectStandardError = false;
    settings.RedirectStandardOutput = false;
    NpmInstall(settings);
});

Task("Npm:Build")
    .IsDependentOn("Npm:Install")
    .WithCriteria<Configuration>((ctx, config) => config.Npm.CanExecuteNpm)
    .Does<Configuration>(config => 
{
    var settings = new NpmRunScriptSettings();
    settings.WorkingDirectory = config.Npm.WorkingDirectory;
    settings.LogLevel = NpmLogLevel.Silent;
    settings.RedirectStandardError = false;
    settings.RedirectStandardOutput = false;
    settings.ScriptName = config.Npm.ScriptName;
    NpmRunScript(settings);
});