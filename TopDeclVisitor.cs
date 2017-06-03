using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Project4
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
                    id.TypeDescriptor = new ErrorDescriptor("Symbol table " +
                        "already contains id: " + id.ID);
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
            AbstractNode modifiers = node.Child;
            AbstractNode typeSpecifier = modifiers.Sib;
            AbstractNode fieldVarDecls = typeSpecifier.Sib;

            typeSpecifier.Accept(TypeVisitor);

            TypeDescriptor declType = typeSpecifier.TypeDescriptor;
            List<ModifiersEnums> modifiersList =
                ((Modifiers)modifiers).ModifierTokens;

            // note: grammar doesn't allow initialization at declaration
            // add each identifier in the list to the symbol table
            AbstractNode fieldVarDeclName = fieldVarDecls.Child;
            while (fieldVarDeclName != null)
            {
                Identifier identifier = fieldVarDeclName.Child as Identifier;
                if (identifier == null)
                {
                    string msg = "Expected Identifier type, found " +
                                 nameof(identifier);
                    fieldVarDeclName.Child.TypeDescriptor =
                        new ErrorDescriptor(msg);
                }
                else
                {
                    string id = identifier.ID;
                    // if variable already declared locally, assign an error
                    if (Table.isDeclaredLocally(id))
                    {
                        identifier.TypeDescriptor = new ErrorDescriptor(
                            "Variable name cannot be redeclared: " + id);
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
                        identifier.TypeDescriptor = declType;
                        identifier.AttributesRef = attr;
                    }
                }
                fieldVarDeclName = fieldVarDeclName.Sib;
            }
            VisitChildren(node);
        }

        // TODO: structs added as requirement
        // TypeDeclaring (n/a only for array and struct types)  

        private void VisitNode(ClassDeclaration node)
        {
            AbstractNode modifiers = node.Child;
            AbstractNode identifier = modifiers.Sib;
            AbstractNode classBody = identifier.Sib;

            ClassTypeDescriptor classDesc = new ClassTypeDescriptor();
            classDesc.ClassBody = new ScopeTable();
            Modifiers mods = modifiers as Modifiers;
            if (mods != null)
            {
                classDesc.Modifiers = mods.ModifierTokens;
            }
            else
            {
                node.TypeDescriptor = new ErrorDescriptor("Expected " +
                    "modifier node, found: " + nameof(node));
            }

            Attr attr = new Attr();
            attr.Kind = Kind.ClassType;
            attr.TypeDescriptor = classDesc;
            string id = ((Identifier)identifier).ID;
            Table.enter(id, attr);
            CurrentClass = classDesc;
            node.TypeDescriptor = classDesc;
            node.AttributesRef = attr;

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

            AbstractNode retType = typeSpecifier.Child;
            if (retType is PrimitiveTypeVoid)
            {
                ((PrimitiveTypeVoid)retType).Accept(TypeVisitor);
            }
            else if (retType is PrimitiveTypeBoolean)
            {
                ((PrimitiveTypeBoolean)retType).Accept(TypeVisitor);
            }
            else if (retType is PrimitiveTypeInt)
            {
                ((PrimitiveTypeInt)retType).Accept(TypeVisitor);
            }
            else if (retType is QualifiedName)
            {
                ((QualifiedName)retType).Accept(TypeVisitor);
            }
            else
            {
                string msg = "Return type of a method must be a " +
                             "PrimitiveType or QualifiedName (found: " +
                             GetSimpleName(retType.TypeDescriptor);
                retType.TypeDescriptor = new ErrorDescriptor(msg);
            }

            AbstractNode methodDeclaratorName = methodDeclarator.Child;
            AbstractNode parameterList = methodDeclaratorName.Sib; // may be null

            MethodTypeDescriptor methDesc = new MethodTypeDescriptor();
            methDesc.ReturnType = retType.TypeDescriptor;
            methDesc.Modifiers = ((Modifiers)modifiers).ModifierTokens;
            methDesc.Locals = new ScopeTable();
            methDesc.IsDefinedIn = CurrentClass;

            Attributes attr = new Attr(methDesc);
            attr.Kind = Kind.MethodType;

            string name = ((Identifier)methodDeclaratorName).ID;
            Table.enter(name, attr);
            node.TypeDescriptor = attr.TypeDescriptor;
            node.AttributesRef = attr;

            Table.openScope(methDesc.Locals);
            MethodTypeDescriptor oldCurrentMethod = CurrentMethod;
            CurrentMethod = methDesc;

            if (parameterList != null)
            {
                parameterList.Accept(this);
                methDesc.Signature.ParameterTypes =
                    ((ParameterListTypeDescriptor)
                    parameterList.TypeDescriptor).ParamTypeDescriptors;
                attr.TypeDescriptor = methDesc;
                Table.updateValue(name, attr);
                node.TypeDescriptor = methDesc;
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
        // removed (added to grammar-node functionality)
        #endregion TopDecl Helpers


        #region TopDecl Travelers
        private void VisitNode(MethodCall node)
        {
            node.Accept(SemanticsVisitor);
        }

        private void VisitNode(SelectionStatement node)
        {
            node.Accept(SemanticsVisitor);
        }

        private void VisitNode(IterationStatement node)
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