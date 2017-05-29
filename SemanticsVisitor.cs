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
                if (modError != null) { error = modError; }
                else { classBody.Accept(this); }
            }
            else
            {
                error = node.TypeDescriptor as ErrorDescriptor;
                if (error == null)
                {
                    error = new ErrorDescriptor("Type Checking failed at " +
                                "Class Declaration: node type is '" +
                                node.TypeDescriptor + "', but should be " +
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

        private void VisitNode(Expression node)
        {
            string message = "Expression type " + node.ExpressionType +
                " not implemented.";

            switch (node.ExpressionType)
            {
                case ExpressionEnums.EQUALS:
                    node.TypeDescriptor = AssignmentExp
                        (node.Child, node.Child.Sib);
                    break;
                case ExpressionEnums.OP_LOR:
                    node.TypeDescriptor = BoolBinaryExp
                        (node.Child, node.Child.Sib, node.ExpressionType);
                    break;
                case ExpressionEnums.OP_LAND:
                    node.TypeDescriptor = BoolBinaryExp
                        (node.Child, node.Child.Sib, node.ExpressionType);
                    break;
                case ExpressionEnums.PIPE:
                    node.TypeDescriptor = new ErrorDescriptor(message);
                    break;
                case ExpressionEnums.HAT:
                    node.TypeDescriptor = new ErrorDescriptor(message);
                    break;
                case ExpressionEnums.AND:
                    node.TypeDescriptor = new ErrorDescriptor(message);
                    break;
                case ExpressionEnums.OP_EQ:
                    node.TypeDescriptor = BoolBinaryExp
                        (node.Child, node.Child.Sib, node.ExpressionType);
                    break;
                case ExpressionEnums.OP_NE:
                    node.TypeDescriptor = BoolBinaryExp
                        (node.Child, node.Child.Sib, node.ExpressionType);
                    break;
                case ExpressionEnums.OP_GT:
                    node.TypeDescriptor = BoolBinaryExp
                        (node.Child, node.Child.Sib, node.ExpressionType);
                    break;
                case ExpressionEnums.OP_LT:
                    node.TypeDescriptor = BoolBinaryExp
                        (node.Child, node.Child.Sib, node.ExpressionType);
                    break;
                case ExpressionEnums.OP_LE:
                    node.TypeDescriptor = BoolBinaryExp
                        (node.Child, node.Child.Sib, node.ExpressionType);
                    break;
                case ExpressionEnums.OP_GE:
                    node.TypeDescriptor = BoolBinaryExp
                        (node.Child, node.Child.Sib, node.ExpressionType);
                    break;
                case ExpressionEnums.PLUSOP:
                    node.TypeDescriptor = EvalBinaryExp
                        (node.Child, node.Child.Sib, node.ExpressionType);
                    break;
                case ExpressionEnums.MINUSOP:
                    node.TypeDescriptor = EvalBinaryExp
                        (node.Child, node.Child.Sib, node.ExpressionType);
                    break;
                case ExpressionEnums.ASTERISK:
                    node.TypeDescriptor = EvalBinaryExp
                        (node.Child, node.Child.Sib, node.ExpressionType);
                    break;
                case ExpressionEnums.RSLASH:
                    node.TypeDescriptor = EvalBinaryExp
                        (node.Child, node.Child.Sib, node.ExpressionType);
                    break;
                case ExpressionEnums.PERCENT:
                    node.TypeDescriptor = EvalBinaryExp
                        (node.Child, node.Child.Sib, node.ExpressionType);
                    break;
                case ExpressionEnums.UNARY:
                    Console.WriteLine(message + " BUT IT SHOULD BE");
                    node.TypeDescriptor = new ErrorDescriptor(message);
                    break;
                default:
                    node.TypeDescriptor = new ErrorDescriptor
                        ("Expression type not found");
                    break;
            }
        }

        private void VisitNode(PrimaryExpression node)
        {
            node.TypeDescriptor = PrimaryExp(node);
        }

        private TypeDescriptor AssignmentExp(AbstractNode qualName, AbstractNode exp)
        {
            QualifiedName name = qualName as QualifiedName;
            Expression expression = exp as Expression;

            Attributes nameAttr;
            TypeDescriptor nameDesc;
            if (name != null && expression != null)
            {
                nameAttr = Table.lookup(name.GetStringName());
                expression.Accept(this);

                if (nameAttr.IsAssignable)
                {
                    if (nameAttr.TypeDescriptor is PrimitiveTypeIntDescriptor
                        && expression.TypeDescriptor is NumberTypeDescriptor)
                    {
                        nameDesc = nameAttr.TypeDescriptor;
                    }
                    else if (nameAttr.TypeDescriptor.GetType()
                             == expression.TypeDescriptor.GetType())
                    {
                        nameDesc = nameAttr.TypeDescriptor;
                    }
                    else
                    {
                        string message = "TODO: Add to AssignmentExp in " +
                                         "Semantics Visitor";
                        Console.WriteLine(message);
                        nameDesc = new ErrorDescriptor(message);
                    }
                }
                //   && nameAttr.TypeDescriptor.GetType() ==
                //    expression.TypeDescriptor.GetType())
                //{
                //    nameDesc = nameAttr.TypeDescriptor;
                //}
                else
                {
                    nameDesc = new ErrorDescriptor("Cannot assign " +
                        expression.TypeDescriptor.GetType().Name + " to " +
                        nameAttr.TypeDescriptor.GetType().Name);
                }
            }
            else
            {
                string message = "";
                if (name == null)
                {
                    message += "EQUALS expression expects QualifiedName on " +
                               "LHS, but has: " + qualName.NodeType + "\n";
                }
                if (expression == null)
                {
                    message += "EQUALS expression expects Expression on " +
                               "RHS, but has: " + exp.NodeType;
                }
                nameDesc = new ErrorDescriptor(message);
            }
            return nameDesc;
        }

        // BoolBinaryExp & EvalBinaryExp = same for now, may change w/code gen
        private TypeDescriptor BoolBinaryExp(AbstractNode lhs,
            AbstractNode rhs, ExpressionEnums op)
        {
            lhs.Accept(this);
            rhs.Accept(this);

            if (lhs.TypeDescriptor.GetType() == rhs.TypeDescriptor.GetType())
            {
                return lhs.TypeDescriptor;
            }
            return new ErrorDescriptor("Cannot compare " +
                lhs.TypeDescriptor.GetType().Name + " with " +
                rhs.TypeDescriptor.GetType().Name);
        }

        private TypeDescriptor EvalBinaryExp(AbstractNode lhs, AbstractNode rhs,
            ExpressionEnums op)
        {
            lhs.Accept(this);
            rhs.Accept(this);

            if (lhs.TypeDescriptor.GetType() == rhs.TypeDescriptor.GetType())
            {
                return lhs.TypeDescriptor;
            }
            return new ErrorDescriptor("Cannot compare " +
                lhs.TypeDescriptor.GetType().Name + " with " +
                rhs.TypeDescriptor.GetType().Name);
        }

        private TypeDescriptor PrimaryExp(AbstractNode node)
        {
            AbstractNode child = node.Child;    // QualifiedName
                                                // NotJustName not implemented
            QualifiedName name = child as QualifiedName;
            if (name != null)
            {
                Attributes attr = Table.lookup(name.GetStringName());
                return attr.TypeDescriptor;
            }
            Number num = child as Number;
            if (num != null)
            {
                return num.TypeDescriptor;
            }
            Literal literal = child as Literal;
            if (literal != null)
            {
                return literal.TypeDescriptor;
            }
            return new ErrorDescriptor("Expected QualifiedName, Number, or " +
                                       "literal as Primary Expression");
        }
    }
}
