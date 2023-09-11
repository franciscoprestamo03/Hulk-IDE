using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HulkPL
{
    public class Token
    {
        public TokenType Type { get; set; }
        public string Value { get; set; }

        public Token(TokenType type, string value)
        {
            this.Type = type;
            this.Value = value;
    }   
    }
}