using System.ComponentModel;



// ReSharper disable UnusedMember.Global // Invoked via reflection by the agent framework


namespace AgentOrch.ChatApp.Wpf.ToolFunctions;


public class FileSystemPlugin
{

    [Description("Write a UTF-8 text file to disk.")]
    public string WriteText(
        [Description("File path")] string path,
        [Description("Text content to write")] string content)
    {
        System.IO.File.WriteAllText(path, content);
        return $"Wrote File {path}";
    }
}