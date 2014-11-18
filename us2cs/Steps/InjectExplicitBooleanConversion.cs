using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Boo.Lang.Compiler.TypeSystem;
using Boo.Lang.Compiler.Steps;
using Boo.Lang.Compiler.Ast;

namespace US2CS
{

// works similar to Boo.Lang.Compiler.Steps.InjectImplicitBooleanConversions,
// explicitly add boolean conversion on if/while/etc condition positions
class InjectExplicitBooleanConversion : AbstractNamespaceSensitiveTransformerCompilerStep
{
    private Expression ExplicitBooleanContext(Expression expression)
    {
        var type = GetExpressionType(expression);
        if (type == TypeSystemServices.BoolType)
        {
            return expression;
        }

        // happening
        //Trace.Assert(!TypeSystemServices.IsError(expression), "shouldn't have error boolean expression.");

        if (TypeSystemServices.IsNumber(type) || type.IsEnum)
        {
            return CodeBuilder.CreateBoundBinaryExpression(
                TypeSystemServices.BoolType,
                BinaryOperatorType.Equality,
                expression,
                CodeBuilder.CreateIntegerLiteral(0));
        }
        else if (TypeSystemServices.IsReferenceType(type))
        {
            return CodeBuilder.CreateBoundBinaryExpression(
                TypeSystemServices.BoolType,
                BinaryOperatorType.Equality,
                expression,
                CodeBuilder.CreateNullLiteral());
        }

        return expression;
    }

    # region copied from InjectImplicitBooleanConversions
    override public void LeaveUnlessStatement(UnlessStatement node)
    {
        node.Condition = ExplicitBooleanContext(node.Condition);
    }

    override public void LeaveIfStatement(IfStatement node)
    {
        node.Condition = ExplicitBooleanContext(node.Condition);
    }

    override public void LeaveConditionalExpression(ConditionalExpression node)
    {
        node.Condition = ExplicitBooleanContext(node.Condition);
    }

    override public void LeaveWhileStatement(WhileStatement node)
    {
        node.Condition = ExplicitBooleanContext(node.Condition);
    }

    public override void LeaveUnaryExpression(UnaryExpression node)
    {
        switch (node.Operator)
        {
            case UnaryOperatorType.LogicalNot:
                node.Operand = ExplicitBooleanContext(node.Operand);
                break;
        }
    }

    public override void LeaveBinaryExpression(BinaryExpression node)
    {
        switch (node.Operator)
        {
            case BinaryOperatorType.And:
            case BinaryOperatorType.Or:
                BindLogicalOperator(node);
                break;
        }
    }

    void BindLogicalOperator(BinaryExpression node)
    {
        if (InjectImplicitBooleanConversions.IsLogicalCondition(node))
        {
            BindLogicalOperatorCondition(node);
        }
        else
        {
            node.Left = ExplicitBooleanContext(node.Left);
            node.Right = ExplicitBooleanContext(node.Right);
        }
    }

    private void BindLogicalOperatorCondition(BinaryExpression node)
    {
        node.Left = ExplicitBooleanContext(node.Left);
        node.Right = ExplicitBooleanContext(node.Right);
        BindExpressionType(node, TypeSystemServices.GetMostGenericType(GetExpressionType(node.Left), GetExpressionType(node.Right)));
    }

    #endregion

}

}
