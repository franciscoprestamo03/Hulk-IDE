using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HulkPL
{

    public interface Visitor
    {
        void VisitNumberNode(NumberNode node);
        void VisitBooleanNode(BooleanNode node);
        void VisitStringNode(StringNode node);
        void VisitUnaryExpressionNode(UnaryExpressionNode node);
        void VisitBinaryExpressionNode(BinaryExpressionNode node);
        void VisitVariableDeclarationNode(VariableDeclarationNode node);
        void VisitVariableAssignmentNode(VariableAssignmentNode node);
        void VisitFunctionDeclarationNode(FunctionDeclarationNode node);
        void VisitFunctionCallNode(FunctionCallNode node);
        void VisitGroupingNode(GroupingNode node);
        void VisitIfNode(IfNode node);
        void VisitWhileNode(WhileNode node);
        void VisitLetNode(LetNode node);
        void VisitConditionalExpressionNode(ConditionalExpressionNode node);
        void VisitPrintNode(PrintNode node);
        void VisitVariableReferenceNode(VariableReferenceNode variableReferenceNode);
    }

}