using System.ComponentModel;



// ReSharper disable once UnusedMember.Global // Used by reflection


// ReSharper disable UnusedMember.Global

namespace AgentOrch.ChatApp.Wpf.ToolFunctions;


public sealed class WebSearchPlugin
{


    [Description("Search the web for information about a topic and return summarized results with links.")]
    public string Search([Description("Query string to search")] string query, [Description("Maximum number of results")] int limit = 5)
    {
        // Replace with a real search API (Bing, SerpAPI, etc.). Demo returns stubbed JSON.

        var json = $$"""
                     {
                       "query": "{{query}}",
                       "results": [
                         { "title": "GitHub REST API Issues", "url": "https://docs.github.com/en/rest/issues/issues" },
                         { "title": "Semantic Kernel Repo", "url": "https://github.com/microsoft/semantic-kernel" }
                       ]
                     }
                     """;
        return json;
    }
}