using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ASTBuilder;

namespace Project3
{
    public enum Kind { TypeAttributes, VariableAttributes, ClassType }

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
            string kind = "null";
            string typeDescriptor = "null";
            if (Kind != null)
            {
                kind = Kind.ToString();
            }
            if (TypeDescriptor != null)
            {
                typeDescriptor = TypeDescriptor.ToString();
            }

            return String.Format("Kind {0}, TypeDescriptor: {1} {2}",
                kind, typeDescriptor, Notes());
        }

        public string Notes()
        {
            return "";
        }
    }

    public class GeneralAttributes : Attributes
    {
        public GeneralAttributes(TypeDescriptor type) : base(type) { }
    }

    public class ClassAttributes : Attributes { }

    public class MethodAttributes : Attributes
    {
        public TypeDescriptor ReturnType { get; set; }
        public List<ModifiersEnums> Modifiers { get; set; }
        public ScopeTable Locals { get; set; }
        //public MethodAttributes Next { get; set; }
        public ClassAttributes IsDefinedIn { get; set; }
        // add signature for matching
    }

    // LocalVariableDeclarationStatement & FieldVariableDeclaration
    public class VariableDeclarationAttributes : Attributes
    {
        public List<ModifiersEnums> Modifiers { get; set; } // null if LocalVariableDeclarationStatement

        public new string Notes()
        {
            string display = "";
            if (Modifiers != null)
            {
                display += String.Join(", ", Modifiers.ToArray());
            }
            return display;
        }
    }
}