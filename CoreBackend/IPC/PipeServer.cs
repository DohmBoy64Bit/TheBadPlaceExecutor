using System;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using MelonLoader;

namespace BadPlaceExecutor.Core.IPC
{
    public static class PipeServer
    {
        private const string PipeName = "TheBadPlace_Executor_Pipe";
        private static readonly ConcurrentQueue<string> _scriptBuffer = new ConcurrentQueue<string>();
        private static Thread _serverThread;
        private static bool _isRunning;

        public static void Start()
        {
            if (_isRunning) return;

            _isRunning = true;
            _serverThread = new Thread(ServerLoop)
            {
                IsBackground = true
            };
            _serverThread.Start();
            MelonLogger.Msg("IPC Pipe Server started.");
        }

        public static void Stop()
        {
            _isRunning = false;
        }

        public static bool TryGetNextScript(out string script)
        {
            return _scriptBuffer.TryDequeue(out script);
        }

        private static void ServerLoop()
        {
            while (_isRunning)
            {
                try
                {
                    using (var server = new NamedPipeServerStream(PipeName, PipeDirection.In))
                    {
                        server.WaitForConnection();

                        using (var reader = new StreamReader(server, Encoding.UTF8))
                        {
                            string script = reader.ReadToEnd();
                            if (!string.IsNullOrEmpty(script))
                            {
                                _scriptBuffer.Enqueue(script);
                                MelonLogger.Msg("Received script via IPC.");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (_isRunning)
                    {
                        MelonLogger.Error($"IPC Pipe Server error: {ex.Message}");
                    }
                }
            }
        }
    }
}
