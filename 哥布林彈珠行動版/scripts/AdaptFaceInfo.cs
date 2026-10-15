using System;
using System.Collections.Generic;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;

public static class AdaptFaceInfo
{
    static string stage = "start";
    static IEnumerable<TypeDefinition> AllTypes(IEnumerable<TypeDefinition> types)
    {
        foreach (TypeDefinition type in types)
        {
            yield return type;
            foreach (TypeDefinition nested in AllTypes(type.NestedTypes)) yield return nested;
        }
    }

    public static int Main(string[] args)
    {
        try { return Run(args); }
        catch (Exception error)
        {
            Console.WriteLine("Adapter failed at " + stage + ": " + error.GetType().Name + " " + error.Message);
            return 1;
        }
    }

    static int Run(string[] args)
    {
        string plugins = args[0];
        string backups = args[1];
        Directory.CreateDirectory(backups);
        string engine = args[2];
        DefaultAssemblyResolver resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(engine);
        resolver.AddSearchDirectory(plugins);
        resolver.AddSearchDirectory("E:/SteamLibrary/steamapps/common/Peglin/Peglin_Data/Managed");
        Dictionary<string, TypeDefinition> engineTypes = new Dictionary<string, TypeDefinition>();
        List<ModuleDefinition> engineModules = new List<ModuleDefinition>();
        foreach (string path in Directory.GetFiles(engine, "*.dll"))
        {
            string name = Path.GetFileName(path);
            if (!name.StartsWith("UnityEngine.") && name != "Unity.Scripting.dll") continue;
            stage = "read engine " + name;
            ModuleDefinition current = ModuleDefinition.ReadModule(path, new ReaderParameters { InMemory = true, AssemblyResolver = resolver });
            engineModules.Add(current);
            foreach (TypeDefinition type in AllTypes(current.Types)) engineTypes[type.FullName] = type;
        }
        foreach (string file in Directory.GetFiles(plugins, "*.dll"))
        {
            stage = "adapt " + Path.GetFileName(file);
            using (ModuleDefinition module = ModuleDefinition.ReadModule(file, new ReaderParameters { InMemory = true, AssemblyResolver = resolver }))
            {
                int getters = 0, setters = 0, glyphCalls = 0, movedTypes = 0;
                stage = "metadata " + Path.GetFileName(file);
                List<TypeReference> typeReferences = new List<TypeReference>(module.GetTypeReferences());
                foreach (TypeReference reference in typeReferences)
                {
                    AssemblyNameReference scope = reference.Scope as AssemblyNameReference;
                    if (scope == null || !scope.Name.StartsWith("UnityEngine.") || File.Exists(Path.Combine(engine, scope.Name + ".dll"))) continue;
                    TypeDefinition replacement;
                    if (engineTypes.TryGetValue(reference.FullName, out replacement))
                    {
                        stage = "adapt " + Path.GetFileName(file) + " type " + reference.FullName + " to " + replacement.Module.Name;
                        reference.Scope = module.ImportReference(replacement).Scope;
                        movedTypes++;
                    }
                }
                MethodDefinition glyphHelper = null;
                foreach (TypeDefinition type in AllTypes(module.Types))
                foreach (MethodDefinition method in new List<MethodDefinition>(type.Methods))
                {
                    stage = "IL " + Path.GetFileName(file) + " " + method.Name;
                    if (!method.HasBody) continue;
                    Instruction[] instructions = new List<Instruction>(method.Body.Instructions).ToArray();
                    ILProcessor processor = method.Body.GetILProcessor();
                    foreach (Instruction instruction in instructions)
                    {
                        MethodReference old = instruction.Operand as MethodReference;
                        if (old == null) continue;
                        if (old.DeclaringType.FullName == "UnityEngine.TextCore.LowLevel.FontEngine" && old.Name == "GetGlyphIndex" && old.Parameters.Count == 1)
                        {
                            if (glyphHelper == null)
                            {
                                TypeDefinition fontAsset = module.GetType("TMPro.TMP_FontAsset");
                                if (fontAsset == null) throw new Exception("Inspect another DLL's glyph API before adapting it: " + file);
                                glyphHelper = new MethodDefinition("BrowserCompatibleGetGlyphIndex", MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig, module.TypeSystem.UInt32);
                                glyphHelper.Parameters.Add(new ParameterDefinition("unicode", ParameterAttributes.None, module.TypeSystem.UInt32));
                                glyphHelper.Body.InitLocals = true;
                                VariableDefinition glyph = new VariableDefinition(module.TypeSystem.UInt32);
                                glyphHelper.Body.Variables.Add(glyph);
                                MethodReference lookup = new MethodReference("TryGetGlyphIndex", module.TypeSystem.Boolean, old.DeclaringType) { HasThis = false };
                                lookup.Parameters.Add(new ParameterDefinition(module.TypeSystem.UInt32));
                                lookup.Parameters.Add(new ParameterDefinition(new ByReferenceType(module.TypeSystem.UInt32)));
                                ILProcessor body = glyphHelper.Body.GetILProcessor();
                                body.Append(body.Create(OpCodes.Ldarg_0));
                                body.Append(body.Create(OpCodes.Ldloca, glyph));
                                body.Append(body.Create(OpCodes.Call, lookup));
                                body.Append(body.Create(OpCodes.Pop));
                                body.Append(body.Create(OpCodes.Ldloc, glyph));
                                body.Append(body.Create(OpCodes.Ret));
                                fontAsset.Methods.Add(glyphHelper);
                            }
                            instruction.Operand = glyphHelper;
                            glyphCalls++;
                            continue;
                        }
                        if (old.DeclaringType.FullName != "UnityEngine.TextCore.FaceInfo") continue;
                        bool getter = old.Name == "get_pointSize" && old.ReturnType.MetadataType == MetadataType.Int32;
                        bool setter = old.Name == "set_pointSize" && old.Parameters.Count == 1 && old.Parameters[0].ParameterType.MetadataType == MetadataType.Int32;
                        if (!getter && !setter) continue;
                        MethodReference adapted = new MethodReference(old.Name, getter ? module.TypeSystem.Single : old.ReturnType, old.DeclaringType)
                        {
                            HasThis = old.HasThis,
                            ExplicitThis = old.ExplicitThis,
                            CallingConvention = old.CallingConvention
                        };
                        foreach (ParameterDefinition parameter in old.Parameters)
                            adapted.Parameters.Add(new ParameterDefinition(parameter.Name, parameter.Attributes, setter ? module.TypeSystem.Single : parameter.ParameterType));
                        instruction.Operand = adapted;
                        if (getter)
                        {
                            processor.InsertAfter(instruction, Instruction.Create(OpCodes.Conv_I4));
                            getters++;
                        }
                        else
                        {
                            processor.InsertBefore(instruction, Instruction.Create(OpCodes.Conv_R4));
                            setters++;
                        }
                    }
                }
                if (getters + setters + glyphCalls + movedTypes == 0) continue;
                HashSet<string> usedScopes = new HashSet<string>();
                foreach (TypeReference reference in module.GetTypeReferences())
                {
                    AssemblyNameReference scope = reference.Scope as AssemblyNameReference;
                    if (scope != null) usedScopes.Add(scope.Name);
                }
                for (int index = module.AssemblyReferences.Count - 1; index >= 0; index--)
                {
                    string name = module.AssemblyReferences[index].Name;
                    if (name.StartsWith("UnityEngine.") && !File.Exists(Path.Combine(engine, name + ".dll")) && !usedScopes.Contains(name)) module.AssemblyReferences.RemoveAt(index);
                }
                string backup = Path.Combine(backups, Path.GetFileName(file));
                if (!File.Exists(backup)) File.Copy(file, backup);
                stage = "write " + Path.GetFileName(file);
                string temporary = file + ".adapted-tmp";
                module.Write(temporary);
                using (ModuleDefinition check = ModuleDefinition.ReadModule(temporary))
                {
                    if (check.Assembly.Name.Name != module.Assembly.Name.Name) throw new Exception("Adapted assembly identity changed.");
                }
                File.Copy(temporary, file, true);
                File.Delete(temporary);
                Console.WriteLine(Path.GetFileName(file) + " getters=" + getters + " setters=" + setters + " glyphCalls=" + glyphCalls + " movedTypes=" + movedTypes);
            }
        }
        foreach (ModuleDefinition engineModule in engineModules) engineModule.Dispose();
        resolver.Dispose();
        return 0;
    }
}
