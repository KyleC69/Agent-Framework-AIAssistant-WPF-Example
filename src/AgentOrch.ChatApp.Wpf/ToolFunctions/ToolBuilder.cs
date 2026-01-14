using Microsoft.Extensions.AI;



namespace AgentOrch.ChatApp.Wpf.ToolFunctions;


internal class ToolBuilder
{


    public IList<AITool> GetAiTools()
    {
        FileSystemPlugin fileSystemPlugin = new();
        TimePlugin timep = new();
        AppInfoPlugin appinfo = new();
        WebSearchPlugin webSearchPlugin = new();
        MathPlugin math = new();
        CodeWriterPlugin code = new();
        OutputMarkDownPlugin nd = new();

        IList<AITool> tools =
        [
            AIFunctionFactory.Create(fileSystemPlugin.WriteText),
            AIFunctionFactory.Create(timep.GetLocalTime),
            AIFunctionFactory.Create(appinfo.GetAppInfo),
            AIFunctionFactory.Create(webSearchPlugin.Search),
            AIFunctionFactory.Create(math.Add),
            AIFunctionFactory.Create(math.Multiply),
            AIFunctionFactory.Create(code.Generate),
            AIFunctionFactory.Create(nd.FormatAsMarkdownAsync)
        ];


        return tools;
    }
}