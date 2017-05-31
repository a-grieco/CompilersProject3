using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Project3
{
    public class TopDeclVisitor : SemanticsVisitor
    {
        public SemanticsVisitor SemanticsVisitor { get; set; }
        public TypeVisitor TypeVisitor { get; set; }

        public TopDeclVisitor(SymbolTable st) : base(st)
        {
            SemanticsVisitor = new SemanticsVisitor(st);
            TypeVisitor = new TypeVisitor(st);
        }

        public override void Visit(dynamic node)
        {
            this.VisitNode(node);
        }

        private new void VisitNode(AbstractNode node)
        {
            Console.WriteLine("VisitNode, TopDeclVisitor [" + node.GetType() + "]");
            VisitChildren(node);
        }

        private new void VisitChildren(AbstractNode node)
        {
            AbstractNode child = node.Child;
            while (child != null)
            {
                child.Accept(this);
                child = child.Sib;
            }
        }


        #region TopDecl Specialized Node Visits
        private void VisitNode(LocalVariableDeclarationStatement node)
        {
            Console.WriteLine("LocalVariableDeclarationStatement node visit " +
                              "in TopDeclVisitor.");

            AbstractNode typeSpecifier = node.Child;
            AbstractNode localVariableDeclarators = typeSpecifier.Sib;


            typeSpecifier.Accept(this);
            TypeDescriptor declType = typeSpecifier.TypeDescriptor;

            AbstractNode identifier = localVariableDeclarators.Child;
            while (identifier != null)
            {
                Identifier id = (Identifier)identifier;
                if (Table.isDeclaredLocally(id.ID))
                {
                    string message = "Symbol table already contains id: " + id.ID;
                    Console.WriteLine(message); // TODO: delete
                    id.TypeDescriptor = new ErrorDescriptor(message);
                    id.AttributesRef = null;
                }
                else
                {
                    VariableDeclarationAttributes attr =
                        new VariableDeclarationAttributes();
                    attr.Kind = Kind.VariableAttributes;
                    attr.TypeDescriptor = declType;
                    attr.IsAssignable = true;
                    attr.Modifiers = null;
                    Table.enter(id.ID, attr);
                    id.TypeDescriptor = declType;
                    id.AttributesRef = attr;
                }
                identifier = identifier.Sib;
            }
        }

        private void VisitNode(FieldVariableDeclaration node)
        {
            Console.WriteLine("FieldVariableDeclaration node visit " +
                              "in TopDeclVisitor");

            AbstractNode modifiers = node.Child;
            AbstractNode typeSpecifier = modifiers.Sib;
            AbstractNode fieldVarDecls = typeSpecifier.Sib;

            typeSpecifier.Accept(TypeVisitor);

            TypeDescriptor declType = typeSpecifier.TypeDescriptor;
            List<ModifiersEnums> modifiersList = ((Modifiers)modifiers).ModifierTokens;

            // note: grammar does not allow initialization here, only declaration
            // add each identifier in the list to the symbol table
            AbstractNode fieldVarDeclName = fieldVarDecls.Child;
            while (fieldVarDeclName != null)
            {
                Identifier identifier = fieldVarDeclName.Child as Identifier;
                if (identifier == null) throw new ArgumentNullException(nameof(identifier));
                string id = identifier.ID;
                // if declared locally, assign an error node
                if (Table.isDeclaredLocally(id))
                {
                    string message = "Variable name cannot be redeclared: " + id;
                    Console.WriteLine(message); // TODO: delete
                    identifier.TypeDescriptor = new ErrorDescriptor(message);
                    identifier.AttributesRef = null;
                }
                else
                {
                    VariableDeclarationAttributes attr =
                        new VariableDeclarationAttributes();
                    attr.Kind = Kind.VariableAttributes;
                    attr.TypeDescriptor = identifier.TypeDescriptor;
                    attr.Modifiers = modifiersList;
                    attr.IsAssignable = true;
                    Table.enter(id, attr);
                    Console.WriteLine("Entered into symbol table: " + id +
                                      " " + attr); // TODO: DELETE
                    identifier.TypeDescriptor = declType;
                    identifier.AttributesRef = attr;
                }
                fieldVarDeclName = fieldVarDeclName.Sib;
            }
            VisitChildren(node);
        }

        // TypeDeclaring (n/a only for array and struct types)  // TODO: structs added as requirement

        private void VisitNode(ClassDeclaration node)
        {
            Console.WriteLine("ClassDeclaration node visit in TopDeclVisitor");

            AbstractNode modifiers = node.Child;
            AbstractNode identifier = modifiers.Sib;
            AbstractNode classBody = identifier.Sib;

            ClassTypeDescriptor typeRef = new ClassTypeDescriptor();
            typeRef.ClassBody = new ScopeTable();
            Modifiers mods = modifiers as Modifiers;
            if (mods != null)
            {
                typeRef.Modifiers = mods.ModifierTokens;
            }
            else
            {
                node.TypeDescriptor =
                    new ErrorDescriptor("Expected modifier node.");
            }

            Attr attr = new Attr();
            attr.Kind = Kind.ClassType;
            attr.TypeDescriptor = typeRef;
            string id = ((Identifier)identifier).ID;
            Table.enter(id, attr);
            CurrentClass = typeRef;
            node.TypeDescriptor = typeRef; // TODO: check if needed 
            node.AttributesRef = attr; // (not included in pseudocode)

            // push the class body scope table onto the symbol table stack
            Table.openScope
                (((ClassTypeDescriptor)attr.TypeDescriptor).ClassBody);

            AbstractNode fieldDecls = classBody.Child;

            // grammar allows one class: if body empty, parsing complete
            if (fieldDecls == null) { return; }
            fieldDecls.Accept(this);
            Table.closeScope();
            CurrentClass = null;
        }

        private void VisitNode(FieldDeclarations node)
        {
            AbstractNode fieldDecl = node.Child;
            while (fieldDecl != null)
            {
                fieldDecl.Accept(this);
                fieldDecl = fieldDecl.Sib;
            }
        }

        private void VisitNode(MethodDeclaration node)
        {
            AbstractNode modifiers = node.Child;
            AbstractNode typeSpecifier = modifiers.Sib;
            AbstractNode methodDeclarator = typeSpecifier.Sib;
            AbstractNode methodBody = methodDeclarator.Sib;

            AbstractNode type = typeSpecifier.Child;
            if (type is PrimitiveTypeVoid)
            {
                ((PrimitiveTypeVoid)type).Accept(TypeVisitor);
            }
            else if (type is PrimitiveTypeBoolean)
            {
                ((PrimitiveTypeBoolean)type).Accept(TypeVisitor);
            }
            else if (type is PrimitiveTypeInt)
            {
                ((PrimitiveTypeInt)type).Accept(TypeVisitor);
            }
            else if (type is QualifiedName)
            {
                ((QualifiedName)type).Accept(TypeVisitor);
            }
            else
            {
                throw new ArgumentException("Return type of a method must " +
                                            "be a PrimitiveType or QualifiedName");
            }

            AbstractNode methodDeclaratorName = methodDeclarator.Child;
            AbstractNode parameterList = methodDeclaratorName.Sib; // may be null

            MethodTypeDescriptor descriptor = new MethodTypeDescriptor();
            descriptor.ReturnType = type.TypeDescriptor;
            descriptor.Modifiers = ((Modifiers)modifiers).ModifierTokens;
            descriptor.Locals = new ScopeTable();
            descriptor.IsDefinedIn = CurrentClass;
            //descriptor.Signature.ParameterTypes = getParameterTypes(parameterList);

            Attributes attr = new Attr(descriptor);
            attr.Kind = Kind.MethodType;

            string name = ((Identifier)methodDeclaratorName).ID;
            Table.enter(name, attr);
            node.TypeDescriptor = attr.TypeDescriptor;
            node.AttributesRef = attr;

            Table.openScope(descriptor.Locals);
            MethodTypeDescriptor oldCurrentMethod = CurrentMethod;
            CurrentMethod = descriptor;

            //descriptor.Signature = new SignatureDescriptor();
            if (parameterList != null)
            {
                parameterList.Accept(this);
                descriptor.Signature.ParameterTypes =
                    ((ParameterListTypeDescriptor)
                    parameterList.TypeDescriptor).ParamTypeDescriptors;
                //((ParameterListTypeDescriptoriptor)parameterList).TypeDescriptor..
                // getParameterTypes(parameterList);
                attr.TypeDescriptor = descriptor;
                Table.updateValue(name, attr);
                node.TypeDescriptor = descriptor;
                node.AttributesRef = Table.lookup(name);
            }
            methodBody.Accept(this);
            CurrentMethod = oldCurrentMethod;
            Table.closeScope();
        }

        private void VisitNode(Block node)
        {
            AbstractNode locVarDeclOrStmt = node.Child;
            while (locVarDeclOrStmt != null)
            {
                locVarDeclOrStmt.Accept(this);
                locVarDeclOrStmt = locVarDeclOrStmt.Sib;
            }
        }

        private void VisitNode(ParameterList node)
        {
            List<TypeDescriptor> paramTypes = new List<TypeDescriptor>();
            AbstractNode parameter = node.Child;
            while (parameter != null)
            {
                parameter.Accept(this);
                paramTypes.Add(parameter.TypeDescriptor);
                parameter = parameter.Sib;
            }
            ParameterListTypeDescriptor paramListDesc =
                new ParameterListTypeDescriptor();
            paramListDesc.ParamTypeDescriptors = paramTypes;
            node.TypeDescriptor = paramListDesc;
        }

        private void VisitNode(Parameter node)
        {
            AbstractNode typeSpecifier = node.Child;
            AbstractNode identifier = typeSpecifier.Sib;

            typeSpecifier.Accept(TypeVisitor);
            TypeDescriptor declType = typeSpecifier.Child.TypeDescriptor;

            // TODO: DELETE ME

            string id = ((Identifier)identifier).ID;
            if (Table.isDeclaredLocally(id))
            {
                identifier.TypeDescriptor =
                    new ErrorDescriptor("Duplicate declaration of " + id);
                identifier.AttributesRef = null;
            }
            else
            {
                ParameterAttributes attr = new ParameterAttributes();
                attr.Kind = Kind.VariableAttributes;
                attr.TypeDescriptor = declType;
                Table.enter(id, attr);
                identifier.TypeDescriptor = declType;
                identifier.AttributesRef = attr;
            }
            node.TypeDescriptor = identifier.TypeDescriptor;
            node.AttributesRef = identifier.AttributesRef;
        }

        private void VisitNode(Literal node)
        {
            var desc = new LiteralTypeDescriptor { Value = node.Lit };
            node.TypeDescriptor = desc;
        }

        private void VisitNode(TypeSpecifier node)
        {
            AbstractNode child = node.Child;    // QualifiedName or PrimitiveType
            child.Accept(TypeVisitor);
            child.AttributesRef.IsAssignable = true;
            node.TypeDescriptor = child.TypeDescriptor;
        }


        private void VisitNode(PrimaryExpression node)
        {
            AbstractNode child = node.Child;
            child.Accept(this);
            node.TypeDescriptor = child.TypeDescriptor;
        }
        #endregion TopDecl Specialized Node visits


        #region TopDecl Helpers
        private List<TypeDescriptor> getParameterTypes(AbstractNode paramList)
        {
            List<TypeDescriptor> parameterTypes = new List<TypeDescriptor>();
            if (paramList != null)
            {
                Parameter param = (Parameter)paramList.Child;
                while (param != null)
                {
                    parameterTypes.Add(param.TypeDescriptor);
                    param = (Parameter)param.Sib;
                }
            }
            return parameterTypes;
        }
        #endregion TopDecl Helpers


        #region TopDecl Travelers
        private void VisitNode(MethodCall node)
        {
            node.Accept(SemanticsVisitor);
        }

        private void VisitNode(Expression node)
        {
            node.Accept(SemanticsVisitor);
        }
        #endregion TopDecl Travelers







    }
}