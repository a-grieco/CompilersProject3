using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project4;

namespace Project4
{
    public class CodeGenVisitor : IReflectiveVisitor
    {
        public TextWriter File { get; set; }
        private string ilFile = "";

        public CodeGenVisitor(TextWriter file)
        {
            File = file;
        }

        public void Visit(dynamic node)
        {
            this.VisitNode(node);
        }

        public void VisitChildren(AbstractNode node)
        {
            AbstractNode child = node.Child;
            while (child != null)
            {
                child.Accept(this);
                child = child.Sib;
            }
        }

        public void VisitNode(AbstractNode node)
        {
            VisitChildren(node);
        }

        public void CreateIlFileContent(AbstractNode node)
        {
            node?.Accept(this);
        }

        private void VisitNode(ClassDeclaration node)
        {
            AbstractNode modifiers = node.Child;
            AbstractNode identifier = modifiers.Sib;
            AbstractNode classBody = identifier.Sib;

            ClassTypeDescriptor desc =
                node.TypeDescriptor as ClassTypeDescriptor;
            string mods = String.Join(" ", desc.Modifiers).ToLower();
            string name = ((Identifier)identifier).ID;

            File.WriteLine(".class {0} {1}", mods, name);
            File.WriteLine("{");
            classBody.Accept(this);
            File.WriteLine("}");
        }

        private void VisitNode(MethodDeclaration node)
        {
            File.WriteLine();
            MethodTypeDescriptor desc =
                node.TypeDescriptor as MethodTypeDescriptor;
            if (desc != null)
            {
                AbstractNode modifiers = node.Child;
                AbstractNode typeSpecifier = modifiers.Sib;
                AbstractNode methodDeclarator = typeSpecifier.Sib;
                AbstractNode methodBody = methodDeclarator.Sib;

                AbstractNode methodDeclaratorName = methodDeclarator.Child;
                AbstractNode parameterList = methodDeclaratorName.Sib; // may be null

                string mods = String.Join(" ", desc.Modifiers).ToLower();
                string typeSpec = GetIlType(desc.ReturnType, typeSpecifier);
                string name = ((Identifier)methodDeclaratorName).ID;
                string argList = GetIlParams(parameterList);
                string begin = name.ToLower().Equals("main") ?
                    "\n{\n.entrypoint\n.maxstack 42\n" : "\n{\n.maxstack 42";
                string end = "ret\n}\n";

                File.WriteLine($".method {mods} {typeSpec} {name}" +
                          $"({argList}) cil managed {begin}");
                methodBody.Accept(this);
                File.WriteLine(end);
            }
            ErrorDescriptor err = node.TypeDescriptor as ErrorDescriptor;
            if (err != null) { PrintError(err); }
        }

        private void VisitNode(MethodCall node)
        {
            // catch errors prior to entering method call
            ErrorDescriptor err = node.TypeDescriptor as ErrorDescriptor;
            if (err != null)
            {
                PrintError(err);
                return;
            }

            MethodTypeDescriptor desc =
                   node.TypeDescriptor as MethodTypeDescriptor;
            string retType = GetIlType(desc.ReturnType, node);

            AbstractNode methodReference = node.Child;
            AbstractNode argumentList = methodReference.Sib;    // may be null

            QualifiedName qualifiedName = methodReference as QualifiedName;
            string methodName = qualifiedName.GetStringName();

            // add arguments
            //if (argumentList != null) { AddArgs(argumentList as ArgumentList); }
            if (argumentList != null) { argumentList.Accept(this); }


            if (methodName.ToLower().Equals("write") ||
                methodName.ToLower().Equals("writeline"))
            {
                CallWrites(methodName.ToLower(), GetIlTypeParams(argumentList));
            }
            //MethodTypeDescriptor desc =
            //    node.TypeDescriptor as MethodTypeDescriptor;
            //if (desc != null)
            //{
            //    AbstractNode methodReference = node.Child;
            //    QualifiedName qualifiedName = methodReference as QualifiedName;

            //    string argList = GetArgList(desc.Signature.ParameterTypes);
            //}
        }



        private void VisitNode(ArgumentList node)
        {
            if (node == null) { return; }
            AbstractNode expression = node.Child;
            while (expression != null)
            {
                expression.Accept(this);
                expression = expression.Sib;
            }
        }

        private void VisitNode(Expression node)
        {
            ExpressionEnums type = node.ExpressionType;
            string typeStr = GetIlOp(node.ExpressionType);
        }

        private void VisitNode(Literal node)
        {
            File.WriteLine("ldstr \"{0}\"", node.Lit);
        }

        private void VisitNode(Number node)
        {
            File.WriteLine("ldc.i4.s {0}", node.Num);
        }


        private string GetIlOp(ExpressionEnums expEnum)
        {
            switch (expEnum)
            {
                case ExpressionEnums.EQUALS:
                    return "I don't know";  // TODO
                case ExpressionEnums.OP_LOR:
                    return "or";
                case ExpressionEnums.OP_LAND:
                    return "and";
                //case ExpressionEnums.PIPE:
                //    break;
                //case ExpressionEnums.HAT:
                //    break;
                case ExpressionEnums.AND:
                    return "and";
                case ExpressionEnums.OP_EQ:
                    return "beq <int32(target)>";
                case ExpressionEnums.OP_NE:
                    return "bne.un <int32 (target)>";
                case ExpressionEnums.OP_GT:
                    return "bgt <int32(target)>";
                case ExpressionEnums.OP_LT:
                    return "blt <int32 (target)>";
                case ExpressionEnums.OP_LE:
                    return "ble <int32 (target)>";
                case ExpressionEnums.OP_GE:
                    return "bge <int32 (target)>";
                case ExpressionEnums.PLUSOP:
                    return "add";
                case ExpressionEnums.MINUSOP:
                    return "sub";
                case ExpressionEnums.ASTERISK:
                    return "nul";
                case ExpressionEnums.RSLASH:
                    return "div";
                case ExpressionEnums.PERCENT:
                    return "rem";
                case ExpressionEnums.UNARY:
                    return "neg";
                //case ExpressionEnums.PRIMARY:
                //    break;
                default:
                    return "";
            }
        }


        #region CodeGen Helpers
        private string GetMethodDecl(MethodDeclarator node)
        {
            if (node != null)
            {
                AbstractNode methodRef = node.Child;
                QualifiedName qualName = methodRef as QualifiedName;
                if (qualName != null) return qualName.GetStringName();
            }
            return "<Insert Method Decl Here>";
        }

        private string GetArgList(List<TypeDescriptor> signatureParameterTypes)
        {
            // TODO: what do parameters look like in CIL!?
            return "";
        }

        private void PrintError(ErrorDescriptor node)
        {
            File.WriteLine("call void [mscorlib]System.Console::" +
                $"WriteLine({node.Message})\n");
        }


        private string GetIlParams(AbstractNode argList)
        {
            if (argList == null) { return ""; }
            List<String> pList = new List<String>();
            AbstractNode parameter = argList.Child;
            while (parameter != null)
            {
                pList.Add(GetIlParam(parameter as Parameter));
                parameter = parameter.Sib;
            }
            return String.Join(" ", pList);
        }

        private string GetIlParam(Parameter parameter)
        {
            AbstractNode typeSpecifier = parameter.Child;
            AbstractNode identifier = typeSpecifier.Sib;
            return $"{GetIlType(typeSpecifier)} {((Identifier)identifier).ID}";
        }

        private string GetIlTypeParams(AbstractNode argList)
        {
            if (argList == null) { return ""; }
            List<String> x = new List<String>();
            AbstractNode parameter = argList.Child;
            while (parameter != null)
            {
                x.Add(GetIlTypeParam(parameter));
                parameter = parameter.Sib;
            }
            return String.Join(" ", x);
        }

        private string GetIlTypeParam(AbstractNode node)
        {
            //Parameter parameter = node as Parameter;
            //if (node != null)
            //{
            AbstractNode typeSpecifier = node.Child;
            return $"{GetIlType(typeSpecifier)}";
            //}

            //PrimaryExpression primary = node as PrimaryExpression;

        }

        private string GetIlType(AbstractNode node)
        {
            return GetIlType(node.TypeDescriptor, node);
        }

        private string GetIlType(TypeDescriptor desc, AbstractNode typeSpec)
        {
            // either the return type is a primitive (can get from the signature)
            PrimitiveTypeIntDescriptor intDesc =
                desc as PrimitiveTypeIntDescriptor;
            NumberTypeDescriptor numDesc = desc as NumberTypeDescriptor;
            if (intDesc != null || numDesc != null)
            {
                return "int";
            }
            PrimitiveTypeStringDescriptor stringDesc =
                desc as PrimitiveTypeStringDescriptor;
            LiteralTypeDescriptor litDesc = desc as LiteralTypeDescriptor;
            if (stringDesc != null || litDesc != null)
            {
                return "string";
            }
            PrimitiveTypeBooleanDescriptor boolDesc =
                desc as PrimitiveTypeBooleanDescriptor;
            if (boolDesc != null)
            {
                return "bool";
            }
            PrimitiveTypeVoidDescriptor voidDesc =
                desc as PrimitiveTypeVoidDescriptor;
            if (voidDesc != null)
            {
                return "void";
            }

            // or it is a qualified name (can extract from the node)
            QualifiedName qName = typeSpec as QualifiedName;
            if (qName != null)
            {
                return qName.GetStringName();
            }

            // or it is an error
            ErrorDescriptor err = desc as ErrorDescriptor;
            if (err != null)
            {
                return err.Message;
            }

            return "";
        }

        private void CallWrites(string writeName, string arg)
        {
            if (writeName.ToLower().Equals("write"))
            {
                File.WriteLine("call void" +
                    "[mscorlib]System.Console::Write({0})", arg);
            }
            else if (writeName.ToLower().Equals("writeline"))
            {
                File.WriteLine("call void " +
                    "[mscorlib]System.Console::WriteLine({0})", arg);
            }
        }
        #endregion CodeGen Helpers

    }


}
