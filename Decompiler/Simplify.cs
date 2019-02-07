using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.IO;

namespace Decompile
{
    public class Simplify
    {
        public static void LogAssembly(ModuleDefMD mod)
        {
            var file = new StreamWriter("Log.txt");
            foreach (var type in mod.GetTypes())
            {
                file.WriteLine($"TypeFullName: {type.FullName}");
                foreach (var @interface in type.Interfaces)
                {
                    file.WriteLine($"  Interface: {@interface.Interface}, {@interface.Interface.GetType()}");
                    if (@interface.Interface is TypeDef interfaceType)
                    {
                        foreach (var method in interfaceType.Methods)
                        {
                            file.WriteLine($"    Method: {method.Name}, {method.MethodSig}");
                        }
                    } else if(@interface.Interface is TypeSpec typeSpec)
                    {

                    }
                }
                foreach (var method in type.Methods.ToArray())
                {
                    file.WriteLine($"  Method: {method.Name}, {method.MethodSig}");
                }
                foreach (var property in type.Properties.ToArray())
                {
                    file.WriteLine($"  Property: {property.Name}");
                }
                foreach (var field in type.Fields.ToArray())
                {
                    file.WriteLine($"  Field: {field.Name}");
                }
                foreach (var @event in type.Events.ToArray())
                {
                    file.WriteLine($"  Event: {@event.Name}");
                }
                foreach (var nestedType in type.NestedTypes.ToArray())
                {
                    file.WriteLine($"  NestedTypes: {nestedType.Name}");
                }
            }
            file.Close();
        }
        public static void RemoveInterfaceProperties(ModuleDefMD mod)
        {
            HashSet<string> MethodBlacklist = new HashSet<string>()
            {
                "UnityEngine.UI.ICanvasElement.get_transform",
                "UnityEngine.UI.ICanvasElement.IsDestroyed",
                "Kingmaker.UI.SettingsUI.IPresetable.get_CurrentValue",
                "Kingmaker.UI.SettingsUI.IPresetable.set_CurrentValue",
                "Kingmaker.UI.Journal.IJournalElement.get_gameObject",
                "Kingmaker.UI.Journal.IJournalElement.get_transform"
            };
            HashSet<string> InterfaceBlacklist = new HashSet<string>()
            {
                "UnityEngine.UI.ICanvasElement",
                "Kingmaker.UI.SettingsUI.IPresetable`1<Kingmaker.UI.SettingsUI.DecalsQuality>",
                "Kingmaker.UI.SettingsUI.IPresetable`1<Kingmaker.UI.SettingsUI.ShadowQuality>",
                "Kingmaker.UI.SettingsUI.IPresetable`1<Kingmaker.UI.SettingsUI.LightsQuality>",
                "Kingmaker.UI.SettingsUI.IPresetable`1<Kingmaker.UI.SettingsUI.TextureQuality>",
                "Kingmaker.UI.Journal.IJournalElement`1<Kingmaker.AreaLogic.QuestSystem.Quest>",

            };
            foreach (var type in mod.GetTypes())
            {
                foreach (var method in type.Methods.ToArray())
                {
                    if (MethodBlacklist.Contains(method.Name))
                    {
                        type.Methods.Remove(method);
                    }
                }
                foreach (var @interface in type.Interfaces.ToArray())
                {
                    if (InterfaceBlacklist.Contains(@interface.Interface.FullName))
                    {
                        //type.Interfaces.Remove(@interface);
                    }
                }
            }
        }
        static void ReplaceBody(CilBody body, IList<Instruction> newInstructions)
        {
            body.Instructions.Clear();
            foreach (var inst in newInstructions)
            {
                body.Instructions.Add(inst);
            }
        }
        static void ClearMethod(MethodDef method)
        {
            method.Body.Instructions.Clear();
            method.Body.Instructions.Add(OpCodes.Ret.ToInstruction());
            method.Body.ExceptionHandlers.Clear();
        }
        static void ManualFixes(ModuleDefMD mod)
        {
            var UMM = mod.Find("UnityModManagerNet.UnityModManager", false);
            if (UMM != null)
            {
                mod.Types.Remove(UMM);
            }
            //Fix Base Constructor
            var UnitInteractWithObject = mod.Find("Kingmaker.UnitLogic.Commands.UnitInteractWithObject", false);
            if (UnitInteractWithObject != null)
            {
                var UnitCommand = mod.Find("Kingmaker.UnitLogic.Commands.Base.UnitCommand", false);
                var baseCtor = UnitCommand.FindMethod(".ctor");
                var ctor = UnitInteractWithObject.FindMethod(".ctor");
                ReplaceBody(ctor.Body, new List<Instruction>()
                {
                    OpCodes.Ldarg_0.ToInstruction(),
                    OpCodes.Ldc_I4_1.ToInstruction(),
                    OpCodes.Ldnull.ToInstruction(),
                    OpCodes.Call.ToInstruction(baseCtor),
                    OpCodes.Ret.ToInstruction()
                });
            }
            //Remove type
            var ExampleSelectable = mod.Find("UnityEngine.UI.Extensions.ExampleSelectable", false);
            if (ExampleSelectable != null)
            {
                mod.Types.Remove(ExampleSelectable);
            }
            //Remove override property
            var QuickGraphQueue = mod.Find("QuickGraph.Collections.Queue`1", false);
            if (QuickGraphQueue != null)
            {
                var method = QuickGraphQueue.FindMethod("QuickGraph.Collections.IQueue.get_Count");
                QuickGraphQueue.Methods.Remove(method);
            }
            /* Remove block from QuickGraph.Collections
             * Comparer<TKey> @default = Comparer<TKey>.Default;
			 *   this._002Ector(maximumErrorRate, keyMaxValue, (Func<TKey, TKey, int>)@default.Compare);
             */
            var BidirectionAdapterGraph = mod.Find("QuickGraph.BidirectionAdapterGraph`2", false);
            if (BidirectionAdapterGraph != null)
            {
                ClearMethod(BidirectionAdapterGraph.FindMethod(".ctor"));
            }
            var BinaryHeap = mod.Find("QuickGraph.Collections.BinaryHeap`2", false);
            if (BinaryHeap != null)
            {
                ClearMethod(BinaryHeap.FindMethod(".ctor"));
            }
            var FibonacciHeap = mod.Find("QuickGraph.Collections.FibonacciHeap`2", false);
            if (FibonacciHeap != null)
            {
                foreach(var ctor in FibonacciHeap.Methods.Where(m => m.Name == ".ctor"))
                {
                    ClearMethod(ctor);
                }
            }
            var FibonacciQueue = mod.Find("QuickGraph.Collections.FibonacciQueue`2", false);
            if (FibonacciQueue != null)
            {
                foreach (var ctor in FibonacciQueue.Methods.Where(m => m.Name == ".ctor"))
                {
                    ClearMethod(ctor);
                }
            }
            var BinaryQueue = mod.Find("QuickGraph.Collections.BinaryQueue`2", false);
            if (BinaryQueue != null)
            {
                ClearMethod(BinaryQueue.FindMethod(".ctor"));
            }
            var SoftHeap = mod.Find("QuickGraph.Collections.SoftHeap`2", false);
            if (SoftHeap != null)
            {
                ClearMethod(SoftHeap.FindMethod(".ctor"));
            }
            foreach(var type in mod.Types.ToArray())
            {
                if (type.Namespace == "ProBuilder2.Examples") mod.Types.Remove(type);
            }
            //Positional Arguments
            var CameraController = mod.Find("Kingmaker.Controllers.Rest.CameraController", false);
            if(CameraController != null)
            {
                CameraController.Remove(CameraController.FindMethod(".ctor"));
            }
            var KingdomCameraController = mod.Find("Kingmaker.Controllers.KingdomCameraController", false);
            if (KingdomCameraController != null)
            {
                KingdomCameraController.Remove(KingdomCameraController.FindMethod(".ctor"));
            }
            //Clear cctor
            var DefaultJsonSettings = mod.Find("Kingmaker.EntitySystem.Persistence.JsonUtility.DefaultJsonSettings", false);
            if (DefaultJsonSettings != null)
            {
                ClearMethod(DefaultJsonSettings.FindMethod(".cctor"));
            }
        }
        public static void RunTest(string assemblyName, string outputPath)
        {
            Console.WriteLine($"Simplifying {assemblyName}");
            var AttributeBlackList = new HashSet<string>()
            {
                "System.Runtime.CompilerServices.AsyncStateMachineAttribute",
                "System.Diagnostics.DebuggerHiddenAttribute",
                "System.Security.SecurityCriticalAttribute"
            };
            var MethodWhiteList = new HashSet<string>(){
                "System.Void QuickGraph.SUndirectedTaggedEdge`2::add_TagChanged(System.EventHandler)",
                "System.Void QuickGraph.SUndirectedTaggedEdge`2::remove_TagChanged(System.EventHandler)",
                "System.Void QuickGraph.STaggedEquatableEdge`2::add_TagChanged(System.EventHandler)",
                "System.Void QuickGraph.STaggedEquatableEdge`2::remove_TagChanged(System.EventHandler)",
                "System.Void QuickGraph.STaggedEdge`2::add_TagChanged(System.EventHandler)",
                "System.Void QuickGraph.STaggedEdge`2::remove_TagChanged(System.EventHandler)",
                "System.Void Steamworks.CallResult`1::add_m_Func(Steamworks.CallResult`1/APIDispatchDelegate<T>)",
                "System.Void Steamworks.CallResult`1::remove_m_Func(Steamworks.CallResult`1/APIDispatchDelegate<T>)",
                "System.String UnityEngine.CreateAssetMenuAttribute::get_menuName()",
                "System.String UnityEngine.CreateAssetMenuAttribute::set_menuName()",
                "System.String UnityEngine.CreateAssetMenuAttribute::get_fileName()",
                "System.String UnityEngine.CreateAssetMenuAttribute::set_fileName()",
                "System.Int32 UnityEngine.CreateAssetMenuAttribute::get_order()",
                "System.Int32 UnityEngine.CreateAssetMenuAttribute::set_order()",
                "System.String UnityEngine.Bindings.NativeTypeAttribute::get_Header()",
                "System.String UnityEngine.Bindings.NativeTypeAttribute::set_Header()",
            };
            ModuleDefMD mod = ModuleDefMD.Load(Path.Combine(Program.ManagedDir, assemblyName));
            var structRef = new TypeRefUser(mod, "System", "ValueType", mod.CorLibTypes.AssemblyRef);
            var exceptionRef = new TypeRefUser(mod, "System", "NotImplementedException", mod.CorLibTypes.AssemblyRef);
            var exceptionCtor = new MemberRefUser(mod, ".ctor",
                        MethodSig.CreateInstance(exceptionRef.ToTypeSig()),
                        exceptionRef);
            var newBody = new List<Instruction>();
            newBody.Add(OpCodes.Nop.ToInstruction());
            newBody.Add(OpCodes.Newobj.ToInstruction(exceptionCtor));
            newBody.Add(OpCodes.Throw.ToInstruction());
            RemoveInterfaceProperties(mod);
            foreach (var type in mod.GetTypes())
            {
                foreach (var method in type.Methods.ToArray())
                {

                    if (method.Name == ".ctor" && method.HasBody && type.BaseType?.FullName != "System.ValueType")
                    {
                        var instructions = method.Body.Instructions;
                        var baseCtor = method.Body.Instructions.FirstOrDefault(i =>
                            i.OpCode == OpCodes.Call &&
                            i.Operand is IMethod m &&
                            m.Name == ".ctor" &&
                            type.BaseType.FullName != "System.Object");
                        var index = baseCtor == null ? -1 : method.Body.Instructions.IndexOf(baseCtor);
                        while (method.Body.Instructions.Count > index + 1)
                        {
                            method.Body.Instructions.RemoveAt(index + 1);
                        }
                        method.Body.Instructions.Add(OpCodes.Ret.ToInstruction());
                        method.Body.ExceptionHandlers.Clear();
                        continue;
                    }
                    if (method.Name == ".ctor")
                    {
                        continue;
                    }
                    if (method.IsInternalCall)
                    {
                        method.IsInternalCall = false;
                        method.Body = new CilBody();
                    }
                    method.Body?.ExceptionHandlers.Clear();
                    if (MethodWhiteList.Contains(method.FullName))
                    {
                        continue;
                    }
                    if (method.IsGetter && method.HasBody && method.Body.Instructions.Count <= 3)
                    {
                        continue;
                    }
                    if (method.IsSetter && method.HasBody && method.Body.Instructions.Count <= 4)
                    {
                        continue;
                    }
                    foreach (var att in method.CustomAttributes.ToArray())
                    {
                        if (AttributeBlackList.Contains(att.TypeFullName))
                        {
                            method.CustomAttributes.Remove(att);
                        }
                    }
                    if (method.Body == null) continue;
                    method.Body.Instructions.Clear();
                    if (method.ReturnType.TypeName != "Void" || method.IsSetter || method.MethodSig.Params.Any(p => p.IsByRef))
                    {
                        foreach (var inst in newBody) method.Body.Instructions.Add(inst);
                    }
                    else
                    {
                        method.Body.Instructions.Add(OpCodes.Ret.ToInstruction());
                    }
                    method.Body.ExceptionHandlers.Clear();
                }
            }
            ManualFixes(mod);
            mod.Write(Path.Combine(Program.ManagedDir, outputPath));
        }
    }
}
 
 
 