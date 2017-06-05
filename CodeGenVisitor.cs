using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Project4;

namespace Project4
{
    public partial class CodeGenVisitor : IReflectiveVisitor
    {
        public TextWriter File { get; set; }
        private readonly LocalVariables _localVariables;
        private const string IF_FALSE = "if_false_loc";
        private const string IF_END = "if_end_loc";
        private int _ifCount;
        private const string WHILE_COND = "while_cond_loc";
        private const string WHILE_TRUE = "while_true_loc";
        private int _whileCount;
        private string ClassName { get; set; }

        public CodeGenVisitor(TextWriter file)
        {
            File = file;
            _localVariables = new LocalVariables();
            _ifCount = -1;
            _whileCount = -1;
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
            // catch errors prior to entering class declaration
            ErrorDescriptor err = node.TypeDescriptor as ErrorDescriptor;
            if (err != null)
            {
                PrintError(err);
                return;
            }
            AbstractNode modifiers = node.Child;
            AbstractNode identifier = modifiers.Sib;
            AbstractNode classBody = identifier.Sib;

            ClassTypeDescriptor desc =
                node.TypeDescriptor as ClassTypeDescriptor;
            string mods = String.Join(" ", desc.Modifiers).ToLower();
            string name = ((Identifier)identifier).ID;
            ClassName = name;

            File.WriteLine(".assembly extern mscorlib {}");
            File.WriteLine(".assembly addnums {}");
            File.WriteLine(".class {0} {1}", mods, name);
            File.WriteLine("{");
            classBody.Accept(this);
            File.WriteLine("}");
        }

        private void VisitNode(MethodDeclaration node)
        {
            // catch errors prior to entering method declaration
            ErrorDescriptor err = node.TypeDescriptor as ErrorDescriptor;
            if (err != null)
            {
                PrintError(err);
                return;
            }
            _localVariables.OpenScope();
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

                string name = ((Identifier)methodDeclaratorName).ID;
                if (!desc.Modifiers.Contains(ModifiersEnums.STATIC) /*&&
                    name.ToLower().Equals("main")*/)
                {
                    desc.Modifiers.Add(ModifiersEnums.STATIC);
                }
                string mods = String.Join(" ", desc.Modifiers).ToLower();
                string typeSpec = GetIlType(desc.ReturnType, typeSpecifier);
                string argList = GetIlTypeParams(parameterList);
                string begin = name.ToLower().Equals("main") ?
                    "\n{\n.entrypoint\n.maxstack 42\n" : "\n{\n.maxstack 42";
                string end = "ret\n}";

                File.WriteLine($".method {mods} {typeSpec} {name}" +
                          $"({argList}) cil managed {begin}");
                methodBody.Accept(this);
                File.WriteLine(end);
                _localVariables.CloseScope();
            }
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
            argumentList?.Accept(this);

            // special calls for Write or WriteLine methods
            if (methodName.ToLower().Equals("write") ||
                methodName.ToLower().Equals("writeline"))
            {
                CallWrites(methodName.ToLower(),
                    GetIlTypeParams(desc.Signature.ParameterTypes));
            }
            // call for declared methods w/in program
            else
            {
                string param = (argumentList == null) ?
                    "" : GetIlTypeParams(argumentList);
                File.WriteLine("call {0} {1}::{2}({3})", retType, ClassName,
                    methodName, param);
            }
        }

        private void VisitNode(LocalVariableDeclarationStatement node)
        {
            // catch errors prior to entering local var decl stmt
            ErrorDescriptor err = node.TypeDescriptor as ErrorDescriptor;
            if (err != null)
            {
                PrintError(err);
                return;
            }

            AbstractNode typeSpecifier = node.Child;
            AbstractNode localVariableDeclarators = typeSpecifier.Sib;

            string type = GetIlType(typeSpecifier);
            List<String> names = GetLocalVarDeclNames(localVariableDeclarators);
            SetLocalVarDeclNames(names);

            File.WriteLine(".locals init(");
            int count = 0;
            int location;
            foreach (var name in names)
            {
                location = _localVariables.GetVarLocation(name);
                File.Write($"   [{location}] {type} {name}");
                File.WriteLine((count < names.Count - 1) ? "," : "");
                ++count;
            }
            File.WriteLine(")");
        }

        private void VisitNode(SelectionStatement node)
        {
            AbstractNode ifExp = node.Child;
            AbstractNode thanStmt = ifExp.Sib;
            AbstractNode elseStmt = thanStmt.Sib;   // may be null

            ifExp.Accept(this);
            ++_ifCount;
            File.WriteLine("brfalse.s {0}{1}", IF_FALSE, _ifCount);

            thanStmt.Accept(this);
            File.WriteLine("br.s {0}{1}", IF_END, _ifCount);

            File.WriteLine(IF_FALSE + _ifCount + ":");
            elseStmt.Accept(this);
            File.WriteLine(IF_END + _ifCount + ":");
        }

        private void VisitNode(IterationStatement node)
        {
            AbstractNode cond_exp = node.Child;
            AbstractNode body_stmt = cond_exp.Sib;

            ++_whileCount;
            File.WriteLine("br.s {0}{1}", WHILE_COND, _whileCount);
            File.WriteLine(WHILE_TRUE + _whileCount + ":");
            body_stmt.Accept(this);
            File.WriteLine(WHILE_COND + _whileCount + ":");
            cond_exp.Accept(this);
            File.WriteLine("brtrue.s {0}{1}", WHILE_TRUE, _whileCount);
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
            // Assignment Expression
            if (node.ExpressionType is ExpressionEnums.EQUALS)
            {
                AbstractNode qName = node.Child;
                AbstractNode rhsExp = qName.Sib;
                rhsExp.Accept(this);

                //string varName = GetQName(qName);
                string varName = ((QualifiedName)qName).GetStringName();
                File.WriteLine("stloc.{0}",
                    _localVariables.GetVarLocation(varName));
            }
            // Binary Expression
            else
            {
                string typeStr = GetIlOp(node.ExpressionType, node.TypeDescriptor);
                AbstractNode lhs = node.Child;
                AbstractNode rhs = lhs.Sib;

                lhs.Accept(this);
                rhs.Accept(this);
                File.WriteLine(typeStr);
            }
            // Primary Expression

        }

        private void VisitNode(Literal node)
        {
            File.WriteLine("ldstr \"{0}\"", node.Lit);
        }

        private void VisitNode(Number node)
        {
            File.WriteLine("ldc.i4.s {0}", node.Num);
        }

        private void VisitNode(QualifiedName node)
        {
            int location = _localVariables.GetVarLocation(node.GetStringName());
            File.WriteLine("ldloc.{0}", location);
        }



        #region CodeGen Helpers
        private void PrintError(ErrorDescriptor node)
        {
            File.WriteLine("call void [mscorlib]System.Console::" +
                $"WriteLine({node.Message})");
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

        private string GetIlTypeParams(List<TypeDescriptor> types)
        {
            List<String> ilTypes = new List<String>();
            foreach (var type in types)
            {
                ilTypes.Add(GetIlType(type));
            }
            return String.Join(" ", ilTypes);
        }

        private string GetIlTypeParam(AbstractNode node)
        {
            AbstractNode typeSpecifier = node.Child;
            return $"{GetIlType(typeSpecifier)}";
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
                return "int32";
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

        private string GetIlType(TypeDescriptor desc)
        {
            // either the return type is a primitive (can get from the signature)
            PrimitiveTypeIntDescriptor intDesc =
                desc as PrimitiveTypeIntDescriptor;
            NumberTypeDescriptor numDesc = desc as NumberTypeDescriptor;
            if (intDesc != null || numDesc != null)
            {
                return "int32";
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

        // TODO: Add functionaility for strings & bools
        private string GetIlOp(ExpressionEnums expEnum, TypeDescriptor type)
        {
            switch (expEnum)
            {
                //case ExpressionEnums.EQUALS:
                //    break; 
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
                    return "ceq";
                case ExpressionEnums.OP_NE:
                    return "cne";
                case ExpressionEnums.OP_GT:
                    return "cgt";
                case ExpressionEnums.OP_LT:
                    return "clt";
                case ExpressionEnums.OP_LE:
                    return "cle";
                case ExpressionEnums.OP_GE:
                    return "cge";
                case ExpressionEnums.PLUSOP:
                    return "add";
                case ExpressionEnums.MINUSOP:
                    return "sub";
                case ExpressionEnums.ASTERISK:
                    return "mul";
                case ExpressionEnums.RSLASH:
                    return "div";
                case ExpressionEnums.PERCENT:
                    return "rem";
                case ExpressionEnums.UNARY:
                    return "neg";
                //case ExpressionEnums.PRIMARY:
                //    break;
                default:
                    return "something bad happened";
            }
        }

        private List<string> GetLocalVarDeclNames(AbstractNode node)
        {
            List<String> names = new List<String>();
            AbstractNode identifier = node.Child;
            while (identifier != null)
            {
                Identifier id = identifier as Identifier;
                if (id != null) names.Add(id.ID);
                identifier = identifier.Sib;
            }
            return names;
        }

        private void SetLocalVarDeclNames(List<string> names)
        {
            _localVariables.AddVariables(names);
        }

        // TODO: no support for access to nested fields names name.name.etc.
        // This just returns the full qualified name as a string (will cause 
        // an error if nested fields exist
        private string GetQName(AbstractNode qName)
        {
            string name = "";
            AbstractNode identifier = qName.Child;
            while (identifier != null)
            {
                // add a period before any additional names
                name += (name.Length > 0) ?
                    "." + ((Identifier)identifier).ID :
                    ((Identifier)identifier).ID;
                identifier = identifier.Sib;
            }
            return name;
        }
        #endregion CodeGen Helpers

    }


}
