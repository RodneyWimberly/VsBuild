using EnvDTE;
using EnvDTE80;
using Microsoft;
using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using System.Windows;
using System.Windows.Forms;
using VsBuild.VsExtension.Properties;

namespace VsBuild.VsExtension.EventSinks
{
    public class CommandEventSink 
    {
        private static IServiceProvider _serviceProvider;
        private static CommandEvents _commandEvents;
        private static Commands2 _commands;
        private static OleMenuCommandService _oleMenuCommandService;
        private static CommandID _buildCommandID;
        private static DTE2 _ide;
        private static Settings _settings;

        public static void Initialize(MSBuildCommandPackage package)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            _serviceProvider = package;
            _ide = _serviceProvider.GetService(typeof(DTE)) as DTE2;
            Assumes.Present(_ide);
            _commands = _ide.Commands as Commands2;
            _commandEvents = _ide.Events.CommandEvents;
            _commandEvents.BeforeExecute += OnBeforeExecute;
            _commandEvents.AfterExecute += OnAfterExecute;

            _oleMenuCommandService = _serviceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            Assumes.Present(_oleMenuCommandService);
            _buildCommandID = new CommandID(MSBuildConstants.MSBuildCommandPackageCmdSet, MSBuildConstants.SubMenuTargetListCommandId);

            _settings = Settings.Default;
        }

        private static void OnBeforeExecute(string Guid, int ID, object CustomIn, object CustomOut, ref bool CancelDefault)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (!_settings.OverrideRebuildCommand || !_ide.Solution.FullName.Contains("AxiomApplication"))
                return;

            string name = GetCommandName(Guid, ID);
            if (name.Contains("Rebuild"))
            {
                CancelDefault = true;

                OleMenuCommand buildCommand = _oleMenuCommandService.FindCommand(_buildCommandID) as OleMenuCommand;
                buildCommand.Invoke();
            }
        }

        private static void OnAfterExecute(string Guid, int ID, object CustomIn, object CustomOut)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (!_settings.BuildAfterGetLatest || !_ide.Solution.FullName.Contains("AxiomApplication"))
                return;

            string name = GetCommandName(Guid, ID);
            if (name.Contains("TfsContextGetLatestVersion"))
            {
                (new System.Threading.Thread(CloseMessageBox)).Start();
                if (System.Windows.MessageBox.Show("Would you like to rebuild the solution?", "Confirm",
                        MessageBoxButton.YesNo, MessageBoxImage.Question,
                MessageBoxResult.Yes) == MessageBoxResult.Yes)
                {
                    OleMenuCommand buildCommand = _oleMenuCommandService.FindCommand(_buildCommandID) as OleMenuCommand;
                    buildCommand.Invoke();
                }
            }
        }


        public static void Uninitialize()
        {
            if (_commandEvents != null)
            {
                _commandEvents.BeforeExecute -= OnBeforeExecute;
                _commandEvents.AfterExecute += OnAfterExecute;
            }
        }

        private static string GetCommandName(string Guid, int ID)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (Guid == null)
                return "null";
            try
            {
                return _commands.Item(Guid, ID).Name;
            }
            catch (Exception)
            {
            }
            return "";
        }

        public static void CloseMessageBox()
        {
            System.Threading.Thread.Sleep(5000);
            SendKeys.SendWait(" ");
        }
    }
}
