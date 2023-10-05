using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;



namespace HulkPL
{
    public static class Compiler
    {

        
        public static string Compile(string code)
        {
            string result = "";

            List<Token> tokens = new();
            try{
            tokens = Lexer.Lex(code);
            }catch (Exception e)
            {
                System.Console.WriteLine(e.Message);
                return e.Message;
            }


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

            Parser parser = new Parser(tokens);
            MainProgramNode mainNode;
            try
            {
                mainNode = parser.Parse();
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.Message);
                return e.Message;
            }

            Evaluator evaluator = new Evaluator();
            try
            {
                result = evaluator.EvaluateMain(mainNode);
                result += "\nCode compiled successfully!";
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.Message);
                return e.Message;
            }


            return result;
        }
    }
}