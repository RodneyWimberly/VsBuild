namespace VsBuild.VsExtension
{
    public class MSBuildTargetMenuItem
    {
        public MSBuildTargetMenuItem() { }
        public MSBuildTargetMenuItem(string data)
        {
            string[] properties = data.Split('=');
            MenuText = properties[0];
            MsBuildTarget = properties[1];
        }

        public string MsBuildTarget { get; set; }
        public string MenuText { get; set; }

        public override string ToString()
        {
            return $"{MenuText}={MsBuildTarget}";
        }
    }
}
