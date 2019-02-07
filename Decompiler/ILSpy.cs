using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.Disassembler;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Decompile
{
    public class TestILSpy
    {
        public static void FixReferences(string csprojPath)
        {
            var manifest = File.ReadAllText(csprojPath);
            var anchor = "<Reference Include=\"UnityEngine.CoreModule\"";
            var gamePath = Program.ManagedDir;
            var lib = "UnityEngine";
            manifest = manifest.Replace(anchor,
                $"<Reference Include=\"{lib}\"><HintPath>{gamePath}\\{lib}.dll</HintPath></Reference>\n{anchor}");
            manifest = manifest.Replace("<TargetFrameworkVersion>v4.0", "<TargetFrameworkVersion>v4.6");
            File.WriteAllText(csprojPath, manifest);
        }
        public static void RunTest(string assemblyName, string outputDirectory)
        {
            Directory.CreateDirectory(outputDirectory);
            var assemblyFileName = Path.Combine(Program.ManagedDir, assemblyName);
            WholeProjectDecompiler decompiler = new WholeProjectDecompiler();
            var settings = decompiler.Settings;
            settings.OutVariables = false;
            Console.WriteLine($"Decompiling {assemblyName} - {decompiler.LanguageVersion}");
            var module = new PEFile(assemblyFileName);
            decompiler.AssemblyResolver = new UniversalAssemblyResolver(assemblyFileName, false, module.Reader.DetectTargetFrameworkId());
            decompiler.DecompileProject(module, outputDirectory);
            FixReferences(Path.Combine(outputDirectory, $"{module.Name}.csproj"));
        }
    }
}