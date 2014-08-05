using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using Boo.Lang.Compiler.Steps;

namespace US2CS
{

class WriteCSharp : AbstractCompilerStep
{
    private string _projectRoot;
    private string _outputRoot;

    public WriteCSharp(string projectRoot, string outputRoot)
    {
        _projectRoot = projectRoot;
        _outputRoot = outputRoot;
    }

    [System.Diagnostics.DebuggerNonUserCode]
    public override void Run()
    {
        var rootDirInfo = new DirectoryInfo(_projectRoot);
        if (Directory.Exists(_outputRoot))
        {
            Directory.Delete(_outputRoot, true);
        }

        foreach (var module in CompileUnit.Modules)
        {
            var fileInfo = new FileInfo(module.LexicalInfo.FileName);
            var relateivePath = fileInfo.FullName.Substring(rootDirInfo.FullName.Length+1);

            var js2csPat = new Regex(".js$");
            var outputPath = js2csPat.Replace(Path.Combine(_outputRoot, relateivePath), ".cs");
            var outputPathParent = Path.GetDirectoryName(outputPath);
            if (!Directory.Exists(outputPathParent))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
            }
            using (StreamWriter writer = File.CreateText(outputPath))
            {
                var visitor = new CSharpPrinterVisitor(writer);
                visitor.OnModule(module);
            }
        }
    }
}

}
