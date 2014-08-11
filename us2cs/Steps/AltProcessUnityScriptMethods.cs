using Boo.Lang.Compiler;
using Boo.Lang.Compiler.Ast;
using Boo.Lang.Compiler.TypeSystem;
using Boo.Lang.Compiler.TypeSystem.Reflection;
using System;
using System.Collections.Generic;
using UnityScript.Steps;

namespace US2CS
{

// alternative ProcessUnityScriptMethods for overriding some methods
class AltProcessUnityScriptMethods : ProcessUnityScriptMethods
{
    // 
    // skip for loop normalizing
    // 
    public override void OnForStatement(Boo.Lang.Compiler.Ast.ForStatement node)
    {
        Visit(node.Iterator);
    }

    //
    // skip resolving operator overload
    //
}

}
