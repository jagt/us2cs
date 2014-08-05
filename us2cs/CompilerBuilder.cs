using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using UnityScript;
using Boo.Lang.Compiler;
using Boo.Lang.Compiler.IO;
using Boo.Lang.Compiler.Resources;
using Boo.Lang.Compiler.TypeSystem.Services;


namespace US2CS
{

class CompilerBuilder
{
    public List<string> References { get; set; }
    public List<string> Resources { get; set; }
    public List<string> Imports { get; set; }
    public List<string> SourceFiles { get; set; }              
    public List<string> SourceDirs { get; set; }
    public List<string> SuppressedWarnings { get; set; }
    public List<string> Defines { get; set; }
    public List<string> Pragmas { get; set; }

    public CompilerOutputType OutputType { get; set; }
    public string Output { get; set; }
    public string BaseClass { get; set; }
    public string MainMethod { get; set; }
    public string TypeInferenceRuleAttribute { get; set; }
    public bool Execute { get; set; }
    public bool Debug { get; set; }

    public CompilerBuilder()
    {
        References = new List<string>();
        Resources = new List<string>();
        Imports = new List<string>();
        SourceFiles = new List<string>();
        SourceDirs = new List<string>();
        SuppressedWarnings = new List<string>();
        Defines = new List<string>();
        Pragmas = new List<string>();

        OutputType = CompilerOutputType.Auto;
        Output = "";
        BaseClass = "";
        MainMethod = "";
        Execute = false;
    }

    // default compiler options from Unity 4.5, can be seen in Editor.log when there's
    // error in the code
    // unityLongVersion : UNITY_4_5_2
    public void SetupUnityProject(string projectRoot, string[] sourceFiles, string unityInstallation = "C:/Program Files (x86)/Unity", string unityLongVersion = "UNITY_4_5_2")
    {
        OutputType = CompilerOutputType.Library;
        Imports.Add("UnityEngine");
        Imports.Add("System.Collections");

        BaseClass = "UnityEngine.MonoBehaviour";
        SuppressedWarnings.Add("BCW0016");
        SuppressedWarnings.Add("BCW0003");

        Output = Path.Combine(projectRoot, "Temp/Assembly-UnityScript.dll");
        MainMethod = "Main";
        TypeInferenceRuleAttribute = "UnityEngineInternal.TypeInferenceRuleAttribute";
        Debug = true;
        Execute = false;

        string unityShortVersion = (new Regex(@"UNITY_\d+_\d+")).Match(unityLongVersion).Groups[0].Value;
        Defines.AddRange(new List<string> { 
            unityLongVersion,
            unityShortVersion,
            "UNITY_WEBPLAYER",
            "WEPLUG",
            "ENABLE_MICROPHONE",
            "ENABLE_TEXTUREID_MAP",
            "ENABLE_UNITYEVENTS",
            "ENABLE_ENABLE_NEW_HIERACHY",
            "ENABLE_AUDIO_FMOD",
            "ENABLE_MONO",
            "ENABLE_TERRAIN",
            "ENABLE_SUBSTANCE",
            "ENABLE_GENERICS",
            "INCLUDE_WP8SUPPORT",
            "ENABLE_MOVIES",
            "ENABLE_WWW",
            "ENABLE_IMAGEEFFECTS",
            "ENABLE_WEBCAM",
            "INCLUDE_METROSUPPORT",
            "RENDER_SOFTWARE_CURSOR",
            "ENABLE_NETWORK",
            "ENABLE_PHYSICS",
            "ENABLE_CACHING",
            "ENABLE_CLOTH",
            "ENABLE_2D_PHYSICS",
            "ENABLE_SHADOWS",
            "ENABLE_AUDIO",
            "ENABLE_NAVMESH_CARVING",
            "ENABLE_DUCK_TYPING",
            "ENABLE_SINGLE_INSTANCE_BUILD_SETTING",
            "ENABLE_PROFILER",
            "UNITY_EDITOR",
            "UNITY_EDITOR_WIN",
            "UNITY_TEAM_LICENSE",
            "UNITY_PRO_LICENSE",
        });

        References.AddRange(new List<string> { 
            unityInstallation + "/Editor/Data/Managed/UnityEngine.dll",
            projectRoot + "/Library/ScriptAssemblies/Assembly-CSharp-firstpass.dll",
            projectRoot + "/Library/ScriptAssemblies/Assembly-UnityScript-firstpass.dll",
            unityInstallation + "/Editor/Data/Managed/UnityEditor.dll",
        });

        Imports.Add("UnityEditor");

        foreach (var file in sourceFiles)
        {
            if (Path.IsPathRooted(file))
            {
                SourceFiles.Add(file);
            }
            else
            {
                SourceFiles.Add(Path.Combine(projectRoot, file));
            }
        }

        return;
    }

    public Compiler BuildCompiler()
    {
        var compiler = new Compiler();

        var asms = new List<Assembly>();
        foreach (var reference in References)
        {
            if (File.Exists(reference))
            {
                var asm = Assembly.LoadFrom(reference);
                Trace.Assert(asm != null, "failed to load assembly: " + reference);
                compiler.Parameters.References.Add(Assembly.LoadFrom(reference));
                asms.Add(asm);
            }
        }

        foreach (var file in SourceFiles)
        {
            compiler.Parameters.Input.Add(new FileInput(file));
        }

        if (!String.IsNullOrEmpty(Output))
        {
            compiler.Parameters.OutputAssembly = Output;
            Directory.CreateDirectory(Path.GetDirectoryName(Output));
        }

        foreach (var define in Defines)
        {
            compiler.Parameters.Defines[define] = define;
        }

        compiler.Parameters.OutputType = OutputType;

        // start building pipelines
        compiler.Parameters.Pipeline = UnityScriptCompiler.Pipelines.CompileToFile();

        if (SuppressedWarnings.Count > 0)
        {
            compiler.Parameters.Pipeline.Insert(0, new UnityScript.Steps.SuppressWarnings(new Boo.Lang.List<string>(SuppressedWarnings)));
        }

        if (Pragmas.Count > 0)
        {
            compiler.Parameters.Pipeline.Insert(0, new UnityScript.Steps.IntroducePragmas(new Boo.Lang.List<string>(Pragmas)));
        }

        compiler.Parameters.Imports.AddRange(Imports);

        Func<string, Type> loadAssemblyType = (name) => {
            foreach (Assembly asm in asms)
            {
                if (asm.GetType(name) != null)
                {
                    return asm.GetType(name);
                }
            }

            return Type.GetType(name);
        };


        compiler.Parameters.ScriptBaseType = loadAssemblyType(BaseClass);
        Trace.Assert(compiler.Parameters.ScriptBaseType != null, "BaseClass not found:" + BaseClass);
        compiler.Parameters.ScriptMainMethod = MainMethod;
        compiler.Parameters.Debug = Debug;
        compiler.Parameters.Expando = false;

        if (!String.IsNullOrEmpty(TypeInferenceRuleAttribute))
        {
            compiler.Parameters.AddToEnvironment(
                typeof(TypeInferenceRuleProvider),
                () => { return new CustomTypeInferenceRuleProvider(TypeInferenceRuleAttribute); }
                );
        }

        return compiler;
    }
}

}
