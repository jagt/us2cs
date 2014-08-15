using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Boo.Lang.Compiler.TypeSystem;
using Boo.Lang.Compiler.Steps;
using Boo.Lang.Compiler.Ast;

namespace US2CS
{

class ProcessBuiltinFunction : AbstractTransformerCompilerStep
{
    private bool _shouldRemoveExpression;
    private Block _currentBlock;

    public override void OnMethodInvocationExpression(MethodInvocationExpression node)
    {
        var entity = TypeSystemServices.GetEntity(node.Target);
        if (entity.EntityType == EntityType.BuiltinFunction)
        {
            ProcessBuiltin(node, (BuiltinFunction)entity);
        }

        base.OnMethodInvocationExpression(node);
    }

    /*
    public override bool EnterExpressionStatement(ExpressionStatement node)
    {
        _shouldRemoveExpression = false; // FIXME might need to use a stack
        return base.EnterExpressionStatement(node);
    }

    public override void LeaveExpressionStatement(ExpressionStatement node)
    {
        if (_shouldRemoveExpression)
        {
            RemoveCurrentNode();
        }

        base.LeaveExpressionStatement(node);
    }
    */

    void ProcessBuiltin(MethodInvocationExpression node, BuiltinFunction builtin)
    {
        // csharp init value type automatically
        if (builtin.FunctionType == BuiltinFunctionType.InitValueType)
        {
            if (node.ParentNode != null && node.ParentNode.NodeType != NodeType.ExpressionStatement)
            {
                node.ParentNode.Replace(node, node.Arguments[0]);
            }
        }
        else if (builtin.FunctionType == BuiltinFunctionType.Eval)
        {
            OnEval(node);
        }
    }

    void OnEval(MethodInvocationExpression node)
    {
        Console.WriteLine(node.LexicalInfo + "|" + node);
        var lambdaBody = new BlockExpression(node.LexicalInfo);

        for (int ix = 0; ix < node.Arguments.Count - 1; ix++)
        {
            Visit(node.Arguments[ix]);
            lambdaBody.Body.Add(node.Arguments[ix]);
        }

        Visit(node.Arguments[-1]);
        lambdaBody.Body.Add(new ReturnStatement(node.Arguments[-1]));

        ReplaceCurrentNode(lambdaBody);
    }



}

}
