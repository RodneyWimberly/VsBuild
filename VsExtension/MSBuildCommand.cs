using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.IO;
using System.Threading;
using VsBuild.MSBuildRunner;
using VsBuild.VsExtension.EventSinks;
using VsBuild.VsExtension.Managers;
using VsBuild.VsExtension.Properties;
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
        private bool _showOnMainMenu;
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

            _consoleColor = Settings.Default.DefaultTextColor;
            _defaultTextColor = Settings.Default.DefaultTextColor;
            _targetMenuList = new MSBuildTargetMenuItems(Settings.Default.MSBuildTargetMenuItems);
            _showOnMainMenu = Settings.Default.ShowOnMainMenu;
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
                    menuCommand.Visible = _showOnMainMenu && ShowTargetList;
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

                    MSBuildProcess msBuild = new MSBuildProcess
                    {
                        VSWorkingDirectory = SolutionEventsSink.Instance.GetSolutionFolder(),
                        MSBuildProjectFile = settings.MSBuildProjectFile,
                        MSBuildTargets = _targetMenuList[targetCmdIndex].MsBuildTarget
                    };

                    msBuild.BuildErrorEvent += OnBuildErrorAsync;
                    msBuild.BuildWarningEvent += OnBuildWarningAsync;
                    msBuild.ErrorEvent += OnErrorEventAsync;
                    msBuild.WriteToLog += OnWriteToLogAsync;
                    msBuild.SetColor += OnSetColorAsync;
                    msBuild.ResetColor += OnResetColorAsync;

                    _toolsWindow = await GetToolsWindowAsync();
                    _toolsWindow.SetLogLayout(settings.DefaultFont, settings.BackgroundColor, settings.BuildLogWordWrap);
                    await _toolsWindow.ClearLogAsync();
                    _toolsWindow.SetCancelButtonEnabledFlag(true);

                    await msBuild.BuildAsync(CancellationTokenSource.Token);

                    _toolsWindow.SetCancelButtonEnabledFlag(false);
                    if (settings.SaveLogToDisk)
                    {
                        string logFile = _toolsWindow.SaveLogToDisk();
                        await OnWriteToLogAsync(this, new EventArgs<string>($"Build Log was saved to {logFile}"));
                    }

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

        private async Task OnErrorEventAsync(object sender, ErrorEventArgs e)
        {
            string errorMessage = string.Empty;
            if (CancellationTokenSource.IsCancellationRequested)
                errorMessage = "MSBuild Process was canceled";
            else
                errorMessage = $"MSBuild Process Exception: {e.GetException().Message}";
            await OutputPaneManager.WriteToPaneAsync(errorMessage);
            await TaskPaneManager.AddErrorAsync(errorMessage);
            await _toolsWindow.AppendLogAsync(ConsoleColor.Red, errorMessage);
        }

        private Task OnSetColorAsync(object sender, EventArgs<ConsoleColor> args)
        {
            _consoleColor = args.Data;
            return Task.CompletedTask;
        }

        private Task OnResetColorAsync(object sender, EventArgs args)
        {
            _consoleColor = _defaultTextColor;
            return Task.CompletedTask;
        }

        private async Task OnWriteToLogAsync(object sender, EventArgs<string> args)
        {
            await _toolsWindow.AppendLogAsync(_consoleColor, args.Data);
        }
               
        private async Task OnBuildWarningAsync(object sender, Microsoft.Build.Framework.BuildWarningEventArgs e)
        {
            await TaskPaneManager.AddBuildWarningAsync(e);
        }

        private async Task OnBuildErrorAsync(object sender, Microsoft.Build.Framework.BuildErrorEventArgs e)
        {
            await TaskPaneManager.AddBuildErrorAsync(e);
        }
    }
}
