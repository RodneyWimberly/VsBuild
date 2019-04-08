using Microsoft.Build.Framework;
using Microsoft.Build.Logging;
using Microsoft.VisualStudio.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using VsBuild.MSBuildLogger;

namespace VsBuild.MSBuildRunner
{
    public class MSBuildProcess
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
                    CancellationTokenRegistration cancellationTokenRegistration = cancellationToken.Register(msBuild.Kill);

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
                    cancellationTokenRegistration.Dispose();
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
            if (e.Data == null)
                return;

            try
            {
                XElement data = XElement.Parse(e.Data);

                if (!Enum.TryParse(data.Attribute("Type").Value, out LoggerElementTypes type))
                    return;
                switch (type)
                {
                    case LoggerElementTypes.SetColor:
                        if (Enum.TryParse(data.Value, out ConsoleColor consoleColor))
                            SetColor?.Invoke(consoleColor);
                        break;
                    case LoggerElementTypes.ResetColor:
                        ResetColor?.Invoke();
                        break;
                    case LoggerElementTypes.Message:
                        WriteToLog?.Invoke(data.Value);
                        break;
                    case LoggerElementTypes.Error:
                        BuildErrorEvent?.Invoke(this, data.Value.Deserialize<BuildErrorEventArgs>());
                        break;
                    case LoggerElementTypes.Warning:
                        BuildWarningEvent?.Invoke(this, data.Value.Deserialize<BuildWarningEventArgs>());
                        break;
                }
                
            }
            catch
            {
                WriteToLog?.Invoke(e.Data);
            }
        }
    }
}
