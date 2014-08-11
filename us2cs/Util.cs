using Boo.Lang.Compiler.Ast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace US2CS
{

static class Util
{
    public static string RightmostName(string name)
    {
        return name.Split('.').Last();
    }
}

}
