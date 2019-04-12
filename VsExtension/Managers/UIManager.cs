using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using Task = System.Threading.Tasks.Task;

namespace VsBuild.VsExtension.Managers
{
    public static class UIManager
    {
        private static IServiceProvider _serviceProvider;

        public static void Initialize(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public static async Task UpdateUIAsync(bool immediateUpdate = false)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            IVsUIShell vsShell = (IVsUIShell)_serviceProvider.GetService(typeof(IVsUIShell));
            if (vsShell != null)
            {
                int hr = vsShell.UpdateCommandUI(Convert.ToInt32(immediateUpdate));
                Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(hr);
            }
        }
    }
}
