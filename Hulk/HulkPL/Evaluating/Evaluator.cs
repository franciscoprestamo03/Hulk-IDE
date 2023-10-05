using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace HulkPL
{
    public class Evaluator
    {
        public Stack<Dictionary<string, object>> scopes;

        public Stack<Dictionary<string, FunctionDeclarationNode>> functionScopes;
        public string output;
        public int whileIterations;

        public int iterations;

        private int Counter;
        public Evaluator()
        {
            Counter = 0;
            whileIterations = 0;
            output = "";
            this.scopes = new Stack<Dictionary<string, object>>();
            this.scopes.Push(new Dictionary<string, object>());
            this.functionScopes = new Stack<Dictionary<string, FunctionDeclarationNode>>();
            this.functionScopes.Push(new Dictionary<string, FunctionDeclarationNode>());
        }

        public string EvaluateMain(MainProgramNode node)
        {
            output = "";
            iterations = 1;
            System.Console.WriteLine(iterations++);
            Evaluate(node);

            return output;
        }
        public object? Evaluate(Node node)
        {
            Counter++;
            if(Counter>80){
                throw new Exception("Hulk Stack Overflow");
            }
            Dictionary<string, FunctionDeclarationNode> scope;
            switch (node)
            {
                case MainProgramNode mainNode:
                    EnterScope();
                    object result = EvaluateBlock(mainNode.Body);
                    ExitScope();
                    Counter--;
                    return result;
                case BinaryExpressionNode binaryExpressionNode:
                    Counter--;
                    return EvaluateBinaryExpression(binaryExpressionNode);
                case UnaryExpressionNode unaryExpressionNode:
                    Counter--;
                    return EvaluateUnaryExpression(unaryExpressionNode);
                case GroupingNode groupingNode:
                    Counter--;
                    return Evaluate(groupingNode.Expression);
                case NumberNode numberNode:
                    Counter--;
                    return numberNode.Value;
                case StringNode stringNode:
                    Counter--;
                    return stringNode.Value;
                case BooleanNode booleanNode:
                    Counter--;
                    return booleanNode.Value;
                case FunctionDeclarationNode functionDeclarationNode:
                    if (functionScopes.Peek().ContainsKey(functionDeclarationNode.Name))
                    {
                        output += $"Function '{functionDeclarationNode.Name}' already exists.";
                        throw new Exception($"Function '{functionDeclarationNode.Name}' already exists.");
                    }
                    else
                    {
                        functionScopes.Peek().Add(functionDeclarationNode.Name, functionDeclarationNode);
                        Counter--;
                        return null;
                    }
                case FunctionCallNode functionCallNode:
                    if (functionCallNode.Name == "sin")
                    {
                        var argument1 = Evaluate(functionCallNode.Arguments[0]);
                        if (argument1 is Double)
                        {   
                            Counter--;
                            return Math.Sin((double)argument1);
                        }
                        else
                        {
                            output += $"Function sen(x) receive a double not a {argument1.GetType()}";
                            throw new Exception($"Function sen(x) receive a double not a {argument1.GetType()}");
                        }
                    }
                    else if (functionCallNode.Name == "cos")
                    {
                        var argument1 = Evaluate(functionCallNode.Arguments[0]);
                        if (argument1 is Double)
                        {
                            Counter--;
                            return Math.Cos((double)argument1);
                        }
                        else
                        {
                            output += $"Function cos(x) receive a double not a {argument1.GetType()}";
                            throw new Exception($"Function cos(x) receive a double not a {argument1.GetType()}");
                        }
                    }
                    else if (functionCallNode.Name == "print")
                    {
                        System.Console.WriteLine("Function print");
                        object? value2 = null;
                        foreach (Node argument in functionCallNode.Arguments)
                        {
                            value2 = Evaluate(argument);
                            Console.WriteLine("value => " + value2);
                            output += $"{value2}";

                        }
                        System.Console.WriteLine("Out Function print");
                        Counter--;
                        return value2;
                    }
                    else if (functionCallNode.Name == "printLine")
                    {
                        System.Console.WriteLine("Function printLine");
                        object? value3 = null;
                        foreach (Node argument in functionCallNode.Arguments)
                        {
                            value3 = Evaluate(argument);
                            Console.WriteLine("value => " + value3);
                            output += $"{value3}\n";

                        }
                        Counter--;
                        return value3;
                    }
                    else if ((scope = FindFuntionScope(functionCallNode.Name)) != null)
                    {
                        
                        FunctionDeclarationNode function = scope[functionCallNode.Name];

                        EnterScope();

                        if (function.Parameters.Count != functionCallNode.Arguments.Count)
                        {
                            throw new Exception($"Function requires exact {function.Parameters.Count} arguments");
                        }
                        for (int i = 0; i < function.Parameters.Count; i++)
                        {
                            VariableDeclarationNode variableDeclaration = function.Parameters[i];
                            variableDeclaration.Initializer = functionCallNode.Arguments[i];

                            Evaluate(variableDeclaration);
                        }
                        object? result4 = function.Body!=null? EvaluateBlock(function.Body):null;

                        object? result5 = Evaluate(function.ReturnNode);

                        ExitScope();
                        Counter--;
                        return result5;
                    }
                    else
                    {
                        output += $"Function '{functionCallNode.Name}' does not exist.";
                        throw new Exception($"Function '{functionCallNode.Name}' does not exist.");
                    }
                case VariableReferenceNode variableReferenceNode:
                    if (FindVariableScope(variableReferenceNode.Name) != null)
                    {
                        Counter--;
                        return FindVariableScope(variableReferenceNode.Name)[variableReferenceNode.Name];
                    }
                    else
                    {
                        output += $"Variable '{variableReferenceNode.Name}' does not exist.";
                        throw new Exception($"Variable '{variableReferenceNode.Name}' does not exist.");
                    }
                case VariableDeclarationNode variableDeclarationNode:
                    if (scopes.Peek().ContainsKey(variableDeclarationNode.Name))
                    {
                        output += $"Variable '{variableDeclarationNode.Name}' already exists.";
                        throw new Exception($"Variable '{variableDeclarationNode.Name}' already exists.");
                    }
                    else
                    {
                        object initialValue = Evaluate(variableDeclarationNode.Initializer);

                        //type cheking
                        System.Console.WriteLine($"verificando valor {initialValue.GetType()} {initialValue}");


                        scopes.Peek().Add(variableDeclarationNode.Name, initialValue);
                        Counter--;
                        return initialValue;
                    }
                case VariableAssignmentNode variableAssignmentNode:
                    if (FindVariableScope(variableAssignmentNode.Name) != null)
                    {
                        object? valueToAssign = Evaluate(variableAssignmentNode.Value);
                        FindVariableScope(variableAssignmentNode.Name)[variableAssignmentNode.Name] = valueToAssign;
                        Counter--;
                        return null;
                    }
                    else
                    {
                        output += $"Variable '{variableAssignmentNode.Name}' does not exist.";
                        throw new Exception($"Variable '{variableAssignmentNode.Name}' does not exist.");
                    }

                case IfNode ifNode:
                    object? conditionValue = Evaluate(ifNode.Condition);
                    if (conditionValue is bool conditionBool && conditionBool)
                    {
                        EnterScope();
                        object result1 = EvaluateBlock(ifNode.ThenStatements);
                        ExitScope();
                        Counter--;
                        return result1;
                        
                    }
                    else if (ifNode.ElseStatements != null)
                    {
                        EnterScope();
                        object result2 = EvaluateBlock(ifNode.ElseStatements);
                        ExitScope();
                        Counter--;
                        return result2;
                    }
                    else
                    {
                        Counter--;
                        return null;
                    }
                case WhileNode whileNode:
                    object? loopConditionValue = Evaluate(whileNode.Condition);
                    while (loopConditionValue is bool loopConditionBool && loopConditionBool)
                    {
                        if (whileIterations == 100000)
                        {
                            throw new Exception("Hulk Stack overflow");
                        }
                        else
                        {
                            whileIterations++;
                        }
                        EnterScope();
                        System.Console.WriteLine("before " + iterations++);
                        EvaluateBlock(whileNode.BodyStatements);
                        System.Console.WriteLine("after " + iterations++);
                        ExitScope();
                        loopConditionValue = Evaluate(whileNode.Condition);
                    }
                    whileIterations = 0;
                    Counter--;
                    return null;
                case LetNode letNode:
                    EnterScope();
                    foreach (VariableDeclarationNode variableDeclaration in letNode.VarDeclarations)
                    {
                        Evaluate(variableDeclaration);
                    }
                    object result3 = EvaluateBlock(letNode.Body);
                    ExitScope();
                    Counter--;
                    return result3;
                default:
                    throw new Exception($"Unhandled node type: {node}");
            }
        }

       
        private Dictionary<string, FunctionDeclarationNode> FindFuntionScope(string functionName)
        {
            foreach (Dictionary<string, FunctionDeclarationNode> scope in functionScopes)
            {
                if (scope.ContainsKey(functionName))
                {
                    return scope;
                }
            }

            return null;
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
            // if (scopes.Count > 1)
            // {
            //     scopes.Pop();
            //     Dictionary<string, object> parentScope = FindVariableScope(variableName);
            //     scopes.Push(parentScope);
            //     return parentScope;
            // }

            return null;
        }


        private void EnterScope()
        {
            System.Console.WriteLine($"Entering scope {scopes.Count}");
            this.scopes.Push(new Dictionary<string, object>());
            //se puede eliminar para que todas las funciones que se declaren sean globales
            this.functionScopes.Push(new Dictionary<string, FunctionDeclarationNode>());
        }

        private void ExitScope()
        {
            this.scopes.Pop();
            this.functionScopes.Pop();
            System.Console.WriteLine($"Exit scope {scopes.Count}");
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
                        output += $"Cannot add {left?.GetType().Name} and {right?.GetType().Name}";
                        throw new Exception($"Cannot add {left?.GetType().Name} and {right?.GetType().Name}");
                    }
                case TokenType.SubtractionToken:
                    if (left is double leftDoubleSub && right is double rightDoubleSub)
                    {
                        return leftDoubleSub - rightDoubleSub;
                    }
                    else
                    {
                        output += $"Cannot subtract {left?.GetType().Name} and {right?.GetType().Name}";
                        throw new Exception($"Cannot subtract {left?.GetType().Name} and {right?.GetType().Name}");
                    }
                case TokenType.MultiplicationToken:
                    if (left is double leftDoubleMul && right is double rightDoubleMul)
                    {
                        return leftDoubleMul * rightDoubleMul;
                    }
                    else
                    {
                        output += $"Cannot multiply {left?.GetType().Name} and {right?.GetType().Name}";
                        throw new Exception($"Cannot multiply {left?.GetType().Name} and {right?.GetType().Name}");
                    }
                case TokenType.DivisionToken:
                    if (left is double leftDoubleDiv && right is double rightDoubleDiv)
                    {
                        return leftDoubleDiv / rightDoubleDiv;
                    }
                    else
                    {
                        output += $"Cannot divide {left?.GetType().Name} and {right?.GetType().Name}";
                        throw new Exception($"Cannot divide {left?.GetType().Name} and {right?.GetType().Name}");
                    }
                case TokenType.ArrobaToken:
                    if(left != null && right != null){
                        return left.ToString()+right.ToString();
                    }
                    else{
                        throw new Exception($"Cannot concatenate {left?.GetType().Name} and {right?.GetType().Name}");
                    }
                case TokenType.ModulusToken:
                    if (left is double leftDoubleMod && right is double rightDoubleMod)
                    {
                        return leftDoubleMod % rightDoubleMod;
                    }
                    else
                    {
                        
                        throw new Exception($"Cannot apply modulus to {left?.GetType().Name} and {right?.GetType().Name}");
                    }
                case TokenType.PowToken:
                    if (left is double leftDoublePow && right is double rightDoublePow)
                    {
                        return Math.Pow(leftDoublePow,rightDoublePow);
                    }
                    else
                    {
                        
                        throw new Exception($"Cannot apply exponenciation to {left?.GetType().Name} and {right?.GetType().Name}");
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
                        output += $"Cannot compare {left?.GetType().Name} and {right?.GetType().Name}";
                        throw new Exception($"Cannot compare {left?.GetType().Name} and {right?.GetType().Name}");
                    }
                case TokenType.LessThanOrEqualToken:
                    if (left is double leftDoubleLTE && right is double rightDoubleLTE)
                    {
                        return leftDoubleLTE <= rightDoubleLTE;
                    }
                    else
                    {
                        output += $"Cannot compare {left?.GetType().Name} and {right?.GetType().Name}";
                        throw new Exception($"Cannot compare {left?.GetType().Name} and {right?.GetType().Name}");
                    }
                case TokenType.GreaterThanToken:
                    if (left is double leftDoubleGT && right is double rightDoubleGT)
                    {
                        return leftDoubleGT > rightDoubleGT;
                    }
                    else
                    {
                        output += $"Cannot compare {left?.GetType().Name} and {right?.GetType().Name}";
                        throw new Exception($"Cannot compare {left?.GetType().Name} and {right?.GetType().Name}");
                    }
                case TokenType.GreaterThanOrEqualToken:
                    if (left is double leftDoubleGTE && right is double rightDoubleGTE)
                    {
                        return leftDoubleGTE >= rightDoubleGTE;
                    }
                    else
                    {
                        output += $"Cannot compare {left?.GetType().Name} and {right?.GetType().Name}";
                        throw new Exception($"Cannot compare {left?.GetType().Name} and {right?.GetType().Name}");
                    }
                case TokenType.LogicalAndToken:
                    if (left is bool leftBool && right is bool rightBool)
                    {
                        return leftBool && rightBool;
                    }
                    else
                    {
                        output += $"Cannot perform logical AND on {left?.GetType().Name} and {right?.GetType().Name}";
                        throw new Exception($"Cannot perform logical AND on {left?.GetType().Name} and {right?.GetType().Name}");
                    }
                case TokenType.LogicalOrToken:
                    if (left is bool leftBoolOr && right is bool rightBoolOr)
                    {
                        return leftBoolOr || rightBoolOr;
                    }
                    else
                    {
                        output += $"Cannot perform logical OR on {left?.GetType().Name} and {right?.GetType().Name}";
                        throw new Exception($"Cannot perform logical OR on {left?.GetType().Name} and {right?.GetType().Name}");
                    }
                default:
                    output += $"Unknown binary operator {node.Operator.Value}";
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
                        output += $"Cannot negate {operand?.GetType().Name}";
                        throw new Exception($"Cannot negate {operand?.GetType().Name}");
                    }
                case TokenType.LogicalNotToken:
                    if (operand is bool operandBool)
                    {
                        return !operandBool;
                    }
                    else
                    {
                        output += $"Cannot perform logical NOT on {operand?.GetType().Name}";
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