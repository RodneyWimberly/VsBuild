using VsBuild.VsExtension.Properties;
using EnvDTE;
using EnvDTE80;
using Microsoft;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Design;
using System.IO;

namespace VsBuild.VsExtension
{
    public class SolutionEventsSink : IVsSolutionEvents
    {
        private static SolutionEventsSink instance;
        private static IServiceProvider _serviceProvider;

        private SolutionEventsSink() { }

        public static SolutionEventsSink Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new SolutionEventsSink();
                }
                return instance;
            }
        }

        public static void Initialize(MSBuildCommandPackage package, IVsSolution solution, uint solutionCookie)
        {
            _serviceProvider = package;
            Instance.Package = package;
            Instance.Solution = solution;
            Instance.SolutionCookie = solutionCookie;
        }

        public IVsSolution Solution { get; set; }

        public MSBuildCommandPackage Package { get; set; }

        public ObservableCollection<string> Messages { get; set; } = new ObservableCollection<string>();

        public uint SolutionCookie { get; set; }

        public string GetSolutionFolder()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            DTE2 dte = _serviceProvider.GetService(typeof(DTE)) as DTE2;
            Assumes.Present(dte);
            Solution solution = dte.Solution;
            Assumes.Present(solution);
            return Path.GetDirectoryName(solution.FileName);
        }

        public int OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
        {
            MSBuildCommand.Instance.ShowTargetList = File.Exists(Path.Combine(GetSolutionFolder(), Settings.Default.MSBuildProjectFile));
            return VSConstants.S_OK;
        }

        public int OnQueryCloseSolution(object pUnkReserved, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeCloseSolution(object pUnkReserved)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterCloseSolution(object pUnkReserved)
        {
            MSBuildCommand.Instance.ShowTargetList = false;
            return VSConstants.S_OK;
        }
    }
}
