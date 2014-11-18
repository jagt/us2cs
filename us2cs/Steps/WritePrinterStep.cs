using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using Boo.Lang.Compiler.Steps;
using Boo.Lang.Compiler.Ast.Visitors;

namespace US2CS
{

class WritePrinterStep : AbstractCompilerStep
{
    private string _projectRoot;
    private string _outputRoot;
    private Type _printerType;
    private string _suffix;

    public WritePrinterStep(string projectRoot, string outputRoot, Type printerType, string suffix)
    {
        _projectRoot = projectRoot;
        _outputRoot = outputRoot;
        _printerType = printerType;
        _suffix = suffix;
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
            // don't really know how this happens
            if (module.LexicalInfo.FileName == null)
            {
                Console.WriteLine("module is missing filename: {0}", module.Name);
                continue;
            }

            var fileInfo = new FileInfo(module.LexicalInfo.FileName);
            var relateivePath = fileInfo.FullName.Substring(rootDirInfo.FullName.Length+1);

            var js2csPat = new Regex(".js$");
            var outputPath = js2csPat.Replace(Path.Combine(_outputRoot, relateivePath), _suffix);
            var outputPathParent = Path.GetDirectoryName(outputPath);
            if (!Directory.Exists(outputPathParent))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
            }
            using (StreamWriter writer = File.CreateText(outputPath))
            {
                TextEmitter visitor = (TextEmitter)Activator.CreateInstance(_printerType, writer);
                visitor.OnModule(module);
            }
        }
    }
}

}
