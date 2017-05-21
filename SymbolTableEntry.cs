using System.Runtime.InteropServices;

namespace Project3
{
    public enum AttributeType
    {
        TypeAttribute,
        VariableAttribute
    }

    public enum EntryType
    {
        ClassType,
        ModifierType,
        MethodType,
        BodyType,
        FieldDeclType,
        StructDeclType,
        FieldVarDeclType,
        ArrayType,
        PrimitiveType,
        FieldVarDeclaratorsType,
        MethodDeclType,
        ParameterListType // TODO: fix these
    }

    public abstract class SymbolTableEntry
    {
        public abstract AttributeType AttributeType { get; set; }
        public abstract EntryType EntryType { get; }
        public abstract Description Desc { get; set; }
        public abstract int Size { get; }

        //public SymbolTableEntry(AttributeType attr)
        //{
        //    AttributeType = attr;
        //}
    }

    public class ClassEntry : SymbolTableEntry
    {
        public override AttributeType AttributeType { get; set; }
        public override EntryType EntryType { get; }
        public override Description Desc { get; set; }
        public override int Size { get; }
    }
}