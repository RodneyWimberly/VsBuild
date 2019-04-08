namespace VsBuild.VsExtension
{
    using Microsoft.VisualStudio.Shell;
    using System;
    using System.Drawing;
    using System.IO;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Forms;
    using System.Windows.Media;
    using VsBuild.VsExtension.Properties;

    /// <summary>
    /// Interaction logic for MSBuildWindowControl.
    /// </summary>
    public partial class MSBuildToolsWindowControl : System.Windows.Controls.UserControl
    {
        MSBuildToolsWindow _parent;

        /// <summary>
        /// Initializes a new instance of the <see cref="MSBuildToolsWindowControl"/> class.
        /// </summary>
        public MSBuildToolsWindowControl(MSBuildToolsWindow window)
        {
            InitializeComponent();
            _parent = window;
            Load();
        }

        private void Load()
        {
            string html = File.ReadAllText($"{Directory.GetCurrentDirectory()}\\BuildLog.html");
            LogViewer.Navigated += LogViewer_Navigated;
            LogViewer.NavigateToString(html);
        }

        private async void LogViewer_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            await System.Threading.Tasks.Task.Delay(10);
            SetLayout(Settings.Default.DefaultFont, Settings.Default.BackgroundColor);
        }

        private async void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            await ClearLogAsync();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if(System.Windows.MessageBox.Show(
                "Do you wish to cancel this build?", 
                "Cancel MSBuild", 
                MessageBoxButton.YesNo, 
                MessageBoxImage.Question, 
                MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                CancelButton.IsEnabled = false;
                MSBuildCommand.Instance.CancellationTokenSource.Cancel();
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveLog(true);
        }

        public async System.Threading.Tasks.Task AppendLogAsync(string text, ConsoleColor textColor)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            using (System.Windows.Threading.DispatcherProcessingDisabled d = Dispatcher.DisableProcessing())
            {
                LogViewer.InvokeScript("appendLog", text, textColor.ToHtmlColor());
            }
        }

        public string SaveLog(bool showMessage = false)
        {
            string fileName = $"{Environment.CurrentDirectory}\\BuildLog_{DateTime.UtcNow.ToString("yyyyMMddTHHmmss")}.html";
            dynamic doc = LogViewer.Document;
            var htmlText = doc.documentElement.InnerHtml;
            File.WriteAllText(fileName, htmlText);
            if (showMessage)
                System.Windows.MessageBox.Show(
                $"The MSBuild Log has been saved to '{fileName}'",
                "Save MSBuild Log",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return fileName;
        }

       public async System.Threading.Tasks.Task ClearLogAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            using (System.Windows.Threading.DispatcherProcessingDisabled d = Dispatcher.DisableProcessing())
            {
                LogViewer.InvokeScript("clearLog");
            }
        }

        public void SetLayout(Font font, System.Drawing.Color backgroundColor)
        {
            string style = font.Style == System.Drawing.FontStyle.Italic ? "Italic " : "";
            string weight = font.Style == System.Drawing.FontStyle.Bold ? "Bold " : "";
            LogViewer.InvokeScript("setLayout", $"{style}{weight}{font.Size}pt {font.Name}", backgroundColor.ToHtmlColor());
        }
    }
}