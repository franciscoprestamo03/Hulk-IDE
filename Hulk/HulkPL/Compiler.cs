using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;



namespace HulkPL
{
    public static class Compiler
    {


        public static string Compile(string code)
        {
            string result = "";

            var tokens = Lexer.Lex(code);
            foreach (var item in tokens)
            {
                System.Console.WriteLine($"{item.Type}  {item.Value}");
            }

            for (int i = 0; i < tokens.Count; i++)
            {
                if (tokens[i].Type == TokenType.WhiteSpaceToken)
                {
                    tokens.RemoveAt(i);
                }
            }


            System.Console.WriteLine("llamando parser");
            Parser parser = new Parser(tokens);
            MainProgramNode mainNode;
            string parserError;
            (mainNode,parserError) = parser.Parse();
            System.Console.WriteLine("parser error: " + parserError);
            if(parserError!=""){
                return parserError;
            }

            // Create an instance of the TypeChecker class
            System.Console.WriteLine("llamando typechecker");
            TypeChecker typeChecker = new TypeChecker();

            // Perform type checking on the root node and get the resulting type
            string typingError = typeChecker.checkTypes(mainNode);
            System.Console.WriteLine(typingError);
            if(typingError!=""){
                return typingError;
            }
            System.Console.WriteLine("llamando evaluator");
            Evaluator evaluator = new Evaluator();
            result = evaluator.EvaluateMain(mainNode);


            return result;
        }
    }
}