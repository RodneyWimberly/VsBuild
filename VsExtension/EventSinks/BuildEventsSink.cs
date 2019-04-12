using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Diagnostics;
using System.Globalization;

namespace VsBuild.VsExtension.EventSinks
{
    //_solutionService = await GetServiceAsync(typeof(SVsSolution)) as IVsBuildStatusCallback;
    /*IVsSolutionBuildManager buildManager = (IVsSolutionBuildManager)GetService(typeof(SVsSolutionBuildManager));

    IVsProjectCfg[] ppIVsProjectCfg = new IVsProjectCfg[1];
    buildManager.FindActiveProjectCfg(IntPtr.Zero, IntPtr.Zero, ppHierarchy, ppIVsProjectCfg);

    IVsBuildableProjectCfg ppIVsBuildableProjectCfg;
    ppIVsProjectCfg[0].get_BuildableProjectCfg(out ppIVsBuildableProjectCfg);

      uint pdwCookie;
    ppIVsBuildableProjectCfg.AdviseBuildStatusCallback(new MyBuildStatusCallback(), out pdwCookie);
    */
    internal class BuildEventsSink : BuildableProjectConfig
    {
        private readonly EventSinkCollection _callbacks = new EventSinkCollection();

        public BuildEventsSink(ProjectConfig config) : base(config)
        {
        }

        public override int StartBuild(IVsOutputWindowPane pane, uint options)
        {
            NotifySubscribers();

            return VSConstants.S_OK;
        }

        private void NotifySubscribers()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            int shouldContinue = 1;
            foreach (IVsBuildStatusCallback cb in _callbacks)
            {
                try
                {
                    ErrorHandler.ThrowOnFailure(cb.BuildBegin(ref shouldContinue));
                    ErrorHandler.ThrowOnFailure(cb.BuildEnd(1));
                }
                catch (Exception e)
                {
                    
                    // If those who ask for status have bugs in their code it should not prevent the build/notification from happening
                    Debug.Fail(String.Format(CultureInfo.CurrentCulture, "Exception was thrown during BuildBegin event\n{0}", CultureInfo.CurrentUICulture), e.Message);
                }
            }
        }

        public override int AdviseBuildStatusCallback(IVsBuildStatusCallback callback, out uint cookie)
        {
            cookie = _callbacks.Add(callback);
            return VSConstants.S_OK;
        }

        public override int UnadviseBuildStatusCallback(uint cookie)
        {
            _callbacks.RemoveAt(cookie);
            return VSConstants.S_OK;
        }

        public override int StartClean(IVsOutputWindowPane pane, uint options)
        {
            NotifySubscribers();

            return VSConstants.S_OK;
        }

        public override int QueryStartBuild(uint options, int[] supported, int[] ready)
        {
            return VSConstants.S_OK;
        }

        public override int QueryStartClean(uint options, int[] supported, int[] ready)
        {
            return VSConstants.S_OK;
        }

        public override int QueryStartUpToDateCheck(uint options, int[] supported, int[] ready)
        {
            return VSConstants.S_OK;
        }

        public override int QueryStatus(out int done)
        {
            done = 1;
            return VSConstants.S_OK;
        }

        public override int Stop(int fsync)
        {
            return VSConstants.S_OK;
        }
    }
}
