using AppWithPlugin;
using PluginSdk;
using System.Reflection;
using System.Runtime.Loader;

namespace App
{
    internal class Program
    {
        static List<AssemblyLoadContext> loadContexts = new();

        static void Main(string[] args)
        {
            Console.WriteLine("Loading, unloading and reloading application plugins...");

            UsePluginsOnce();
            UnloadAlcs();
            UsePluginsOnce();
            UnloadAlcs();
        }

        private static void UnloadAlcs()
        {
            foreach (var item in loadContexts)
            {
                item?.Unload();
            }

            loadContexts.Clear();
        }

        private static void UsePluginsOnce()
        {
            var plugins = LoadPlugins();

            foreach (IPlugin item in plugins)
            {
                Console.WriteLine(item.GetMsg());

                if (item is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }

        static IList<IPlugin> LoadPlugins()
        {
            string[] pluginPaths = new string[]
            {
                @"RxUiPlugin\bin\Debug\net6.0-windows10.0.19041\RxUiPlugin.dll",
            };

            return pluginPaths.SelectMany(pluginPath =>
            {
                Assembly pluginAssembly = LoadPluginAssembly(pluginPath);
                return CreatePlugins(pluginAssembly);
            }).ToList();
        }

        static Assembly LoadPluginAssembly(string relativePath)
        {
            // Navigate up to the solution root
            string root = Path.GetFullPath(Path.Combine(
                Path.GetDirectoryName(
                    Path.GetDirectoryName(
                        Path.GetDirectoryName(
                            Path.GetDirectoryName(
                                Path.GetDirectoryName(typeof(Program).Assembly.Location)))))));

            string pluginLocation = Path.GetFullPath(Path.Combine(root, relativePath.Replace('\\', Path.DirectorySeparatorChar)));
            Console.WriteLine($"Loading commands from: {pluginLocation}");
            PluginLoadContext loadContext = new PluginLoadContext(pluginLocation);
            loadContexts.Add(loadContext);
            return loadContext.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(pluginLocation)));
        }

        static IEnumerable<IPlugin> CreatePlugins(Assembly assembly)
        {
            int count = 0;

            foreach (Type type in assembly.GetTypes())
            {
                if (typeof(IPlugin).IsAssignableFrom(type))
                {
                    IPlugin result = Activator.CreateInstance(type) as IPlugin;
                    if (result != null)
                    {
                        count++;
                        yield return result;
                    }
                }
            }

            if (count == 0)
            {
                string availableTypes = string.Join(",", assembly.GetTypes().Select(t => t.FullName));
                throw new ApplicationException(
                    $"Can't find any type which implements {nameof(IPlugin)} in {assembly} from {assembly.Location}.\n" +
                    $"Available types: {availableTypes}");
            }
        }
    }
}