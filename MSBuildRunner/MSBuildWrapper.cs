using Microsoft.Build.Framework;
using Microsoft.Build.Logging;
using Microsoft.VisualStudio.Threading;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace VsBuild.MSBuildRunner
{
    public class MSBuildWrapper
    {
        public ErrorEventHandler ErrorEvent;
        public BuildErrorEventHandler BuildErrorEvent;
        public BuildWarningEventHandler BuildWarningEvent;
        public ColorSetter SetColor;
        public ColorResetter ResetColor;
        public WriteHandler WriteToLog;

        public string MSBuildTargets { get; set; } 
        public string MSBuildProjectFile { get; set; } 
        public string MSBuildPath { get; set; }
        public LoggerVerbosity LoggerVerbosity { get; set; } = LoggerVerbosity.Normal;
        public string VSWorkingDirectory { get; set; } 

        public List<string> GetProjectTargets()
        {
            string file = Path.Combine(VSWorkingDirectory, MSBuildProjectFile);
            if (!File.Exists(file))
                throw new FileNotFoundException("MSBuild project file can't be found", file);
            XElement project = XElement.Load(file);
            List<string> targets = (from element in project.Descendants("Targets")
                                      from attribute in element.Attributes()
                                      where attribute.Name.LocalName == "Name"
                                      select attribute.Value).ToList();
            return targets;
        }

        public async Task BuildAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                if(string.IsNullOrEmpty(MSBuildPath))
                    MSBuildPath = MSBuildLocator.GetMSBuildLocations().First();

                if (!File.Exists(MSBuildPath))
                    throw new FileNotFoundException("MSBuild executable can't be found", MSBuildPath);

                if (!Directory.Exists(VSWorkingDirectory))
                   throw new DirectoryNotFoundException($"Working directory '{VSWorkingDirectory}' can't be found");

                if (!File.Exists(Path.Combine(VSWorkingDirectory, MSBuildProjectFile)))
                    throw new FileNotFoundException("MSBuild project file can't be found", Path.Combine(VSWorkingDirectory, MSBuildProjectFile));

                using (Process msBuild = new Process())
                {
                    msBuild.StartInfo.FileName = MSBuildPath;
                    msBuild.StartInfo.Arguments = $"{MSBuildProjectFile} /t:{MSBuildTargets} /v:{LoggerVerbosity} /noconlog /logger:VsBuild.MSBuildLogger.ColorConsoleLogger,\"{Environment.CurrentDirectory}\\VsBuild.MSBuildLogger.dll\"";
                    msBuild.StartInfo.WorkingDirectory = VSWorkingDirectory;
                    msBuild.StartInfo.UseShellExecute = false;
                    msBuild.StartInfo.CreateNoWindow = true;
                    msBuild.StartInfo.RedirectStandardOutput = true;
                    msBuild.StartInfo.RedirectStandardInput = true;
                    msBuild.StartInfo.RedirectStandardError = true;

                    msBuild.OutputDataReceived += MsBuild_OutputDataReceived;
                    msBuild.ErrorDataReceived += (sender, args) => ErrorEvent?.Invoke(this, new ErrorEventArgs(new Exception(args.Data)));

                    msBuild.Start();
                    msBuild.BeginOutputReadLine();
                    msBuild.BeginErrorReadLine();
                    await msBuild.WaitForExitAsync(cancellationToken);
                    msBuild.Close();
                }

                if (cancellationToken.IsCancellationRequested)
                    WriteToLog?.Invoke("Build was canceled");
            }
            catch (Exception ex)
            {
                ErrorEvent?.Invoke(this, new ErrorEventArgs(ex));
                return;
            }
        }

        private void MsBuild_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            string data = e.Data;
            if (data.Contains("#SetColor:"))
            {
                if(Enum.TryParse(data.Replace("#SetColor:", string.Empty).Replace("#", string.Empty), out ConsoleColor consoleColor))
                    SetColor?.Invoke(consoleColor);
            }
            else if (data.Contains("#ResetColor#"))
                ResetColor?.Invoke();
            else if (data.Contains("#BuildWarning:"))
            {
                data = data.Replace("#BuildWarning:", string.Empty).Replace("#", string.Empty);
                BuildWarningEventArgs args = JsonConvert.DeserializeObject<BuildWarningEventArgs>(data);
                BuildWarningEvent?.Invoke(this, args);
            }
            else if (data.Contains("#BuildError:"))
            {
                data = data.Replace("#BuildError:", string.Empty).Replace("#", string.Empty);
                BuildErrorEventArgs args = JsonConvert.DeserializeObject<BuildErrorEventArgs>(data);
                BuildErrorEvent?.Invoke(this, args);
            }
            else
                WriteToLog?.Invoke(data);
        }
    }
}
