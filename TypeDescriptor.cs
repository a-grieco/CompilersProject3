using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project3
{
    public abstract class TypeDescriptor { }

    public class ErrorDescriptor : TypeDescriptor
    {
        public string Message { get; set; }
    }

    public class JavaObjectDescriptor : TypeDescriptor { }

    public class VariableDeclarationDescriptor : TypeDescriptor { }

    public class IntegerTypeDescriptor : TypeDescriptor { }

    public class ArrayTypeDescriptor : TypeDescriptor { }

    public class ClassTypeDescriptor : TypeDescriptor
    {
        public List<ModifiersEnums> Modifiers { get; set; }
        public ScopeTable ClassBody { get; set; }
    }

    public class MethodTypeDescriptor : TypeDescriptor { }

    public enum PrimitiveTypes { VOID, INT, BOOLEAN }
    public abstract class PrimitiveTypeDescriptor : TypeDescriptor
    {
        public virtual PrimitiveTypes PrimitiveTypes { get; }
        public virtual string Name { get; }
    }

    public class PrimitiveVoidTypeDescriptor : PrimitiveTypeDescriptor
    {
        public override PrimitiveTypes PrimitiveTypes
        {
            get { return PrimitiveTypes.VOID; }
        }
        public override string Name { get { return "VOID"; } }
    }

    public class PrimitiveIntTypeDescriptor : PrimitiveTypeDescriptor
    {
        public override PrimitiveTypes PrimitiveTypes
        {
            get { return PrimitiveTypes.INT; }
        }
        public override string Name { get { return "INT"; } }
    }

    public class PrimitiveBooleanTypeDescriptor : PrimitiveTypeDescriptor
    {
        public override PrimitiveTypes PrimitiveTypes
        {
            get { return PrimitiveTypes.BOOLEAN; }
        }
        public override string Name { get { return "BOOLEAN"; } }
    }

    public class MethodCallDescriptor : TypeDescriptor { }

    public class SpecialNameDescriptor : TypeDescriptor { }

    public class NotJustNameDescriptor : TypeDescriptor { }

    public class LiteralTypeDescriptor : TypeDescriptor
    {
        public string Value { get; set; }
    }

    //public class MethodReferenceDescriptor : TypeDescriptor { }

}
