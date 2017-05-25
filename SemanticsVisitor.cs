using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using ASTBuilder;

namespace Project3
{
    class SemanticsVisitor : IReflectiveVisitor
    {
        public static SymbolTable _symbolTable = new SymbolTable();
        public static ClassAttributes CurrentClass { get; set; }
        public static MethodAttributes CurrentMethod { get; set; }

        // This method is the key to implenting the Reflective Visitor pattern
        // in C#, using the 'dynamic' type specification to accept any node type.
        // The subsequent call to VisitNode does dynamic lookup to find the
        // appropriate Version.

        public virtual void Visit(dynamic node)
        {
            // TODO: switch statment to get to topDeclVisitor
            this.VisitNode(node);
        }

        // Call this method to begin the semantic checking process
        public void CheckSemantics(AbstractNode node)
        {
            if (node == null)
            {
                return;
            }
            TopDeclVisitor topDeclVisitor = new TopDeclVisitor();
            node.Accept(topDeclVisitor);
            //node.Accept(this);
            //VisitChildren(node);
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
    }

    class TopDeclVisitor : SemanticsVisitor
    {
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
            TypeVisitor typeVisitor = new TypeVisitor();

            AbstractNode typeSpecifier = node.Child;
            AbstractNode localVariableDeclarators = typeSpecifier.Sib;

            typeSpecifier.Accept(typeVisitor);

            AbstractNode identifier = localVariableDeclarators.Child;
            while (identifier != null)
            {
                string id = ((Identifier)identifier).ID;
                if (_symbolTable.isDeclaredLocally(id))
                {
                    identifier.TypeDescriptor = new ErrorDescriptor();
                    identifier.AttributesRef = null;
                }
                else
                {
                    identifier.TypeDescriptor = typeSpecifier.TypeDescriptor;
                    VariableDeclarationAttributes attribute =
                        new VariableDeclarationAttributes();
                    attribute.TypeDescriptor = identifier.TypeDescriptor;
                    attribute.Kind = Kind.VariableAttributes;
                    attribute.Modifiers = null;
                    identifier.AttributesRef = attribute;
                    _symbolTable.enter(id, attribute);
                    Console.WriteLine("Entered into symbol table: " + id + " " + attribute);
                }
                identifier = identifier.Sib;
            }
        }

        private void VisitNode(FieldVariableDeclaration node)
        {
            Console.WriteLine("FieldVariableDeclaration node visit " +
                                 "in TopDeclVisitor");
            TypeVisitor typeVisitor = new TypeVisitor();

            AbstractNode modifiers = node.Child;
            AbstractNode typeSpecifier = modifiers.Sib;
            AbstractNode fieldVarDecls = typeSpecifier.Sib;

            typeSpecifier.Accept(typeVisitor);

            // add each identifier in the list to the symbol table
            AbstractNode fieldVarDeclName = fieldVarDecls.Child;
            while (fieldVarDeclName != null)
            {
                Identifier identifier = (Identifier)fieldVarDeclName.Child;
                string id = identifier.ID;
                // if declared locally, assign an error node
                if (_symbolTable.isDeclaredLocally(id))
                {
                    identifier.TypeDescriptor = new ErrorDescriptor();
                    identifier.AttributesRef = null;
                }
                else
                {
                    identifier.TypeDescriptor = typeSpecifier.TypeDescriptor;
                    VariableDeclarationAttributes attribute = new VariableDeclarationAttributes();
                    attribute.TypeDescriptor = identifier.TypeDescriptor;
                    attribute.Kind = Kind.VariableAttributes;
                    attribute.Modifiers = ((Modifiers)modifiers).ModifierTokens;
                    identifier.AttributesRef = attribute;
                    _symbolTable.enter(id, attribute);
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
            attr.TypeDescriptor = typeRef;
            attr.Kind = Kind.ClassType;
            string id = ((Identifier)identifier).ID;
            _symbolTable.enter(id, attr);
            CurrentClass = attr;

            // push the class body scope table onto the symbol table stack
            _symbolTable.openScope
                (((ClassTypeDescriptor)attr.TypeDescriptor).ClassBody);

            AbstractNode fieldDecls = classBody.Child;
            if (fieldDecls == null) { return; } // grammar allows one class
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
            _symbolTable.closeScope();
        }

        // MethodDeclaring
        private void VisitNode(MethodDeclaration node)
        {
            AbstractNode modifiers = node.Child;
            AbstractNode typeSpecifier = modifiers.Sib;
            AbstractNode methodDeclarator = typeSpecifier.Sib;
            AbstractNode methodBody = methodDeclarator.Sib;

            TypeVisitor typeVisitor = new TypeVisitor();
            if (typeSpecifier is PrimitiveTypeVoid)
            {
                ((PrimitiveTypeVoid)typeSpecifier).Accept(typeVisitor);
            }
            else if (typeSpecifier is PrimitiveTypeBoolean)
            {
                ((PrimitiveTypeBoolean)typeSpecifier).Accept(typeVisitor);
            }
            else if (typeSpecifier is PrimitiveTypeInt)
            {
                ((PrimitiveTypeInt)typeSpecifier).Accept(typeVisitor);
            }
            else if (typeSpecifier is QualifiedName)
            {
                ((QualifiedName)typeSpecifier).Accept(typeVisitor);
            }
            else
            {
                throw new ArgumentException("Return type of a method must " +
                    "be a PrimitiveType or QualifiedName");
            }

            MethodAttributes attr = new MethodAttributes();
            attr.ReturnType = typeSpecifier.TypeDescriptor;
            attr.Modifiers = ((Modifiers) modifiers).ModifierTokens;
            attr.IsDefinedIn = CurrentClass;
            attr.Locals = new ScopeTable();

            AbstractNode methodDeclaratorName = methodDeclarator.Child;
            AbstractNode parameterList = methodDeclaratorName.Sib;

            string name = ((Identifier) methodDeclaratorName).ID;
            _symbolTable.enter(name, attr);
            _symbolTable.openScope(attr.Locals);
            MethodAttributes oldCurrentMethod = CurrentMethod;

            // YOU ARE HERE

        }
    }

    class TypeVisitor : TopDeclVisitor
    {
        public override void Visit(dynamic node)
        {
            this.VisitNode(node);
        }

        private new void VisitNode(AbstractNode node)
        {
            Console.WriteLine("VisitNode, TypeVisitor [" + node.GetType() + "]");
            VisitChildren(node);
        }

        // Identifier
        private void VisitNode(Identifier node)
        {
            Console.WriteLine("Identifier node visit in TypeVisitor.");
            Attributes attr = _symbolTable.lookup(node.ID);
            if (attr != null && attr.Kind == Kind.TypeAttributes)
            {
                node.TypeDescriptor = attr.TypeDescriptor;
                node.AttributesRef = attr;
            }
            else
            {
                node.TypeDescriptor = new ErrorDescriptor();
                node.AttributesRef = null;
            }
        }

        private void VisitNode(QualifiedName node)
        {
            Console.WriteLine("QualifiedName in TypeVisitor");
            AbstractNode child = node.Child;
            while (child != null)
            {
                Identifier identifier = (Identifier)child;
                identifier.Accept(this);
                node.TypeDescriptor = identifier.TypeDescriptor;
                child = child.Sib;
            }
        }

        // ArrayDefining

        // Struct Defining

        // EnumDefining
    }
}
