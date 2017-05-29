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
        public static MethodTypeDescriptor CurrentMethod { get; set; }

        public TypeVisitor TypeVisitor { get; set; }

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
                ErrorDescriptor modError = checkModifiers(descriptor.Modifiers);
                if (modError != null)
                {
                    error = modError;
                }
                else
                {
                    // TO DO: is there a need to check identifiers?
                    //check class body
                    classBody.Accept(this);
                }
            }
            else
            {
                error = node.TypeDescriptor as ErrorDescriptor;
                if (error == null)  //TODO: fix me
                {
                    error = new ErrorDescriptor("Type Checking failed at " +
                                      "Class Declaration: node type is '" +
                                      node.TypeDescriptor + "', should be " +
                                      "'ClassTypeDescriptor' or 'Error'");
                }
            }

            if (error != null) { Console.WriteLine(error.Message); }
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

        private void VisitNode(MethodCall node)
        {
            AbstractNode methodReference = node.Child;
            AbstractNode argumentList = methodReference.Sib;    // may be null

            Attributes attr;
            TypeDescriptor descriptor = null;
            List<TypeDescriptor> argListTypes = new List<TypeDescriptor>();

            QualifiedName qualifiedName = methodReference as QualifiedName;
            if (qualifiedName == null)
            {
                descriptor = new ErrorDescriptor
                    ("Only Qualified Name supported for Method Call reference");
            }
            else
            {
                // get parameters from method call
                if (argumentList != null)
                {
                    AbstractNode expression = argumentList.Child;
                    while (expression != null)
                    {
                        expression.Accept(this);
                        argListTypes.Add(expression.TypeDescriptor);
                        expression = expression.Sib;
                    }
                }
                // get parameters (signature) from declared method
                attr = Table.lookup(qualifiedName.GetStringName());
                SignatureDescriptor methodSignature =
                    attr.TypeDescriptor as SignatureDescriptor;

                if (methodSignature != null)
                {
                    // check that parameter types match
                    Boolean isMatch =
                        checkParameters(methodSignature, argListTypes);
                    if (isMatch)
                    {
                        // replace sig parameters & remove Next (no overload)
                        descriptor = methodSignature;
                        ((SignatureDescriptor)descriptor).ParameterTypes =
                            argListTypes;
                        ((SignatureDescriptor)descriptor).Next = null;
                    }
                }
                else
                {
                    descriptor = new ErrorDescriptor("No signature" +
                        " found for method: " + qualifiedName.GetStringName());
                }

                // TODO: remove print
                ErrorDescriptor error = descriptor as ErrorDescriptor;
                if (error != null) { Console.WriteLine(error.Message); }

                node.TypeDescriptor = descriptor;
                Attributes methodCallAttr = new Attr(descriptor);
                methodCallAttr.Kind = Kind.MethodType;
                node.AttributesRef = methodCallAttr;
            }
        }

        private bool checkParameters(SignatureDescriptor sig, List<TypeDescriptor> param)
        {
            Boolean matchFound = false;
            List<TypeDescriptor> sigParam;

            // check each signature type in sig
            while (sig != null && !matchFound)
            {
                matchFound = true;
                sigParam = sig.ParameterTypes;

                //check current signature parameters against param
                if (sigParam.Count == param.Count)
                {
                    for (int i = 0; i < param.Count; i++)
                    {
                        Console.WriteLine(sigParam[i] + " " + param[i]);
                        if (sigParam[i].GetType() != param[i].GetType())
                        {
                            matchFound = false;
                        }
                    }
                }
                else { matchFound = false; }

                sig = sig.Next; // go to next signature type
            }
            return matchFound;
        }
    }
}
