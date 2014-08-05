using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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

    public void OnArrayLiteralExpression(ArrayLiteralExpression node)
    {
        throw new NotImplementedException();
    }

    public void OnArrayTypeReference(ArrayTypeReference node)
    {
        throw new NotImplementedException();
    }

    public void OnAttribute(Attribute node)
    {
        throw new NotImplementedException();
    }

    public void OnBinaryExpression(BinaryExpression node)
    {
        throw new NotImplementedException();
    }

    public void OnBlock(Block node)
    {
        throw new NotImplementedException();
    }

    public void OnBlockExpression(BlockExpression node)
    {
        throw new NotImplementedException();
    }

    public void OnBoolLiteralExpression(BoolLiteralExpression node)
    {
        throw new NotImplementedException();
    }

    public void OnBreakStatement(BreakStatement node)
    {
        throw new NotImplementedException();
    }

    public void OnCallableDefinition(CallableDefinition node)
    {
        throw new NotImplementedException();
    }

    public void OnCallableTypeReference(CallableTypeReference node)
    {
        throw new NotImplementedException();
    }

    public void OnCastExpression(CastExpression node)
    {
        throw new NotImplementedException();
    }

    public void OnCharLiteralExpression(CharLiteralExpression node)
    {
        throw new NotImplementedException();
    }

    public void OnClassDefinition(ClassDefinition node)
    {
        throw new NotImplementedException();
    }

    public void OnCollectionInitializationExpression(CollectionInitializationExpression node)
    {
        throw new NotImplementedException();
    }

    public void OnCompileUnit(CompileUnit node)
    {
        throw new NotImplementedException();
    }

    public void OnConditionalExpression(ConditionalExpression node)
    {
        throw new NotImplementedException();
    }

    public void OnConstructor(Constructor node)
    {
        throw new NotImplementedException();
    }

    public void OnContinueStatement(ContinueStatement node)
    {
        throw new NotImplementedException();
    }

    public void OnCustomExpression(CustomExpression node)
    {
        throw new NotImplementedException();
    }

    public void OnCustomStatement(CustomStatement node)
    {
        throw new NotImplementedException();
    }

    public void OnDeclaration(Declaration node)
    {
        throw new NotImplementedException();
    }

    public void OnDeclarationStatement(DeclarationStatement node)
    {
        throw new NotImplementedException();
    }

    public void OnDestructor(Destructor node)
    {
        throw new NotImplementedException();
    }

    public void OnDoubleLiteralExpression(DoubleLiteralExpression node)
    {
        throw new NotImplementedException();
    }

    public void OnEnumDefinition(EnumDefinition node)
    {
        throw new NotImplementedException();
    }

    public void OnEnumMember(EnumMember node)
    {
        throw new NotImplementedException();
    }

    public void OnEvent(Event node)
    {
        throw new NotImplementedException();
    }

    public void OnExceptionHandler(ExceptionHandler node)
    {
        throw new NotImplementedException();
    }

    public void OnExplicitMemberInfo(ExplicitMemberInfo node)
    {
        throw new NotImplementedException();
    }

    public void OnExpressionInterpolationExpression(ExpressionInterpolationExpression node)
    {
        throw new NotImplementedException();
    }

    public void OnExpressionPair(ExpressionPair node)
    {
        throw new NotImplementedException();
    }

    public void OnExpressionStatement(ExpressionStatement node)
    {
        throw new NotImplementedException();
    }

    public void OnExtendedGeneratorExpression(ExtendedGeneratorExpression node)
    {
        throw new NotImplementedException();
    }

    public void OnField(Field node)
    {
        throw new NotImplementedException();
    }

    public void OnForStatement(ForStatement node)
    {
        throw new NotImplementedException();
    }

    public void OnGeneratorExpression(GeneratorExpression node)
    {
        throw new NotImplementedException();
    }

    public void OnGenericParameterDeclaration(GenericParameterDeclaration node)
    {
        throw new NotImplementedException();
    }

    public void OnGenericReferenceExpression(GenericReferenceExpression node)
    {
        throw new NotImplementedException();
    }

    public void OnGenericTypeDefinitionReference(GenericTypeDefinitionReference node)
    {
        throw new NotImplementedException();
    }

    public void OnGenericTypeReference(GenericTypeReference node)
    {
        throw new NotImplementedException();
    }

    public void OnGotoStatement(GotoStatement node)
    {
        throw new NotImplementedException();
    }

    public void OnHashLiteralExpression(HashLiteralExpression node)
    {
        throw new NotImplementedException();
    }

    public void OnIfStatement(IfStatement node)
    {
        throw new NotImplementedException();
    }

    public void OnImport(Import node)
    {
        throw new NotImplementedException();
    }

    public void OnIntegerLiteralExpression(IntegerLiteralExpression node)
    {
        throw new NotImplementedException();
    }

    public void OnInterfaceDefinition(InterfaceDefinition node)
    {
        throw new NotImplementedException();
    }

    public void OnLabelStatement(LabelStatement node)
    {
        throw new NotImplementedException();
    }

    public void OnListLiteralExpression(ListLiteralExpression node)
    {
        throw new NotImplementedException();
    }

    public void OnLocal(Local node)
    {
        throw new NotImplementedException();
    }

    public void OnMacroStatement(MacroStatement node)
    {
        throw new NotImplementedException();
    }

    public void OnMemberReferenceExpression(MemberReferenceExpression node)
    {
        throw new NotImplementedException();
    }

    public void OnMethod(Method node)
    {
        throw new NotImplementedException();
    }

    public void OnMethodInvocationExpression(MethodInvocationExpression node)
    {
        throw new NotImplementedException();
    }

    public void OnModule(Module node)
    {
        WriteLine(node.Name);
    }

    public void OnNamespaceDeclaration(NamespaceDeclaration node)
    {
        throw new NotImplementedException();
    }

    public void OnNullLiteralExpression(NullLiteralExpression node)
    {
        throw new NotImplementedException();
    }

    public void OnOmittedExpression(OmittedExpression node)
    {
        throw new NotImplementedException();
    }

    public void OnParameterDeclaration(ParameterDeclaration node)
    {
        throw new NotImplementedException();
    }

    public void OnProperty(Property node)
    {
        throw new NotImplementedException();
    }

    public void OnQuasiquoteExpression(QuasiquoteExpression node)
    {
        throw new NotImplementedException();
    }

    public void OnRELiteralExpression(RELiteralExpression node)
    {
        throw new NotImplementedException();
    }

    public void OnRaiseStatement(RaiseStatement node)
    {
        throw new NotImplementedException();
    }

    public void OnReferenceExpression(ReferenceExpression node)
    {
        throw new NotImplementedException();
    }

    public void OnReturnStatement(ReturnStatement node)
    {
        throw new NotImplementedException();
    }

    public void OnSelfLiteralExpression(SelfLiteralExpression node)
    {
        throw new NotImplementedException();
    }

    public void OnSimpleTypeReference(SimpleTypeReference node)
    {
        throw new NotImplementedException();
    }

    public void OnSlice(Slice node)
    {
        throw new NotImplementedException();
    }

    public void OnSlicingExpression(SlicingExpression node)
    {
        throw new NotImplementedException();
    }

    public void OnSpliceExpression(SpliceExpression node)
    {
        throw new NotImplementedException();
    }

    public void OnSpliceMemberReferenceExpression(SpliceMemberReferenceExpression node)
    {
        throw new NotImplementedException();
    }

    public void OnSpliceParameterDeclaration(SpliceParameterDeclaration node)
    {
        throw new NotImplementedException();
    }

    public void OnSpliceTypeDefinitionBody(SpliceTypeDefinitionBody node)
    {
        throw new NotImplementedException();
    }

    public void OnSpliceTypeMember(SpliceTypeMember node)
    {
        throw new NotImplementedException();
    }

    public void OnSpliceTypeReference(SpliceTypeReference node)
    {
        throw new NotImplementedException();
    }

    public void OnStatementModifier(StatementModifier node)
    {
        throw new NotImplementedException();
    }

    public void OnStatementTypeMember(StatementTypeMember node)
    {
        throw new NotImplementedException();
    }

    public void OnStringLiteralExpression(StringLiteralExpression node)
    {
        throw new NotImplementedException();
    }

    public void OnStructDefinition(StructDefinition node)
    {
        throw new NotImplementedException();
    }

    public void OnSuperLiteralExpression(SuperLiteralExpression node)
    {
        throw new NotImplementedException();
    }

    public void OnTimeSpanLiteralExpression(TimeSpanLiteralExpression node)
    {
        throw new NotImplementedException();
    }

    public void OnTryCastExpression(TryCastExpression node)
    {
        throw new NotImplementedException();
    }

    public void OnTryStatement(TryStatement node)
    {
        throw new NotImplementedException();
    }

    public void OnTypeMemberStatement(TypeMemberStatement node)
    {
        throw new NotImplementedException();
    }

    public void OnTypeofExpression(TypeofExpression node)
    {
        throw new NotImplementedException();
    }

    public void OnUnaryExpression(UnaryExpression node)
    {
        throw new NotImplementedException();
    }

    public void OnUnlessStatement(UnlessStatement node)
    {
        throw new NotImplementedException();
    }

    public void OnUnpackStatement(UnpackStatement node)
    {
        throw new NotImplementedException();
    }

    public void OnWhileStatement(WhileStatement node)
    {
        throw new NotImplementedException();
    }

    public void OnYieldStatement(YieldStatement node)
    {
        throw new NotImplementedException();
    }
}


}
