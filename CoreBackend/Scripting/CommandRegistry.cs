using System;
using Il2CppMoonSharp.Interpreter;
using MelonLoader;

namespace BadPlaceExecutor.Core.Scripting
{
    public static class CommandRegistry
    {
        private static bool _commandsRegistered = false;
        private static readonly Func<ScriptExecutionContext, CallbackArguments, DynValue> _printCustomDelegate = PrintCustom;

        public static void RegisterCustomCommands(Script script)
        {
            if (script == null) return;

            try
            {
                // Register print_custom using the callback pattern
                script.Globals.Set("print_custom", DynValue.NewCallback(_printCustomDelegate));
                
                MelonLogger.Msg("Registered custom commands to Script instance.");
                _commandsRegistered = true;
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Failed to register custom commands: {ex.Message}");
            }
        }

        private static DynValue PrintCustom(ScriptExecutionContext context, CallbackArguments args)
        {
            string message = args.Count > 0 ? args[0].String : "";
            MelonLogger.Msg($"[Lua Custom Print] {message}");
            return DynValue.Nil;
        }
    }
}
