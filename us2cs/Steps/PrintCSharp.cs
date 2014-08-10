using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Boo.Lang.Compiler.Steps;


namespace US2CS
{
    class PrintCSharp : AbstractCompilerStep
    {
        public override void Run()
        {
            CSharpPrinterVisitor visitor = new CSharpPrinterVisitor(OutputWriter);
            visitor.Print(CompileUnit);
        }
    }
}
