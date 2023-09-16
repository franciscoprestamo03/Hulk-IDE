
using System.ComponentModel.Design;

namespace HulkPL;
public class TypeChecker
{
    private Stack<Dictionary<string, Type>> scopes;
    private Stack<Dictionary<string, FunctionDeclarationNode>> functionScopes;
    private string error;
    public TypeChecker()
    {
        error = "";
        this.scopes = new Stack<Dictionary<string, Type>>();
        this.scopes.Push(new Dictionary<string, Type>());

        this.functionScopes = new Stack<Dictionary<string, FunctionDeclarationNode>>();
        this.functionScopes.Push(new Dictionary<string, FunctionDeclarationNode>());
    }


    private Dictionary<string, Type> FindVariableScope(string variableName)
    {
        foreach (Dictionary<string, Type> scope in scopes)
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
            Dictionary<string, Type> parentScope = FindVariableScope(variableName);
            scopes.Push(parentScope);
            return parentScope;
        }

        return null;
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

        // If the variable is not found in the local scope,
        // recursively search in the parent scope (the previous scope in the stack).
        if (functionScopes.Count > 1)
        {
            functionScopes.Pop();
            Dictionary<string, FunctionDeclarationNode> parentScope = FindFuntionScope(functionName);
            functionScopes.Push(parentScope);
            return parentScope;
        }

        return null;
    }

    private void EnterScope()
    {
        this.scopes.Push(new Dictionary<string, Type>());
        //se puede eliminar para que todas las funciones que se declaren sean globales
        this.functionScopes.Push(new Dictionary<string, FunctionDeclarationNode>());
    }

    private void ExitScope()
    {
        this.scopes.Pop();
        this.functionScopes.Pop();
    }

    public string checkTypes(MainProgramNode mainProgramNode)
    {
        Type result = Check(mainProgramNode);
        if (error == "")
        {
            return "";
        }
        else
        {
            string temp = error;
            error = "";
            return temp;
        }
    }

    private Type Chek2(Node node)
    {

        switch (node)
        {
            case NumberNode:
                return typeof(double);
            case StringNode:
                return typeof(string);
            case BooleanNode:
                return typeof(bool);
            case FunctionDeclarationNode functionDeclarationNode:
                return null;

            case VariableDeclarationNode variableDeclarationNode:
                scopes.Peek().Add(variableDeclarationNode.Name, variableDeclarationNode.VarType);
                if (variableDeclarationNode.Initializer != null)
                {
                    Type initializerType = Check(variableDeclarationNode.Initializer);
                    if (initializerType != variableDeclarationNode.VarType)
                    {
                        AddError($"Son diferentes los tipos initializerType: {initializerType}  variableType: {variableDeclarationNode.VarType}");
                    }
                }

                return null;
            default:
                return null;
        }
        return null;
    }


    public Type Check(Node node)
    {
        switch (node)
        {
            case MainProgramNode mainNode:
                EnterScope();
                Type? result = CheckBlock(mainNode.Body);
                ExitScope();
                return result;
            case BinaryExpressionNode binaryExpressionNode:
                return EvaluateBinaryExpression(binaryExpressionNode);
            case UnaryExpressionNode unaryExpressionNode:
                return EvaluateUnaryExpression(unaryExpressionNode);
            case GroupingNode groupingNode:
                return Check(groupingNode.Expression);
            case NumberNode numberNode:
                return typeof(double);
            case StringNode stringNode:
                return typeof(string);
            case BooleanNode booleanNode:
                return typeof(bool);
            case FunctionDeclarationNode functionDeclarationNode:
                if (functionScopes.Peek().ContainsKey(functionDeclarationNode.Name))
                {
                    AddError($"Function '{functionDeclarationNode.Name}' already exists.");
                    return null;
                }
                else
                {
                    functionScopes.Peek().Add(functionDeclarationNode.Name, functionDeclarationNode);
                    return null;
                }
            case FunctionCallNode functionCallNode:
                if (functionCallNode.Name == "sin")
                {
                    var argument1 = Check(functionCallNode.Arguments[0]);
                    if (argument1 == typeof(double))
                    {
                        return typeof(double);
                    }
                    else
                    {
                        AddError($"Function sen(x) receive a Number not a {argument1.GetType()}");
                        return null;
                    }
                }
                else if (functionCallNode.Name == "cos")
                {
                    var argument1 = Check(functionCallNode.Arguments[0]);
                    if (argument1 == typeof(double))
                    {
                        return typeof(double);
                    }
                    else
                    {
                        AddError($"Function cos(x) receive a Number not a {argument1.GetType()}");
                        return null;
                    }
                }
                else if (functionCallNode.Name == "print")
                {
                    Type value = null;
                    foreach (Node argument in functionCallNode.Arguments)
                    {
                        value = Check(argument);
                    }
                    return value;
                }
                else if (functionCallNode.Name == "printLine")
                {
                    Type value = null;
                    foreach (Node argument in functionCallNode.Arguments)
                    {
                        value = Check(argument);
                    }
                    System.Console.WriteLine(value);
                    return value;
                }
                else if (FindFuntionScope(functionCallNode.Name) != null)
                {
                    FunctionDeclarationNode function = FindFuntionScope(functionCallNode.Name)[functionCallNode.Name];
                    EnterScope();
                    if (function.Parameters.Count != functionCallNode.Arguments.Count)
                    {
                        AddError($"Function requires exact {function.Parameters.Count} arguments");
                        return null;
                    }
                    for (int i = 0; i < function.Parameters.Count; i++)
                    {
                        VariableDeclarationNode variableDeclaration = function.Parameters[i];
                        Type parmeterType = Check(functionCallNode.Arguments[i]);
                        if (variableDeclaration.VarType != parmeterType)
                        {
                            AddError($"The function {function.Name} variable {variableDeclaration} is {variableDeclaration.VarType} and cant be {parmeterType}");
                        }

                    }
                    if (function.Body != null)
                    {
                        Type? blockType = CheckBlock(function.Body);
                    }
                    Type returnType = Check(function.ReturnNode);
                    if (returnType != function.type)
                    {
                        AddError($"The function {function.Name} must return an {function.type} not {returnType}");
                    }
                    ExitScope();
                    return function.type;
                }
                else
                {
                    AddError($"Function '{functionCallNode.Name}' does not exist.");
                    return null;
                }
            case VariableReferenceNode variableReferenceNode:
                if (FindVariableScope(variableReferenceNode.Name) != null)
                {
                    return FindVariableScope(variableReferenceNode.Name)[variableReferenceNode.Name];
                }
                else
                {
                    AddError($"Variable '{variableReferenceNode.Name}' does not exist.");
                    return null;
                }
            case VariableDeclarationNode variableDeclarationNode:
                if (scopes.Peek().ContainsKey(variableDeclarationNode.Name))
                {
                    AddError($"Variable '{variableDeclarationNode.Name}' already exists.");
                    return null;
                }
                else
                {
                    scopes.Peek().Add(variableDeclarationNode.Name, variableDeclarationNode.VarType);
                    if (variableDeclarationNode.Initializer != null)
                    {
                        Type initializerType = Check(variableDeclarationNode.Initializer);
                        if (initializerType != variableDeclarationNode.VarType)
                        {
                            AddError($"Son diferentes los tipos initializerType: {initializerType}  variableType: {variableDeclarationNode.VarType}");
                        }
                    }

                    return null;
                }
            case VariableAssignmentNode variableAssignmentNode:
                if (FindVariableScope(variableAssignmentNode.Name) != null)
                {
                    Type valueToAssignType = Check(variableAssignmentNode.Value);
                    Type variableType = FindVariableScope(variableAssignmentNode.Name)[variableAssignmentNode.Name];
                    if (variableType != valueToAssignType)
                    {
                        AddError($"Son diferentes los tipos initializerType: {valueToAssignType}  variableType: {variableType}");

                    }
                    return null;
                }
                else
                {
                    AddError($"Variable '{variableAssignmentNode.Name}' does not exist.");
                    return null;
                }
            /*
            case IfNode ifNode:
                object? conditionValue = Evaluate(ifNode.Condition);
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
                object? loopConditionValue = Evaluate(whileNode.Condition);
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
            */
            default:
                AddError($"Unhandled node type: {node}");
                return null;
        }
    }

    private string GetMyType(Type type){
        if(type == typeof(string)) return "String";
        else if(type == typeof(double)) return "Number";
        else if(type == typeof(bool)) return "Boolean";
        else return "Unknow";
    }

    private Type EvaluateBinaryExpression(BinaryExpressionNode node)
    {
        Type left = Check(node.Left);
        Type right = Check(node.Right);

        switch (node.Operator.Type)
        {
            case TokenType.AdditionToken:
                if (left == typeof(double) && right == typeof(double))
                {
                    return typeof(double);
                }
                else
                {
                    AddError($"Cannot add {GetMyType(left)} and {GetMyType(right)}");
                    return null;
                }
            case TokenType.SubtractionToken:
                if (left == typeof(double) && right == typeof(double))
                {
                    return typeof(double);
                }
                else
                {
                    AddError($"Cannot subtract {GetMyType(left)} and {GetMyType(right)}");
                    return null;
                }
            case TokenType.MultiplicationToken:
                if (left == typeof(double) && right == typeof(double))
                {
                    return typeof(double);
                }
                else
                {
                    AddError($"Cannot multiply {GetMyType(left)} and {GetMyType(right)}");
                    return null;
                }
            case TokenType.DivisionToken:
                if (left == typeof(double) && right == typeof(double))
                {
                    return typeof(double);
                }
                else
                {
                    AddError($"Cannot divide {GetMyType(left)} and {GetMyType(right)}");
                    return null;
                }
            case TokenType.EqualityToken:
                return typeof(bool);
            case TokenType.InequalityToken:
                return typeof(bool);
            case TokenType.LessThanToken:
                if (left == typeof(double) && right == typeof(double))
                {
                    return typeof(bool);
                }
                else
                {
                    AddError($"Cannot compare {GetMyType(left)} and {GetMyType(right)}");
                    return null;
                }
            case TokenType.LessThanOrEqualToken:
                if (left == typeof(double) && right == typeof(double))
                {
                    return typeof(bool);
                }
                else
                {
                    AddError($"Cannot compare {GetMyType(left)} and {GetMyType(right)}");
                    return null;
                }
            case TokenType.GreaterThanToken:
                if (left == typeof(double) && right == typeof(double))
                {
                    return typeof(bool);
                }
                else
                {
                    AddError($"Cannot compare {GetMyType(left)} and {GetMyType(right)}");
                    return null;
                }
            case TokenType.GreaterThanOrEqualToken:
                if (left == typeof(double) && right == typeof(double))
                {
                    return typeof(bool);
                }
                else
                {
                    AddError($"Cannot compare {GetMyType(left)} and {GetMyType(right)}");
                    return null;
                }
            case TokenType.LogicalAndToken:
                if (left == typeof(bool) && right == typeof(bool))
                {
                    return typeof(bool);
                }
                else
                {
                    AddError($"Cannot perform logical AND on {GetMyType(left)} and {GetMyType(right)}");
                    return null;
                }
            case TokenType.LogicalOrToken:
                if (left == typeof(bool) && right == typeof(bool))
                {
                    return typeof(bool);
                }
                else
                {
                    AddError($"Cannot perform logical OR on {GetMyType(left)} and {GetMyType(right)}");
                    return null;
                }
            default:
                AddError($"Unknown binary operator {node.Operator.Value}");
                return null;
        }
    }

    private Type EvaluateUnaryExpression(UnaryExpressionNode node)
    {
        Type operand = Check(node.Expression);

        switch (node.Operator.Type)
        {
            case TokenType.SubtractionToken:
                if (operand == typeof(double))
                {
                    return typeof(double);
                }
                else
                {
                    AddError($"Cannot negate {GetMyType(operand)}");
                    return null;
                }
            case TokenType.LogicalNotToken:
                if (operand == typeof(bool))
                {
                    return typeof(bool);
                }
                else
                {
                    AddError($"Cannot perform logical NOT on {GetMyType(operand)}");
                    return null;
                }
            default:
                AddError($"Unknown unary operator {node.Operator.Value}");
                return null;
        }
    }
    private Type? CheckBlock(List<Node> statements)
    {
        Type result = null;
        foreach (Node statement in statements)
        {
            result = Check(statement);
        }
        return null;
    }

    private void AddError(string message)
    {
        if (error == "")
        {
            error = message;
        }
    }
}