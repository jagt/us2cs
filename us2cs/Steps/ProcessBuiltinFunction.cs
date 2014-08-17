using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Boo.Lang.Compiler.TypeSystem;
using Boo.Lang.Compiler.Steps;
using Boo.Lang.Compiler.Ast;
using Boo.Lang.Compiler.TypeSystem.Internal;

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
        var firstAssignment = node.Arguments[0] as BinaryExpression;
        if (firstAssignment != null
            && firstAssignment.Operator == BinaryOperatorType.Assign
            && firstAssignment.Left is ReferenceExpression)
        {
            var lhsReference = (ReferenceExpression)firstAssignment.Left;
            var localReference = lhsReference.Entity as InternalLocal;
            if (localReference != null)
            {
                var declaration = new Declaration(localReference.Name, new SimpleTypeReference(localReference.Type.DisplayName()));
                var declarationStmt = new DeclarationStatement(declaration, firstAssignment.Right);
                Console.WriteLine(declarationStmt);
                Visit(firstAssignment.Right);
                lambdaBody.Body.Add(declarationStmt);
            }
            else
            {
                Visit(node.Arguments[0]);
                lambdaBody.Body.Add(firstAssignment);
            }
        }
        else
        {
            Visit(node.Arguments[0]);
            lambdaBody.Body.Add(node.Arguments[0]);
        }

        for (int ix = 1; ix < node.Arguments.Count - 1; ix++)
        {
            Visit(node.Arguments[ix]);
            lambdaBody.Body.Add(node.Arguments[ix]);
        }

        var lastArgument = node.Arguments[-1];
        Visit(node.Arguments[-1]);
        lambdaBody.Body.Add(new ReturnStatement(node.Arguments[-1]));

        // create a inplace labmda invocation like:
        // ((Func<bool>)(() => { int a = 2; return true; }))();
        var funcTypeRef = new GenericTypeReference("Func", new SimpleTypeReference(lastArgument.ExpressionType.DisplayName()));
        var castExpr = new CastExpression(lambdaBody, funcTypeRef);
        var invokeExpr = new MethodInvocationExpression(castExpr);
        ReplaceCurrentNode(invokeExpr);
    }



}

}
