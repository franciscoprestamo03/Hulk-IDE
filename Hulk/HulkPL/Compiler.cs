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
            System.Console.WriteLine("aaaaaaa");
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
            (mainNode, parserError) = parser.Parse();
            System.Console.WriteLine("parser error: " + parserError);
            if (parserError != "")
            {
                return parserError;
            }

            System.Console.WriteLine("llamando evaluator");
            Evaluator evaluator = new Evaluator();
            try{
            result = evaluator.EvaluateMain(mainNode);}
            catch(Exception ex){
                return ex.Message;
            }


            return result;
        }
    }
}