using System;
using System.IO;

namespace Frontend.Utils
{
    public static class EnvironmentUtils
    {
        public static string BasePath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TheBadPlace");
        public static string ScriptsPath => Path.Combine(BasePath, "Scripts");
        public static string AutoExecPath => Path.Combine(BasePath, "AutoExec");
        public static string WorkspacePath => Path.Combine(BasePath, "Workspace");

        public static void InitializeFolders()
        {
            Directory.CreateDirectory(BasePath);
            Directory.CreateDirectory(ScriptsPath);
            Directory.CreateDirectory(AutoExecPath);
            Directory.CreateDirectory(WorkspacePath);
        }
    }
}
