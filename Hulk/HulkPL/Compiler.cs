using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;



namespace HulkPL
{
    public static class Compiler
    {


        public static string Compile(string code){
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

            Parser parser = new Parser(tokens);
            Node mainNode = parser.Parse();
            Evaluator evaluator = new Evaluator();
            result = evaluator.EvaluateMain(mainNode);


            return result;
        }
    }
}