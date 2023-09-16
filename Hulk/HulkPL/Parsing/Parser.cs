namespace HulkPL;

public class Parser
{
    private List<Token> tokens;
    private int currentTokenIndex;
    private Dictionary<TokenType, int> precedence;
    private string error;

    public Parser(List<Token> tokens)
    {
        this.error = "";
        this.tokens = tokens;
        this.currentTokenIndex = 0;
        this.precedence = new Dictionary<TokenType, int>
        {
            { TokenType.EqualityToken, 1 },
            { TokenType.InequalityToken, 1 },
            { TokenType.LogicalAndToken,1},
            { TokenType.LogicalOrToken,1},
            { TokenType.LessThanToken, 2 },
            { TokenType.LessThanOrEqualToken, 2 },
            { TokenType.GreaterThanToken, 2 },
            { TokenType.GreaterThanOrEqualToken, 2 },
            { TokenType.AdditionToken, 3 },
            { TokenType.SubtractionToken, 3 },
            { TokenType.MultiplicationToken, 4 },
            { TokenType.DivisionToken, 4 },
        };
    }

    public (MainProgramNode, string) Parse()
    {
        List<Node> statements = new List<Node>();
        while (!IsAtEnd())
        {
            Node? statement = ParseStatement();

            if (statement != null)
            {
                statements.Add(statement);
            }
            else
            {
                break;
            }
        }
        MainProgramNode mainProgram = new MainProgramNode(statements);
        string temp = "";

        if (error != "")
        {
            temp = error;
            error = "";

        }

        return (mainProgram, temp);
    }

    private Node? ParseStatement()
    {
        System.Console.WriteLine("llamada a parse statement");
        if (Match(TokenType.PrintToken))
        {

            Node? expr = ParseExpression();
            //Consume(TokenType.SemicolonToken, "Expected ';' after expression.");
            if (expr != null)
            {
                return new PrintNode(expr);
            }
            return null;
        }
        else if (Match(TokenType.IfToken))
        {
            Node? condition = ParseExpression();
            if (condition == null)
            {
                return null;
            }

            List<Node>? thenStatements = new List<Node>();

            if (Check(TokenType.LeftBraceToken))
            {
                Consume(TokenType.LeftBraceToken, "Expected '{' after if condition.");
                thenStatements = ParseBlock();
                if (thenStatements == null)
                {
                    return null;
                }
            }
            else
            {
                Node? exp = ParseExpression();
                if (exp == null)
                {
                    return null;
                }
                thenStatements.Add(exp);
            }

            List<Node>? elseStatements = null;

            if (Match(TokenType.ElseToken))
            {
                if (Check(TokenType.LeftBraceToken))
                {
                    Consume(TokenType.LeftBraceToken, "Expected '{' after else keyword.");
                    elseStatements = ParseBlock();
                    if (elseStatements == null)
                    {
                        return null;
                    }
                }
                else
                {
                    elseStatements = new List<Node>();
                    Node? exp = ParseExpression();
                    if (exp != null)
                    {
                        elseStatements.Add(exp);
                    }
                    else
                    {
                        return null;
                    }
                }

            }

            return new IfNode(condition, thenStatements, elseStatements);
        }
        else if (Match(TokenType.WhileToken))
        {
            Node? condition = ParseExpression();
            if (condition == null)
            {
                return null;
            }
            Consume(TokenType.LeftBraceToken, "Expected '{' after while condition.");
            List<Node>? bodyStatements = ParseBlock();
            if (bodyStatements == null)
            {
                return null;
            }
            return new WhileNode(condition, bodyStatements);
        }
        else if (Match(TokenType.FunctionToken))
        {
            Token functionName = Consume(TokenType.IdentifierToken, "Expected function name after 'function' keyword.");
            Consume(TokenType.LeftParenthesisToken, "Expected '(' after function name.");
            List<VariableDeclarationNode> variables = new List<VariableDeclarationNode>();
            if (!Check(TokenType.RightParenthesisToken))
            {
                do
                {
                    Node? expr = ParseExpression();
                    if (expr == null)
                    {
                        return null;
                    }

                    Type variableType1 = null;

                    if (Check(TokenType.TwoDotsToken))
                    {
                        Advance();
                        if (Check(TokenType.StringTypeToken))
                        {
                            variableType1 = typeof(string);
                            Advance();
                        }
                        else if (Check(TokenType.NumberTypeToken))
                        {
                            variableType1 = typeof(double);
                            Advance();
                        }
                        else
                        {
                            AddError("Expected variable type after : ");
                            return null;
                        }
                    }
                    else
                    {
                        AddError("Expected ':' ");
                        return null;
                    }


                    if (Match(TokenType.AssignmentToken))
                    {
                        Node? right = ParseExpression();
                        if (right == null)
                        {
                            return null;
                        }

                        variables.Add(new VariableDeclarationNode(((VariableReferenceNode)expr).Name, right, variableType1));
                    }
                    else
                    {
                        variables.Add(new VariableDeclarationNode(((VariableReferenceNode)expr).Name, null, variableType1));
                    }



                } while (Match(TokenType.CommaToken));
            }
            

            Consume(TokenType.RightParenthesisToken, "Expected ')' after function parameters.");

            Consume(TokenType.TwoDotsToken, "Expected ':' after function parameters for indicate return type.");
            Type variableType = null;
            if (Check(TokenType.StringTypeToken))
            {
                variableType = typeof(string);
                Advance();
            }
            else if (Check(TokenType.NumberTypeToken))
            {
                variableType = typeof(double);
                Advance();
            }
            else
            {
                AddError("Expected variable type after : ");
                return null;
            }

            if (Match(TokenType.ArrowToken))
            {
                Node? returnNode2 = ParseExpression();
                if (returnNode2 == null)
                {
                    return null;
                }
                Consume(TokenType.SemicolonToken, "Expected ';' after return statement.");
                return new FunctionDeclarationNode(functionName.Value, variables, null, returnNode2, variableType);
            }
            Consume(TokenType.LeftBraceToken, "Expected '{' before function body.");



            List<Node> body = new List<Node>();
            Node returnNode = null;
            while (!Check(TokenType.RightBraceToken) && !IsAtEnd())
            {
                if (Check(TokenType.ReturnToken))
                {
                    Advance();
                    Node? returnValue = ParseExpression();
                    System.Console.WriteLine(returnValue);
                    if (returnValue == null)
                    {
                        return null;
                    }
                    Consume(TokenType.SemicolonToken, "Expected ';' after return statement.");
                    returnNode = returnValue;
                    break;
                }
                else
                {
                    Node? statement = ParseStatement();
                    
                    if (statement != null)
                    {
                        body.Add(statement);
                    }
                    else
                    {
                        return null;
                    }
                }

            }
            System.Console.WriteLine("test3333333333");
            Consume(TokenType.RightBraceToken, "Expected '}' after block.");

            Console.WriteLine(returnNode);
            return new FunctionDeclarationNode(functionName.Value, variables, body, returnNode, variableType);
        }
        else if (Match(TokenType.VarToken))
        {
            Token identifier = Consume(TokenType.IdentifierToken, "Expected variable name after 'var' keyword.");
            Type variableType2 = null;

            if (Check(TokenType.TwoDotsToken))
            {
                Advance();
                if (Check(TokenType.StringTypeToken))
                {
                    variableType2 = typeof(string);
                    Advance();
                }
                else if (Check(TokenType.NumberTypeToken))
                {
                    variableType2 = typeof(double);
                    Advance();
                }
                else
                {
                    AddError("Expected variable type after : ");
                    return null;

                }
            }
            else
            {
                AddError("Expected ':' ");
                return null;
            }

            if (Match(TokenType.AssignmentToken))
            {
                Node? initializer = ParseExpression();
                if (initializer == null)
                {
                    return null;
                }
                Consume(TokenType.SemicolonToken, "Expected ';' after variable declaration.");
                return new VariableDeclarationNode(identifier.Value, initializer, variableType2);
            }
            else
            {
                Consume(TokenType.SemicolonToken, "Expected ';' after variable declaration.");
                return new VariableDeclarationNode(identifier.Value, new NumberNode(0), variableType2);
            }
        }
        else if (Match(TokenType.LetToken))
        {
            List<Node> variables = new List<Node>();
            List<Node> initializers = new List<Node>();

            do
            {
                Node? expr = ParseExpression();
                if (expr == null)
                {
                    return null;
                }

                Type variableType = null;

                if (Check(TokenType.TwoDotsToken))
                {
                    Advance();
                    if (Check(TokenType.StringTypeToken))
                    {
                        variableType = typeof(string);
                        Advance();
                    }
                    else if (Check(TokenType.NumberTypeToken))
                    {
                        variableType = typeof(double);
                        Advance();
                    }
                    else
                    {
                        AddError("Expected variable type after : ");
                        return null;
                    }
                }


                if (Match(TokenType.AssignmentToken))
                {

                    Node? right = ParseExpression();
                    if (right == null)
                    {
                        return null;
                    }

                    variables.Add(new VariableDeclarationNode(((VariableReferenceNode)expr).Name, right, variableType));
                }
                else
                {
                    AddError("Expected assignation value after variable");
                    return null;
                }



            } while (Match(TokenType.CommaToken));

            Consume(TokenType.InToken, "Expected 'in' keyword after let bindings.");

            List<Node> body = new List<Node>();

            if (Check(TokenType.LeftBraceToken))
            {
                Consume(TokenType.LeftBraceToken, "Expected '{' after while condition.");
                body = ParseBlock();
            }
            else
            {
                Node? expresion = ParseExpression();
                if (expresion == null)
                {
                    return null;
                }
                body.Add(expresion);
                Consume(TokenType.SemicolonToken, "Expected ';' after variable assignment.");
            }

            return new LetNode(variables, body);
        }
        else
        {
            Node? expr = ParseExpression();
            if (expr == null)
            {
                return null;
            }
            if (Match(TokenType.AssignmentTwoDotsToken))
            {
                Node? right = ParseExpression();
                if (right == null)
                {
                    return null;
                }

                Consume(TokenType.SemicolonToken, "Expected ';' after variable assignment.");
                return new VariableAssignmentNode(((VariableReferenceNode)expr).Name, right);
            }
            else
            {
                //Consume(TokenType.SemicolonToken, "Expected ';' after expression.");
                return expr;
            }
        }
    }

    private List<Node>? ParseBlock()
    {
        List<Node> statements = new List<Node>();
        while (!Check(TokenType.RightBraceToken) && !IsAtEnd())
        {
            Node? statement = ParseStatement();
            if (statement != null)
            {
                statements.Add(statement);
            }
            else
            {
                return null;
            }
        }
        Consume(TokenType.RightBraceToken, "Expected '}' after block.");
        return statements;
    }

    private Node? ParseExpression()
    {
        return ParseBinaryExpression(0);
    }



    private Node? ParseBinaryExpression(int minPrecedence)
    {
        Node? left = ParseUnaryExpression();
        if (left == null)
        {
            return null;
        }
        while (true)
        {
            Token operator1 = Peek();
            TokenType operatorType = operator1.Type;
            if (!precedence.ContainsKey(operatorType) || precedence[operatorType] < minPrecedence)
            {
                break;
            }
            Advance();
            Node? right = ParseBinaryExpression(precedence[operatorType] + 1);
            if (right == null)
            {
                return null;
            }
            left = new BinaryExpressionNode(left, operator1, right);
        }
        return left;
    }

    private Node? ParseUnaryExpression()
    {
        if (Match(TokenType.SubtractionToken))
        {
            Token op = Previous();
            Node? expr = ParseUnaryExpression();
            if (expr == null)
            {
                return null;
            }
            return new UnaryExpressionNode(op, expr);
        }
        else
        {
            return ParsePrimaryExpression();
        }
    }

    private Node? ParsePrimaryExpression()
    {
        if (Match(TokenType.PrintToken))
        {
            Node? expr = ParseExpression();
            if (expr == null)
            {
                return null;
            }
            //Consume(TokenType.SemicolonToken, "Expected ';' after expression.");
            return new PrintNode(expr);
        }
        else if (Match(TokenType.IfToken))
        {
            Node? condition = ParseExpression();
            if (condition == null)
            {
                return null;
            }
            List<Node>? thenStatements = new List<Node>();

            if (Check(TokenType.LeftBraceToken))
            {
                Consume(TokenType.LeftBraceToken, "Expected '{' after if condition.");
                thenStatements = ParseBlock();
                if (thenStatements == null)
                {
                    return null;
                }
            }
            else
            {
                Node? exp = ParseExpression();
                if (exp == null)
                {
                    return null;
                }
                thenStatements.Add(exp);
            }

            List<Node>? elseStatements = null;

            if (Match(TokenType.ElseToken))
            {
                if (Check(TokenType.LeftBraceToken))
                {
                    Consume(TokenType.LeftBraceToken, "Expected '{' after else keyword.");
                    elseStatements = ParseBlock();
                    if (elseStatements == null)
                    {
                        return null;
                    }
                }
                else
                {
                    elseStatements = new List<Node>();
                    Node? exp = ParseExpression();
                    if (exp == null)
                    {
                        return null;
                    }
                    elseStatements.Add(exp);
                }

            }

            return new IfNode(condition, thenStatements, elseStatements);
        }

        else if (Match(TokenType.FalseToken))
        {
            return new BooleanNode(false);
        }
        else if (Match(TokenType.TrueToken))
        {
            return new BooleanNode(true);
        }
        else if (Match(TokenType.NumberToken))
        {
            return new NumberNode(double.Parse(Previous().Value));
        }
        else if (Match(TokenType.IdentifierToken))
        {
            Token identifier = Previous();
            if (Match(TokenType.LeftParenthesisToken))
            {
                List<Node> arguments = new List<Node>();
                if (!Check(TokenType.RightParenthesisToken))
                {
                    do
                    {
                        Node? argument = ParseExpression();
                        if (argument == null)
                        {
                            return null;
                        }
                        arguments.Add(argument);
                    } while (Match(TokenType.CommaToken));
                }
                Consume(TokenType.RightParenthesisToken, "Expected ')' after function arguments.");
                return new FunctionCallNode(identifier.Value, arguments);
            }
            else
            {
                return new VariableReferenceNode(identifier.Value);
            }
        }
        else if (Match(TokenType.LeftParenthesisToken))
        {
            Node? expr = ParseExpression();
            if (expr == null)
            {
                return null;
            }
            Consume(TokenType.RightParenthesisToken, "Expected ')' after expression. " + Peek().Type + "value " + Peek().Value);
            return new GroupingNode(expr);
        }
        else if (Match(TokenType.StringToken))
        {
            return new StringNode(Previous().Value);
        }
        else
        {
            AddError("Expected expression." + Peek().Type);
            return null;
        }
    }

    private Token Consume(TokenType expectedType, string errorMessage)
    {
        if (Check(expectedType))
        {
            return Advance();
        }
        else
        {
            AddError(errorMessage);
            return null;

        }
    }

    private bool Match(TokenType type)
    {
        if (Check(type))
        {
            Advance();
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool Check(TokenType type)
    {
        if (IsAtEnd())
        {
            return false;
        }
        else
        {
            return Peek().Type == type;
        }
    }

    private Token Advance()
    {
        if (!IsAtEnd())
        {
            currentTokenIndex++;
        }
        return Previous();
    }

    private bool IsAtEnd()
    {
        return currentTokenIndex >= tokens.Count - 1;
    }

    private Token Peek()
    {
        return tokens[currentTokenIndex];
    }

    private Token Previous()
    {
        return tokens[currentTokenIndex - 1];
    }

    private void AddError(string message)
    {
        if (error == "")
        {
            error = message;
        }
    }
}

