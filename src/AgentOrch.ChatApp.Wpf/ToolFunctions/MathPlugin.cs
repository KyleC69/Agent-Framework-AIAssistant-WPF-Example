using System.ComponentModel;



// ReSharper disable UnusedMember.Global Types aren't instantiated by user code but are necessary for reflection


namespace AgentOrch.ChatApp.Wpf.ToolFunctions;


public sealed class MathPlugin
{

    [Description("Add two numbers.")]
    public double Add(
        [Description("First number")] double a,
        [Description("Second number")] double b)
    {
        return a + b;
    }








    [Description("Multiply two numbers.")]
    public double Multiply(
        [Description("First number")] double a,
        [Description("Second number")] double b)
    {
        return a * b;
    }
}