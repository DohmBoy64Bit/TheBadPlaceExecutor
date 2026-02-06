using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;

namespace Frontend.IPC
{
    public static class PipeClient
    {
        private const string PipeName = "TheBadPlace_Executor_Pipe";

        public static async Task SendScript(string script)
        {
            try
            {
                using (var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out))
                {
                    // Timeout after 1 second if game isn't running
                    await client.ConnectAsync(1000);

                    using (var writer = new StreamWriter(client, Encoding.UTF8))
                    {
                        await writer.WriteAsync(script);
                        await writer.FlushAsync();
                    }
                }
            }
            catch (TimeoutException)
            {
                // Game probably not running or attached
                System.Windows.Forms.MessageBox.Show("Failed to connect to game. Is it running and attached?", "IPC Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"IPC Error: {ex.Message}", "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
        }
    }
}
