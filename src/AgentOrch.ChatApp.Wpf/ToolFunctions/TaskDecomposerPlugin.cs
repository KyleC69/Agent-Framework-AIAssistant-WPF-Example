using System.ComponentModel;
using System.Threading.Tasks;



// ReSharper disable UnusedMember.Global - Marked as tool functions does not get called directly.


public sealed class TaskDecomposerPlugin
{

    [Description("Break a user goal/spec into ordered, assignable tasks with agent mentions and rationale.")]
    public Task<string> DecomposeGoalAsync(
            [Description("The goal or specification to decompose.")]
            string goal)
    {
        // Normally you’d do structured decomposition + validation. For demo, return JSON.
        const string json = """
                            {
                              "tasks": [
                                { "assignTo": "@researcher", "title": "Confirm GitHub API endpoints", "rationale": "Need correct REST URLs and auth." },
                                { "assignTo": "@coder", "title": "Implement console app to fetch issues", "rationale": "Core deliverable." },
                                { "assignTo": "@reviewer", "title": "Validate output and error handling", "rationale": "Quality gate." }
                              ]
                            }
                            """;
        return Task.FromResult(json);
    }
}