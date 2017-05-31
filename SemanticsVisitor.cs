using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Configuration;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
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
                ErrorDescriptor modError = CheckModifiers(descriptor.Modifiers);
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


        #region Semantics Specialized Node Visits
        private void VisitNode(MethodCall node)
        {
            AbstractNode methodReference = node.Child;
            AbstractNode argumentList = methodReference.Sib;    // may be null

            TypeDescriptor descriptor;

            QualifiedName qualifiedName = methodReference as QualifiedName;
            if (qualifiedName == null)
            {
                descriptor = new ErrorDescriptor
                    ("Only Qualified Name supported for Method Call reference");
            }
            else
            {
                // get parameters from method call
                List<TypeDescriptor> argListTypes =
                    GetParameterTypes(argumentList as ArgumentList);

                // get parameters (signature) from declared method
                Attributes attr = Table.lookup(qualifiedName.GetStringName());
                MethodTypeDescriptor methodDescriptor =
                    attr.TypeDescriptor as MethodTypeDescriptor;

                if (methodDescriptor != null)
                {
                    SignatureDescriptor methodSignature = methodDescriptor.Signature;

                    if (methodSignature != null &&
                        ParametersMatch(methodSignature, argListTypes))
                    {
                        // method descriptor for only current signature 
                        MethodTypeDescriptor temp = new MethodTypeDescriptor();
                        temp.ReturnType = methodDescriptor.ReturnType;
                        temp.Signature.ParameterTypes = argListTypes;
                        temp.Signature.Next = null;
                        descriptor = temp;
                    }
                    else
                    {
                        if (methodSignature == null)
                        {
                            descriptor = new ErrorDescriptor
                                ("No signature found for method: " +
                                qualifiedName.GetStringName());
                        }
                        else
                        {
                            descriptor = new ErrorDescriptor
                                ("No method signature found matching: (" +
                                String.Join(", ", argListTypes) + ")");
                        }
                    }
                }
                else
                {
                    descriptor = new ErrorDescriptor("Method not declared: " +
                        qualifiedName.GetStringName());
                }
                node.TypeDescriptor = descriptor;
                Attributes methodCallAttr = new Attr(descriptor);
                methodCallAttr.Kind = Kind.MethodType;
                node.AttributesRef = methodCallAttr;
            }
        }

        private void VisitNode(SelectionStatement node)
        {
            AbstractNode ifExp = node.Child;
            AbstractNode thanStmt = ifExp.Sib;
            AbstractNode elseStmt = thanStmt.Sib;   // may be null

            SelectionStatementDescriptor ssDesc =
                new SelectionStatementDescriptor();

            Boolean ifEval;
            String errMsg = "";

            // if expression
            ifExp.Accept(this);
            ssDesc.IfDescriptor = ifExp.TypeDescriptor;
            PrimitiveTypeBooleanDescriptor ifBoolDesc =
                ifExp.TypeDescriptor as PrimitiveTypeBooleanDescriptor;
            if (ifBoolDesc == null)
            {
                errMsg = "If statement does not evaluate to a Boolean " +
                          "expression. (Has type: " +
                          ifExp.TypeDescriptor.GetType().Name + ")" + errMsg;
            }
            // than statement
            thanStmt.Accept(this);
            ssDesc.ThanDescriptor = thanStmt.TypeDescriptor;
            if (!IsCompatibleStatement(thanStmt.TypeDescriptor))
            {
                if (errMsg.Length > 0) { errMsg += "\n"; }
                errMsg += "Non-compatible THAN statement type: " +
                          thanStmt.TypeDescriptor.GetType().Name;
            }
            // else statement
            if (elseStmt != null)
            {
                elseStmt.Accept(this);
                ssDesc.HasElseStmt = true;
                ssDesc.ElseDescriptor = elseStmt.TypeDescriptor;
                if (!IsCompatibleStatement(elseStmt.TypeDescriptor))
                {
                    if (errMsg.Length > 0)
                    {
                        errMsg += "\n";
                    }
                    errMsg += "Non-compatible ELSE statement type: " +
                              elseStmt.TypeDescriptor.GetType().Name;
                }
            }
            else { ssDesc.HasElseStmt = false; }

            // if any components have errors, propogate them up the tree
            if (ifExp.TypeDescriptor is ErrorDescriptor ||
                thanStmt.TypeDescriptor is ErrorDescriptor ||
                elseStmt?.TypeDescriptor is ErrorDescriptor)
            {
                // creates an error containing any and all errors lower in tree
                if (ifExp.TypeDescriptor is ErrorDescriptor)
                {
                    ssDesc.TypeDescriptor = ifExp.TypeDescriptor;
                    if (thanStmt.TypeDescriptor is ErrorDescriptor)
                    {
                        ((ErrorDescriptor)ssDesc.TypeDescriptor).CombineErrors
                            ((ErrorDescriptor)thanStmt.TypeDescriptor);
                    }
                    if (elseStmt?.TypeDescriptor is ErrorDescriptor)
                    {
                        ((ErrorDescriptor)ssDesc.TypeDescriptor).CombineErrors
                            ((ErrorDescriptor)elseStmt.TypeDescriptor);
                    }
                }
                else if (thanStmt.TypeDescriptor is ErrorDescriptor)
                {
                    ssDesc.TypeDescriptor = thanStmt.TypeDescriptor;
                    if (elseStmt?.TypeDescriptor is ErrorDescriptor)
                    {
                        ((ErrorDescriptor)ssDesc.TypeDescriptor).CombineErrors
                            ((ErrorDescriptor)elseStmt.TypeDescriptor);
                    }
                }
                else
                {
                    ssDesc.TypeDescriptor = elseStmt.TypeDescriptor;
                }
            }
            // if IF/THAN/ELSE was incompatible, create new error
            if (errMsg.Length > 0)
            {
                ssDesc.TypeDescriptor = new ErrorDescriptor(errMsg);
            }
            // otherwise assign evaluated boolean to selection statement desc
            else
            {
                ssDesc.TypeDescriptor = ifExp.TypeDescriptor;
            }
            node.TypeDescriptor = ssDesc;
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
        #endregion Semantics Specialized Node Visits


        #region Semantics Helpers
        private ErrorDescriptor CheckModifiers(List<ModifiersEnums> mods)
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

        private bool ParametersMatch(SignatureDescriptor sig,
            List<TypeDescriptor> param)
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
                        //Console.Write("Comparing " + sigParam[i].GetType().Name +
                        //    " & " + param[i].GetType().Name);
                        if (!TypesCompatible(sigParam[i], param[i]))
                        {
                            matchFound = false;
                        }
                    }
                }
                else { matchFound = false; }
                //Console.WriteLine(" Match? " + matchFound);

                sig = sig.Next; // go to next signature type
            }
            return matchFound;
        }

        private bool TypesCompatible(TypeDescriptor a, TypeDescriptor b)
        {
            Console.WriteLine("a is " + a.GetType() + " b is " + b.GetType());

            return (a.GetType() == b.GetType()) ||
                (a is PrimitiveTypeIntDescriptor && b is NumberTypeDescriptor) ||
                (b is PrimitiveTypeIntDescriptor && a is NumberTypeDescriptor);
        }

        private List<TypeDescriptor> GetParameterTypes(ArgumentList argumentList)
        {
            List<TypeDescriptor> types = new List<TypeDescriptor>();
            if (argumentList != null)
            {
                AbstractNode expression = argumentList.Child;
                while (expression != null)
                {
                    expression.Accept(this);
                    types.Add(expression.TypeDescriptor);
                    expression = expression.Sib;
                }
            }
            return types;
        }

        private TypeDescriptor AssignmentExp(AbstractNode qualName, AbstractNode exp)
        {
            QualifiedName name = qualName as QualifiedName;
            Expression expression = exp as Expression;
            PrimaryExpression primaryExp = exp as PrimaryExpression;

            Attributes nameAttr;
            TypeDescriptor nameDesc;

            if (name != null && (expression != null || primaryExp != null))
            {
                nameAttr = Table.lookup(name.GetStringName());
                qualName.AttributesRef = nameAttr;
                qualName.TypeDescriptor = nameAttr.TypeDescriptor;

                exp.Accept(this);

                // Check for errors
                // if both types are Error Descriptors, combine errors
                ErrorDescriptor nameErrDesc =
                    nameAttr.TypeDescriptor as ErrorDescriptor;
                ErrorDescriptor expErrDesc =
                    exp.TypeDescriptor as ErrorDescriptor;
                if (nameErrDesc != null && expErrDesc != null)
                {
                    nameDesc = nameErrDesc.CombineErrors(expErrDesc);
                }
                // if one or the other is an error, propagate the error up
                else if (nameErrDesc != null)
                {
                    nameDesc = nameAttr.TypeDescriptor;
                }
                else if (expErrDesc != null)
                {
                    nameDesc = exp.TypeDescriptor;
                }
                // check that the variable being assigned to is assignable
                else if (nameAttr.IsAssignable)
                {
                    // if types compatible, assign successfully assigned type
                    if (TypesCompatible(nameAttr.TypeDescriptor,
                        exp.TypeDescriptor))
                    {
                        nameDesc = nameAttr.TypeDescriptor;
                    }
                    // otherwise, assign new error for incompatible types
                    else
                    {
                        nameDesc = new ErrorDescriptor("Cannot assign " +
                                exp.TypeDescriptor.GetType().Name + " to " +
                                nameAttr.TypeDescriptor.GetType().Name);
                    }
                }
                // variable is not assignable
                else
                {
                    nameDesc = new ErrorDescriptor(
                        nameAttr.TypeDescriptor.GetType().Name +
                        " is not assigable. Cannot assign as " +
                        exp.TypeDescriptor.GetType().Name);
                }
            }
            // Assignment not made up of correct parts
            else
            {
                string message = "";
                if (name == null && expression == null)
                {
                    message += "EQUALS expression expects 'QualifiedName' " +
                               "on LHS, but has: " + qualName.GetType().Name +
                               " & 'Expression' or 'PrimaryExression' on " +
                               "RHS, but has " + exp.GetType().Name;
                }
                else if (name == null)
                {
                    message += "EQUALS expression expects 'QualifiedName' on" +
                               " LHS, but has: " + qualName.GetType().Name;
                }
                else
                {
                    message += "EQUALS expression expects 'Expression' or " +
                               "'PrimaryExpression' on RHS, but has: " +
                               exp.GetType().Name;
                }
                nameDesc = new ErrorDescriptor(message);
            }
            return nameDesc;
        }

        private TypeDescriptor BoolBinaryExp(AbstractNode lhs,
            AbstractNode rhs, ExpressionEnums op)
        {
            lhs.Accept(this);
            rhs.Accept(this);

            if (TypesCompatible(lhs.TypeDescriptor, rhs.TypeDescriptor))
            {
                return new PrimitiveTypeBooleanDescriptor();
            }
            return new ErrorDescriptor("Comparison of Incompatible types: " +
                lhs.TypeDescriptor.GetType().Name + " and " +
                rhs.TypeDescriptor.GetType().Name);
        }

        private TypeDescriptor EvalBinaryExp(AbstractNode lhs, AbstractNode rhs,
            ExpressionEnums op)
        {
            lhs.Accept(this);
            rhs.Accept(this);

            if (TypesCompatible(lhs.TypeDescriptor, rhs.TypeDescriptor))
            {
                return lhs.TypeDescriptor;
            }
            if (lhs.TypeDescriptor is ErrorDescriptor &&
                rhs.TypeDescriptor is ErrorDescriptor)
            {
                // combine error messages
                return ((ErrorDescriptor)lhs.TypeDescriptor).CombineErrors
                    ((ErrorDescriptor)rhs.TypeDescriptor);
            }
            if (lhs.TypeDescriptor is ErrorDescriptor)
            {
                return lhs.TypeDescriptor;
            }
            if (rhs.TypeDescriptor is ErrorDescriptor)
            {
                return rhs.TypeDescriptor;
            }
            return new ErrorDescriptor("Incompatible types: " +
                lhs.TypeDescriptor.GetType().Name + " [" + op + "] " +
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
            // Special Name
            SpecialName specialName = child as SpecialName;
            if (specialName != null)
            {
                return new ErrorDescriptor("SpecialName not implemented");
            }
            // Complex Primary
            Expression exp = child as Expression;
            if (exp != null)
            {
                exp.Accept(this);
                return exp.TypeDescriptor;
            }
            //Complex Primary No Parentheses
            Literal literal = child as Literal;
            if (literal != null)
            {
                return literal.TypeDescriptor;
            }
            Number num = child as Number;
            if (num != null)
            {
                return num.TypeDescriptor;
            }
            FieldAccess fieldAcc = child as FieldAccess;
            if (fieldAcc != null)
            {
                return new ErrorDescriptor("FieldAccess not implemented");
            }
            MethodCall methodCall = child as MethodCall;
            if (methodCall != null)
            {
                methodCall.Accept(this);
                return methodCall.TypeDescriptor;
            }

            return new ErrorDescriptor("Expected QualifiedName, Expression, " +
                "Literal or Number as Primary Expression");
        }

        // TODO: this should check for something...
        private bool IsCompatibleStatement(TypeDescriptor stmt)
        {
            return true;
        }
        #endregion Semantics Helpers


        #region Semantics Travelers
        // can't travel away from here, gotta be pulled back by TopDecl
        #endregion Semantics Travelers

    }
}
