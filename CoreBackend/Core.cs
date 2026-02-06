using MelonLoader;
using BadPlaceExecutor.Core.Scripting;
using BadPlaceExecutor.Core.IPC;
using BadPlaceExecutor.Core.Utils;

[assembly: MelonInfo(typeof(BadPlaceExecutor.Core.Core), "The Bad Place Executor", "1.0.0", "Zencoder")]
[assembly: MelonGame("Polytoria", "Polytoria Client")]

namespace BadPlaceExecutor.Core
{
    public class Core : MelonMod
    {
        private bool _hasRunAutoExec = false;

        public override void OnInitializeMelon()
        {
            MelonLogger.Msg("The Bad Place Executor - Initializing...");
            
            // Initialize folder structure
            EnvironmentUtils.InitializeFolders();
            
            // Apply Harmony patches for script capture
            ScriptEngineCapture.ApplyPatches(HarmonyInstance);
            
            // Start IPC Pipe Server
            PipeServer.Start();
            
            MelonLogger.Msg("Initialization complete.");
        }

        public override void OnUpdate()
        {
            var capturedScript = ScriptEngineCapture.CapturedScript;
            if (capturedScript == null) return;

            if (!_hasRunAutoExec)
            {
                _hasRunAutoExec = true;
                RunAutoExec(capturedScript);
            }

            // Execute scripts received via IPC on the main thread
            if (PipeServer.TryGetNextScript(out string scriptCode))
            {
                try
                {
                    MelonLogger.Msg("Executing script from IPC...");
                    capturedScript.DoString(scriptCode);
                }
                catch (System.Exception ex)
                {
                    MelonLogger.Error($"Error executing script: {ex.Message}");
                }
            }
        }

        private void RunAutoExec(Il2CppMoonSharp.Interpreter.Script script)
        {
            try
            {
                string path = EnvironmentUtils.AutoExecPath;
                if (!Directory.Exists(path)) return;

                MelonLogger.Msg("Checking AutoExec folder...");
                foreach (string file in Directory.GetFiles(path))
                {
                    string ext = Path.GetExtension(file).ToLower();
                    if (ext == ".lua" || ext == ".txt")
                    {
                        MelonLogger.Msg($"AutoExecuting: {Path.GetFileName(file)}");
                        try
                        {
                            string content = File.ReadAllText(file);
                            script.DoString(content);
                        }
                        catch (System.Exception ex)
                        {
                            MelonLogger.Error($"Error in AutoExec script {Path.GetFileName(file)}: {ex.Message}");
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"AutoExec error: {ex.Message}");
            }
        }
    }
}
