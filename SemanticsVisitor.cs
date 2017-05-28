using System;
using System.Collections.Generic;
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
        public static ClassTypeDescriptor CurrentClass { get; set; }
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

        private void VisitNode(ClassDeclaration node)
        {
            ErrorDescriptor error = null;
            ClassTypeDescriptor descriptor = node.TypeDescriptor as ClassTypeDescriptor;
            if (descriptor != null)
            {
                AbstractNode modifiers = node.Child;
                AbstractNode identifier = modifiers.Sib;
                AbstractNode classBody = identifier.Sib;

                // check modifiers
                //if(!(modifiers.TypeDescriptor is Modif))
                modifiers.Accept(this);
                ErrorDescriptor modError = modifiers.TypeDescriptor as ErrorDescriptor;
                if (modError != null)
                {
                    error = modError;
                }
                // TO DO: is there a need to check identifiers?
                //check class body
                classBody.Accept(this);
            }
            else
            {
                error = node.TypeDescriptor as ErrorDescriptor;
                if (error == null)
                {
                    Console.WriteLine("Type Checking failed at Class " +
                                      "Declaration: node type is '" +
                                      node.TypeDescriptor + "', s/b " +
                                      "'ClassTypeDescriptor' or 'Error'");
                }
            }

            if (error != null) { Console.WriteLine(error.Message); }
        }

        private void VisitNode(Modifiers node)
        {
            ErrorDescriptor error = checkModifiers(node.ModifierTokens);
            if (error != null)
            {
                node.TypeDescriptor = error;
            }
        }

        private ErrorDescriptor checkModifiers(List<ModifiersEnums> mods)
        {
            string message;
            ErrorDescriptor error = null;
            if (mods.Contains(ModifiersEnums.PRIVATE) &&
                    mods.Contains(ModifiersEnums.PUBLIC))
            {
                message = "Cannot contain both modifiers PUBLIC and PRVATE.";
                error = new ErrorDescriptor(message);
            }
            if (mods.Count <= 0)
            {
                message = "Must contain at least one modifier PUBLIC, " +
                          "PRIVATE, or STATIC";
                error = new ErrorDescriptor(message);
            }
            return error;
        }
    }
}
