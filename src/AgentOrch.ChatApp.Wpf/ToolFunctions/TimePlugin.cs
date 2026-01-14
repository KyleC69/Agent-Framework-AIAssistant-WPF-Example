using System.ComponentModel;



// ReSharper disable UnusedMember.Global // Methods aren't instantiated by user code but are necessary for reflection

namespace AgentOrch.ChatApp.Wpf.ToolFunctions;


public sealed class TimePlugin
{

    [Description("Get the current local time for the user.")]
    public string GetLocalTime()
    {
        return DateTimeOffset.Now.ToString("O");
    }
}