using System;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Boo.Lang.Compiler.Ast;
using Boo.Lang.Compiler.TypeSystem;
using Boo.Lang.Compiler.Steps;
using Boo.Lang.Compiler.TypeSystem.Internal;
using System.Text.RegularExpressions;

namespace US2CS
{


class CSharpRewriteTransformer : AbstractTransformerCompilerStep
{
    private TypeDefinition _currentDefinition;
    private Method _currentMethod;
    private CodeSerializer _serializer;
    private Regex _stripGenericPat;

    class BreakDummyExpression : Expression
    {
        public override NodeType NodeType
        {
            get { throw new NotImplementedException(); }
        }

        public override void Accept(IAstVisitor visitor)
        {
            // pass
        }
    }

    public static readonly Expression YieldBreakExpression = new BreakDummyExpression();

    public CSharpRewriteTransformer()
    {
        _serializer = new CodeSerializer();
        _stripGenericPat = new Regex(@"(.+)\.<.+>$");
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

    // rewrite constructor name
    public override void OnConstructor(Constructor node)
    {
        node.Name = _currentDefinition.Name;

        if (node.IsStatic) node.Modifiers &= ~TypeMemberModifiers.Private;

        base.OnConstructor(node);
    }

    public override void OnMethodInvocationExpression(MethodInvocationExpression node)
    {
        var reference = node.Target as ReferenceExpression;
        if (reference == null || reference.Entity == null || reference.Entity.EntityType != EntityType.Method)
        {
            base.OnMethodInvocationExpression(node);
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
        else if (method.Name.StartsWith("set_"))
        {
            UnresolveSimpleSetter(node, method);
        }

        base.OnMethodInvocationExpression(node);
    }

    private bool UnresolveSimpleSetter(MethodInvocationExpression node, IMethod method)
    {
        var setterName = method.Name.Substring(4);
        Trace.Assert(setterName == "Item", "shouldn't have other setters");

        var memberRefExpression = node.Target as MemberReferenceExpression;
        Trace.Assert(memberRefExpression != null, "set_Item lhs should be member ref");
        var slicingExpression = new SlicingExpression();
        slicingExpression.Target = memberRefExpression.Target;
        for (var ix = 0; ix < node.Arguments.Count - 1; ix++)
        {
            slicingExpression.Indices.Add(new Slice(node.Arguments[ix]));
        }

        var assignment = CodeBuilder.CreateAssignment(slicingExpression, node.Arguments[-1]);
        ReplaceCurrentNode(assignment);

        return true;
    }
    
    private bool UnresolveSimpleGetter(MethodInvocationExpression node, IMethod method)
    {
        var getterName = method.Name.Substring(4);
        Expression replacementExpression = null;

        if (getterName == "Item")
        {
            var memberRefExpression = node.Target as MemberReferenceExpression;
            Trace.Assert(memberRefExpression != null, "get_Index lhs should be member ref.");
            var slicingExpression = new SlicingExpression();
            slicingExpression.Target = memberRefExpression.Target;
            foreach (var argument in node.Arguments)
            {
                slicingExpression.Indices.Add(new Slice(argument));
            }
            replacementExpression = slicingExpression;
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

    // rewrite local names
    public override void OnReferenceExpression(ReferenceExpression node)
    {
        node.Name = node.Name.Replace("$", "tmp");
        base.OnReferenceExpression(node);

        // rewrite type literals
        if (GetExpressionType(node) == TypeSystemServices.TypeType)
        {
            var type = (IType)(node.Entity);
            // hack to get only the non generic part
            var typeName = type.DisplayName();
            var match = _stripGenericPat.Match(typeName);
            if (match.Success) typeName = match.Groups[1].Value;
            var typeofExpr = new TypeofExpression(node.LexicalInfo, new SimpleTypeReference(typeName));
            ReplaceCurrentNode(typeofExpr);
        }
    }

    public override void OnLocal(Local node)
    {
        node.Name = node.Name.Replace("$", "tmp");
        base.OnLocal(node);
    }

    public override void OnDeclaration(Declaration node)
    {
        node.Name = node.Name.Replace("$", "tmp");
        base.OnDeclaration(node);
    }

    // replace return in coroutines with yield break
    public override void OnMethod(Method node)
    {
        _currentMethod = node;

        base.OnMethod(node);
    }

    public override void OnReturnStatement(ReturnStatement node)
    {
        InternalMethod entity = (InternalMethod)(_currentMethod.Entity);
        if (entity != null && entity.IsGenerator)
        {
            var yieldBreak = new YieldStatement(YieldBreakExpression);
            ReplaceCurrentNode(yieldBreak);

        }
        else
        {
            base.OnReturnStatement(node);
        }

    }

}

}
