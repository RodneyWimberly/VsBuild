using VsBuild.VsExtension.Properties;
using VsBuild.MSBuildRunner;
using EnvDTE;
using EnvDTE80;
using Microsoft;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections;
using System.ComponentModel.Design;
using System.Globalization;
using System.IO;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace VsBuild.VsExtension
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class MSBuildCommand
    {
        private MSBuildToolsWindow _toolsWindow;
        private ConsoleColor _consoleColor;
        private ConsoleColor _defaultTextColor;
        private MSBuildTargetMenuItems _targetMenuList;
        private readonly AsyncPackage package;
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider => package;

        public CancellationTokenSource CancellationTokenSource { get; set; }
        public bool ShowTargetList { get; set; }
        public static MSBuildCommand Instance { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MSBuildCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private MSBuildCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            _defaultTextColor = Settings.Default.DefaultTextColor;
            _targetMenuList = new MSBuildTargetMenuItems(Settings.Default.MSBuildTargetMenuItems);

            ShowTargetList = false;
            InitializeTargetsMenu(commandService);
        }

        private void InitializeTargetsMenu(OleMenuCommandService menuCommandService)
        {
            int index = 0;
            foreach (MSBuildTargetMenuItem item in _targetMenuList)
            {
                CommandID commandID = new CommandID(MSBuildConstants.MSBuildCommandPackageCmdSet, MSBuildConstants.SubMenuTargetListCommandId + index);
                OleMenuCommand menuCommand = new OleMenuCommand(new EventHandler(Execute), commandID);
                menuCommand.BeforeQueryStatus += new EventHandler(OnTargetMenuQueryStatus);
                menuCommandService.AddCommand(menuCommand);

                CommandID solutionCommandID = new CommandID(MSBuildConstants.MSBuildCommandPackageCmdSet, MSBuildConstants.SolutionTargetListCommandId + index);
                OleMenuCommand solutionMenuCommand = new OleMenuCommand(new EventHandler(Execute), solutionCommandID);
                solutionMenuCommand.BeforeQueryStatus += new EventHandler(OnSolutionTargetMenuQueryStatus);
                menuCommandService.AddCommand(solutionMenuCommand);

                index++;
            }
        }

        private void OnTargetMenuQueryStatus(object sender, EventArgs e)
        {
            OleMenuCommand menuCommand = sender as OleMenuCommand;
            if (menuCommand != null)
            {
                int targetCmdIndex = menuCommand.CommandID.ID - MSBuildConstants.SubMenuTargetListCommandId;
                if (targetCmdIndex >= 0 && targetCmdIndex < _targetMenuList.Count)                { }
                {
                    menuCommand.Visible = ShowTargetList;
                    menuCommand.Text = _targetMenuList[targetCmdIndex].MenuText;
                }
            }
        }

        private void OnSolutionTargetMenuQueryStatus(object sender, EventArgs e)
        {
            OleMenuCommand menuCommand = sender as OleMenuCommand;
            if (menuCommand != null)
            {
                int targetCmdIndex = menuCommand.CommandID.ID - MSBuildConstants.SolutionTargetListCommandId;
                if (targetCmdIndex >= 0 && targetCmdIndex < _targetMenuList.Count)
                {
                    menuCommand.Visible = ShowTargetList;
                    menuCommand.Text = _targetMenuList[targetCmdIndex].MenuText;
                }
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in MSBuildCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
            Instance = new MSBuildCommand(package, commandService);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private async void Execute(object sender, EventArgs e)
        {
            OleMenuCommand menuCommand = sender as OleMenuCommand;
            if (menuCommand != null)
            {
                int targetCmdIndex = menuCommand.CommandID.ID - MSBuildConstants.SubMenuTargetListCommandId;;
                if (targetCmdIndex >= 0 && targetCmdIndex < _targetMenuList.Count)
                {
                    CancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(package.DisposalToken);
                    Settings settings = Settings.Default;

                    MSBuildWrapper msBuild = new MSBuildWrapper
                    {
                        VSWorkingDirectory = SolutionEventsSink.Instance.GetSolutionFolder(),
                        MSBuildProjectFile = settings.MSBuildProjectFile,
                        MSBuildTargets = _targetMenuList[targetCmdIndex].MsBuildTarget
                    };

                    msBuild.BuildErrorEvent += OnBuildError;
                    msBuild.BuildWarningEvent += OnBuildWarning;
                    msBuild.ErrorEvent += OnErrorEvent;
                    msBuild.WriteToLog += OnWriteToLog;
                    msBuild.SetColor += OnSetColor;
                    msBuild.ResetColor += OnResetColor;

                    _toolsWindow = await GetToolsWindowAsync();
                    _toolsWindow.SetLogLayout(settings.DefaultFont, settings.BackgroundColor);
                    _toolsWindow.ClearLog();
                    _toolsWindow.SetCancelButtonEnabledFlag(true);

                    await msBuild.BuildAsync(CancellationTokenSource.Token);

                    _toolsWindow.SetCancelButtonEnabledFlag(false);
                    if (settings.SaveLogToDisk)
                        _toolsWindow.SaveLogToDisk();
                }
            }
        }

        private async System.Threading.Tasks.Task<MSBuildToolsWindow> GetToolsWindowAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            ToolWindowPane window = package.FindToolWindow(typeof(MSBuildToolsWindow), 0, true);
            if ((null == window) || (null == window.Frame))
                throw new NotSupportedException("Cannot create MSBuild Tool Window");
            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
            return (MSBuildToolsWindow)window;
        }

        private async void OnErrorEvent(object sender, ErrorEventArgs e)
        {
            string errorMessage = $"MSBuild Wrapper Exception: {e.GetException().Message}";
            await OutputPaneManager.WriteToPaneAsync(errorMessage);
            await TaskPaneManager.AddErrorAsync(errorMessage);
            await _toolsWindow.AppendLogAsync(ConsoleColor.Red, errorMessage);
        }

        private void OnSetColor(ConsoleColor color)
        {
            _consoleColor = color;
        }

        private void OnResetColor()
        {
            _consoleColor = _defaultTextColor;
        }

        private async void OnWriteToLog(string data)
        {
            await _toolsWindow.AppendLogAsync(_consoleColor, data);
        }
               
        private async void OnBuildWarning(object sender, Microsoft.Build.Framework.BuildWarningEventArgs e)
        {
            await TaskPaneManager.AddBuildWarningAsync(e);
        }

        private async void OnBuildError(object sender, Microsoft.Build.Framework.BuildErrorEventArgs e)
        {
            await TaskPaneManager.AddBuildErrorAsync(e);
        }
    }
}
