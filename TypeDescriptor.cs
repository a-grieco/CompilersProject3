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
        public ErrorDescriptor(string msg)
        {
            Message = msg;
        }

        public string Message { get; }
    }

    public class JavaObjectDescriptor : TypeDescriptor { }

    public class SignatureDescriptor : TypeDescriptor
    {
        public TypeDescriptor ReturnType;
        public List<TypeDescriptor> ParameterTypes { get; set; }
        public SignatureDescriptor Next { get; set; }

        public SignatureDescriptor()
        {
            ParameterTypes = new List<TypeDescriptor>();
        }

        public SignatureDescriptor(TypeDescriptor attrReturnType)
        {
            ReturnType = attrReturnType;
            ParameterTypes = new List<TypeDescriptor>();
        }

        public void AddParameter(TypeDescriptor typeDescriptor)
        {
            ParameterTypes.Add(typeDescriptor);
        }

        public int NumParameters()
        {
            return ParameterTypes.Count;
        }
    }

    //public class VariableDeclarationDescriptor : TypeDescriptor { }

    //public class IntegerTypeDescriptor : TypeDescriptor { }

    //public class ArrayTypeDescriptor : TypeDescriptor { }

    public class ClassTypeDescriptor : TypeDescriptor
    {
        public List<ModifiersEnums> Modifiers { get; set; }
        public ScopeTable ClassBody { get; set; }
    }

    public class MethodTypeDescriptor : TypeDescriptor
    {
        public TypeDescriptor ReturnType { get; set; }
        public List<ModifiersEnums> Modifiers { get; set; }
        public ScopeTable Locals { get; set; }
        public ClassTypeDescriptor IsDefinedIn { get; set; }
        public SignatureDescriptor Signature { get; set; }
    }

    public enum PrimitiveTypes { VOID, INT, BOOLEAN, OBJECT }
    public abstract class PrimitiveTypeDescriptor : TypeDescriptor
    {
        public virtual PrimitiveTypes PrimitiveTypes { get; }
    }

    public class PrimitiveTypeVoidDescriptor : PrimitiveTypeDescriptor
    {
        public override PrimitiveTypes PrimitiveTypes
        {
            get { return PrimitiveTypes.VOID; }
        }
    }

    public class PrimitiveTypeIntDescriptor : PrimitiveTypeDescriptor
    {
        public override PrimitiveTypes PrimitiveTypes
        {
            get { return PrimitiveTypes.INT; }
        }
    }

    public class PrimitiveTypeBooleanDescriptor : PrimitiveTypeDescriptor
    {
        public override PrimitiveTypes PrimitiveTypes
        {
            get { return PrimitiveTypes.BOOLEAN; }
        }
    }

    public class PrimitiveObjectTypeDescriptor : PrimitiveTypeDescriptor
    {
        public override PrimitiveTypes PrimitiveTypes
        {
            get { return PrimitiveTypes.OBJECT; }
        }
    }

    public class NumberTypeDescriptor : TypeDescriptor
    {
        public int Num { get; }
        public NumberTypeDescriptor(int num)
        {
            Num = num;
        }
    }


    //public class MethodCallDescriptor : TypeDescriptor
    //{
    //    public MethodCallDescriptor()
    //    {
    //        ExpressionAttributeRef = new List<TypeDescriptor>();
    //    }

    //    public MethodCallDescriptor(TypeDescriptor methodRefType)
    //    {
    //        MethodRecerenceType = methodRefType;
    //        ExpressionAttributeRef = new List<TypeDescriptor>();
    //    }

    //    public TypeDescriptor MethodRecerenceType { get; set; }
    //    public List<TypeDescriptor> ExpressionAttributeRef { get; set; }    // TODO: refactor?
    //}

    public class SpecialNameDescriptor : TypeDescriptor
    {
        public SpecialNameEnums SpecialNameType { get; set; }

        public SpecialNameDescriptor(SpecialNameEnums type)
        {
            SpecialNameType = type;
        }
    }

    public class NotJustNameDescriptor : TypeDescriptor { }

    public class LiteralTypeDescriptor : TypeDescriptor
    {
        public string Value { get; set; }

        public LiteralTypeDescriptor() { }

        public LiteralTypeDescriptor(string val)
        {
            Value = val;
        }
    }


}
