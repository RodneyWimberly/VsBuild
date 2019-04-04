using System.Collections.Generic;

namespace VsBuild.VsExtension
{
    public class MSBuildTargetMenuItems : List<MSBuildTargetMenuItem>
    {
        public MSBuildTargetMenuItems() { }

        public MSBuildTargetMenuItems(string data)
        {
            Load(data);
        }

        public string Save()
        {
            string data = string.Empty;
            foreach(MSBuildTargetMenuItem item in this)
                data += item.ToString() + ";";
            return data.Substring(0, data.Length -1);
        }

        public void Load(string data)
        {
            string[] items = data.Split(';');
            foreach(string item in items)
                Add(new MSBuildTargetMenuItem(item));
        }
    }
}
