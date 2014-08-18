using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Diagnostics;
using Boo.Lang.Compiler.Ast;
using Boo.Lang.Compiler.Ast.Visitors;
using Attribute = Boo.Lang.Compiler.Ast.Attribute;
using System.Globalization;
using Boo.Lang.Compiler.TypeSystem.Internal;
using Boo.Lang.Compiler.TypeSystem;
using MethodInfo = System.Reflection.MethodInfo;

namespace US2CS
{

// modified on BooPrinterVisitor
class CSharpPrinterVisitor : TextEmitter
{
    private int _isStatementInline;
    private CodeSerializer _serializer;
    private Method _currentMethod;

    public CSharpPrinterVisitor(TextWriter writer) : base(writer)
    {
        _isStatementInline = 0;
        _serializer = new CodeSerializer();
    }

    public void Print(CompileUnit ast)
    {
        OnCompileUnit(ast);
    }

    #region Write helpers from BooPrinterVisitor
    static bool CanBeRepresentedAsQualifiedName(string s)
    {
        foreach (char ch in s)
            if (!char.IsLetterOrDigit(ch) && ch != '_' && ch != '.')
                return false;
        return true;
    }

    private static bool IsInterfaceMember(TypeMember node)
    {
        return node.ParentNode != null && node.ParentNode.NodeType == NodeType.InterfaceDefinition;
    }

    bool NeedParensAround(Expression e)
    {
        if (e.ParentNode == null) return false;
        switch (e.ParentNode.NodeType)
        {
            case NodeType.ExpressionStatement:
                return false;
        }

        return true;
    }


    void WriteStringLiteral(string text)
    {
        Write("\"");
        BooPrinterVisitor.WriteStringLiteralContents(text, _writer, false);
        Write("\"");
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

    void WriteKeyword(string keyword)
    {
        Write(keyword);
    }

    void WriteComma()
    {
        Write(";");
    }

    void WriteModifiers(TypeMember member)
    {
        WriteIndented();
        if (member.IsPublic)
            WriteKeyword("public ");
        else if (member.IsProtected)
            WriteKeyword("protected ");
        else if (member.IsPrivate)
            WriteKeyword("private ");
        else if (member.IsInternal)
            WriteKeyword("internal ");
        if (member.IsStatic)
            WriteKeyword("static ");
        else if (member.IsOverride)
            WriteKeyword("override ");
        else if (member.IsModifierSet(TypeMemberModifiers.Virtual))
            WriteKeyword("virtual ");
        else if (member.IsModifierSet(TypeMemberModifiers.Abstract))
            WriteKeyword("abstract ");
        if (member.IsFinal)
            WriteKeyword("final ");
        if (member.IsNew)
            WriteKeyword("new ");
        if (member.HasTransientModifier)
            WriteKeyword("transient ");
        if (member.IsPartial) // c# partial needs to appear right before class/struct
            WriteKeyword("partial ");
    }

    private void WriteOptionalModifiers(TypeMember node)
    {
        if (IsInterfaceMember(node))
        {
            WriteIndented();
        }
        else
        {
            WriteModifiers(node);
        }
    }

    void WriteGenericParameterList(GenericParameterDeclarationCollection items)
    {
        Write("<");
        WriteCommaSeparatedList(items);
        Write(">");
    }

    void WriteGenericArguments(TypeReferenceCollection items)
    {
        Write("<");
        WriteCommaSeparatedList(items);
        Write(">");
    }

    public void WriteBlock(Block b)
    {
        BeginBlock();
        Visit(b.Statements);
        EndBlock();
    }

    void BeginBlock()
    {
        WriteLine();
        WriteIndented("{{");
        WriteLine();
        Indent();
    }

    void EndBlock()
    {
        Dedent();
        WriteIndented("}}");
        WriteLine();
    }

    void WriteTypeReference(TypeReference tr)
    {
        //Trace.Assert(tr != null, "typereference shouldn't be null");
        Visit(tr);
    }

    void WriteTypeDefinition(string keyword, TypeDefinition td)
    {
        WriteAttributes(td.Attributes, true);
        WriteModifiers(td);
        WriteIndented();
        WriteKeyword(keyword);
        Write(" ");

        Write(td.Name);
        if (td.GenericParameters.Count != 0)
        {
            WriteGenericParameterList(td.GenericParameters);
        }

        if (td.BaseTypes.Count > 0)
        {
            Write(" : ");
            WriteCommaSeparatedList(td.BaseTypes);
        }

        BeginBlock();

        if (td.Members.Count > 0)
        {
            foreach (TypeMember member in td.Members)
            {
                Visit(member);
            }
        }

        EndBlock();

        Trace.Assert(td.ParentNode as SpliceTypeMember == null, "shouldn't get splice type.");
    }

    void WriteOperator(string text)
    {
        Write(text);
    }

    public static string GetBinaryOperatorText(BinaryOperatorType op)
    {
        switch (op)
        {
            case BinaryOperatorType.Assign:
                return "=";

            case BinaryOperatorType.Match:
                return "=~";

            case BinaryOperatorType.NotMatch:
                return "!~";

            case BinaryOperatorType.Equality:
                return "==";

            case BinaryOperatorType.Inequality:
                return "!=";

            case BinaryOperatorType.Addition:
                return "+";

            case BinaryOperatorType.InPlaceAddition:
                return "+=";

            case BinaryOperatorType.InPlaceBitwiseAnd:
                return "&=";

            case BinaryOperatorType.InPlaceBitwiseOr:
                return "|=";

            case BinaryOperatorType.InPlaceSubtraction:
                return "-=";

            case BinaryOperatorType.InPlaceMultiply:
                return "*=";

            case BinaryOperatorType.InPlaceModulus:
                return "%=";

            case BinaryOperatorType.InPlaceExclusiveOr:
                return "^=";

            case BinaryOperatorType.InPlaceDivision:
                return "/=";

            case BinaryOperatorType.Subtraction:
                return "-";

            case BinaryOperatorType.Multiply:
                return "*";

            case BinaryOperatorType.Division:
                return "/";

            case BinaryOperatorType.GreaterThan:
                return ">";

            case BinaryOperatorType.GreaterThanOrEqual:
                return ">=";

            case BinaryOperatorType.LessThan:
                return "<";

            case BinaryOperatorType.LessThanOrEqual:
                return "<=";

            case BinaryOperatorType.Modulus:
                return "%";

            case BinaryOperatorType.ReferenceEquality:
                return "==";

            case BinaryOperatorType.ReferenceInequality:
                return "!=";

            case BinaryOperatorType.TypeTest:
                return "is";

            case BinaryOperatorType.Or:
                return "||";

            case BinaryOperatorType.And:
                return "&&";

            case BinaryOperatorType.BitwiseOr:
                return "|";

            case BinaryOperatorType.BitwiseAnd:
                return "&";

            case BinaryOperatorType.ExclusiveOr:
                return "^";

            case BinaryOperatorType.ShiftLeft:
                return "<<";

            case BinaryOperatorType.ShiftRight:
                return ">>";

            case BinaryOperatorType.InPlaceShiftLeft:
                return "<<=";

            case BinaryOperatorType.InPlaceShiftRight:
                return ">>=";

            case BinaryOperatorType.Exponentiation:
            case BinaryOperatorType.Member:
            case BinaryOperatorType.NotMember:
                throw new NotImplementedException(op.ToString());

        }
        throw new NotImplementedException(op.ToString());
    }

    public static string GetUnaryOperatorText(UnaryOperatorType op)
    {
        switch (op)
        {
            case UnaryOperatorType.PostIncrement:
            case UnaryOperatorType.Increment:
                return "++";

            case UnaryOperatorType.PostDecrement:
            case UnaryOperatorType.Decrement:
                return "--";

            case UnaryOperatorType.UnaryNegation:
                return "-";

            case UnaryOperatorType.LogicalNot:
                return "!";

            case UnaryOperatorType.OnesComplement:
                return "~";

            case UnaryOperatorType.AddressOf:
                return "&";

            case UnaryOperatorType.Indirection:
            case UnaryOperatorType.Explode:
                throw new NotImplementedException(op.ToString());
        }
        throw new ArgumentException(op.ToString());
    }


    void WriteParameterList(ParameterDeclarationCollection items)
    {
        WriteParameterList(items, "(", ")");
    }

    void WriteParameterList(ParameterDeclarationCollection items, string st, string ed)
    {
        Write(st);
        int i = 0;
        foreach (ParameterDeclaration item in items)
        {
            if (i > 0)
            {
                Write(", ");
            }
            if (item.IsParamArray)
            {
                WriteKeyword("params ");
            }

            Visit(item);
            ++i;
        }
        Write(ed);
    }

    void WriteImplementationComment(string comment)
    {
        WriteIndented("// impl {0}", comment);
    }

    // a hack to remove super()
    void ProcessSuperConstructor(Method node)
    {
        if (node.Body.Statements.Count == 0) return;
        var baseInvokeStmt = node.Body.Statements[0] as ExpressionStatement;
        var baseInvokeExpr = baseInvokeStmt.Expression as MethodInvocationExpression;
        if (baseInvokeExpr == null) return;
        else if (baseInvokeExpr.Target is SuperLiteralExpression)
        {
            Write(" : ");
            Visit(baseInvokeStmt.Expression);
        }
        node.Body.Statements.Remove(baseInvokeStmt);
    }

    enum CallableType { Constructor, Destructor, Usual }
    void WriteCallableDefinitionHeader(Method node, CallableType ct)
    {
        WriteAttributes(node.Attributes, true);
        WriteOptionalModifiers(node);

        switch (ct)
        {
            case CallableType.Constructor:
                break; // pass
            case CallableType.Destructor:
                Write("~");
                break;
            case CallableType.Usual:
                WriteTypeReference(node.ReturnType);
                Write(" ");
                break;
        }

        IExplicitMember em = node as IExplicitMember;
        if (null != em)
        {
            Visit(em.ExplicitInfo);
        }

        Write(node.Name);
        if (node.GenericParameters.Count > 0)
        {
            WriteGenericParameterList(node.GenericParameters);
        }

        WriteParameterList(node.Parameters);

        if (ct == CallableType.Constructor)
        {
            ProcessSuperConstructor(node);
        }

        Trace.Assert(node.ReturnTypeAttributes.Count == 0, "shouldnt get return type attributes");
    }

    void WriteCallableDefinition(Method node, CallableType ct)
    {
        WriteLine();
        if (node.IsRuntime) WriteImplementationComment("runtime");

        WriteCallableDefinitionHeader(node, ct);
        if (IsInterfaceMember(node))
        {
            WriteLine();
        }
        else
        {
            BeginBlock();
            Visit(node.Locals);
            Visit(node.Body.Statements);
            EndBlock();
        }
    }

    void WriteIfBlock(string keyword, IfStatement ifs)
    {
        WriteIndented();
        WriteKeyword(keyword);
        Write("(");
        Visit(ifs.Condition);
        Write(")");
        WriteBlock(ifs.TrueBlock);
    }

    #endregion

    readonly Dictionary<string, string> RenameTypeMap = new Dictionary<string,string> {
        {"boolean", "bool"},
        {"Regex", "System.Text.RegularExpressions.Regex"},
        {"Function", "Boo.Lang.Callable"},
        {"Number", "double"},
        {"Array", "Unity.Lang.Array"},
        {"String", "string"},
    };

    void WriteProcessedType(SimpleTypeReference node)
    {
        Write(RenameTypeMap.ContainsKey(node.Name) ? RenameTypeMap[node.Name] : node.Name);
    }

    void WriteProcessedType(IType type)
    {
        var rawName = type.DisplayName();
        // a hack to transform List.<int> into List<int>
        rawName = rawName.Replace(".<", "<");
        Write(RenameTypeMap.ContainsKey(rawName) ? RenameTypeMap[rawName] : rawName);
    }

    public override void OnArrayLiteralExpression(ArrayLiteralExpression node)
    {
        Write("new ");
        WriteProcessedType(node.ExpressionType.ElementType);
        Write("[] {");
        for (int i = 0; i < node.Items.Count; i++)
        {
            if (i > 0) Write(", ");
            Visit(node.Items[i]);
        }
        Write("}");
    }

    public override void OnArrayTypeReference(ArrayTypeReference node)
    {
        Visit(node.ElementType);
        if (node.Rank != null && node.Rank.Value >= 1)
        {
            Write("[");
            for (int ix = 1; ix < node.Rank.Value; ix++)
            {
                Write(",");
            }
            Write("]");
        }
    }

    public override void OnAttribute(Attribute node)
    {
        WriteAttribute(node);
    }

    public override void OnBinaryExpression(BinaryExpression node)
    {
        bool needParens = NeedParensAround(node);
        if (needParens) Write("(");

        Visit(node.Left);
        Write(" ");
        Write(GetBinaryOperatorText(node.Operator));
        Write(" ");

        if (node.Operator == BinaryOperatorType.TypeTest)
        {
            // isa rhs is encoded in a typeof expression
            Visit(((TypeofExpression)node.Right).Type);
        }
        else
        {
            Visit(node.Right);
        }

        if (needParens) Write(")");
    }



    public override void OnBlock(Block node)
    {
        // FIXME do nothing?
    }

    public override void OnBlockExpression(BlockExpression node)
    {
        WriteParameterList(node.Parameters);
        // noway C# lambda can specify return type ?
        Write("=>");
        _isStatementInline++;
        Write("{");
        Visit(node.Body.Statements);
        Write("}");
        _isStatementInline--;
    }

    public override void OnBoolLiteralExpression(BoolLiteralExpression node)
    {
        Write(node.Value ? "true" : "false");
    }

    public override void OnBreakStatement(BreakStatement node)
    {
        WriteIndented();
        WriteKeyword("break;");
        WriteLine();
        Trace.Assert(node.Modifier == null, "shouldn't get modifier");
    }

    public override void OnCallableDefinition(CallableDefinition node)
    {
        // BOO specific feature
        throw new NotImplementedException();
    }

    public override void OnCallableTypeReference(CallableTypeReference node)
    {
        // BOO specific feature
        throw new NotImplementedException();
    }

    public override void OnCastExpression(CastExpression node)
    {
        Write("((");
        Visit(node.Type);
        Write(")");
        Write("(");
        Visit(node.Target);
        Write("))");
    }

    public override void OnCharLiteralExpression(CharLiteralExpression node)
    {
        // US don't support char literal
        throw new NotImplementedException();
    }

    public override void OnClassDefinition(ClassDefinition node)
    {
        WriteTypeDefinition("class", node);
    }

    public override void OnCollectionInitializationExpression(CollectionInitializationExpression node)
    {
        Visit(node.Collection);
        Write(" ");
        if (node.Initializer is ListLiteralExpression)
        {
            throw new NotImplementedException();
        }
        else
        {
            Visit(node.Initializer);
        }
    }

    public override void OnConditionalExpression(ConditionalExpression node)
    {
        Write("(");
        Visit(node.Condition);
        Write(" ? ");
        Visit(node.TrueValue);
        Write(" : ");
        Visit(node.FalseValue);
        Write(")");
    }

    public override void OnConstructor(Constructor node)
    {
        WriteCallableDefinition(node, CallableType.Constructor);
    }

    public override void OnContinueStatement(ContinueStatement node)
    {
        WriteIndented();
        WriteKeyword("continue;");
        WriteLine();
        Trace.Assert(node.Modifier == null, "shouldn't get modifier");
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
        WriteTypeReference(node.Type);
        Write(" ");
        Write(node.Name);
    }

    public override void OnDeclarationStatement(DeclarationStatement node)
    {
        WriteIndented();
        Visit(node.Declaration);
        if (node.Initializer != null)
        {
            WriteOperator(" = ");
            Visit(node.Initializer);
        }
        WriteComma();
        if (_isStatementInline == 0) WriteLine();
    }

    public override void OnDestructor(Destructor node)
    {
        WriteCallableDefinition(node, CallableType.Destructor);
    }

    public override void OnDoubleLiteralExpression(DoubleLiteralExpression node)
    {
        Write(node.Value.ToString("########0.0##########", CultureInfo.InvariantCulture));
        if (node.IsSingle)
        {
            Write("f");
        }
    }

    public override void OnEnumDefinition(EnumDefinition node)
    {
        WriteTypeDefinition("enum", node);
    }

    public override void OnEnumMember(EnumMember node)
    {
        // FIXME pretty sure this is broken
        WriteImplementationComment("// enum is broken");
        WriteAttributes(node.Attributes, true);
        WriteIndented(node.Name);
        if (node.Initializer != null)
        {
            WriteOperator(" = ");
            Visit(node.Initializer);
        }
        WriteLine();
    }

    public override void OnEvent(Event node)
    {
        WriteAttributes(node.Attributes, true);
        WriteOptionalModifiers(node);
        WriteKeyword("event ");
        WriteTypeReference(node.Type);
        Write(node.Name);
        WriteLine();
    }

    public override void OnExceptionHandler(ExceptionHandler node)
    {
        WriteIndented();
        WriteKeyword("catch");
        Write("(");
        if ((node.Flags & ExceptionHandlerFlags.Untyped) == ExceptionHandlerFlags.None)
        {
            if ((node.Flags & ExceptionHandlerFlags.Anonymous) == ExceptionHandlerFlags.None)
            {
                Write(" ");
                Visit(node.Declaration);
            }
            else
            {
                WriteTypeReference(node.Declaration.Type);
            }
        }
        else if ((node.Flags & ExceptionHandlerFlags.Anonymous) == ExceptionHandlerFlags.None)
        {
            Write(" ");
            Write(node.Declaration.Name);
        }
        Write(")");

        Trace.Assert((node.Flags & ExceptionHandlerFlags.Filter) != ExceptionHandlerFlags.Filter, "shouldn't get filtered exception handler");
        WriteBlock(node.Block);
        Dedent();

    }

    public override void OnExplicitMemberInfo(ExplicitMemberInfo node)
    {
        Visit(node.InterfaceType);
        Write(".");
    }

    public override void OnExpressionInterpolationExpression(ExpressionInterpolationExpression node)
    {
        // US don't has string interpolation
        throw new NotImplementedException();
    }

    public override void OnExpressionPair(ExpressionPair node)
    {
        // US don't have expression pair (used in list slicing maybe)
        throw new NotImplementedException();
    }

    public override void OnExpressionStatement(ExpressionStatement node)
    {
        WriteIndented();
        Visit(node.Modifier);
        Visit(node.Expression);
        WriteComma();
        if (_isStatementInline == 0) WriteLine();
    }

    public override void OnExtendedGeneratorExpression(ExtendedGeneratorExpression node)
    {
        // US don't have generator expression (list comprehension like things)
        throw new NotImplementedException();
    }

    public override void OnField(Field node)
    {
        WriteAttributes(node.Attributes, true);
        WriteModifiers(node);
        WriteTypeReference(node.Type);
        Write(" {0}", node.Name);
        if (node.Initializer != null)
        {
            Visit(node.Initializer);
        }
        WriteComma();
        WriteLine();
    }

    public override void OnForStatement(ForStatement node)
    {
        WriteLine();
        WriteIndented();
        WriteKeyword("foreach ");
        Write("(");
        Trace.Assert(node.Declarations.Count == 1, "don't support multiple statements.");
        Visit(node.Declarations[0]);
        WriteKeyword(" in ");
        Visit(node.Iterator);
        Write(")");
        WriteBlock(node.Block);

        Trace.Assert(node.OrBlock == null, "don't support for or block.");
        Trace.Assert(node.ThenBlock == null, "don't support for or block.");
    }

    public override void OnGeneratorExpression(GeneratorExpression node)
    {
        // US don't have generator expression (list comprehension like things)
        throw new NotImplementedException();
    }

    public override void OnGenericParameterDeclaration(GenericParameterDeclaration node)
    {
        // US don't have generic declaration
        throw new NotImplementedException();
    }

    public override void OnGenericReferenceExpression(GenericReferenceExpression node)
    {
        Visit(node.Target);
        Write("<");
        WriteCommaSeparatedList(node.GenericArguments);
        Write(">");
    }

    public override void OnGenericTypeDefinitionReference(GenericTypeDefinitionReference node)
    {
        // US don't have this
        throw new NotImplementedException();
    }

    public override void OnGenericTypeReference(GenericTypeReference node)
    {
        OnSimpleTypeReference(node);
        WriteGenericArguments(node.GenericArguments);
    }

    public override void OnGotoStatement(GotoStatement node)
    {
        // US don't have goto
        throw new NotImplementedException();
    }

    public override void OnHashLiteralExpression(HashLiteralExpression node)
    {
        // US don't have hash literal
        throw new NotImplementedException();
    }

    public override void OnIfStatement(IfStatement node)
    {
        WriteIfBlock("if ", node);
        Block falseBlock = node.FalseBlock;
        while (IsElseIf(falseBlock))
        {
            IfStatement stmt = (IfStatement)falseBlock.Statements[0];
            WriteIfBlock("else if", stmt);
            falseBlock = stmt.FalseBlock;
        }
        Block elseBlock = falseBlock;
        if (elseBlock != null)
        {
            WriteIndented("else");
            WriteBlock(elseBlock);
        }
        WriteLine();
    }

    private bool IsElseIf(Block block)
    {
        if (block == null) return false;
        if (block.Statements.Count != 1) return false;
        return block.Statements[0] is IfStatement;
    }

    public override void OnImport(Import node)
    {
        WriteKeyword("using");
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

        WriteComma();
        WriteLine();
    }

    public override void OnIntegerLiteralExpression(IntegerLiteralExpression node)
    {
        Write(node.Value.ToString());
        if (node.IsLong)
        {
            Write("l");
        }
    }

    public override void OnInterfaceDefinition(InterfaceDefinition node)
    {
        WriteTypeDefinition("interface", node);
    }

    public override void OnLabelStatement(LabelStatement node)
    {
        WriteLine("{0} :", node.Name);
    }

    public override void OnListLiteralExpression(ListLiteralExpression node)
    {
        Write("new Object[] {");
        WriteCommaSeparatedList(node.Items);
        Write("}");
    }

    public override void OnLocal(Local node)
    {
        // TODO should be a better way to handle this
        var local = (InternalLocal)node.Entity;
        // skip private scope locals, as they're wrapped into declaration in lambdas
        if (local.IsPrivateScope) return;

        WriteIndented();
        WriteProcessedType(local.Type);
        Write(" ");
        Write(node.Name);
        WriteComma();
        WriteLine();
    }

    public override void OnMacroStatement(MacroStatement node)
    {
        // US don't have macros
        throw new NotImplementedException();
    }

    public override void OnMemberReferenceExpression(MemberReferenceExpression node)
    {
        Visit(node.Target);
        Write(".");
        Write(node.Name);
    }

    public override void OnMethod(Method node)
    {
        _currentMethod = node;
        WriteCallableDefinition(node, CallableType.Usual);
    }

    MethodInfo GetMethodInfo(IEntity entity)
    {
        return (MethodInfo)((ExternalMethod)entity).MethodInfo;
    }

    void WriteCommaSeperatedArgumentsWithMethodInfo(MethodInfo methodInfo, ExpressionCollection arguments)
    {
        var ix = 0;
        foreach (var para in methodInfo.GetParameters())
        {
            if (ix > 0) Write(", ");
            if (para.IsOut) Write("out ");
            else if (para.ParameterType.IsByRef) Write("ref ");

            Visit(arguments[ix++]);
        }
    }

    public override void OnMethodInvocationExpression(MethodInvocationExpression node)
    {
        Trace.Assert(node.NamedArguments.Count == 0);

        var entity = TypeSystemServices.GetEntity(node.Target);
        MethodInfo methodInfo = null;
        if (entity.EntityType == EntityType.Constructor
            && !(node.Target is SuperLiteralExpression))
        {
            Write("new ");
        }
        else if (entity.EntityType == EntityType.Method
            && entity is ExternalMethod)
        {
            // hack to get the out/ref arguments right
            methodInfo = GetMethodInfo(entity);
        }

        Visit(node.Target);
        Write("(");
        if (methodInfo == null)
        {
            WriteCommaSeparatedList(node.Arguments);
        }
        else
        {
            WriteCommaSeperatedArgumentsWithMethodInfo(methodInfo, node.Arguments);
        }
        Write(")");
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
            Visit(member);
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
        // US don't support namespace
        throw new NotImplementedException();
    }

    public override void OnNullLiteralExpression(NullLiteralExpression node)
    {
        WriteKeyword("null");
    }

    public override void OnOmittedExpression(OmittedExpression node)
    {
        // US don't have this?
        throw new NotImplementedException();
    }

    public override void OnParameterDeclaration(ParameterDeclaration node)
    {
        WriteAttributes(node.Attributes, false);

        if (node.IsByRef) WriteKeyword("ref ");

        WriteTypeReference(node.Type);
        Write(" ");
        Write(node.Name);

        if (node.ParentNode != null)
        {
            Trace.Assert(node.ParentNode.NodeType != NodeType.CallableTypeReference, "shouldn't get callable type reference");
        }
    }

    public override void OnProperty(Property node)
    {
        // US don't have property
        throw new NotImplementedException();
    }

    public override void OnQuasiquoteExpression(QuasiquoteExpression node)
    {
        // US don't have quasi quote expression
        throw new NotImplementedException();
    }

    public override void OnRELiteralExpression(RELiteralExpression node)
    {
        // US don't have RE literal
        throw new NotImplementedException();
    }

    public override void OnRaiseStatement(RaiseStatement node)
    {
        WriteIndented();
        WriteKeyword("throw ");
        Visit(node.Exception);
        Visit(node.Modifier);
        WriteComma();
        WriteLine();
    }

    public override void OnReferenceExpression(ReferenceExpression node)
    {
        Write(node.Name);
    }

    public override void OnReturnStatement(ReturnStatement node)
    {
        WriteIndented();
        WriteKeyword("return");
        if (node.Expression != null || node.Modifier != null)
            Write(" ");
        Visit(node.Expression);
        Visit(node.Modifier);
        WriteComma();
        if (_isStatementInline == 0) WriteLine();
    }

    public override void OnSelfLiteralExpression(SelfLiteralExpression node)
    {
        WriteKeyword("this");
    }

    public override void OnSimpleTypeReference(SimpleTypeReference node)
    {
        WriteProcessedType(node);
    }

    public override void OnSlice(Slice node)
    {
        Visit(node.Begin);
        Trace.Assert(node.End == null, "shouldn't get slice with end.");
        Trace.Assert(node.Step == null, "shouldn't get slice with step.");
    }

    public override void OnSlicingExpression(SlicingExpression node)
    {
        Visit(node.Target);
        Write("[");
        WriteCommaSeparatedList(node.Indices);
        Write("]");
    }

    public override void OnSpliceExpression(SpliceExpression node)
    {
        // US don't have splicing
        throw new NotImplementedException();
    }

    public override void OnSpliceMemberReferenceExpression(SpliceMemberReferenceExpression node)
    {
        // US don't have splicing
        throw new NotImplementedException();
    }

    public override void OnSpliceParameterDeclaration(SpliceParameterDeclaration node)
    {
        // US don't have splicing
        throw new NotImplementedException();
    }

    public override void OnSpliceTypeDefinitionBody(SpliceTypeDefinitionBody node)
    {
        // US don't have splicing
        throw new NotImplementedException();
    }

    public override void OnSpliceTypeMember(SpliceTypeMember node)
    {
        // US don't have splicing
        throw new NotImplementedException();
    }

    public override void OnSpliceTypeReference(SpliceTypeReference node)
    {
        // US don't have splicing
        throw new NotImplementedException();
    }

    public override void OnStatementModifier(StatementModifier node)
    {
        // US don't have statement modifier
        throw new NotImplementedException();
    }

    public override void OnStatementTypeMember(StatementTypeMember node)
    {
        WriteModifiers(node);
        Visit(node.Statement);
    }

    public override void OnStringLiteralExpression(StringLiteralExpression node)
    {
        if (node != null && node.Value != null)
            WriteStringLiteral(node.Value);
        else
            WriteKeyword("null");
    }

    public override void OnStructDefinition(StructDefinition node)
    {
        WriteTypeDefinition("struct", node);
    }

    public override void OnSuperLiteralExpression(SuperLiteralExpression node)
    {
        WriteKeyword("base");
    }

    public override void OnTimeSpanLiteralExpression(TimeSpanLiteralExpression node)
    {
        throw new NotImplementedException();
    }

    public override void OnTryCastExpression(TryCastExpression node)
    {
        Write("(");
        Visit(node.Target);
        WriteKeyword(" as ");
        WriteTypeReference(node.Type);
        Write(")");
    }

    public override void OnTryStatement(TryStatement node)
    {
        WriteIndented();
        WriteKeyword("try");
        WriteBlock(node.ProtectedBlock);
        Visit(node.ExceptionHandlers);

        if (null != node.FailureBlock)
        {
            WriteIndented();
            WriteKeyword("catch");
            WriteBlock(node.FailureBlock);
        }

        if (null != node.EnsureBlock)
        {
            WriteIndented();
            WriteKeyword("finally");
            WriteBlock(node.EnsureBlock);
        }
    }

    public override void OnTypeMemberStatement(TypeMemberStatement node)
    {
        throw new NotImplementedException();
    }

    public override void OnTypeofExpression(TypeofExpression node)
    {
        var needTypeOf = node.ParentNode.NodeType != NodeType.GenericReferenceExpression;

        if (needTypeOf) Write("typeof(");
        Visit(node.Type);
        if (needTypeOf) Write(")");
    }

    public override void OnUnaryExpression(UnaryExpression node)
    {
        bool needParens = NeedParensAround(node);
        if (needParens) Write("(");

        bool postOperator = AstUtil.IsPostUnaryOperator(node.Operator);
        if (!postOperator)
        {
            WriteOperator(GetUnaryOperatorText(node.Operator));
        }
        Visit(node.Operand);
        if (postOperator)
        {
            WriteOperator(GetUnaryOperatorText(node.Operator));
        }

        if (needParens) Write(")");
    }

    public override void OnUnlessStatement(UnlessStatement node)
    {
        // US don't have unless
        throw new NotImplementedException();
    }

    public override void OnUnpackStatement(UnpackStatement node)
    {
        // US don't have unless
        throw new NotImplementedException();
    }

    public override void OnWhileStatement(WhileStatement node)
    {
        WriteLine();
        WriteIndented();
        WriteKeyword("while ");
        Write("(");
        Visit(node.Condition);
        Write(")");
        WriteBlock(node.Block);

        Trace.Assert(node.OrBlock == null, "shoudn't get or block");
        Trace.Assert(node.ThenBlock == null, "shoudn't get then block");
    }

    public override void OnYieldStatement(YieldStatement node)
    {
        WriteIndented();
        if (node.Expression == CSharpRewriteTransformer.YieldBreakExpression)
        {
            Write("yield break");
        }
        else if (node.Expression == null)
        {
            Write("yield return null");
        }
        else
        {
            WriteKeyword("yield return ");
            Visit(node.Expression);
        }
        Visit(node.Modifier);
        WriteComma();
        WriteLine();
    }

}


}
