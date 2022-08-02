using PluginBase;

namespace PluginNoDep;
public class HelloCommand : ICommand
{
    public string Name => "hello";
    public string Description => "Displays hello message";

    public int Execute() { System.Console.WriteLine("Hello from PluginNoDep!"); return 0; }
}
