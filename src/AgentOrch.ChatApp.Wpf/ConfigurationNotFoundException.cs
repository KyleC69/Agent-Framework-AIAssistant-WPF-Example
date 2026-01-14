namespace AgentOrch.ChatApp.Wpf;


[Serializable]
internal class ConfigurationNotFoundException : Exception
{
    public ConfigurationNotFoundException(string section)
    {
    }








    public ConfigurationNotFoundException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}