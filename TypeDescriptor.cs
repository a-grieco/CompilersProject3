using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project3
{

    static class ErrorList
    {
        public static List<ErrorDescriptor> Errors =
            new List<ErrorDescriptor>();

        public static void Print()
        {
            foreach (var error in Errors)
            {
                Console.WriteLine(error.Message);
            }
        }
    }

    public abstract class TypeDescriptor { }

    public class ErrorDescriptor : TypeDescriptor
    {
        private ErrorDescriptor() { }

        public ErrorDescriptor(string msg)
        {
            Message = msg;
            ErrorList.Errors.Add(this);
        }

        public ErrorDescriptor CombineErrors(ErrorDescriptor err)
        {
            ErrorDescriptor comboErr = new ErrorDescriptor();
            comboErr.Message = this.Message + "\n" + err.Message;
            return comboErr;
        }

        public string Message { get; private set; }
    }

    public class JavaObjectDescriptor : TypeDescriptor { }

    public class SignatureDescriptor : TypeDescriptor
    {
        //public TypeDescriptor ReturnType;
        public List<TypeDescriptor> ParameterTypes { get; set; }
        public SignatureDescriptor Next { get; set; }

        public SignatureDescriptor()
        {
            ParameterTypes = new List<TypeDescriptor>();
        }

        //public SignatureDescriptor(TypeDescriptor attrReturnType)
        //{
        //    ReturnType = attrReturnType;
        //    ParameterTypes = new List<TypeDescriptor>();
        //}

        public void AddParameter(TypeDescriptor typeDescriptor)
        {
            ParameterTypes.Add(typeDescriptor);
        }

        public int NumParameters()
        {
            return ParameterTypes.Count;
        }

        public String ParametersString()
        {
            return String.Join(", ", ParameterTypes);
        }
    }

    public class ParameterListTypeDescriptor : TypeDescriptor
    {
        public List<TypeDescriptor> ParamTypeDescriptors { get; set; }
    }

    public class ClassTypeDescriptor : TypeDescriptor
    {
        public List<ModifiersEnums> Modifiers { get; set; }
        public ScopeTable ClassBody { get; set; }
    }

    [DebuggerDisplay("MethodTypeDescriptor: {ReturnType, Signature}")]
    public class MethodTypeDescriptor : TypeDescriptor
    {
        public TypeDescriptor ReturnType { get; set; }
        public List<ModifiersEnums> Modifiers { get; set; }
        public ScopeTable Locals { get; set; }
        public ClassTypeDescriptor IsDefinedIn { get; set; }
        public SignatureDescriptor Signature { get; set; }

        public MethodTypeDescriptor()
        {
            Signature = new SignatureDescriptor();
        }
    }

    public class IterationStatementDescriptor : TypeDescriptor
    {
        public TypeDescriptor TypeDescriptor { get; set; }  // for self
        public TypeDescriptor WhileDescriptor { get; set; } // tracking
        public TypeDescriptor BodyDescriptor { get; set; }  // tracking
    }

    public class SelectionStatementDescriptor : TypeDescriptor
    {
        public Boolean HasElseStmt { get; set; }
        public TypeDescriptor TypeDescriptor { get; set; }  // for self
        public TypeDescriptor IfDescriptor { get; set; }    // tracking
        public TypeDescriptor ThanDescriptor { get; set; }  // tracking
        public TypeDescriptor ElseDescriptor { get; set; }  // tracking
    }

    public enum PrimitiveTypes { VOID, INT, BOOLEAN, OBJECT }
    public abstract class PrimitiveTypeDescriptor : TypeDescriptor
    {
        public virtual PrimitiveTypes PrimitiveTypes { get; }
    }

    [DebuggerDisplay("VOID")]
    public class PrimitiveTypeVoidDescriptor : PrimitiveTypeDescriptor
    {
        public override PrimitiveTypes PrimitiveTypes
        {
            get { return PrimitiveTypes.VOID; }
        }
    }

    [DebuggerDisplay("PrimitiveTypeIntDescriptor (INT)")]
    public class PrimitiveTypeIntDescriptor : PrimitiveTypeDescriptor
    {
        public int Value { get; set; }
        public override PrimitiveTypes PrimitiveTypes
        {
            get { return PrimitiveTypes.INT; }
        }
    }

    [DebuggerDisplay("PrimitiveTypeBooleanDescriptor (BOOLEAN)")]
    public class PrimitiveTypeBooleanDescriptor : PrimitiveTypeDescriptor
    {
        public Boolean Value { get; set; }
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

    [DebuggerDisplay("NumberTypeDescriptor: {" + nameof(Num) + "}")]
    public class NumberTypeDescriptor : TypeDescriptor
    {
        public int Num { get; }
        public NumberTypeDescriptor(int num)
        {
            Num = num;
        }
    }

    [DebuggerDisplay("SpecialNameDescriptor: {" + nameof(SpecialNameType) + "}")]
    public class SpecialNameDescriptor : TypeDescriptor
    {
        public SpecialNameEnums SpecialNameType { get; set; }

        public SpecialNameDescriptor(SpecialNameEnums type)
        {
            SpecialNameType = type;
        }
    }

    public class NotJustNameDescriptor : TypeDescriptor { }

    [DebuggerDisplay("LiteralTypeDescriptor: {" + nameof(Value) + "}")]
    public class LiteralTypeDescriptor : TypeDescriptor
    {
        public string Value { get; set; }

        public LiteralTypeDescriptor() { }

        public LiteralTypeDescriptor(string val)
        {
            Value = val;
        }
    }

    public class EmptyStatementDescriptor : TypeDescriptor { }
}
