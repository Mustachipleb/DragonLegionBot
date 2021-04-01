using Microsoft.Win32;
using System;
using System.Diagnostics;
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
            
            process.StartInfo.FileName = GetJavaInstallationPath() + "\\bin\\java.exe";
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
