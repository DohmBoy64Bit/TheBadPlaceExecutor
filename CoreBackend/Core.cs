using MelonLoader;
using BadPlaceExecutor.Core.Scripting;

[assembly: MelonInfo(typeof(BadPlaceExecutor.Core.Core), "The Bad Place Executor", "1.0.0", "Zencoder")]
[assembly: MelonGame("Polytoria", "Polytoria Client")]

namespace BadPlaceExecutor.Core
{
    public class Core : MelonMod
    {
        public override void OnInitializeMelon()
        {
            MelonLogger.Msg("The Bad Place Executor - Initializing...");
            
            // Apply Harmony patches for script capture
            ScriptEngineCapture.ApplyPatches(HarmonyInstance);
            
            MelonLogger.Msg("Initialization complete.");
        }
    }
}
