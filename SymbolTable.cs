using System;
using System.Collections.Generic;
using System.Linq;

namespace Project3
{
    public class SymbolTable : AbstractSymbolTable, ISymbolTable
    {
        private const bool PRINT_STATUS = true;

        private int nestLevel;
        private Stack<ScopeTable> symbolTable;

        public SymbolTable()
        {
            symbolTable = new Stack<ScopeTable>();
            openScope();
            nestLevel = 0;
        }

        // Open a new scope, retaining outer ones
        public void openScope()
        {
            nestLevel++;
            symbolTable.Push(new ScopeTable());
            setBaseline();
        }

        public void openScope(ScopeTable scopeTable)
        {
            nestLevel++;
            symbolTable.Push(scopeTable);
            setBaseline();
        }

        private void setBaseline()
        {
            ScopeTable currScopeTable = symbolTable.Pop();
            // Until given additional info, Java objects are placeholders
            Attr javaAttr = new Attr(new JavaObjectDescriptor());
            javaAttr.Kind = Kind.TypeAttributes;
            currScopeTable.Add("java", javaAttr);
            currScopeTable.Add("io", javaAttr);
            currScopeTable.Add("lang", javaAttr);
            currScopeTable.Add("System", javaAttr);
            currScopeTable.Add("out", javaAttr);
            currScopeTable.Add("print", javaAttr);
            currScopeTable.Add("outint", javaAttr);
            currScopeTable.Add("PrintStream", javaAttr);
            currScopeTable.Add("TestClasses", javaAttr);

            // primitive types
            PrimitiveAttributes primVoidAttrs =
                new PrimitiveAttributes(new PrimitiveTypeVoidDescriptor());
            currScopeTable.Add("VOID", primVoidAttrs);
            PrimitiveAttributes primIntAttrs =
                new PrimitiveAttributes(new PrimitiveTypeIntDescriptor());
            currScopeTable.Add("INT", primIntAttrs);
            PrimitiveAttributes primBooleanAttrs =
                new PrimitiveAttributes(new PrimitiveTypeBooleanDescriptor());
            currScopeTable.Add("BOOLEAN", primBooleanAttrs);

            // special names
            SpecialNameAttributes spNameThis = new SpecialNameAttributes
                (SpecialNameEnums.THIS);
            currScopeTable.Add(spNameThis.Name, spNameThis);
            SpecialNameAttributes spNameNull = new SpecialNameAttributes(
                SpecialNameEnums.NULL);
            currScopeTable.Add(spNameNull.Name, spNameNull);

            // WriteLine
            PrimitiveTypeVoidDescriptor returnType = new PrimitiveTypeVoidDescriptor();
            // no parameters
            SignatureDescriptor sigDescNone = new SignatureDescriptor(returnType);
            // integer parameter
            SignatureDescriptor sigDescInt = new SignatureDescriptor(returnType);
            sigDescInt.AddParameter(new PrimitiveTypeIntDescriptor());
            // boolean parameter
            SignatureDescriptor sigDescBoolean = new SignatureDescriptor(returnType);
            sigDescBoolean.AddParameter(new PrimitiveTypeBooleanDescriptor());
            // literal parameter (string)
            SignatureDescriptor sigDescLiteral = new SignatureDescriptor(returnType);
            sigDescLiteral.AddParameter(new LiteralTypeDescriptor());
            // chain signature type descriptors together
            sigDescNone.Next = sigDescInt;
            sigDescInt.Next = sigDescBoolean;
            sigDescBoolean.Next = sigDescLiteral;
            Attr sigAttr = new Attr(sigDescNone);
            currScopeTable.Add("Write", sigAttr);
            currScopeTable.Add("WriteLine", sigAttr);

            // TODO: add additional baseline information

            symbolTable.Push(currScopeTable);
        }

        // Close the innermost scope
        public Dictionary<string, Attributes> closeScope()
        {
            nestLevel--;
            ScopeTable copy = symbolTable.Pop();
            return copy.GetCopy();
        }

        public override int CurrentNestLevel
        {
            get { return nestLevel; }
        }

        // Returns the information associated with the innermost currently valid
        // declaration of the given symbol.  If there is no such valid declaration,
        // throw an excpetion.
        public Attributes lookup(string id)
        {
            foreach (var scope in symbolTable)
            {
                if (scope.Contains(id))
                {
                    return scope.Get(id);
                }
            }
            throw new System.ArgumentException("Symbol " + id + " not present in table.");
        }

        // Enter the given symbol information into the symbol table.  If the given
        // symbol is already present at the current nest level, throw an exception.
        public void enter(string id, Attributes attr)
        {
            ScopeTable currentScopeTable = symbolTable.Pop();
            currentScopeTable.Add(id, attr);
            symbolTable.Push(currentScopeTable);
            if (PRINT_STATUS)
            {
                printStatus(id, attr, symbolTable);
            }
        }

        private void printStatus(string id, Attributes attr, Stack<ScopeTable> scopeTables)
        {
            Console.WriteLine("[\"" + id + "\"" + ", " + attr + "] added to " +
                              "Symbol Table, scope level: " + CurrentNestLevel);
        }

        public bool isDeclaredLocally(string id)
        {
            ScopeTable currentScopeTable = symbolTable.Peek();
            return currentScopeTable.Contains(id);
        }
    }

    public class ScopeTable
    {
        readonly Dictionary<string, Attributes> _thisScope =
            new Dictionary<string, Attributes>();

        // adds a key/value pair to the symbol table at this scope
        // (keeps the original value if duplicate)
        public void Add(string key, Attributes val)
        {
            if (_thisScope.ContainsKey(key))
            {
                throw new System.ArgumentException("Symbol already in table.");
            }
            _thisScope.Add(key, val);
        }

        // returns the value stored for the given key
        public Attributes Get(string key)
        {
            if (_thisScope.ContainsKey(key))
            {
                return _thisScope[key];
            }
            return null;    // key not found 
        }

        // returns whether or not the key provided is defined in this scope
        public bool Contains(string key)
        {
            return _thisScope.ContainsKey(key);
        }

        // returns a copy of the ScopeTable as a dictionary
        public Dictionary<string, Attributes> GetCopy()
        {
            return _thisScope.ToDictionary(entry => entry.Key,
                entry => entry.Value);
        }
    }
}
