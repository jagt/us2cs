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
        Console.WriteLine(System.AppDomain.CurrentDomain.FriendlyName);
        Console.WriteLine("woot");
        Console.WriteLine(args.Length);
        Console.WriteLine(args[0]);
        Console.WriteLine(args[1]);


        var builder = new CompilerBuilder();
        var compiler = builder.SetupUnityProject(args[0], args[1])
            .BuildCompiler()
            .AdjustWriteCSharpPipeline()
            //.AdjustWriteBooPipeline()
            //.AdjustTestingPipeline()
            .Get;

        foreach (var step in compiler.Parameters.Pipeline)
        {
            Console.WriteLine(step.GetType().FullName);
        }



        var results = compiler.Run();
        if (results.Errors.Count > 0)
        {
            Console.WriteLine(results.Errors.ToString(true));
        }

    }
}

}
