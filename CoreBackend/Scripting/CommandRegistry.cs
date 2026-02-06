using System;
using Il2CppMoonSharp.Interpreter;
using MelonLoader;

namespace BadPlaceExecutor.Core.Scripting
{
    public static class CommandRegistry
    {
        private static bool _commandsRegistered = false;

        public static void RegisterCustomCommands(Script script)
        {
            if (script == null) return;

            try
            {
                // Register print_custom
                script.Globals["print_custom"] = (Action<string>)PrintCustom;
                
                MelonLogger.Msg("Registered custom commands to Script instance.");
                _commandsRegistered = true;
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Failed to register custom commands: {ex.Message}");
            }
        }

        private static void PrintCustom(string message)
        {
            MelonLogger.Msg($"[Lua Custom Print] {message}");
        }
    }
}
