using EnvDTE;
using EnvDTE80;
using Microsoft;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using Task = System.Threading.Tasks.Task;

namespace VsBuild.VsExtension
{
    interface IOutputPane
    {
        IServiceProvider ServiceProvider { get; set; }
        Task InitializeAsync(string title = "");
        Task WriteToPaneAsync(string message);
    }

    class BuildPane : IOutputPane
    {
        public IServiceProvider ServiceProvider { get; set; }
        private OutputWindowPane _buildPane = null;

        public async Task InitializeAsync(string title = "")
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            DTE2 dte = (DTE2)ServiceProvider.GetService(typeof(DTE));
            Assumes.Present(dte);

            OutputWindowPanes panes = dte.ToolWindows.OutputWindow.OutputWindowPanes;
            foreach (OutputWindowPane pane in panes)
            {
                if (pane.Name.Contains("Build"))
                {
                    pane.Activate();
                    _buildPane = pane;
                }
            }
        }

        public BuildPane(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public async Task WriteToPaneAsync(string message)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            _buildPane.OutputString(message);
        }
    }

    class GeneralPane : IOutputPane
    {
        private IVsOutputWindowPane _generalPane = null;
        public IServiceProvider ServiceProvider { get; set; }
        public GeneralPane(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public async Task InitializeAsync(string title = "")
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            IVsOutputWindowPane _generalPane = (IVsOutputWindowPane)ServiceProvider.GetService(typeof(SVsGeneralOutputWindowPane));
            Assumes.Present(_generalPane);
            _generalPane.Activate();
        }

        public async Task WriteToPaneAsync(string message)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            _generalPane.OutputString(message);
        }
    }

    class CustomPane : IOutputPane
    {
        private OutputWindowPane _customPane = null;
        public IServiceProvider ServiceProvider { get; set; }

        public CustomPane(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public async Task InitializeAsync(string title = "")
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            DTE2 dte = (DTE2)ServiceProvider.GetService(typeof(DTE));
            Assumes.Present(dte);
            OutputWindowPanes panes = dte.ToolWindows.OutputWindow.OutputWindowPanes;
            try
            {
                _customPane = panes.Item(title);
            }
            catch (ArgumentException)
            {
                _customPane = panes.Add(title);
            }
            _customPane.Activate();
        }

        public async Task WriteToPaneAsync(string message)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            _customPane.OutputString(message);
        }
    }

    class CustomVsPane : IOutputPane
    {
        private IVsOutputWindowPane _customVsPane = null;
        public IServiceProvider ServiceProvider { get; set; }
        private Guid _guid;

        public CustomVsPane(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public async Task DeinitializeAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            IVsOutputWindow output = (IVsOutputWindow)ServiceProvider.GetService(typeof(SVsOutputWindow));
            Assumes.Present(output);
            IVsOutputWindowPane pane;
            output.GetPane(ref _guid, out pane);
            if (pane != null)
                output.DeletePane(ref _guid);
        }

        public async Task InitializeAsync(string title = "")
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            _guid = Guid.NewGuid();
            
            IVsOutputWindow output = (IVsOutputWindow)ServiceProvider.GetService(typeof(SVsOutputWindow));
            Assumes.Present(output);

            // Create a new pane.
            output.CreatePane(
                ref _guid,
                title,
                Convert.ToInt32(true),
                Convert.ToInt32(true));

            // Retrieve the new pane.
            output.GetPane(ref _guid, out _customVsPane);
            _customVsPane.Activate();
        }

        public async Task WriteToPaneAsync(string message)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            _customVsPane.OutputString(message);
        }
    }

    enum OutputPaneTypes
    {
        Build,
        General,
        Custom,
        CustomVs
    }

    internal static class OutputPaneManager
    {
        private static IOutputPane _outputPane;

        public static async Task InitializeAsync(IServiceProvider serviceProvider, OutputPaneTypes outputPaneType, string title = "")
        {
            switch(outputPaneType)
            {
                case OutputPaneTypes.Build:
                    _outputPane = new BuildPane(serviceProvider);
                    break;
                case OutputPaneTypes.General:
                    _outputPane = new GeneralPane(serviceProvider);
                    break;
                case OutputPaneTypes.Custom:
                    _outputPane = new CustomPane(serviceProvider);
                    break;
                case OutputPaneTypes.CustomVs:
                    _outputPane = new CustomVsPane(serviceProvider);
                    break;
            }
            await _outputPane.InitializeAsync(title);
        }

        public static async Task WriteToPaneAsync(string message)
        {
            await _outputPane.WriteToPaneAsync($"{message}\r\n");
            await UIManager.UpdateUIAsync();
        }
    }
}
