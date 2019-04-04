using VsBuild.VsExtension.Properties;
using Microsoft.Build.Framework;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;

namespace VsBuild.VsExtension
{
    [Guid(MSBuildConstants.guidMSBuildOptionsWindow)]
    public class MSBuildOptions : DialogPage
    {
        public const string Category = "MSBuild";
        public const string SubCategory = "General";

        public MSBuildOptions()
        {
            MSBuildTargetMenuItems = new MSBuildTargetMenuItems();
        }

        public override void LoadSettingsFromStorage()
        {
            Settings settings = Settings.Default;
            settings.Reload();

            MSBuildTargetMenuItems.Clear();
            MSBuildTargetMenuItems.Load(settings.MSBuildTargetMenuItems);

            MSBuildProjectFile = settings.MSBuildProjectFile;
            LoggerVerbosity = settings.LoggerVerbosity;
            SaveLogToDisk = settings.SaveLogToDisk;

            BackgroundColor = settings.BackgroundColor;
            DefaultFont = settings.DefaultFont;
            DefaultTextColor = settings.DefaultTextColor;
        }

        public override void SaveSettingsToStorage()
        {
            Settings settings = Settings.Default;

            settings.MSBuildTargetMenuItems = MSBuildTargetMenuItems.Save();

            settings.MSBuildProjectFile = MSBuildProjectFile;
            settings.LoggerVerbosity = LoggerVerbosity;
            settings.SaveLogToDisk = SaveLogToDisk;
            settings.DefaultTextColor = DefaultTextColor;

            settings.BackgroundColor = BackgroundColor;
            settings.DefaultFont = DefaultFont;

            settings.Save();
        }

        ////////////////////////////////////////////////////////////////////////////////////
        private const string TargetMenuItemsSubCategory = "Target Menu Items";

        [Category(TargetMenuItemsSubCategory)]
        [DisplayName("MSBuild Target Menu Items")]
        [Description("Menu item and MSBuild Target associations")]
        public MSBuildTargetMenuItems MSBuildTargetMenuItems { get; set; }

        ////////////////////////////////////////////////////////////////////////////////////
        private const string BuildParametersSubCategory = "Build Parameters";

        [Category(BuildParametersSubCategory)]
        [DisplayName("MSBuild Project File")]
        [Description("Name the MSBuild project file  (*.proj) that will be used to do the build.")]
        public string MSBuildProjectFile { get; set; }

        [Category(BuildParametersSubCategory)]
        [DisplayName("MSBuild Logger Verbosity")]
        [Description("Determines the verbosity level of the MSBuild logger")]
        public LoggerVerbosity LoggerVerbosity { get; set; }

        [Category(BuildParametersSubCategory)]
        [DisplayName("Save Log File")]
        [Description("Should the log file be persisted to disk after creation?")]
        public bool SaveLogToDisk { get; set; }

        ////////////////////////////////////////////////////////////////////////////////////
        private const string LayoutSubCategory = "Layout";

        [Category(LayoutSubCategory)]
        [DisplayName("Background Color")]
        [Description("The background color for the Log Viewer.")]
        public Color BackgroundColor { get; set; }

        [Category(LayoutSubCategory)]
        [DisplayName("Text Color")]
        [Description("The default text color for the Log Viewer.")]
        public ConsoleColor DefaultTextColor { get; set; }

        [Category(LayoutSubCategory)]
        [DisplayName("Default Font")]                                                                                                                             
        [Description("The default font for the Log Viewer.")]
        public Font DefaultFont { get; set; }
    }
}
