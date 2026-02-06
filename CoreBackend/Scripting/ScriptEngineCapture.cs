using HarmonyLib;
using Il2CppMoonSharp.Interpreter;
using MelonLoader;

namespace BadPlaceExecutor.Core.Scripting
{
    public static class ScriptEngineCapture
    {
        private static Script _capturedScript;

        public static Script CapturedScript => _capturedScript;

        public static void ApplyPatches(HarmonyLib.Harmony harmony)
        {
            try
            {
                var doStringMethod = typeof(Script).GetMethod("DoString", new[] { typeof(string), typeof(Table), typeof(string) });
                if (doStringMethod != null)
                {
                    harmony.Patch(doStringMethod, prefix: new HarmonyMethod(typeof(ScriptEngineCapture).GetMethod(nameof(OnDoStringPrefix))));
                    MelonLogger.Msg("Successfully patched Script.DoString for capture.");
                }
                else
                {
                    MelonLogger.Error("Failed to find Script.DoString method.");
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"Error applying script capture patches: {ex.Message}");
            }
        }

        public static bool OnDoStringPrefix(Script __instance)
        {
            if (__instance != null && _capturedScript != __instance)
            {
                _capturedScript = __instance;
                MelonLogger.Msg("Captured Script Instance!");
                
                CommandRegistry.RegisterCustomCommands(_capturedScript);
            }
            return true; // Continue execution
        }
    }
}
