using System.Collections.Generic;

namespace HulkPL
{
    public class Evaluator
    {
        private Stack<Dictionary<string, object>> scopes;
        private string output;

        public Evaluator()
        {
            this.scopes = new Stack<Dictionary<string, object>>();
            this.scopes.Push(new Dictionary<string, object>());
        }

        public string EvaluateMain(Node node){
            output = "";

            Evaluate(node);

            return output;
        }
        public object Evaluate(Node node)
        {
            switch (node)
            {
                case MainProgramNode mainNode:
                    EnterScope();
                    object result = EvaluateBlock(mainNode.Body);
                    ExitScope();
                    return result;
                case BinaryExpressionNode binaryExpressionNode:
                    return EvaluateBinaryExpression(binaryExpressionNode);
                case UnaryExpressionNode unaryExpressionNode:
                    return EvaluateUnaryExpression(unaryExpressionNode);
                case GroupingNode groupingNode:
                    return Evaluate(groupingNode.Expression);
                case NumberNode numberNode:
                    return numberNode.Value;
                case StringNode stringNode:
                    return stringNode.Value;
                case BooleanNode booleanNode:
                    return booleanNode.Value;
                case PrintNode printNode:
                    object value = Evaluate(printNode.Expression);
                    Console.WriteLine(value);
                    output += $"{value} \n";
                    return null;
                case VariableReferenceNode variableReferenceNode:
                    if (FindVariableScope(variableReferenceNode.Name) != null)
                    {
                        return FindVariableScope(variableReferenceNode.Name)[variableReferenceNode.Name];
                    }
                    else
                    {
                        throw new Exception($"Variable '{variableReferenceNode.Name}' does not exist.");
                    }
                case VariableDeclarationNode variableDeclarationNode:
                    if (scopes.Peek().ContainsKey(variableDeclarationNode.Name))
                    {
                        throw new Exception($"Variable '{variableDeclarationNode.Name}' already exists.");
                    }
                    else
                    {
                        object initialValue = Evaluate(variableDeclarationNode.Initializer);
                        scopes.Peek().Add(variableDeclarationNode.Name, initialValue);
                        return null;
                    }
                case VariableAssignmentNode variableAssignmentNode:
                    if (FindVariableScope(variableAssignmentNode.Name) != null)
                    {
                        object valueToAssign = Evaluate(variableAssignmentNode.Value);
                        FindVariableScope(variableAssignmentNode.Name)[variableAssignmentNode.Name] = valueToAssign;
                        return null;
                    }
                    else
                    {
                        throw new Exception($"Variable '{variableAssignmentNode.Name}' does not exist.");
                    }

                case IfNode ifNode:
                    object conditionValue = Evaluate(ifNode.Condition);
                    if (conditionValue is bool conditionBool && conditionBool)
                    {
                        EnterScope();
                        object result1 = EvaluateBlock(ifNode.ThenStatements);
                        ExitScope();
                        return result1;
                    }
                    else if (ifNode.ElseStatements != null)
                    {
                        EnterScope();
                        object result2 = EvaluateBlock(ifNode.ElseStatements);
                        ExitScope();
                        return result2;
                    }
                    else
                    {
                        return null;
                    }
                case WhileNode whileNode:
                    object loopConditionValue = Evaluate(whileNode.Condition);
                    while (loopConditionValue is bool loopConditionBool && loopConditionBool)
                    {
                        EnterScope();
                        EvaluateBlock(whileNode.BodyStatements);
                        ExitScope();
                        loopConditionValue = Evaluate(whileNode.Condition);
                    }
                    return null;
                case LetNode letNode:
                    EnterScope();
                    foreach (VariableDeclarationNode variableDeclaration in letNode.VarDeclarations)
                    {
                        Evaluate(variableDeclaration);
                    }
                    object result3 = EvaluateBlock(letNode.Body);
                    ExitScope();
                    return result3;
                default:
                    throw new Exception($"Unhandled node type: {node.GetType()}");
            }
        }

        private Dictionary<string, object> FindVariableScope(string variableName)
        {
            foreach (Dictionary<string, object> scope in scopes)
            {
                if (scope.ContainsKey(variableName))
                {
                    return scope;
                }
            }

            // If the variable is not found in the local scope,
            // recursively search in the parent scope (the previous scope in the stack).
            if (scopes.Count > 1)
            {
                scopes.Pop();
                Dictionary<string, object> parentScope = FindVariableScope(variableName);
                scopes.Push(parentScope);
                return parentScope;
            }

            return null;
        }

        private void EnterScope()
        {
            this.scopes.Push(new Dictionary<string, object>());
        }

        private void ExitScope()
        {
            this.scopes.Pop();
        }

        private object EvaluateBinaryExpression(BinaryExpressionNode node)
        {
            object left = Evaluate(node.Left);
            object right = Evaluate(node.Right);

            switch (node.Operator.Type)
            {
                case TokenType.AdditionToken:
                    if (left is double leftDouble && right is double rightDouble)
                    {
                        return leftDouble + rightDouble;
                    }
                    else if (left is string leftString && right is string rightString)
                    {
                        return leftString + rightString;
                    }
                    else
                    {
                        throw new Exception($"Cannot add {left?.GetType().Name} and {right?.GetType().Name}");
                    }
                case TokenType.SubtractionToken:
                    if (left is double leftDoubleSub && right is double rightDoubleSub)
                    {
                        return leftDoubleSub - rightDoubleSub;
                    }
                    else
                    {
                        throw new Exception($"Cannot subtract {left?.GetType().Name} and {right?.GetType().Name}");
                    }
                case TokenType.MultiplicationToken:
                    if (left is double leftDoubleMul && right is double rightDoubleMul)
                    {
                        return leftDoubleMul * rightDoubleMul;
                    }
                    else
                    {
                        throw new Exception($"Cannot multiply {left?.GetType().Name} and {right?.GetType().Name}");
                    }
                case TokenType.DivisionToken:
                    if (left is double leftDoubleDiv && right is double rightDoubleDiv)
                    {
                        return leftDoubleDiv / rightDoubleDiv;
                    }
                    else
                    {
                        throw new Exception($"Cannot divide {left?.GetType().Name} and {right?.GetType().Name}");
                    }
                case TokenType.EqualityToken:
                    return left.Equals(right);
                case TokenType.InequalityToken:
                    return !left.Equals(right);
                case TokenType.LessThanToken:
                    if (left is double leftDoubleLT && right is double rightDoubleLT)
                    {
                        return leftDoubleLT < rightDoubleLT;
                    }
                    else
                    {
                        throw new Exception($"Cannot compare {left?.GetType().Name} and {right?.GetType().Name}");
                    }
                case TokenType.LessThanOrEqualToken:
                    if (left is double leftDoubleLTE && right is double rightDoubleLTE)
                    {
                        return leftDoubleLTE <= rightDoubleLTE;
                    }
                    else
                    {
                        throw new Exception($"Cannot compare {left?.GetType().Name} and {right?.GetType().Name}");
                    }
                case TokenType.GreaterThanToken:
                    if (left is double leftDoubleGT && right is double rightDoubleGT)
                    {
                        return leftDoubleGT > rightDoubleGT;
                    }
                    else
                    {
                        throw new Exception($"Cannot compare {left?.GetType().Name} and {right?.GetType().Name}");
                    }
                case TokenType.GreaterThanOrEqualToken:
                    if (left is double leftDoubleGTE && right is double rightDoubleGTE)
                    {
                        return leftDoubleGTE >= rightDoubleGTE;
                    }
                    else
                    {
                        throw new Exception($"Cannot compare {left?.GetType().Name} and {right?.GetType().Name}");
                    }
                case TokenType.LogicalAndToken:
                    if (left is bool leftBool && right is bool rightBool)
                    {
                        return leftBool && rightBool;
                    }
                    else
                    {
                        throw new Exception($"Cannot perform logical AND on {left?.GetType().Name} and {right?.GetType().Name}");
                    }
                case TokenType.LogicalOrToken:
                    if (left is bool leftBoolOr && right is bool rightBoolOr)
                    {
                        return leftBoolOr || rightBoolOr;
                    }
                    else
                    {
                        throw new Exception($"Cannot perform logical OR on {left?.GetType().Name} and {right?.GetType().Name}");
                    }
                default:
                    throw new Exception($"Unknown binary operator {node.Operator.Value}");
            }
        }

        private object EvaluateUnaryExpression(UnaryExpressionNode node)
        {
            object operand = Evaluate(node.Expression);

            switch (node.Operator.Type)
            {
                case TokenType.SubtractionToken:
                    if (operand is double operandDouble)
                    {
                        return -operandDouble;
                    }
                    else
                    {
                        throw new Exception($"Cannot negate {operand?.GetType().Name}");
                    }
                case TokenType.LogicalNotToken:
                    if (operand is bool operandBool)
                    {
                        return !operandBool;
                    }
                    else
                    {
                        throw new Exception($"Cannot perform logical NOT on {operand?.GetType().Name}");
                    }
                default:
                    throw new Exception($"Unknown unary operator {node.Operator.Value}");
            }
        }

        private object EvaluateBlock(List<Node> statements)
        {
            object result = null;
            foreach (Node statement in statements)
            {
                result = Evaluate(statement);
            }
            return result;
        }
    }
}