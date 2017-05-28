﻿using System;
using System.Collections.Generic;

namespace Project3
{
    public enum Kind { TypeAttributes, VariableAttributes, ClassType, PrimitiveType }

    public abstract class Attributes
    {
        public Kind Kind { get; set; }

        public TypeDescriptor TypeDescriptor { get; set; }

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

    public class GeneralAttributes : Attributes
    {
        public GeneralAttributes(TypeDescriptor type) : base(type) { }
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

    public class ClassAttributes : Attributes { }

    public class MethodAttributes : Attributes
    {
        public TypeDescriptor ReturnType { get; set; }
        public List<ModifiersEnums> Modifiers { get; set; }
        public ScopeTable Locals { get; set; }
        public ClassTypeDescriptor IsDefinedIn { get; set; }
        public SignatureDescriptor Signature { get; set; }
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