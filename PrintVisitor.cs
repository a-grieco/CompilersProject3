using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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

        private void PrintAttribute(AbstractNode node)
        {
            if (node.AttributesRef != null)
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.Write("  " + node.AttributesRef);
                if (node.TypeDescriptor is ErrorDescriptor)
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.Write("  " + node.TypeDescriptor.GetType().Name);
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine(" " + ((ErrorDescriptor)
                        node.TypeDescriptor).Message);
                }
                else { Console.WriteLine();}
            }
            else if (node.TypeDescriptor != null)
            {
                if (node.TypeDescriptor is ErrorDescriptor)
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.Write("  " + node.TypeDescriptor.GetType().Name);
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine(" " + ((ErrorDescriptor)
                        node.TypeDescriptor).Message);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine("  " + node.TypeDescriptor.GetType().Name);
                }
            }
            else { Console.WriteLine(); }
            Console.ResetColor();
        }

        public void VisitNode(AbstractNode node)
        {
            //Console.WriteLine("<" + node.ClassName() + ">");
            Console.Write("<" + node.ClassName() + ">");
            Console.ResetColor();
            PrintAttribute(node);
        }

        public void VisitNode(Modifiers node)
        {
            Console.Write("<" + node.ClassName() + ">: ");
            var stringEnums = node.ModifierTokens.Select(x => x.ToString());
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(string.Join(", ", stringEnums));
            Console.ResetColor();
            PrintAttribute(node);
        }

        public void VisitNode(Identifier node)
        {
            Console.Write("<" + node.ClassName() + ">: ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(node.ID);
            Console.ResetColor();
            PrintAttribute(node);
        }

        //public void VisitNode(PrimitiveType node)
        //{
        //    Console.Write("<" + node.ClassName() + ">: ");
        //    Console.ForegroundColor = ConsoleColor.Green;
        //    Console.WriteLine(node.Type);
        //    Console.ResetColor();
        //}

        public void VisitNode(PrimitiveTypeInt node)
        {
            Console.Write("<" + node.ClassName() + ">: ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("INT");
            Console.ResetColor();
            PrintAttribute(node);
        }

        public void VisitNode(PrimitiveTypeBoolean node)
        {
            Console.Write("<" + node.ClassName() + ">: ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("BOOLEAN");
            Console.ResetColor();
            PrintAttribute(node);
        }

        public void VisitNode(PrimitiveTypeVoid node)
        {
            Console.Write("<" + node.ClassName() + ">: ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("VOID");
            Console.ResetColor();
            PrintAttribute(node);
        }

        public void VisitNode(Expression node)
        {
            Console.Write("<" + node.ClassName() + ">: ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(node.ExpressionType);
            Console.ResetColor();
            PrintAttribute(node);
        }

        public void VisitNode(SelectionStatement node)
        {
            Console.Write("<" + node.ClassName() + ">: ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("IF");
            Console.ResetColor();
            PrintAttribute(node);
        }

        public void VisitNode(ElseStatement node)
        {
            Console.Write("<" + node.ClassName() + ">: ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("ELSE");
            Console.ResetColor();
            PrintAttribute(node);
        }

        public void VisitNode(IterationStatement node)
        {
            Console.Write("<" + node.ClassName() + ">: ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("WHILE");
            Console.ResetColor();
            PrintAttribute(node);
        }

        public void VisitNode(SpecialName node)
        {
            Console.Write("<" + node.ClassName() + ">: ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(node.Name);
            Console.ResetColor();
            PrintAttribute(node);
        }

        public void VisitNode(Literal node)
        {
            Console.Write("<" + node.ClassName() + ">: ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(node.Lit);
            Console.ResetColor();
            PrintAttribute(node);
        }

        public void VisitNode(Number node)
        {
            Console.Write("<" + node.ClassName() + ">: ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(node.Num);
            Console.ResetColor();
            PrintAttribute(node);
        }
    }
}
