using System;
using System.Collections.Generic;
using System.Linq;
using Boo.Lang.Compiler;

namespace US2CS
{

static class Entry
{
    public static void Main(string[] args)
    {
        Console.WriteLine("woot");
        var builder = new CompilerBuilder();
        builder.SetupUnityProject(args[1], args.Skip(2).ToArray());
        var compiler = builder.BuildCompiler();

        var results = compiler.Run();
        if (results.Errors.Count > 0)
        {
            Console.WriteLine(results.Errors.ToString(true));
        }

    }
}

}
