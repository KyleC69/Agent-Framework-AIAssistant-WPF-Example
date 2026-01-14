using System.ComponentModel;



// ReSharper disable once UnusedMember.Global // Methods aren't instantiated by user code but are necessary for reflection


namespace AgentOrch.ChatApp.Wpf.ToolFunctions;


public sealed class AppInfoPlugin
{

    [Description("Get basic app runtime information useful for debugging.")]
    public string GetAppInfo()
    {
        return $"OS={Environment.OSVersion}; Process={Environment.ProcessPath}; .NET={Environment.Version}";
    }
}