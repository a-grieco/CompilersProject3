using System;
using System.Data;
using System.Linq;
using System.Net.Configuration;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using ASTBuilder;

namespace Project3
{
    public class SemanticsVisitor : IReflectiveVisitor
    {
        public static SymbolTable Table { get; set; }
        public static ClassAttributes CurrentClass { get; set; }
        public static MethodAttributes CurrentMethod { get; set; }

        public SemanticsVisitor(SymbolTable st)
        {
            Table = st;
        }

        // This method is the key to implenting the Reflective Visitor pattern
        // in C#, using the 'dynamic' type specification to accept any node type.
        // The subsequent call to VisitNode does dynamic lookup to find the
        // appropriate Version.

        public virtual void Visit(dynamic node)
        {
            this.VisitNode(node);
        }

        // Call this method to begin the semantic checking process
        public void CheckSemantics(AbstractNode node)
        {
            if (node == null)
            {
                return;
            }
            node.Accept(this);
        }

        public void VisitChildren(AbstractNode node)
        {
            AbstractNode child = node.Child;
            while (child != null)
            {
                CheckSemantics(child);
                child = child.Sib;
            }
        }

        public void VisitNode(AbstractNode node)
        {
            Console.WriteLine("VisitNode, SemanticsVisitor [" + node.GetType() + "]");
            VisitChildren(node);
        }
    }
}
