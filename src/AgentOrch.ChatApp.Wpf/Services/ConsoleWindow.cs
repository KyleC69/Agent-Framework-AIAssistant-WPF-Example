using System.Runtime.InteropServices;



namespace AgentOrch.ChatApp.Wpf.Services;


internal static class ConsoleWindow
{
    private const int AttachParentProcess = -1;








    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool AllocConsole();








    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool AttachConsole(int dwProcessId);








    public static void Ensure()
    {
        // If launched from a console, attach to it; otherwise allocate a new one.
        if (!AttachConsole(AttachParentProcess)) _ = AllocConsole();

        try
        {
            Console.Title = "AgentOrch Chat (Debug Console)";
        }
        catch
        {
            // Ignore environments that don't allow setting title.
        }
    }
}