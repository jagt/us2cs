using Boo.Lang.Compiler.Steps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityScript.Steps;

namespace US2CS
{

// alternative ProcessUnityScriptMethods for overriding some methods
class AltProcessUnityScriptMethods : ProcessUnityScriptMethods
{
    public override void OnForStatement(Boo.Lang.Compiler.Ast.ForStatement node)
    {
        // skip normalizing
        Visit(node.Iterator);
    }

    // handle 'transform.position.x += 1.2', which is not allowed in C# as position is a property.
    public override void ProcessStaticallyTypedAssignment(Boo.Lang.Compiler.Ast.BinaryExpression node)
    {
        TryToResolveAmbiguousAssignment(node);
        if (ValidateAssignment(node))
            BindExpressionType(node, GetExpressionType(node.Right));
        else
            Error(node);
    }

}

}
