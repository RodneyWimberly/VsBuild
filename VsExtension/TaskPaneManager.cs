using Microsoft;
using Microsoft.Build.Framework;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using Task = System.Threading.Tasks.Task;

namespace VsBuild.VsExtension
{
    internal static class TaskPaneManager
    {
        private static ErrorListProvider _errorListProvider;
        private static IServiceProvider _serviceProvider;

        public static void Initialize(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _errorListProvider = new ErrorListProvider(_serviceProvider);
        }

        public static async Task AddErrorAsync(string message)
        {
            await AddTaskAsync(message, TaskErrorCategory.Error);
        }
        public static async Task AddBuildErrorAsync(BuildErrorEventArgs buildError)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            IVsSolution ivsSolution = (IVsSolution)_serviceProvider.GetService(typeof(IVsSolution));
            Assumes.Present(ivsSolution);
            IVsHierarchy hierarchyItem;
            ivsSolution.GetProjectOfUniqueName(buildError.ProjectFile, out hierarchyItem);

            ErrorTask errorTask = new ErrorTask
            {
                ErrorCategory = TaskErrorCategory.Error,
                Category = TaskCategory.BuildCompile,
                Text = buildError.Message,
                Document = buildError.File,
                Line = buildError.LineNumber,
                Column = buildError.ColumnNumber,
                HierarchyItem = hierarchyItem
            };
            // There are two Bugs in the errorListProvider.Navigate method:
            // Line number needs adjusting and Column is not shown
            errorTask.Navigate += (sender, e) =>
            {
                errorTask.Line++;
                _errorListProvider.Navigate(errorTask, MSBuildConstants.vsViewKindCode);
                errorTask.Line--;
            };
            
            _errorListProvider.Tasks.Add(errorTask);
            await UIManager.UpdateUIAsync();
        }

        public static async Task AddWarningAsync(string message)
        {
            await AddTaskAsync(message, TaskErrorCategory.Warning);
        }

        public static async Task AddBuildWarningAsync(BuildWarningEventArgs buildWarning)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            IVsSolution ivsSolution = (IVsSolution)_serviceProvider.GetService(typeof(IVsSolution));
            Assumes.Present(ivsSolution);
            IVsHierarchy hierarchyItem;
            ivsSolution.GetProjectOfUniqueName(buildWarning.ProjectFile, out hierarchyItem);

            ErrorTask errorTask = new ErrorTask
            {
                ErrorCategory = TaskErrorCategory.Warning,
                Category = TaskCategory.BuildCompile,
                Text = buildWarning.Message,
                Document = buildWarning.File,
                Line = buildWarning.LineNumber,
                Column = buildWarning.ColumnNumber,
                HierarchyItem = hierarchyItem
            };
            // There are two Bugs in the errorListProvider.Navigate method:
            // Line number needs adjusting and Column is not shown
            errorTask.Navigate += (sender, e) =>
            {
                errorTask.Line++;
                _errorListProvider.Navigate(errorTask, MSBuildConstants.vsViewKindCode);
                errorTask.Line--;
            };

            _errorListProvider.Tasks.Add(errorTask);
            await UIManager.UpdateUIAsync();
        }

        public static async Task AddMessageAsync(string message)
        {
            await AddTaskAsync(message, TaskErrorCategory.Message);
        }

        public static async Task AddBuildMessageAsync(BuildMessageEventArgs buildMessage)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            IVsSolution ivsSolution = (IVsSolution)_serviceProvider.GetService(typeof(IVsSolution));
            Assumes.Present(ivsSolution);
            IVsHierarchy hierarchyItem;
            ivsSolution.GetProjectOfUniqueName(buildMessage.ProjectFile, out hierarchyItem);

            ErrorTask errorTask = new ErrorTask
            {
                ErrorCategory = TaskErrorCategory.Message,
                Category = TaskCategory.BuildCompile,
                Text = buildMessage.Message,
                Document = buildMessage.File,
                Line = buildMessage.LineNumber,
                Column = buildMessage.ColumnNumber,
                HierarchyItem = hierarchyItem
            };
            // There are two Bugs in the errorListProvider.Navigate method:
            // Line number needs adjusting and Column is not shown
            errorTask.Navigate += (sender, e) =>
            {
                errorTask.Line++;
                _errorListProvider.Navigate(errorTask, MSBuildConstants.vsViewKindCode);
                errorTask.Line--;
            };

            _errorListProvider.Tasks.Add(errorTask);
            await UIManager.UpdateUIAsync();
        }

        private static async Task AddTaskAsync(string message, TaskErrorCategory category)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            _errorListProvider.Tasks.Add(new ErrorTask
            {
                Category = TaskCategory.User,
                ErrorCategory = category,
                Text = message
            });
            await UIManager.UpdateUIAsync();
        }
    }
}
