namespace VsBuild.VsExtension
{
    using Microsoft.VisualStudio.Shell;
    using System;
    using System.IO;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Forms;
    using System.Windows.Media;

    /// <summary>
    /// Interaction logic for MSBuildWindowControl.
    /// </summary>
    public partial class MSBuildToolsWindowControl : System.Windows.Controls.UserControl
    {
        MSBuildToolsWindow parent;

        /// <summary>
        /// Initializes a new instance of the <see cref="MSBuildToolsWindowControl"/> class.
        /// </summary>
        public MSBuildToolsWindowControl(MSBuildToolsWindow window)
        {
            InitializeComponent();
            parent = window;
            LogTextBox.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
            LogTextBox.Document.PageWidth = 1000;
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ClearLog();
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
            Run run = new Run(text)
            {
                Foreground = new SolidColorBrush(textColor.MediaColor())
            };
            using (System.Windows.Threading.DispatcherProcessingDisabled d = Dispatcher.DisableProcessing())
            {
                LogTextBox.Document.Blocks.Add(new Paragraph(run));
                LogTextBox.ScrollToEnd();
            }
        }

        public string SaveLog(bool showMessage = false)
        {
            string fileName = $"{Environment.CurrentDirectory}\\{DateTime.UtcNow.ToString("yyyyMMddTHHmmss")}.rtf";
            FileStream fileStream = new FileStream(fileName, FileMode.Create);
            TextRange range = new TextRange(LogTextBox.Document.ContentStart, LogTextBox.Document.ContentEnd);
            range.Save(fileStream, System.Windows.DataFormats.Rtf);
            if (showMessage)
                System.Windows.MessageBox.Show(
                $"The MSBuild Log has been saved to '{fileName}'",
                "Save MSBuild Log",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return fileName;
        }

       public void ClearLog()
        {
            TextRange range = new TextRange(LogTextBox.Document.ContentStart, LogTextBox.Document.ContentEnd);
            range.Text = "";
        }
    }
}