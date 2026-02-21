using System.Collections.Generic;

using AgentOrchestration.Wpf.Controls;

using Microsoft.Extensions.AI;




namespace AgentOrchestration.Wpf.ToolFunctions;





//Class contains AI tool functions to simulate an AI agent performing tasks
//Functions simply trigger a visual in UI to demonstrate local model functionality
internal class AiFunctionTests
{

    public IList<AITool> GetSampleTools()
    {
        return
        [
                AIFunctionFactory.Create(
                        (string location, string unit) =>
                        {
                            AIToolIndicatorHub.Pulse(0);
                            return $"Sunny, 25 {unit}";
                        },
                        "get_current_weather",
                        "Gets the current weather in a given location"),

                AIFunctionFactory.Create(
                        (string topic) =>
                        {
                            AIToolIndicatorHub.Pulse(1);
                            return "Latest news on " + topic;
                        },
                        "get_latest_news",
                        "Gets the latest news on a given topic"),

                AIFunctionFactory.Create(
                        () =>
                        {
                            AIToolIndicatorHub.Pulse(2);
                            return SwitchLight();
                        },
                        "switch_light",
                        "Switches the light on or off"),

                AIFunctionFactory.Create(
                        (string text) =>
                        {
                            AIToolIndicatorHub.Pulse(3);
                            return text.ToUpperInvariant();
                        },
                        "uppercase",
                        "Converts text to uppercase"),

                AIFunctionFactory.Create(
                        (int a, int b) =>
                        {
                            AIToolIndicatorHub.Pulse(4);
                            return (a + b).ToString();
                        },
                        "add_numbers",
                        "Adds two numbers and returns the sum")
        ];
    }








    private string SwitchLight()
    {
        //Flips a visual indicator in the UI to simulate switching a light on/off
        return "Light switched";
    }
}





public class Lights
{
    public bool IsOn { get; set; }
}