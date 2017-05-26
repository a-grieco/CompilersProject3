using System;
using System.Collections.Generic;

namespace Project3
{
    public class TopDeclVisitor : SemanticsVisitor
    {
        public TypeVisitor TypeVisitor { get; set; }

        public TopDeclVisitor(SymbolTable st) : base(st)
        {
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

        // Variable List Declaring
        private void VisitNode(LocalVariableDeclarationStatement node)
        {
            Console.WriteLine("LocalVariableDeclarationStatement node visit " +
                              "in TopDeclVisitor.");

            AbstractNode typeSpecifier = node.Child;
            AbstractNode localVariableDeclarators = typeSpecifier.Sib;

            
            typeSpecifier.Accept(TypeVisitor);
            TypeDescriptor declType = typeSpecifier.TypeDescriptor;

            AbstractNode identifier = localVariableDeclarators.Child;
            while (identifier != null)
            {
                Identifier id = (Identifier)identifier;
                if (Table.isDeclaredLocally(id.ID))
                {
                    string message = "Symbol table already contains id: " + id.ID;
                    Console.WriteLine(message); // TODO: delete
                    id.TypeDescriptor = new ErrorDescriptor();
                    ((ErrorDescriptor)identifier.TypeDescriptor).Message = message;
                    id.AttributesRef = null;
                }
                else
                {
                    VariableDeclarationAttributes attr =
                        new VariableDeclarationAttributes();
                    attr.Kind = Kind.VariableAttributes;
                    attr.TypeDescriptor = declType;
                    attr.Modifiers = null;
                    Table.enter(id.ID, attr);
                    Console.WriteLine("Entered into symbol table: " + id.ID +
                                      " " + attr); // TODO: DELETE
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
                    identifier.TypeDescriptor = new ErrorDescriptor();
                    ((ErrorDescriptor)identifier.TypeDescriptor).Message = message;
                    identifier.AttributesRef = null;
                }
                else
                {
                    VariableDeclarationAttributes attr =
                        new VariableDeclarationAttributes();
                    attr.Kind = Kind.VariableAttributes;
                    attr.TypeDescriptor = identifier.TypeDescriptor;
                    attr.Modifiers = modifiersList;
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

        // TypeDeclaring (n/a only for array and struct types)

        // ClassDeclaring
        private void VisitNode(ClassDeclaration node)
        {
            Console.WriteLine("ClassDeclaration node visit in TopDeclVisitor");

            AbstractNode modifiers = node.Child;
            AbstractNode identifier = modifiers.Sib;
            AbstractNode classBody = identifier.Sib;

            ClassTypeDescriptor typeRef = new ClassTypeDescriptor();
            typeRef.ClassBody = new ScopeTable();
            ClassAttributes attr = new ClassAttributes();
            attr.Kind = Kind.ClassType;
            attr.TypeDescriptor = typeRef;
            string id = ((Identifier)identifier).ID;
            Table.enter(id, attr);
            CurrentClass = attr;
            node.TypeDescriptor = typeRef; // TODO: check if needed 
            node.AttributesRef = attr; // (not included in pseudocode)

            // push the class body scope table onto the symbol table stack
            Table.openScope
                (((ClassTypeDescriptor)attr.TypeDescriptor).ClassBody);

            AbstractNode fieldDecls = classBody.Child;
            if (fieldDecls == null)
            {
                return;
            } // grammar allows one class: so if
            // body is empty, parsing complete
            AbstractNode fieldDecl = fieldDecls.Child;
            while (fieldDecl != null)
            {
                if (fieldDecl is MethodDeclaration)
                {
                    ((MethodDeclaration)fieldDecl).Accept(this);
                }
                else if (fieldDecl is FieldVariableDeclaration)
                {
                    ((FieldVariableDeclaration)fieldDecl).Accept(this);
                }
                else
                {
                    throw new ArgumentException("FieldDeclaration should be " +
                                                "FieldVariableDeclaration or MethodDeclaration");
                }
                fieldDecl = fieldDecls.Sib;
            }
            Table.closeScope();
            CurrentClass = null;
        }

        // MethodDeclaring
        private void VisitNode(MethodDeclaration node)
        {
            AbstractNode modifiers = node.Child;
            AbstractNode typeSpecifier = modifiers.Sib;
            AbstractNode methodDeclarator = typeSpecifier.Sib;
            AbstractNode methodBody = methodDeclarator.Sib;

            if (typeSpecifier is PrimitiveTypeVoid)
            {
                ((PrimitiveTypeVoid)typeSpecifier).Accept(TypeVisitor);
            }
            else if (typeSpecifier is PrimitiveTypeBoolean)
            {
                ((PrimitiveTypeBoolean)typeSpecifier).Accept(TypeVisitor);
            }
            else if (typeSpecifier is PrimitiveTypeInt)
            {
                ((PrimitiveTypeInt)typeSpecifier).Accept(TypeVisitor);
            }
            else if (typeSpecifier is QualifiedName)
            {
                ((QualifiedName)typeSpecifier).Accept(TypeVisitor);
            }
            else
            {
                throw new ArgumentException("Return type of a method must " +
                                            "be a PrimitiveType or QualifiedName");
            }

            MethodAttributes attr = new MethodAttributes();
            attr.TypeDescriptor = new MethodTypeDescriptor();
            //TODO: method doesn't seem to need or use a type descriptor
            attr.ReturnType = typeSpecifier.TypeDescriptor;
            attr.Modifiers = ((Modifiers)modifiers).ModifierTokens;
            attr.IsDefinedIn = CurrentClass;
            attr.Locals = new ScopeTable();

            AbstractNode methodDeclaratorName = methodDeclarator.Child;
            AbstractNode parameterList = methodDeclaratorName.Sib; // may be null

            string name = ((Identifier)methodDeclaratorName).ID;
            Table.enter(name, attr);
            node.TypeDescriptor = attr.TypeDescriptor;
            node.AttributesRef = attr;

            Table.openScope(attr.Locals);
            MethodAttributes oldCurrentMethod = CurrentMethod;
            CurrentMethod = attr;
            if (parameterList != null)
            {
                // signature = parameter types + return value
                parameterList.Accept(this);
                attr.Signature = ((ParameterList)parameterList).
                    AddReturnToSignature(attr.ReturnType);
            }
            else
            {
                // signature = return value only
                attr.Signature = new Signature(attr.ReturnType);
            }
            methodBody.Accept(this);
            CurrentMethod = oldCurrentMethod;
            Table.closeScope();
        }

        private void VisitNode(Parameter node)
        {
            AbstractNode typeSpecifier = node.Child;
            AbstractNode identifier = typeSpecifier.Sib;

            typeSpecifier.Accept(TypeVisitor);
            TypeDescriptor declType = typeSpecifier.TypeDescriptor;

            string id = ((Identifier)identifier).ID;
            if (Table.isDeclaredLocally(id))
            {
                string message = "Symbol table already contains id: " + id;
                Console.WriteLine(message); // TODO: delete
                identifier.TypeDescriptor = new ErrorDescriptor();
                ((ErrorDescriptor)identifier.TypeDescriptor).Message = message;
                identifier.AttributesRef = null;
            }
            else
            {
                ParameterAttributes attr = new ParameterAttributes();
                attr.Kind = Kind.VariableAttributes;
                attr.TypeDescriptor = declType;
                Table.enter(id, attr);
                Console.WriteLine("Entered into symbol table: " + id +
                                  " " + attr); // TODO: DELETE
                identifier.TypeDescriptor = declType;
                identifier.AttributesRef = attr;
            }
        }

        private void VisitNode(MethodCall node)
        {
            AbstractNode methodReference = node.Child;
            AbstractNode argumentList = methodReference.Sib;    // may be null

            methodReference.Accept(TypeVisitor);
            TypeDescriptor methodRefType = methodReference.TypeDescriptor;

            if (argumentList != null)
            {
                AbstractNode child = argumentList.Child;
                while (child != null)
                {
                    MethodReferenceAttributes attr = new MethodReferenceAttributes();
                    Expression expression = (Expression)child;
                    expression.Accept(this);
                    // TODO: these expressions (in the Argument List) should
                    // probably hold a reference to the attribute? or should a 
                    // list be kept of the expressions?
                    child = child.Sib;
                }
            }
        }

        private void VisitNode(Literal node)
        {
            var desc = new LiteralTypeDescriptor {Value = node.Lit};
            node.TypeDescriptor = desc;
        }


    }
}