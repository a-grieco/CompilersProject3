using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Project3
{
    class SemanticsVisitor : IReflectiveVisitor
    {
        // This method is the key to implenting the Reflective Visitor pattern
        // in C#, using the 'dynamic' type specification to accept any node type.
        // The subsequent call to VisitNode does dynamic lookup to find the
        // appropriate Version.

        public void Visit(dynamic node)
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
            // TODO: create pattern of traversal here
            node.Accept(this);
            VisitChildren(node);
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

        }

        //public void VisitNode(Modifiers node)
        //{

        //}
    }

    class TopDeclVisitor : SemanticsVisitor
    {
        public void Visit(dynamic node)
        {
            this.VisitNode(node);
        }


        private void VisitNode(AbstractNode node)
        {

        }

        // Variable List Declaring

        // TypeDeclaring

        // ClassDeclaring
        private void VisitNode(ClassDeclaration node)
        {
            ClassDescription desc = new ClassDescription();
            AbstractNode modNode = node.Child;
            AbstractNode idNode = modNode.Sib;
            AbstractNode bodyNode = idNode.Sib;

            if (bodyNode.Sib != null)
            {
                // too many children, add errorType to symbol table
            }




            desc.Modifiers = (Modifiers) node.Child;

        }

        // MethodDeclaring
    }

    class TypeVisitor : TopDeclVisitor
    {
        public void Visit(dynamic node)
        {
            this.VisitNode(node);
        }


        public void VisitNode(AbstractNode node)
        {
            Console.WriteLine("Do nothing for  "+ node.NodeType.ToString());
        }

        private void VisitNode(Identifier id)
        {


        }
    }
}
