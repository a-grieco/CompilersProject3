using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project3
{
    class PrintVisitor : IReflectiveVisitor
    {
        // This method is the key to implenting the Reflective Visitor pattern
        // in C#, using the 'dynamic' type specification to accept any node type.
        // The subsequent call to VisitNode does dynamic lookup to find the
        // appropriate Version.

        public void Visit(dynamic node)
        {
            this.VisitNode(node);
        }

        // Call this method to begin the tree printing process
        public void PrintTree(AbstractNode node, string prefix = "")
        {
            if (node == null)
            {
                return;
            }
            Console.Write(prefix);
            node.Accept(this);
            VisitChildren(node, prefix + "   ");
        }

        public void VisitChildren(AbstractNode node, String prefix)
        {
            AbstractNode child = node.Child;
            while (child != null)
            {
                PrintTree(child, prefix);
                child = child.Sib;
            };
        }

        public void VisitNode(AbstractNode node)
        {
            Console.WriteLine("<" + node.ClassName() + ">");
        }

        public void VisitNode(Modifiers node)
        {
            Console.Write("<" + node.ClassName() + ">: ");
            // Add code here to print Modifier info
            var stringEnums = node.ModifierTokens.Select(x => x.ToString());
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(string.Join(", ", stringEnums));
            Console.ResetColor();
        }

        public void VisitNode(Identifier node)
        {
            Console.Write("<" + node.ClassName() + ">: ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(node.ID);
            Console.ResetColor();

        }
        public void VisitNode(PrimitiveType node)
        {
            Console.Write("<" + node.ClassName() + ">: ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(node.Type);
            Console.ResetColor();
        }
        public void VisitNode(Expression node)
        {
            Console.Write("<" + node.ClassName() + ">: ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(node.ExpressionType);
            Console.ResetColor();
        }

        public void VisitNode(SpecialName node)
        {
            Console.Write("<" + node.ClassName() + ">: ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(node.Name);
            Console.ResetColor();
        }

        public void VisitNode(Literal node)
        {
            Console.Write("<" + node.ClassName() + ">: ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(node.Lit);
            Console.ResetColor();
        }

        public void VisitNode(Number node)
        {
            Console.Write("<" + node.ClassName() + ">: ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(node.Num);
            Console.ResetColor();
        }
    }
}
