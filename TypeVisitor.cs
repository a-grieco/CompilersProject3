using System;

namespace Project3
{
    public class TypeVisitor : SemanticsVisitor
    {
        public TypeVisitor(SymbolTable st) : base(st)
        {
        }

        public override void Visit(dynamic node)
        {
            this.VisitNode(node);
        }

        private new void VisitNode(AbstractNode node)
        {
            Console.WriteLine("VisitNode, TypeVisitor [" + node.GetType() + "]");
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

        // Identifier
        private void VisitNode(Identifier node)
        {
            Console.WriteLine("Identifier node visit in TypeVisitor.");
            Attributes attr = Table.lookup(node.ID);
            if (attr != null && attr.Kind == Kind.TypeAttributes)
            {
                node.TypeDescriptor = attr.TypeDescriptor;
                node.AttributesRef = attr;
            }
            else
            {
                string message = "This identifier is not a type name: "
                                 + node.ID;
                Console.WriteLine(message); // TODO: delete me
                node.TypeDescriptor = new ErrorDescriptor(message);
                node.AttributesRef = null;
            }
        }

        private void VisitNode(PrimitiveTypeVoid node)
        {
            Console.WriteLine("PrimitiveTypeVoid node visit in TypeVisitor.");
            Attributes attr = new PrimitiveAttributes(node.TypeDescriptor);
            node.AttributesRef = attr;
        }

        private void VisitNode(PrimitiveTypeInt node)
        {
            Console.WriteLine("PrimitiveTypeInt node visit in TypeVisitor.");
            Attributes attr = new PrimitiveAttributes(node.TypeDescriptor);
            node.AttributesRef = attr;
        }

        private void VisitNode(PrimitiveTypeBoolean node)
        {
            Console.WriteLine("PrimitiveTypeBoolean node visit in TypeVisitor.");
            Attributes attr = new PrimitiveAttributes(node.TypeDescriptor);
            node.AttributesRef = attr;
        }

        private void VisitNode(QualifiedName node)
        {
            Console.WriteLine("QualifiedName in TypeVisitor");
            AbstractNode child = node.Child;
            while (child != null)
            {
                ((Identifier)child).Accept(this);
                node.TypeDescriptor = child.TypeDescriptor;
                node.AttributesRef = child.AttributesRef;
                child = child.Sib;
            }
        }

        private void VisitNode(SpecialName node)
        {
            Console.WriteLine("SpecialName in TypeVisitor");
            Attributes attr = Table.lookup(node.Name.ToString());
            node.TypeDescriptor = attr.TypeDescriptor;
            node.AttributesRef = attr;
        }

        private void VisitNode(FieldAccess node)
        {
            Console.WriteLine("FieldAccess in TypeVisitor");
            AbstractNode notJustName = node.Child;
            AbstractNode identifier = notJustName.Sib;

            FieldAccessAttributes attr = new FieldAccessAttributes();
            notJustName.Accept(this);
            attr.NotJustNameTypeDescriptor = notJustName.TypeDescriptor;
            identifier.Accept(this);
            attr.TypeDescriptor = identifier.TypeDescriptor;
            node.TypeDescriptor = attr.TypeDescriptor;
            node.AttributesRef = attr;
        }
    }
}