using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Decompile
{
    class Program
    {
        public static string ManagedDir = @"C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Kingmaker\Kingmaker_Data\Managed";
        public static string ProjectDir = @"C:\Files\Code\UnityProject";
        static void DecompileAll()
        {
            var unityAssemblies = new string[] {
                "Unity.Analytics.DataPrivacy.dll",
                "UnityEngine.AccessibilityModule.dll",
                "UnityEngine.AIModule.dll",
                "UnityEngine.Analytics.dll",
                "UnityEngine.AnimationModule.dll",
                "UnityEngine.ARModule.dll",
                "UnityEngine.AssetBundleModule.dll",
                "UnityEngine.AudioModule.dll",
                "UnityEngine.BaselibModule.dll",
                "UnityEngine.ClothModule.dll",
                "UnityEngine.CloudWebServicesModule.dll",
                "UnityEngine.ClusterInputModule.dll",
                "UnityEngine.ClusterRendererModule.dll",
                "UnityEngine.CoreModule.dll",
                "UnityEngine.CrashReportingModule.dll",
                "UnityEngine.DirectorModule.dll",
                "UnityEngine.dll",
                "UnityEngine.FacebookModule.dll",
                "UnityEngine.GameCenterModule.dll",
                "UnityEngine.GridModule.dll",
                "UnityEngine.HotReloadModule.dll",
                "UnityEngine.ImageConversionModule.dll",
                "UnityEngine.IMGUIModule.dll",
                "UnityEngine.InputModule.dll",
                "UnityEngine.JSONSerializeModule.dll",
                "UnityEngine.Networking.dll",
                "UnityEngine.ParticlesLegacyModule.dll",
                "UnityEngine.ParticleSystemModule.dll",
                "UnityEngine.PerformanceReportingModule.dll",
                "UnityEngine.Physics2DModule.dll",
                "UnityEngine.PhysicsModule.dll",
                "UnityEngine.ScreenCaptureModule.dll",
                "UnityEngine.SharedInternalsModule.dll",
                "UnityEngine.SpatialTracking.dll",
                "UnityEngine.SpatialTrackingModule.dll",
                "UnityEngine.SpriteMaskModule.dll",
                "UnityEngine.SpriteShapeModule.dll",
                "UnityEngine.StandardEvents.dll",
                "UnityEngine.StyleSheetsModule.dll",
                "UnityEngine.SubstanceModule.dll",
                "UnityEngine.TerrainModule.dll",
                "UnityEngine.TerrainPhysicsModule.dll",
                "UnityEngine.TextRenderingModule.dll",
                "UnityEngine.TilemapModule.dll",
                "UnityEngine.Timeline.dll",
                "UnityEngine.TimelineModule.dll",
                "UnityEngine.TLSModule.dll",
                "UnityEngine.UI.dll",
                "UnityEngine.UIElementsModule.dll",
                "UnityEngine.UIModule.dll",
                "UnityEngine.UmbraModule.dll",
                "UnityEngine.UNETModule.dll",
                "UnityEngine.UnityAnalyticsModule.dll",
                "UnityEngine.UnityConnectModule.dll",
                "UnityEngine.UnityWebRequestAssetBundleModule.dll",
                "UnityEngine.UnityWebRequestAudioModule.dll",
                "UnityEngine.UnityWebRequestModule.dll",
                "UnityEngine.UnityWebRequestTextureModule.dll",
                "UnityEngine.UnityWebRequestWWWModule.dll",
                "UnityEngine.VehiclesModule.dll",
                "UnityEngine.VideoModule.dll",
                "UnityEngine.VRModule.dll",
                "UnityEngine.WebModule.dll",
                "UnityEngine.WindModule.dll",
                "UnityEngine.XRModule.dll",
            };
            var thirdParty = new string[] {
                "AuraSupportPlugin.dll",
                "DemiLib.dll",
                "DOTween.dll",
                "DOTween46.dll",
                "DOTweenPro.dll",
                "Pathfinding.ClipperLib.dll",
                "Pathfinding.Ionic.Zip.Reduced.dll",
                "Pathfinding.JsonFx.dll",
                "Pathfinding.Poly2Tri.dll",
                "Kingmaker.Import.dll",
            };
            var mainAssemblies = new string[] {
                "Assembly-CSharp.dll",
                "Assembly-CSharp-FirstPass.dll"
            };
            foreach (var assembly in unityAssemblies)
            {
                Simplify.SimplifyLib(assembly, $"simplified-{assembly}");
                ILSpy.DecompileProject($"simplified-{assembly}", $"{ProjectDir}/UnityEngine");
            }
            foreach (var assembly in mainAssemblies)
            {
                Simplify.SimplifyLib(assembly, $"simplified-{assembly}");
                ILSpy.DecompileProject($"simplified-{assembly}", $"{ProjectDir}/{assembly.Replace(".dll", "")}");
            }
            foreach (var assembly in thirdParty)
            {
                Simplify.SimplifyLib(assembly, $"simplified-{assembly}");
                ILSpy.DecompileProject($"simplified-{assembly}", $"{ProjectDir}/ThirdParty");
            }
        }
        static void CreateUnityProject()
        {
            Project.Create();
            Simplify.SimplifyLib("Assembly-CSharp.dll", "simplified-kingmaker.dll");
            Simplify.SimplifyLib("Assembly-CSharp-FirstPass.dll", "simplified-kingmaker-FirstPass.dll");
            ILSpy.DecompileProject("simplified-kingmaker.dll", $"{ProjectDir}/Assets/Kingmaker");
            ILSpy.DecompileProject("simplified-kingmaker-firstPass.dll", $"{ProjectDir}/Assets/Standard Assets");
        }
        static void Main(string[] args)
        {
            //CreateUnityProject();
            DecompileAll();
        }
    }
}
