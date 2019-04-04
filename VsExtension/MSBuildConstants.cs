using System;

namespace VsBuild.VsExtension
{
    public class MSBuildConstants
    {   
        public const int TopMenu = 0x1021;
        public const int GroupForSubMenuTargetList = 0x1200;
        public const int SubMenuTargetListCommandId = 0x0200;

        public const int GroupForSolutionCommands = 0x2020;
        public const int MSBuildSolutionMenu = 0x2021;
        public const int GroupForSolutionTargetList = 0x2200;
        public const int SolutionTargetListCommandId = 0x2300;

        public const int MSBuildWindowCommandId = 4130;

        public static Guid MSBuildCommandPackage = new Guid(guidMSBuildCommandPackage);
        public const string guidMSBuildCommandPackage = "3cb97470-de5a-4a22-ab70-9d209d50d890";
        public static Guid MSBuildCommandPackageCmdSet = new Guid(guidMSBuildCommandPackageCmdSet);
        public const string guidMSBuildCommandPackageCmdSet = "714da179-e5f2-497f-ae7e-55161ab84a63";

        public static Guid ImagesMSBuildMenu = new Guid(guidImagesMSBuildMenu);
        public const string guidImagesMSBuildMenu = "b42b5039-ee1e-4eaf-af59-4d73aeca2a9c";
        public static Guid ImagesMSBuildWindow = new Guid(guidImagesMSBuildWindow);
        public const string guidImagesMSBuildWindow = "cd0d7a96-bb4b-4485-8316-2addfcb843d7";

        public static Guid MSBuildOptionsWindow = new Guid(guidMSBuildOptionsWindow);
        public const string guidMSBuildOptionsWindow = "FFCC6DB0-C42B-4ECB-938F-5C674D34B219";
        public static Guid MSBuildToolsWindow = new Guid(guidMSBuildToolsWindow);
        public const string guidMSBuildToolsWindow = "f1204738-7287-4ced-9fbb-ce1d84bd3178";

        public static Guid vsViewKindCode = new Guid(EnvDTE.Constants.vsViewKindCode);
    }
}
