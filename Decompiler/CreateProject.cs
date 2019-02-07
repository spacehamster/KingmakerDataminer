using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Decompile
{
    class Project
    {
        public static void Create()
        {
            var libs = new string[]
            {
                "AuraSupportPlugin.dll",
                "DOTween.dll",
                "DOTween43.dll",
                "DOTween46.dll",
                "DOTween50.dll",
                "DOTweenPro.dll",
                "DemiLib.dll",
                "DotNetZip.dll",
                "GalaxyCSharp.dll",
                "Kingmaker.Import.dll",
                "Newtonsoft.Json.dll",
                "Pathfinding.ClipperLib.dll",
                "Pathfinding.Ionic.Zip.Reduced.dll",
                "Pathfinding.JsonFx.dll",
                "Pathfinding.Poly2Tri.dll",
                "System.Net.Http.dll",
                "Unity.Analytics.DataPrivacy.dll",
            };
            var projectDir = Program.ProjectDir;
            var managedDir = Program.ManagedDir;
            Directory.CreateDirectory(Path.Combine(projectDir, "Assets", "Libs"));
            Directory.CreateDirectory(Path.Combine(projectDir, "Assets", "Kingmaker"));
            Directory.CreateDirectory(Path.Combine(projectDir, "Assets", "Standard Assets"));
            foreach (var lib in libs)
            {
                var source = Path.Combine(managedDir, lib);
                var target = Path.Combine(projectDir, "Assets", "Libs", lib);
                File.Copy(source, target, true);
            }
        }
    }
}
