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

    public override void OnMethodInvocationExpression(MethodInvocationExpression node)
    {
        var entity = TypeSystemServices.GetEntity(node.Target);
        if (entity.EntityType == EntityType.BuiltinFunction)
        {
            ProcessBuiltin(node, (BuiltinFunction)entity);
        }

        base.OnMethodInvocationExpression(node);
    }

    public override bool EnterExpressionStatement(ExpressionStatement node)
    {
        _shouldRemoveExpression = false;
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

    void ProcessBuiltin(MethodInvocationExpression node, BuiltinFunction builtin)
    {
        // csharp init value type automatically
        if (builtin.FunctionType == BuiltinFunctionType.InitValueType)
        {
            if (node.ParentNode != null && node.ParentNode.NodeType == NodeType.ExpressionStatement)
            {
                _shouldRemoveExpression = true;
            }
        }
        else if (builtin.FunctionType == BuiltinFunctionType.Eval)
        {
            OnEval(node);
        }
    }

    void OnEval(MethodInvocationExpression node)
    {
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
                lambdaBody.Body.Add(declarationStmt);
            }
            else
            {
                lambdaBody.Body.Add(firstAssignment);
            }
        }
        else
        {
            var firstInitObj = node.Arguments[0] as MethodInvocationExpression;
            if (TypeSystemServices.GetEntity(firstInitObj.Target) == BuiltinFunction.InitValueType)
            {
                var valueTypeRef = (ReferenceExpression)firstInitObj.Arguments[0];
                var localReference = valueTypeRef.Entity as InternalLocal;
                Trace.Assert(localReference != null, "initobj on non private local");
                var declaration = new Declaration(localReference.Name, new SimpleTypeReference(localReference.Type.DisplayName()));
                var declarationStmt = new DeclarationStatement();
                declarationStmt.Declaration = declaration;
                lambdaBody.Body.Add(declarationStmt);
            }
            else
            {
                lambdaBody.Body.Add(node.Arguments[0]);
            }
        }

        for (int ix = 1; ix < node.Arguments.Count - 1; ix++)
        {
            lambdaBody.Body.Add(node.Arguments[ix]);
        }

        var lastArgument = node.Arguments[-1];
        lambdaBody.Body.Add(new ReturnStatement(node.Arguments[-1]));

        // create a inplace labmda invocation like:
        // ((Func<bool>)(() => { int a = 2; return true; }))();
        var funcTypeRef = new GenericTypeReference("System.Func", new SimpleTypeReference(lastArgument.ExpressionType.DisplayName()));
        var castExpr = new CastExpression(lambdaBody, funcTypeRef);
        var invokeExpr = new MethodInvocationExpression(castExpr);
        ReplaceCurrentNode(invokeExpr);

        Visit(lambdaBody);
    }



}

}
