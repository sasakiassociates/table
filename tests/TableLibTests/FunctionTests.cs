using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

using TableLib;

namespace TableLibTests
{
    public class FunctionTests
    {
        [SetUp]
        public void Setup()
        {
        }
        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void StartTableLibApp()
        {
            try
            {
                // Replace with the actual path to your TableLib.dll
                string consoleAppPath = "C:\\Users\\nshikada\\Documents\\GitHub\\table\\src\\tb-lib\\obj\\Debug\\netstandard2.0\\TableLib.dll";
                string arguments = "setup";

                // Create a new process start info
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "dotnet", // Use "dotnet" to run .NET Core apps
                    Arguments = $"{consoleAppPath} {arguments}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                // Start the process
                Process process = new Process { StartInfo = startInfo };
                process.OutputDataReceived += (sender, e) => Console.WriteLine(e.Data);
                process.ErrorDataReceived += (sender, e) => Console.WriteLine(e.Data);
                process.Start();

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                process.WaitForExit();
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
        }
    }
}
