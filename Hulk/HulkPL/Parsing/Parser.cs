namespace HulkPL;

public class Parser
{
    private List<Token> tokens;
    private int currentTokenIndex;
    private Dictionary<TokenType, int> precedence;

    public Parser(List<Token> tokens)
    {
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

    public Node Parse()
    {
        List<Node> statements = new List<Node>();
        while (!IsAtEnd())
        {
            Node statement = ParseStatement();

            if (statement != null)
            {
                statements.Add(statement);
            }
        }
        MainProgramNode mainProgram = new MainProgramNode(statements);
        return mainProgram;
    }

    private Node ParseStatement()
    {
        if (Match(TokenType.PrintToken))
        {
            Node expr = ParseExpression();
            Consume(TokenType.SemicolonToken, "Expected ';' after expression.");
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
        else if (Match(TokenType.WhileToken))
        {
            Node condition = ParseExpression();
            Consume(TokenType.LeftBraceToken, "Expected '{' after while condition.");
            List<Node> bodyStatements = ParseBlock();
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
                    Node expr = ParseExpression();

                    VariableType variableType = VariableType.Implicit;

                    if (Check(TokenType.TwoDotsToken))
                    {
                        Advance();
                        if (Check(TokenType.StringTypeToken))
                        {
                            variableType = VariableType.String;
                            Advance();
                        }
                        else if (Check(TokenType.NumberTypeToken))
                        {
                            variableType = VariableType.Number;
                            Advance();
                        }
                        else
                        {
                            throw new Exception("Expected variable type after : ");
                        }
                    }


                    if (Match(TokenType.AssignmentToken))
                    {
                        Node right = ParseExpression();

                        variables.Add(new VariableDeclarationNode(((VariableReferenceNode)expr).Name, right, variableType));
                    }
                    else
                    {
                        variables.Add(new VariableDeclarationNode(((VariableReferenceNode)expr).Name, null, variableType));
                    }



                } while (Match(TokenType.CommaToken));
            }
            Consume(TokenType.RightParenthesisToken, "Expected ')' after function parameters.");
            if (Match(TokenType.ArrowToken))
            {
                Node returnNode2 = ParseExpression();
                Consume(TokenType.SemicolonToken, "Expected ';' after return statement.");
                return new FunctionDeclarationNode(functionName.Value, variables, null, returnNode2);
            }


            Consume(TokenType.LeftBraceToken, "Expected '{' before function body.");



            List<Node> body = new List<Node>();
            Node returnNode = null;
            while (!Check(TokenType.RightBraceToken) && !IsAtEnd())
            {
                if (Check(TokenType.ReturnToken))
                {
                    Advance();
                    Node returnValue = ParseExpression();
                    Consume(TokenType.SemicolonToken, "Expected ';' after return statement.");
                    returnNode = returnValue;
                    break;
                }
                Node statement = ParseStatement();
                if (statement != null)
                {
                    body.Add(statement);
                }
            }
            Consume(TokenType.RightBraceToken, "Expected '}' after block.");

            Console.WriteLine(returnNode);
            return new FunctionDeclarationNode(functionName.Value, variables, body, returnNode);
        }
        else if (Match(TokenType.VarToken))
        {
            Token identifier = Consume(TokenType.IdentifierToken, "Expected variable name after 'var' keyword.");
            VariableType variableType = VariableType.Implicit;

            if (Check(TokenType.TwoDotsToken))
            {
                Advance();
                if (Check(TokenType.StringTypeToken))
                {
                    variableType = VariableType.String;
                    Advance();
                }
                else if (Check(TokenType.NumberTypeToken))
                {
                    variableType = VariableType.Number;
                    Advance();
                }
                else
                {
                    throw new Exception("Expected variable type after : ");
                }
            }

            if (Match(TokenType.AssignmentToken))
            {
                Node initializer = ParseExpression();
                Consume(TokenType.SemicolonToken, "Expected ';' after variable declaration.");
                return new VariableDeclarationNode(identifier.Value, initializer, variableType);
            }
            else
            {
                Consume(TokenType.SemicolonToken, "Expected ';' after variable declaration.");
                return new VariableDeclarationNode(identifier.Value, new NumberNode(0), variableType);
            }
        }
        else if (Match(TokenType.LetToken))
        {
            List<Node> variables = new List<Node>();
            List<Node> initializers = new List<Node>();

            do
            {
                Node expr = ParseExpression();

                VariableType variableType = VariableType.Implicit;

                if (Check(TokenType.TwoDotsToken))
                {
                    Advance();
                    if (Check(TokenType.StringTypeToken))
                    {
                        variableType = VariableType.String;
                        Advance();
                    }
                    else if (Check(TokenType.NumberTypeToken))
                    {
                        variableType = VariableType.Number;
                        Advance();
                    }
                    else
                    {
                        throw new Exception("Expected variable type after : ");
                    }
                }


                if (Match(TokenType.AssignmentToken))
                {
                    Node right = ParseExpression();

                    variables.Add(new VariableDeclarationNode(((VariableReferenceNode)expr).Name, right, variableType));
                }
                else
                {
                    throw new Exception("Expected assignation value after variable");
                }
                /*
                if (Match(TokenType.AssignmentToken))
                {
                    Node initializer = ParseExpression();
                    initializers.Add(initializer);
                }
                */


            } while (Match(TokenType.CommaToken));

            Consume(TokenType.InToken, "Expected 'in' keyword after let bindings.");
            Consume(TokenType.LeftBraceToken, "Expected '{' after while condition.");
            List<Node> body = ParseBlock();



            return new LetNode(variables, body);
        }
        else
        {
            Node expr = ParseExpression();
            if (Match(TokenType.AssignmentTwoDotsToken))
            {
                Node right = ParseExpression();
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

    private List<Node> ParseBlock()
    {
        List<Node> statements = new List<Node>();
        while (!Check(TokenType.RightBraceToken) && !IsAtEnd())
        {
            Node statement = ParseStatement();
            if (statement != null)
            {
                statements.Add(statement);
            }
        }
        Consume(TokenType.RightBraceToken, "Expected '}' after block.");
        return statements;
    }

    private Node ParseExpression()
    {
        return ParseBinaryExpression(0);
    }



    private Node ParseBinaryExpression(int minPrecedence)
    {
        Node left = ParseUnaryExpression();
        while (true)
        {
            Token operator1 = Peek();
            TokenType operatorType = operator1.Type;
            if (!precedence.ContainsKey(operatorType) || precedence[operatorType] < minPrecedence)
            {
                break;
            }
            Advance();
            Node right = ParseBinaryExpression(precedence[operatorType] + 1);
            left = new BinaryExpressionNode(left, operator1, right);
        }
        return left;
    }

    private Node ParseUnaryExpression()
    {
        if (Match(TokenType.SubtractionToken))
        {
            Token op = Previous();
            Node expr = ParseUnaryExpression();
            return new UnaryExpressionNode(op, expr);
        }
        else
        {
            return ParsePrimaryExpression();
        }
    }

    private Node ParsePrimaryExpression()
    {
        if (Match(TokenType.IfToken))
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
                        Node argument = ParseExpression();
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
            Node expr = ParseExpression();
            Consume(TokenType.RightParenthesisToken, "Expected ')' after expression.");
            return new GroupingNode(expr);
        }
        else if (Match(TokenType.StringToken))
        {
            return new StringNode(Previous().Value);
        }
        else
        {
            throw new Exception("Expected expression." + Peek().Type);
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
            throw new Exception(errorMessage);
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
}

