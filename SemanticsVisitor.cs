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
        public const bool DISPLAY_PROGRESS = false;

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
                    else if (methodSignature == null)
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

        private void VisitNode(IterationStatement node)
        {
            AbstractNode whileExp = node.Child;
            AbstractNode bodyStmt = whileExp.Sib;

            IterationStatementDescriptor iterDesc =
                new IterationStatementDescriptor();

            String errMsg = "";

            // while expression
            whileExp.Accept(this);
            iterDesc.WhileDescriptor = whileExp.TypeDescriptor;
            PrimitiveTypeBooleanDescriptor whileBoolDesc =
                whileExp.TypeDescriptor as PrimitiveTypeBooleanDescriptor;
            if (whileBoolDesc == null)
            {
                iterDesc.TypeDescriptor = new ErrorDescriptor("If statement " +
                    "does not evaluate to a Boolean expression. (Has type: " +
                    whileExp.TypeDescriptor.GetType().Name + ")");
            }
            // if body stmt empty, infinite loop error
            bodyStmt.Accept(this);
            bodyStmt.TypeDescriptor = bodyStmt.Child.TypeDescriptor;
            iterDesc.BodyDescriptor = bodyStmt.TypeDescriptor;

            // propagate up errors as needed
            if (iterDesc.WhileDescriptor is ErrorDescriptor ||
                iterDesc.BodyDescriptor is ErrorDescriptor)
            {
                if (iterDesc.WhileDescriptor is ErrorDescriptor)
                {
                    iterDesc.TypeDescriptor = iterDesc.WhileDescriptor;
                    if (iterDesc.BodyDescriptor is ErrorDescriptor)
                    {
                        ((ErrorDescriptor)iterDesc.TypeDescriptor).CombineErrors(
                            (ErrorDescriptor)iterDesc.BodyDescriptor);
                    }
                }
                else
                {
                    iterDesc.TypeDescriptor = iterDesc.BodyDescriptor;
                }
            }
            // otherwise assign appropriate iteration statement descriptor
            else
            {
                iterDesc.TypeDescriptor = iterDesc.WhileDescriptor;
            }
            node.TypeDescriptor = iterDesc;
        }

        private void VisitNode(SelectionStatement node)
        {
            AbstractNode ifExp = node.Child;
            AbstractNode thanStmt = ifExp.Sib;
            AbstractNode elseStmt = thanStmt.Sib;   // may be null

            SelectionStatementDescriptor ssDesc =
                new SelectionStatementDescriptor();

            String errMsg = "";

            // if expression
            ifExp.Accept(this);
            ssDesc.IfDescriptor = ifExp.TypeDescriptor;
            PrimitiveTypeBooleanDescriptor ifBoolDesc =
                ifExp.TypeDescriptor as PrimitiveTypeBooleanDescriptor;
            if (ifBoolDesc == null)
            {
                ifExp.TypeDescriptor = new ErrorDescriptor("If statement " +
                    "does not evaluate to a Boolean expression. (Has type: " +
                    ifExp.TypeDescriptor.GetType().Name + ")");
            }
            // than statement
            thanStmt.Accept(this);
            if (thanStmt.TypeDescriptor == null)
            {
                thanStmt.TypeDescriptor = thanStmt.Child.TypeDescriptor;
            }
            else
            {   // TODO: delete me unless this actually happens... shouldn't
                Console.WriteLine("**THAN** STATEMENT WASN'T NULL " +
                                  "(SelectionStatement node)");
            }
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
                if (elseStmt.TypeDescriptor == null)
                {
                    elseStmt.TypeDescriptor = elseStmt.Child.TypeDescriptor;
                }
                else
                {   // TODO: delete me unless this actually happens... shouldn't
                    Console.WriteLine("**ELSE** STATEMENT WASN'T NULL " +
                                      "(SelectionStatement node)");
                }
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
            else if (errMsg.Length > 0)
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
                    Console.WriteLine(message + " BUT IT SHOULD BE");   // TODO
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
                if (DISPLAY_PROGRESS) { Console.WriteLine("Beginning signature comparison:"); }
                if (sigParam.Count == param.Count)
                {
                    for (int i = 0; i < param.Count; i++)
                    {
                        if (DISPLAY_PROGRESS)
                        {
                            Console.Write("\tComparing " + GetSimpleName(sigParam[i]) +
                                          " & " + GetSimpleName(param[i]));
                        }
                        if (!TypesCompatible(sigParam[i], param[i]))
                        {
                            matchFound = false;
                        }
                    }
                }
                else { matchFound = false; }
                if (DISPLAY_PROGRESS) { Console.WriteLine(" Match? " + matchFound); }
                sig = sig.Next; // go to next signature type
            }
            return matchFound;
        }

        private bool TypesCompatible(TypeDescriptor a, TypeDescriptor b)
        {
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

            TypeDescriptor nameDesc;

            if (name != null && (expression != null || primaryExp != null))
            {
                Attributes nameAttr = Table.lookup(name.GetStringName());
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
                        nameDesc = GetAssignmentDesc(nameAttr.TypeDescriptor,
                            exp.TypeDescriptor);
                    }
                    // otherwise, assign new error for incompatible types
                    else
                    {
                        nameDesc = new ErrorDescriptor("Incompatible types: " +
                            "cannot assign " + GetSimpleName(exp.TypeDescriptor)
                            + " to " + GetSimpleName(nameAttr.TypeDescriptor) +
                            " variable");
                    }
                }
                // variable is not assignable
                else
                {
                    nameDesc = new ErrorDescriptor(nameAttr +
                        " is not assigable. Cannot assign as " +
                        GetSimpleName(exp.TypeDescriptor));
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

        private TypeDescriptor GetAssignmentDesc(TypeDescriptor name,
            TypeDescriptor exp)
        {
            // assign integer value
            int valueInt = 0;   // TODO: setting to 0 is not a good solution
                                // fails if type check is not accurate       
            NumberTypeDescriptor expNum = exp as NumberTypeDescriptor;
            PrimitiveTypeIntDescriptor expInt = exp as PrimitiveTypeIntDescriptor;
            if (expNum != null || expInt != null)
            {
                if (expNum != null) { valueInt = expNum.Num; }
                if (expInt != null) { valueInt = expInt.Value; }
                NumberTypeDescriptor nameNum = name as NumberTypeDescriptor;
                PrimitiveTypeIntDescriptor nameInt = name as PrimitiveTypeIntDescriptor;
                if (nameNum != null)
                {
                    nameNum.Num = valueInt;
                    return nameNum;
                }
                if (nameInt != null)
                {
                    nameInt.Value = valueInt;
                    return nameInt;
                }
            }

            // assign bool value
            PrimitiveTypeBooleanDescriptor expBoolean = exp as PrimitiveTypeBooleanDescriptor;
            PrimitiveTypeBooleanDescriptor nameBoolean = name as PrimitiveTypeBooleanDescriptor;
            if (expBoolean != null && nameBoolean != null)
            {
                nameBoolean.Value = expBoolean.Value;
                return nameBoolean;
            }

            // assign string value
            LiteralTypeDescriptor expLiteral = exp as LiteralTypeDescriptor;
            LiteralTypeDescriptor nameLiteral = name as LiteralTypeDescriptor;
            if (expLiteral != null && nameLiteral != null)
            {
                nameLiteral.Value = expLiteral.Value;
                return nameLiteral;
            }

            // assign an error if value unassignable
            return new ErrorDescriptor("Assignment of value from " +
                exp.GetType().Name + " to " + name.GetType().Name + " failed");
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
            return new ErrorDescriptor("Comparison of incompatible types: " +
                GetSimpleName(lhs.TypeDescriptor) + GetOpSymbol(op) +
                GetSimpleName(rhs.TypeDescriptor));
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
            // Otherwise return incompatible types error
            string err = (GetOpName(op).Length > 0) ?
                "attempted " + GetOpName(op) + ": " : "attempted: ";
            err = "Evaluation of incompatible types, " + err +
                GetSimpleName(lhs.TypeDescriptor) + GetOpSymbol(op) +
                GetSimpleName(rhs.TypeDescriptor);
            return new ErrorDescriptor(err);
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

        // TODO: are additional checks needed for statements?
        private bool IsCompatibleStatement(TypeDescriptor stmt)
        {
            return true;
        }

        private string GetSimpleName(TypeDescriptor desc)
        {
            if (desc is PrimitiveTypeBooleanDescriptor)
            {
                return "Boolean";
            }
            if (desc is PrimitiveTypeIntDescriptor)
            {
                return "Integer";
            }
            if (desc is LiteralTypeDescriptor)
            {
                return "String";
            }
            return desc.GetType().Name;
        }

        private string GetOpName(ExpressionEnums op)
        {
            switch (op)
            {
                case ExpressionEnums.EQUALS:
                    return "assignment";
                case ExpressionEnums.OP_LOR:
                    return "comparison";
                case ExpressionEnums.OP_LAND:
                    return "comparison";
                case ExpressionEnums.HAT:
                    return "exponentiation";
                case ExpressionEnums.OP_EQ:
                    return "comparison";
                case ExpressionEnums.OP_NE:
                    return "comparison";
                case ExpressionEnums.OP_GT:
                    return "comparison";
                case ExpressionEnums.OP_LT:
                    return "comparison";
                case ExpressionEnums.OP_LE:
                    return "comparison";
                case ExpressionEnums.OP_GE:
                    return "comparison";
                case ExpressionEnums.PLUSOP:
                    return "addition";
                case ExpressionEnums.MINUSOP:
                    return "subtraction";
                case ExpressionEnums.ASTERISK:
                    return "multiplication";
                case ExpressionEnums.RSLASH:
                    return "division";
                case ExpressionEnums.PERCENT:
                    return "mod";
                default:
                    return "[" + op + "]";
            }
        }

        private string GetOpSymbol(ExpressionEnums op)
        {
            switch (op)
            {
                case ExpressionEnums.EQUALS:
                    return " = ";
                case ExpressionEnums.OP_LOR:
                    return " || ";
                case ExpressionEnums.OP_LAND:
                    return " && ";
                case ExpressionEnums.PIPE:
                    return " | ";
                case ExpressionEnums.HAT:
                    return " ^ ";
                case ExpressionEnums.AND:
                    return " & ";
                case ExpressionEnums.OP_EQ:
                    return " == ";
                case ExpressionEnums.OP_NE:
                    return " != ";
                case ExpressionEnums.OP_GT:
                    return " > ";
                case ExpressionEnums.OP_LT:
                    return " < ";
                case ExpressionEnums.OP_LE:
                    return " <= ";
                case ExpressionEnums.OP_GE:
                    return " >= ";
                case ExpressionEnums.PLUSOP:
                    return " + ";
                case ExpressionEnums.MINUSOP:
                    return " - ";
                case ExpressionEnums.ASTERISK:
                    return " * ";
                case ExpressionEnums.RSLASH:
                    return " / ";
                case ExpressionEnums.PERCENT:
                    return " % ";
                default:
                    return "[" + op + "]";
            }
        }
        #endregion Semantics Helpers


        #region Semantics Travelers
        // can't travel away from here, gotta be pulled back by TopDecl
        #endregion Semantics Travelers

    }
}
