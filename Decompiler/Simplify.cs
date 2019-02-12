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
        List<Instruction> NotImplementedBody;
        public static void RemoveInterfaceProperties(ModuleDefMD mod)
        {
            /*
             * Override of generic interface properties are not decompiled properly
             * TODO: look at fixing decompiler
             */ 
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
        void ClearMethod(MethodDef method)
        {
            /*
             * Remove method body
             * void return methods replaced with a return instruction
             * non-void return methods are replaced with a not implemented exception
             * TODO: look at returning default values
             * Try to clear constructors, preserves call to base constructor
             * struct members must be initialized inside the constructor, so body is left instact
             * TODO: look at removing struct bodies and initializing members with default values
             * base constructor calls are in the form
             * call instance void BaseClass::.ctor(parameters)
             */ 
            if (method.Name == ".ctor" && method.HasBody && method.DeclaringType.BaseType?.FullName != "System.ValueType")
            {
                var instructions = method.Body.Instructions;
                var baseCtor = method.Body.Instructions.FirstOrDefault(i =>
                    i.OpCode == OpCodes.Call &&
                    i.Operand is IMethod m &&
                    m.Name == ".ctor" &&
                    method.DeclaringType.BaseType.FullName != "System.Object");
                var index = baseCtor == null ? -1 : method.Body.Instructions.IndexOf(baseCtor);
                while (method.Body.Instructions.Count > index + 1)
                {
                    method.Body.Instructions.RemoveAt(index + 1);
                }
                method.Body.Instructions.Add(OpCodes.Ret.ToInstruction());
                method.Body.ExceptionHandlers.Clear();
                return;
            }
            if (method.Name == ".ctor")
            {
                return;
            }
            /*
             * convert extern methods into normal methods
             */ 
            if (method.IsInternalCall)
            {
                method.IsInternalCall = false;
                method.Body = new CilBody();
            }
            method.Body?.ExceptionHandlers.Clear();
            if (method.Body == null) return;
            method.Body.Instructions.Clear();
            if (method.ReturnType.TypeName != "Void" || method.IsSetter || method.MethodSig.Params.Any(p => p.IsByRef))
            {
                foreach (var inst in NotImplementedBody) method.Body.Instructions.Add(inst);
            }
            else
            {
                method.Body.Instructions.Add(OpCodes.Ret.ToInstruction());
            }
            method.Body.ExceptionHandlers.Clear();
        }
        void ManualFixes(ModuleDefMD mod)
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
            //Change UnitPartVisualChanges.SourceBone from Boo.List to System.List
            var UnitPartVisualChanges = mod.Find("Kingmaker.UnitLogic.Parts.UnitPartVisualChanges", false);
            if (DefaultJsonSettings != null)
            {
                var booList = UnitPartVisualChanges.FindField("SourceBone");
                Importer importer = new Importer(mod);
                TypeSig listGenericInstSig = importer.ImportAsTypeSig(typeof(System.Collections.Generic.List<String>));
                booList.FieldSig = new FieldSig(listGenericInstSig);
                var ctor = UnitPartVisualChanges.FindMethod(".ctor");
                foreach (var inst in ctor.Body.Instructions.ToArray())
                {
                    if(inst.OpCode == OpCodes.Newobj && inst.Operand is MemberRef memberRef)
                    {
                        memberRef.Class = listGenericInstSig.ToTypeDefOrRef();
                    }
                }
            }
        }
        private Simplify() { }
        public static void SimplifyLib(string assemblyName, string outputPath)
        {
            var simplify = new Simplify();
            ModuleDefMD mod = ModuleDefMD.Load(Path.Combine(Program.ManagedDir, assemblyName));
            var structRef = new TypeRefUser(mod, "System", "ValueType", mod.CorLibTypes.AssemblyRef);
            var exceptionRef = new TypeRefUser(mod, "System", "NotImplementedException", mod.CorLibTypes.AssemblyRef);
            var exceptionCtor = new MemberRefUser(mod, ".ctor",
                        MethodSig.CreateInstance(exceptionRef.ToTypeSig()),
                        exceptionRef);
            simplify.NotImplementedBody = new List<Instruction>(){
                OpCodes.Nop.ToInstruction(),
                OpCodes.Newobj.ToInstruction(exceptionCtor),
                OpCodes.Throw.ToInstruction()
            };
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
                "Kingmaker.Utility.Feet Kingmaker.Utility.FeetExtension::Feet(System.Int32)",
                "Kingmaker.UnitLogic.Mechanics.ContextValue Kingmaker.UnitLogic.Mechanics.ContextValue::op_Implicit(System.Int32)",
                "System.Boolean Kingmaker.UnitLogic.Customization.UnitCustomizationVariation::Equals(Kingmaker.UnitLogic.Customization.UnitCustomizationVariation)",
                "System.Boolean Kingmaker.UnitLogic.Customization.UnitCustomizationVariation::Equals(System.Object)",
            };
            var PropertyWhiteList = new HashSet<string>()
            {
                "System.String UnityEngine.CreateAssetMenuAttribute::menuName()",
                "System.String UnityEngine.CreateAssetMenuAttribute::ileName()",
                "System.Int32 UnityEngine.CreateAssetMenuAttribute::order()",
                "System.String UnityEngine.Bindings.NativeTypeAttribute::Header()",
                "Kingmaker.Blueprints.BlueprintComponent Kingmaker.Blueprints.BlueprintScriptableObject::ComponentsArray()",
                "UnityEngine.Color UnityEngine.Color::red()",
                "UnityEngine.Color UnityEngine.Color::green()",
                "UnityEngine.Color UnityEngine.Color::blue()",
                "UnityEngine.Color UnityEngine.Color::white()",
                "UnityEngine.Color UnityEngine.Color::black()",
                "UnityEngine.Color UnityEngine.Color::yellow()",
                "UnityEngine.Color UnityEngine.Color::cyan()",
                "UnityEngine.Color UnityEngine.Color::magenta()",
                "UnityEngine.Color UnityEngine.Color::gray()",
                "UnityEngine.Color UnityEngine.Color::grey()",
                "UnityEngine.Color UnityEngine.Color::clear()",
                "UnityEngine.Vector3 UnityEngine.Vector3::zero()",
                "UnityEngine.Vector3 UnityEngine.Vector3::one()",
                "UnityEngine.Vector3 UnityEngine.Vector3::up()",
                "UnityEngine.Vector3 UnityEngine.Vector3::down()",
                "UnityEngine.Vector3 UnityEngine.Vector3::left()",
                "UnityEngine.Vector3 UnityEngine.Vector3::right()",
                "UnityEngine.Vector3 UnityEngine.Vector3::forward()",
                "UnityEngine.Vector3 UnityEngine.Vector3::back()",
                "UnityEngine.Vector3 UnityEngine.Vector3::positiveInfinity()",
                "UnityEngine.Vector3 UnityEngine.Vector3::negativeInfinity()",
            };
            RemoveInterfaceProperties(mod);
            foreach (var type in mod.GetTypes())
            {
                foreach(var property in type.Properties.ToArray())
                {
                    /*
                     * Properties must be changed or preserved in unison
                     * removing a getter body while keeping the setter body causes the 
                     * setter to become recursive
                     * PropertyName { 
                     *  getter { throw new NotImplementedException() }
                     *  setter { PropertyName = value } 
                     * }
                     * TODO: Look at replacing properties with default implemtation
                     * PropertyName { get; set; }
                     */ 
                    if (PropertyWhiteList.Contains(property.FullName))
                    {
                        continue;
                    }
                    var getter = property.GetMethod;
                    var setter = property.SetMethod;
                    bool canSkipGetter = getter == null || !getter.HasBody || getter.Body.Instructions.Count <= 3;
                    bool canSkipSetter = getter == null || !getter.HasBody || getter.Body.Instructions.Count <= 4;
                    if (canSkipGetter && canSkipSetter)
                    {
                        continue;
                    }
                    if (getter != null && getter.HasBody) simplify.ClearMethod(getter);
                    if (setter != null && setter.HasBody) simplify.ClearMethod(setter);
                }
                foreach (var method in type.Methods.ToArray())
                {
                    if (method.IsGetter || method.IsSetter)
                    {
                        continue;
                    }
                    if (MethodWhiteList.Contains(method.FullName))
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
                    simplify.ClearMethod(method);
                }
            }
            simplify.ManualFixes(mod);
            mod.Write(Path.Combine(Program.ManagedDir, outputPath));
        }
    }
}
 
 
 