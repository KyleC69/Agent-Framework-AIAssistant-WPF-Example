using System;
using System.ComponentModel;



// ReSharper disable UnusedMember.Global // Types aren't instantiated by user code but are necessary for reflection


public class SpecCheckPlugin
{
    [Description("Check if output meets spec; return verdict and suggested fixes.")]
    public string Check([Description("Spec or acceptance criteria text")] string spec, [Description("Produced output to validate")] string output)
    {
        // Demo: naive checks and verdict
        string verdict = output.Contains("https://api.github.com", StringComparison.OrdinalIgnoreCase) ? "PASS" : "WARN";
        string json = $$"""
                     {
                       "verdict": "{{verdict}}",
                       "notes": "Ensure filtering by last 24h and robust error handling.",
                       "signal": "{{(verdict == "PASS" ? "READY TO SHIP" : "NEEDS FIXES")}}"
                     }
                     """;
        return json;
    }
}