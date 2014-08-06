using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Diagnostics;
using Boo.Lang.Compiler.Ast;
using Boo.Lang.Compiler.Ast.Visitors;
using Attribute = Boo.Lang.Compiler.Ast.Attribute;

namespace US2CS
{

class CSharpPrinterVisitor : TextEmitter
{
    public CSharpPrinterVisitor(TextWriter writer) : base(writer)
    {
    }

    #region Write helpers from BooPrinterVisitor
    static bool CanBeRepresentedAsQualifiedName(string s)
    {
        foreach (char ch in s)
            if (!char.IsLetterOrDigit(ch) && ch != '_' && ch != '.')
                return false;
        return true;
    }

    public void WriteStringLiteral(string text)
    {
        BooPrinterVisitor.WriteStringLiteral(text, _writer);
    }

    void WriteAttribute(Attribute attribute)
    {
        WriteAttribute(attribute, null);
    }

    void WriteAttribute(Attribute attribute, string prefix)
    {
        WriteIndented("[");
        if (null != prefix)
        {
            Write(prefix);
        }
        Write(attribute.Name);
        if (attribute.Arguments.Count > 0 ||
            attribute.NamedArguments.Count > 0)
        {
            Write("(");
            WriteCommaSeparatedList(attribute.Arguments);
            if (attribute.NamedArguments.Count > 0)
            {
                if (attribute.Arguments.Count > 0)
                {
                    Write(", ");
                }
                WriteCommaSeparatedList(attribute.NamedArguments);
            }
            Write(")");
        }
        Write("]");
    }

    void WriteAttributes(AttributeCollection attributes, bool addNewLines)
    {
        foreach (Boo.Lang.Compiler.Ast.Attribute attribute in attributes)
        {
            Visit(attribute);
            if (addNewLines)
            {
                WriteLine();
            }
            else
            {
                Write(" ");
            }
        }
    }

    
    #endregion

    public override void OnArrayLiteralExpression(ArrayLiteralExpression node)
    {
        throw new NotImplementedException();
    }

    public override void OnArrayTypeReference(ArrayTypeReference node)
    {
        throw new NotImplementedException();
    }

    public override void OnAttribute(Attribute node)
    {
        throw new NotImplementedException();
    }

    public override void OnBinaryExpression(BinaryExpression node)
    {
        throw new NotImplementedException();
    }

    public override void OnBlock(Block node)
    {
        throw new NotImplementedException();
    }

    public override void OnBlockExpression(BlockExpression node)
    {
        throw new NotImplementedException();
    }

    public override void OnBoolLiteralExpression(BoolLiteralExpression node)
    {
        throw new NotImplementedException();
    }

    public override void OnBreakStatement(BreakStatement node)
    {
        throw new NotImplementedException();
    }

    public override void OnCallableDefinition(CallableDefinition node)
    {
        throw new NotImplementedException();
    }

    public override void OnCallableTypeReference(CallableTypeReference node)
    {
        throw new NotImplementedException();
    }

    public override void OnCastExpression(CastExpression node)
    {
        throw new NotImplementedException();
    }

    public override void OnCharLiteralExpression(CharLiteralExpression node)
    {
        throw new NotImplementedException();
    }

    public override void OnClassDefinition(ClassDefinition node)
    {
        throw new NotImplementedException();
    }

    public override void OnCollectionInitializationExpression(CollectionInitializationExpression node)
    {
        throw new NotImplementedException();
    }

    public override void OnCompileUnit(CompileUnit node)
    {
        throw new NotImplementedException();
    }

    public override void OnConditionalExpression(ConditionalExpression node)
    {
        throw new NotImplementedException();
    }

    public override void OnConstructor(Constructor node)
    {
        throw new NotImplementedException();
    }

    public override void OnContinueStatement(ContinueStatement node)
    {
        throw new NotImplementedException();
    }

    public override void OnCustomExpression(CustomExpression node)
    {
        throw new NotImplementedException();
    }

    public override void OnCustomStatement(CustomStatement node)
    {
        throw new NotImplementedException();
    }

    public override void OnDeclaration(Declaration node)
    {
        throw new NotImplementedException();
    }

    public override void OnDeclarationStatement(DeclarationStatement node)
    {
        throw new NotImplementedException();
    }

    public override void OnDestructor(Destructor node)
    {
        throw new NotImplementedException();
    }

    public override void OnDoubleLiteralExpression(DoubleLiteralExpression node)
    {
        throw new NotImplementedException();
    }

    public override void OnEnumDefinition(EnumDefinition node)
    {
        throw new NotImplementedException();
    }

    public override void OnEnumMember(EnumMember node)
    {
        throw new NotImplementedException();
    }

    public override void OnEvent(Event node)
    {
        throw new NotImplementedException();
    }

    public override void OnExceptionHandler(ExceptionHandler node)
    {
        throw new NotImplementedException();
    }

    public override void OnExplicitMemberInfo(ExplicitMemberInfo node)
    {
        throw new NotImplementedException();
    }

    public override void OnExpressionInterpolationExpression(ExpressionInterpolationExpression node)
    {
        throw new NotImplementedException();
    }

    public override void OnExpressionPair(ExpressionPair node)
    {
        throw new NotImplementedException();
    }

    public override void OnExpressionStatement(ExpressionStatement node)
    {
        throw new NotImplementedException();
    }

    public override void OnExtendedGeneratorExpression(ExtendedGeneratorExpression node)
    {
        throw new NotImplementedException();
    }

    public override void OnField(Field node)
    {
        throw new NotImplementedException();
    }

    public override void OnForStatement(ForStatement node)
    {
        throw new NotImplementedException();
    }

    public override void OnGeneratorExpression(GeneratorExpression node)
    {
        throw new NotImplementedException();
    }

    public override void OnGenericParameterDeclaration(GenericParameterDeclaration node)
    {
        throw new NotImplementedException();
    }

    public override void OnGenericReferenceExpression(GenericReferenceExpression node)
    {
        throw new NotImplementedException();
    }

    public override void OnGenericTypeDefinitionReference(GenericTypeDefinitionReference node)
    {
        throw new NotImplementedException();
    }

    public override void OnGenericTypeReference(GenericTypeReference node)
    {
        throw new NotImplementedException();
    }

    public override void OnGotoStatement(GotoStatement node)
    {
        throw new NotImplementedException();
    }

    public override void OnHashLiteralExpression(HashLiteralExpression node)
    {
        throw new NotImplementedException();
    }

    public override void OnIfStatement(IfStatement node)
    {
        throw new NotImplementedException();
    }

    public override void OnImport(Import node)
    {
        Write("using");
        Write(" {0}", node.Namespace);

        if (node.Alias != null)
        {
            Write(" {0} = ", node.Alias.Name);
        }

        if (node.AssemblyReference != null)
        {
            Write(" from ");
            if (CanBeRepresentedAsQualifiedName(node.AssemblyReference.Name))
            {
                Write(node.AssemblyReference.Name);
            }
            else
            {
                WriteStringLiteral(node.AssemblyReference.Name);
            }
        }

        Trace.Assert(node.Expression.NodeType != NodeType.MethodInvocationExpression, "shouldn't get import method invoke");

        Write(";");
        WriteLine();
    }

    public override void OnIntegerLiteralExpression(IntegerLiteralExpression node)
    {
        throw new NotImplementedException();
    }

    public override void OnInterfaceDefinition(InterfaceDefinition node)
    {
        throw new NotImplementedException();
    }

    public override void OnLabelStatement(LabelStatement node)
    {
        throw new NotImplementedException();
    }

    public override void OnListLiteralExpression(ListLiteralExpression node)
    {
        throw new NotImplementedException();
    }

    public override void OnLocal(Local node)
    {
        throw new NotImplementedException();
    }

    public override void OnMacroStatement(MacroStatement node)
    {
        throw new NotImplementedException();
    }

    public override void OnMemberReferenceExpression(MemberReferenceExpression node)
    {
        throw new NotImplementedException();
    }

    public override void OnMethod(Method node)
    {
        throw new NotImplementedException();
    }

    public override void OnMethodInvocationExpression(MethodInvocationExpression node)
    {
        throw new NotImplementedException();
    }

    public override void OnModule(Module node)
    {
        Visit(node.Namespace);

        if (node.Imports.Count > 0)
        {
            Visit(node.Imports);
            WriteLine();
        }

        foreach (var member in node.Members)
        {
            Write("// {0}", member.Name);
            //Visit(member);
            WriteLine();
        }

        //Trace.Assert(node.Globals == null, "module still contains globals.");

        foreach (var attribute in node.Attributes)
            WriteAttribute(attribute, "module: ");

        foreach (var attribute in node.AssemblyAttributes)
            WriteAttribute(attribute, "assembly: ");
    }

    public override void OnNamespaceDeclaration(NamespaceDeclaration node)
    {
        throw new NotImplementedException();
    }

    public override void OnNullLiteralExpression(NullLiteralExpression node)
    {
        throw new NotImplementedException();
    }

    public override void OnOmittedExpression(OmittedExpression node)
    {
        throw new NotImplementedException();
    }

    public override void OnParameterDeclaration(ParameterDeclaration node)
    {
        throw new NotImplementedException();
    }

    public override void OnProperty(Property node)
    {
        throw new NotImplementedException();
    }

    public override void OnQuasiquoteExpression(QuasiquoteExpression node)
    {
        throw new NotImplementedException();
    }

    public override void OnRELiteralExpression(RELiteralExpression node)
    {
        throw new NotImplementedException();
    }

    public override void OnRaiseStatement(RaiseStatement node)
    {
        throw new NotImplementedException();
    }

    public override void OnReferenceExpression(ReferenceExpression node)
    {
        throw new NotImplementedException();
    }

    public override void OnReturnStatement(ReturnStatement node)
    {
        throw new NotImplementedException();
    }

    public override void OnSelfLiteralExpression(SelfLiteralExpression node)
    {
        throw new NotImplementedException();
    }

    public override void OnSimpleTypeReference(SimpleTypeReference node)
    {
        throw new NotImplementedException();
    }

    public override void OnSlice(Slice node)
    {
        throw new NotImplementedException();
    }

    public override void OnSlicingExpression(SlicingExpression node)
    {
        throw new NotImplementedException();
    }

    public override void OnSpliceExpression(SpliceExpression node)
    {
        throw new NotImplementedException();
    }

    public override void OnSpliceMemberReferenceExpression(SpliceMemberReferenceExpression node)
    {
        throw new NotImplementedException();
    }

    public override void OnSpliceParameterDeclaration(SpliceParameterDeclaration node)
    {
        throw new NotImplementedException();
    }

    public override void OnSpliceTypeDefinitionBody(SpliceTypeDefinitionBody node)
    {
        throw new NotImplementedException();
    }

    public override void OnSpliceTypeMember(SpliceTypeMember node)
    {
        throw new NotImplementedException();
    }

    public override void OnSpliceTypeReference(SpliceTypeReference node)
    {
        throw new NotImplementedException();
    }

    public override void OnStatementModifier(StatementModifier node)
    {
        throw new NotImplementedException();
    }

    public override void OnStatementTypeMember(StatementTypeMember node)
    {
        throw new NotImplementedException();
    }

    public override void OnStringLiteralExpression(StringLiteralExpression node)
    {
        throw new NotImplementedException();
    }

    public override void OnStructDefinition(StructDefinition node)
    {
        throw new NotImplementedException();
    }

    public override void OnSuperLiteralExpression(SuperLiteralExpression node)
    {
        throw new NotImplementedException();
    }

    public override void OnTimeSpanLiteralExpression(TimeSpanLiteralExpression node)
    {
        throw new NotImplementedException();
    }

    public override void OnTryCastExpression(TryCastExpression node)
    {
        throw new NotImplementedException();
    }

    public override void OnTryStatement(TryStatement node)
    {
        throw new NotImplementedException();
    }

    public override void OnTypeMemberStatement(TypeMemberStatement node)
    {
        throw new NotImplementedException();
    }

    public override void OnTypeofExpression(TypeofExpression node)
    {
        throw new NotImplementedException();
    }

    public override void OnUnaryExpression(UnaryExpression node)
    {
        throw new NotImplementedException();
    }

    public override void OnUnlessStatement(UnlessStatement node)
    {
        throw new NotImplementedException();
    }

    public override void OnUnpackStatement(UnpackStatement node)
    {
        throw new NotImplementedException();
    }

    public override void OnWhileStatement(WhileStatement node)
    {
        throw new NotImplementedException();
    }

    public override void OnYieldStatement(YieldStatement node)
    {
        throw new NotImplementedException();
    }
}


}
