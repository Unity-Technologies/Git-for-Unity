using UnityEditor;
using UnityEngine;
using System.IO;
using System;

namespace GitHub.Unity
{
    [InitializeOnLoad]
    public class ExtensionLoader : ScriptableSingleton<ExtensionLoader>
    {
        [SerializeField] private bool initialized = true;

        public bool Initialized
        {
            get
            {
                return initialized;
            }
            set
            {
                initialized = value;
                Save(true);
            }
        }

        private static bool inSourceMode = false;
        private const string sourceModePath = "Assets/Editor/build/";
        private const string realPath = "Assets/Plugins/GitHub/Editor/";

        private static string[] assemblies20 = { "System.Threading.dll", "AsyncBridge.Net35.dll", "ReadOnlyCollectionsInterfaces.dll", "GitHub.Api.dll", "GitHub.Unity.dll" };
        private static string[] assemblies45 = { "GitHub.Api.45.dll", "GitHub.Unity.45.dll" };

        static ExtensionLoader()
        {
            EditorApplication.update += Initialize;
        }

        private static void Initialize()
        {
            EditorApplication.update -= Initialize;
            //if (!ExtensionLoader.instance.Initialized)
            {
                var scriptPath = Path.Combine(Application.dataPath, "Editor" + Path.DirectorySeparatorChar + "GitHub.Unity" + Path.DirectorySeparatorChar + "EntryPoint.cs");
                inSourceMode = File.Exists(scriptPath);
                ToggleAssemblies();
                //ExtensionLoader.instance.Initialized = true;
            }

        }

        private static void ToggleAssemblies()
        {
            var path = inSourceMode ? sourceModePath : realPath;
#if NET_4_6
            ToggleAssemblies(path, assemblies20, false);
            ToggleAssemblies(path, assemblies45, true);
#else
            ToggleAssemblies(path, assemblies45, false);
            ToggleAssemblies(path, assemblies20, true);
#endif
        }

        private static void ToggleAssemblies(string path, string[] assemblies, bool enable)
        {
            foreach (var file in assemblies)
            {
                var filepath = path + file;
                PluginImporter importer = AssetImporter.GetAtPath(filepath) as PluginImporter;
                if (importer == null)
                {
                    Debug.LogFormat("GitHub for Unity: Could not find importer for {0}. Some functionality may fail.", filepath);
                    continue;
                }
                if (importer.GetCompatibleWithEditor() != enable)
                {
                    importer.SetCompatibleWithEditor(enable);
                    importer.SaveAndReimport();
                }
            }
        }
    }
}
