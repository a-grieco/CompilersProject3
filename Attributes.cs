using System;
using System.Collections.Generic;

namespace Project4
{
    public enum Kind { TypeAttributes, VariableAttributes, ClassType, MethodType }

    public abstract class Attributes
    {
        public Kind Kind { get; set; }

        public TypeDescriptor TypeDescriptor { get; set; }

        public Boolean IsAssignable { get; set; }

        protected Attributes() { }

        protected Attributes(TypeDescriptor type)
        {
            TypeDescriptor = type;
        }

        public override string ToString()
        {
            string kind = Kind.ToString(); ;
            string typeDescriptor = "null";
            if (TypeDescriptor != null)
            {
                typeDescriptor = TypeDescriptor.GetType().Name;
            }

            if (Notes().Length > 0)
            {
                return String.Format("{0} {1} {2}", 
                    kind, typeDescriptor, Notes());
            }
            return String.Format("{0} {1}", kind, typeDescriptor);
        }

        public virtual string Notes()
        {
            return "";
        }
    }

    public class Attr : Attributes
    {
        public Attr() { }
        public Attr(TypeDescriptor typeDescriptor) : base(typeDescriptor) { }
    }

    // LocalVariableDeclarationStatement & FieldVariableDeclaration
    public class VariableDeclarationAttributes : Attributes
    {
        public List<ModifiersEnums> Modifiers { get; set; } // null if LocalVariableDeclarationStatement

        public override string Notes()
        {
            string display = "";
            if (Modifiers != null)
            {
                display += String.Join(", ", Modifiers.ToArray());
            }
            return display;
        }
    }

    public class PrimitiveAttributes : Attributes
    {
        public PrimitiveAttributes(TypeDescriptor typeDescriptor)
        {
            TypeDescriptor = typeDescriptor;
        }
    }

    public class ParameterAttributes : Attributes { }

    public class SpecialNameAttributes : Attributes
    {
        public SpecialNameAttributes(SpecialNameEnums type)
        {
            SpecialNameType = type;
            TypeDescriptor = new SpecialNameDescriptor(type);
        }

        public SpecialNameEnums SpecialNameType { get; set; }
        public string Name { get { return SpecialNameType.ToString(); } }
    }

    public class FieldAccessAttributes : Attributes
    {
        public TypeDescriptor NotJustNameTypeDescriptor { get; set; }
    }

}