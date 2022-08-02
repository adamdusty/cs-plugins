using System.Reflection;
using Newtonsoft.Json;
using PluginBase;

namespace PluginWithDep;
public class JsonPlugin : ICommand
{
    public string Name => "json";
    public string Description => "Outputs JSON value.";

    private struct Info
    {
        public string JsonVersion;
        public string User;
    }

    public int Execute()
    {
        Assembly jsonAssembly = typeof(JsonConvert).Assembly;

        Info info = new Info()
        {
            JsonVersion = jsonAssembly.FullName ?? string.Empty,
            User = Environment.UserName
        };

        System.Console.WriteLine(JsonConvert.SerializeObject(info, Formatting.Indented));
        return 0;
    }
}
