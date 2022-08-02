using System.Reflection;
using System.Runtime.Loader;
using PluginBase;

namespace App;

class Program
{
    static void Main(string[] args)
    {
        try
        {
            if (args.Length == 1 && args[0] == "/d")
            {
                Console.WriteLine("Waiting for any key...");
                Console.ReadLine();
            }

            string[] pluginPaths = new string[]
            {
                @"./PluginNoDep/bin/Debug/net6.0/PluginNoDep.dll",
                @"./PluginWithDep/bin/Debug/net6.0/PluginWithDep.dll",
            };

            IEnumerable<ICommand> commands = pluginPaths.SelectMany(pluginPath =>
            {
                Assembly pluginAssembly = LoadPlugin(pluginPath);
                return CreateCommands(pluginAssembly);
            }).ToList();

            if (args.Length == 0)
            {
                Console.WriteLine("Commands: ");
                foreach (var command in commands)
                {
                    System.Console.WriteLine($"{command.Name}\t - {command.Description}");
                }
            }
            else
            {
                foreach (string commandName in args)
                {
                    Console.WriteLine($"-- {commandName} --");

                    ICommand? command = commands.FirstOrDefault(c => c.Name == commandName);
                    if (command is null)
                    {
                        System.Console.WriteLine("No such command");
                        return;
                    }

                    command.Execute();


                    Console.WriteLine();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    static Assembly LoadPlugin(string path)
    {
        string root = @"/home/ad/dev/cs-plugins";
        string pluginLocation = Path.GetFullPath(Path.Combine(root, path.Replace('\\', Path.DirectorySeparatorChar)));
        System.Console.WriteLine($"Loading commands from: {pluginLocation}");
        PluginLoadContext context = new PluginLoadContext(pluginLocation);
        return context.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(pluginLocation)));
    }

    static IEnumerable<ICommand> CreateCommands(Assembly assembly)
    {
        int count = 0;

        foreach (Type type in assembly.GetTypes())
        {
            if (typeof(ICommand).IsAssignableFrom(type))
            {
                ICommand? res = Activator.CreateInstance(type) as ICommand;
                if (res is not null)
                {
                    count++;
                    yield return res;
                }
            }
        }

        if (count == 0)
        {
            string availableTypes = string.Join(",", assembly.GetTypes().Select(t => t.FullName));
            throw new ApplicationException(
                $"Can't find any type which implements ICommand in {assembly} from {assembly.Location}\n" +
                $"Avaiable types: {availableTypes}"
            );
        }
    }
}

public class PluginLoadContext : AssemblyLoadContext
{
    private AssemblyDependencyResolver _resolver;

    public PluginLoadContext(string path)
    {
        _resolver = new AssemblyDependencyResolver(path);
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        string? assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
        if (assemblyPath is not null)
        {
            return LoadFromAssemblyPath(assemblyPath);
        }

        return null;
    }

    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        string? libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
        if (libraryPath is not null)
        {
            return LoadUnmanagedDllFromPath(libraryPath);
        }

        return IntPtr.Zero;
    }
}