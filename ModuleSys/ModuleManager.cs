using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace SteamBotV2.ModuleSys
{
    using Translations;

    /// <summary>
    /// This class handles loading/finding modules for SteamBotV2.
    /// </summary>
    public static class ModuleManager
    {
        private static readonly IList<string> searchPaths;
        private static readonly IDictionary<string, Assembly> loadedModules;

        static ModuleManager()
        {
            searchPaths = new List<string>();
            loadedModules = new Dictionary<string, Assembly>();
            AddPath("modules");
        }

        /// <summary>
        /// Combines an array of strings into a path then adds result to internal list of paths modules can be obtained from.
        /// </summary>
        /// <param name="paths">An array of parts of the path.</param>
        /// <remarks>Aborts if path is already in internal list.</remarks>
        public static void AddPath(params string[] paths)
        {
            string path = Path.Combine(paths);
            if (searchPaths.Contains(path))
                return;
            else if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            searchPaths.Add(path);
        }

        /// <summary>
        /// This allows already loading modules to add themselves to internal list of loaded modules that can be otained.
        /// </summary>
        /// <remarks>Aborts if your already added or the key is reserved for another module.</remarks>
        public static void AddModule()
        {
            Assembly caller = Assembly.GetCallingAssembly();
            string dllName = Path.GetFileNameWithoutExtension(caller.Location);
            KeyValuePair<string, Assembly> module = new KeyValuePair<string, Assembly>(dllName, caller);
            if (loadedModules.Contains(module))
                return;
            else if (loadedModules.ContainsKey(dllName) && loadedModules[dllName] != null)
            {
                Console.WriteLine(Phrases.module_name_reserved, dllName);
                return;
            }
            loadedModules.Add(module);
        }

        /// <summary>
        /// This attempts to find a module by it's the specified dll name(minus the .dll part).
        /// </summary>
        /// <param name="dllName">The file name of the dll(minus the .dll part).</param>
        /// <returns>The module if found, otherwise <code>null</code>.</returns>
        public static Assembly FindModule(string dllName)
        {
            Assembly module;
            if (loadedModules.TryGetValue(dllName, out module))
                return module;
            foreach (string path in searchPaths)
            {
                if (!Directory.Exists(path))
                    continue;
                string dllPath = Path.Combine(path, $"{dllName}.dll");
                if (!File.Exists(dllPath))
                    continue;
                try
                {
                    module = Assembly.LoadFile(dllPath);
                    loadedModules.Add(new KeyValuePair<string, Assembly>(dllName, module));
                    return module;
                }
                catch (Exception) { return null; }
            }
            Console.WriteLine(Phrases.module_missing, dllName);
            return null;
        }
    }
}
