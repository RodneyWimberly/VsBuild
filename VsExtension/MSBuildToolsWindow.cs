namespace VsBuild.VsExtension
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;
    using Microsoft.VisualStudio.Shell;
    using System.Threading;
    using System.Drawing;

    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    /// </summary>
    /// <remarks>
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane,
    /// usually implemented by the package implementer.
    /// <para>
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its
    /// implementation of the IVsUIElementPane interface.
    /// </para>
    /// </remarks>
    [Guid(MSBuildConstants.guidMSBuildToolsWindow)]
    public class MSBuildToolsWindow : ToolWindowPane
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MSBuildToolsWindow"/> class.
        /// </summary>
        public MSBuildToolsWindow() : base(null)
        {
            this.Caption = "MSBuild Log";

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property.
            this.Content = new MSBuildToolsWindowControl(this);
        }

        public MSBuildToolsWindowControl MSBuildToolsWindowControl => (MSBuildToolsWindowControl)Content;

        public void SetCancelButtonEnabledFlag(bool enabled)
        {
            MSBuildToolsWindowControl.CancelButton.IsEnabled = enabled;
        }

        public async System.Threading.Tasks.Task AppendLogAsync(ConsoleColor color, string text)
        {
            await MSBuildToolsWindowControl.AppendLogAsync(text, color);
        }

        public string SaveLogToDisk()
        {
            return MSBuildToolsWindowControl.SaveLog();
        }

        public async System.Threading.Tasks.Task ClearLogAsync()
        {
            await MSBuildToolsWindowControl.ClearLogAsync();
        }

        public void SetLogLayout(Font font, Color backgroundColor, bool enableWordWrap)
        {
            MSBuildToolsWindowControl.SetLayout(font, backgroundColor, enableWordWrap);
        }
    }
}
