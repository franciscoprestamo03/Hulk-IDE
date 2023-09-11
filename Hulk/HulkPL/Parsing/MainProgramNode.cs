using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HulkPL
{
    public class MainProgramNode : Node
    {
        public List<Node> Body { get; }


        public MainProgramNode(List<Node> body)
        {

            Body = body;
        }

        public override void Accept(Visitor visitor)
        {
            throw new NotImplementedException();
        }
    }
}