using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace TableUiReceiver
{
    public class Launcher
    {
        // Launches the detection program (non-blocking)
        public static void LaunchDetectionProgram(string detectionPath = "C:\\Users\\nshikada\\Documents\\GitHub\\table\\src\\detector")
        {
            try
            {
                string virtualEnvPath = Path.Combine(detectionPath, ".env");
                // string virtualEnvPath = "..\\..\\..\\..\\..\\src\\tb-detection\\.env";
                string pythonPathInEnv = Path.Combine(virtualEnvPath, "Scripts", "python.exe"); // For Windows
                // string scriptPath = "..\\..\\..\\..\\..\\src\\tb-detection\\main.py";
                string scriptPath = Path.Combine(detectionPath, "main.py");

                string argument1 = $"udp";
                string arguments = $"{scriptPath} {argument1}";

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = pythonPathInEnv,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = false
                };

                Process process = new Process
                {
                    StartInfo = startInfo
                };

                process.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
