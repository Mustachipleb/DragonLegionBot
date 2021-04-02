using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LavalinkClient
{
    public class LavalinkClient
    {
        private readonly StringBuilder logger;
        private readonly Process process;
        private CancellationToken token;


        public LavalinkClient()
        {
            logger = new StringBuilder();
            process = new Process();
        }

        public async Task StartAsync(CancellationToken token)
        {
            this.token = token;
            token.Register(Kill);
            var jarLocation = AppDomain.CurrentDomain.BaseDirectory + "lavalink\\";
            Console.WriteLine(jarLocation);
            var javaloc = Environment.GetEnvironmentVariable("JAVA_HOME") + "\\java";
            Console.WriteLine(javaloc);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                jarLocation = jarLocation.Replace('\\', '/');
                javaloc = javaloc.Replace('\\', '/');
            }
            Console.WriteLine(javaloc);
            Console.WriteLine(jarLocation);
            process.StartInfo.FileName = javaloc;
            process.StartInfo.Arguments = $"-jar {jarLocation}Lavalink.jar";
            process.StartInfo.WorkingDirectory = jarLocation;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.UseShellExecute = false;

            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.OutputDataReceived += (sender, args) => logger.AppendLine(args.Data);

            string stdError = null;
            process.Start();
            process.BeginOutputReadLine();
            stdError = await process.StandardError.ReadToEndAsync();
            process.WaitForExit();

            //throw new Exception($"Lavalink exited with code {process.ExitCode}");
        }

        public void Kill() => process.Kill();

        public string GetNewLogs()
        {
            var output = logger.ToString();
            logger.Clear();
            if (token.IsCancellationRequested)
                Kill();
            return output;
        }

        private string GetJavaInstallationPath()
        {
            string javaKey = "SOFTWARE\\JavaSoft\\JDK"; // TODO change
            using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey(javaKey))
            {
                string currentVersion = baseKey?.GetValue("CurrentVersion")?.ToString();
                using (var homeKey = baseKey?.OpenSubKey("13.0.2")) // todo change
                    return homeKey?.GetValue("JavaHome")?.ToString();
            }
        }
    }
}
