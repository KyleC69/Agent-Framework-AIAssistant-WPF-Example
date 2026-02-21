using System;
using System.ComponentModel;
using System.Text.Json;
using System.Threading.Tasks;

using PuppeteerSharp;





// ReSharper disable once UnusedMember.Global // Used by reflection


// ReSharper disable UnusedMember.Global

namespace AgentOrchestration.Wpf.ToolFunctions;





public class WebSearchPlugin
{





    [Description("Search the web for information about a topic and return summarized results with links.")]
    public string? WebSearch(string query)
    {
        // Replace with a real search API (Bing, SerpAPI, etc.). Demo returns stubbed JSON.

        IBrowser browser = LoadBrowserAsync().Result;

        IPage page = browser.NewPageAsync().Result;

        page.DefaultNavigationTimeout = 60000; // Set navigation timeout to 60 seconds

        // Need to navigate to a search engine and perform a search with the results being returned as json 
        //so that the agent can parse the results and use it to answer the question. 
        page.GoToAsync($"https://www.bing.com/search?q={Uri.EscapeDataString(query)}&format=rss").Wait();

        page.WaitForNavigationAsync().Wait();
        string result = page.GetContentAsync().Result ?? "Page did not return contents";

        JsonDocument? doc = JsonDocument.Parse(result);

        return doc is not null ? doc.RootElement.ToString() : "No results found.";
    }








    private async Task<IBrowser> LoadBrowserAsync()
    {


        LaunchOptions options = new()
        {
            Channel = null,
            Headless = true,
            HeadlessMode = HeadlessMode.True,
            SlowMo = 0,
            Args = new[]
                {
                        "--no-sandbox",
                        "--disable-gpu", "--disable-dev-shm-usage"
                },
            Timeout = 60000,
            DumpIO = false,
            IgnoreDefaultArgs = false,
            Browser = SupportedBrowser.Chrome,
            ProtocolTimeout = 30_000,
            WaitForInitialPage = true,
            ExecutablePath = "E:\\chrome-win64\\chrome.exe"
        };


        //safely startup the brower, and handle any exceptions that may occur during startup
        try
        {
            Launcher launcher = new();
            return await launcher.LaunchAsync(options);
        }
        catch (Exception ex)
        {
            // Log the exception and re-throw or handle it appropriately
            // For this example, we'll just re-throw
            throw new InvalidOperationException("Failed to launch browser.", ex);
        }









    }
}