using System;

namespace Project3
{
    public class TypeVisitor : SemanticsVisitor
    {
        public TypeVisitor(SymbolTable st) : base(st) { }

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

        // Identifier
        private void VisitNode(Identifier node)
        {
            Attributes attr = Table.lookup(node.ID);
            if (attr != null && (attr.Kind == Kind.TypeAttributes || 
                attr.TypeDescriptor is ErrorDescriptor))
            {
                node.TypeDescriptor = attr.TypeDescriptor;
                node.AttributesRef = attr;
            }
            else
            {
                node.TypeDescriptor = new ErrorDescriptor("Identifier " +
                    node.ID + " not listes as a type.");
                node.AttributesRef = null;
            }
        }

        private void VisitNode(PrimitiveTypeVoid node)
        {
            Attributes attr = new Attr(node.TypeDescriptor);
            node.AttributesRef = attr;
        }

        private void VisitNode(PrimitiveTypeInt node)
        {
            Attributes attr = new Attr(node.TypeDescriptor);
            node.AttributesRef = attr;
        }

        private void VisitNode(PrimitiveTypeBoolean node)
        {
            Attributes attr = new Attr(node.TypeDescriptor);
            node.AttributesRef = attr;
        }

        private void VisitNode(QualifiedName node)
        {
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
            Attributes attr = Table.lookup(node.Name.ToString());
            node.TypeDescriptor = attr.TypeDescriptor;
            node.AttributesRef = attr;
        }

        private void VisitNode(FieldAccess node)
        {
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