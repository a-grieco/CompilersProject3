using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ASTBuilder;

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

            return String.Format("{0} {1}", kind, typeDescriptor);
        }

        public virtual string Notes()
        {
            return "";
        }
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
        public ClassAttributes IsDefinedIn { get; set; }
        public Signature Signature { get; set; }
    }

    public class PrimitiveAttributes : Attributes
    {
        public PrimitiveAttributes(PrimitiveTypes type)
        {
            switch (type)
            {
                case PrimitiveTypes.VOID:
                    TypeDescriptor = new PrimitiveVoidTypeDescriptor();
                    break;
                case PrimitiveTypes.INT:
                    TypeDescriptor = new PrimitiveIntTypeDescriptor();
                    break;
                case PrimitiveTypes.BOOLEAN:
                    TypeDescriptor = new PrimitiveBooleanTypeDescriptor();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type),
                        type, null);
            }
        }

        public string Name()
        {
            return ((PrimitiveTypeDescriptor)TypeDescriptor).Name;
        }

        public override string Notes()
        {
            return ((PrimitiveTypeDescriptor)TypeDescriptor).Name;
        }
    }

    public class ParameterAttributes : Attributes { }

    public class MethodCallAttributes : Attributes
    {

    }

    public class SpecialNameAttributes : Attributes
    {
        public SpecialNameAttributes(SpecialNameEnums type)
        {
            SpecialNameType = type;
            TypeDescriptor = new SpecialNameDescriptor();
        }

        public SpecialNameEnums SpecialNameType { get; set; }
        public string Name { get { return SpecialNameType.ToString(); } }
    }

    public class FieldAccessAttributes : Attributes
    {
        public TypeDescriptor NotJustNameTypeDescriptor { get; set; }
    }

    public class MethodReferenceAttributes : Attributes
    {
        public ExpressionAttributes ExpressionAttributeRef { get; set; }
    }

    public class ExpressionAttributes : Attributes
    {
        
    }
}