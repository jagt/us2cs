﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Boo.Lang.Compiler.Ast;
using Boo.Lang.Compiler.TypeSystem;
using Boo.Lang.Compiler.Steps;

namespace US2CS
{

class CSharpRewriteTransformer : AbstractTransformerCompilerStep
{
    private TypeDefinition _currentDefinition;
    private CodeSerializer _serializer;

    public CSharpRewriteTransformer()
    {
        _serializer = new CodeSerializer();
    }

    public override void Run()
    {
        if (Errors.Count > 0) return;
        Visit(CompileUnit);
    }


    public override void OnClassDefinition(ClassDefinition node)
    {
        _currentDefinition = node;
        base.OnClassDefinition(node);
    }

    public override void OnStructDefinition(StructDefinition node)
    {
        _currentDefinition = node;
        base.OnStructDefinition(node);
    }

    public override void OnConstructor(Constructor node)
    {
        node.Name = _currentDefinition.Name;
    }


    public override void OnMethodInvocationExpression(MethodInvocationExpression node)
    {
        var reference = node.Target as ReferenceExpression;
        if (reference == null || reference.Entity == null || reference.Entity.EntityType != EntityType.Method)
        {
            return;
        }

        var method = (IMethod)reference.Entity;
        if (method.Name.StartsWith("op_"))
        {
            UnresolveOperatorOverload(node, method);
        }
        else if (method.Name.StartsWith("get_"))
        {
            UnresolveSimpleGetter(node, method);
        }

        base.OnMethodInvocationExpression(node);
    }

    private bool UnresolveSimpleGetter(MethodInvocationExpression node, IMethod method)
    {
        var getterName = method.Name.Substring(4);
        Expression replacementExpression = null;

        if (getterName == "Item")
        {
            replacementExpression = CodeBuilder.CreateSlicing(node.Arguments[0], 0);
        }
        else if (getterName == "HasValue")
        {
            // FIXME untested
            replacementExpression = CodeBuilder.CreateMemberReference((IMember)NameResolutionService.ResolveMember(method.DeclaringType, "HasValue", EntityType.Property));
            Trace.Assert(node.Arguments.Count == 0, "didn't expect non conventional HasValue getter.");
        }
        else
        {
            Trace.Fail("unknown getter: " + getterName);
        }

        if (replacementExpression != null)
        {
            node.ParentNode.Replace(node, replacementExpression);
            return true;
        }

        return true;
    }

    private bool UnresolveOperatorOverload(MethodInvocationExpression node, IMethod method)
    {
        var operatorName = method.Name.Substring(3);
        Expression replacementExpression = null;

        if (operatorName == "Implicit")
        {
            var castToTypeRef = CodeBuilder.CreateTypeReference(method.DeclaringType);
            replacementExpression = new CastExpression(node.LexicalInfo, node.Arguments[0], castToTypeRef);
        }
        else if (node.Arguments.Count == 1)
        {
            var unaryType = (UnaryOperatorType)Enum.Parse(typeof(UnaryOperatorType), operatorName);
            replacementExpression = new UnaryExpression(node.LexicalInfo, unaryType, node.Arguments[0]);
        }
        else if (node.Arguments.Count == 2)
        {
            var binaryType = (BinaryOperatorType)Enum.Parse(typeof(BinaryOperatorType), operatorName);
            replacementExpression = new BinaryExpression(node.LexicalInfo, binaryType, node.Arguments[0], node.Arguments[1]);
        }
        else
        {
            Trace.Fail("operator overload method should have more than 1 arg.");
        }

        if (replacementExpression != null)
        {
            node.ParentNode.Replace(node, replacementExpression);
            return true;
        }

        return false;
    }

}

}