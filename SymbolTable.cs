﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Project4
{
    public class SymbolTable : AbstractSymbolTable, ISymbolTable
    {
        private const bool PRINT_STATUS = true;
        private bool created = false;

        private int scopeLevel;
        private Stack<ScopeTable> symbolTable;

        public SymbolTable()
        {
            symbolTable = new Stack<ScopeTable>();
            openScope();
            scopeLevel = 0;
        }

        // Open a new scope, retaining outer ones
        public void openScope()
        {
            scopeLevel++;
            symbolTable.Push(new ScopeTable());
            setBaseline();
            if (PRINT_STATUS)
            {
                if (created)
                {
                    Console.WriteLine("   Pushed Symbol Table: scope level " +
                                      scopeLevel);
                }
                else { created = true; }
            }
        }

        public void openScope(ScopeTable scopeTable)
        {
            scopeLevel++;
            symbolTable.Push(scopeTable);
            setBaseline();
            if (PRINT_STATUS)
            {
                Console.WriteLine("   Pushed Symbol Table: scope level " +
                    scopeLevel);
            }
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

            // Write & WriteLine
            PrimitiveTypeVoidDescriptor returnType = new PrimitiveTypeVoidDescriptor();
            // no parameters
            SignatureDescriptor sigDescNone = new SignatureDescriptor();
            // integer parameter
            SignatureDescriptor sigDescInt = new SignatureDescriptor();
            sigDescInt.AddParameter(new PrimitiveTypeIntDescriptor());
            // boolean parameter
            SignatureDescriptor sigDescBoolean = new SignatureDescriptor();
            sigDescBoolean.AddParameter(new PrimitiveTypeBooleanDescriptor());
            // literal parameter (string)
            SignatureDescriptor sigDescLiteral = new SignatureDescriptor();
            sigDescLiteral.AddParameter(new LiteralTypeDescriptor());
            // chain signature type descriptors together (sigDescNode = first)
            sigDescNone.Next = sigDescInt;
            sigDescInt.Next = sigDescBoolean;
            sigDescBoolean.Next = sigDescLiteral;
            MethodTypeDescriptor methodTypeDescriptor = new MethodTypeDescriptor();
            methodTypeDescriptor.ReturnType = returnType;
            methodTypeDescriptor.Signature = sigDescNone;
            Attr methodAttr = new Attr(methodTypeDescriptor);
            currScopeTable.Add("Write", methodAttr);
            currScopeTable.Add("WriteLine", methodAttr);

            // TODO: add additional baseline information

            symbolTable.Push(currScopeTable);
        }

        // Close the innermost scope
        public Dictionary<string, Attributes> closeScope()
        {
            scopeLevel--;
            ScopeTable copy = symbolTable.Pop();
            if (PRINT_STATUS)
            {
                Console.WriteLine("   Popped Symbol Table: scope level " +
                    scopeLevel);
            }
            return copy.GetCopy();
        }

        public override int CurrentScopeLevel
        {
            get { return scopeLevel; }
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
            return new Attr(new ErrorDescriptor("Undeclared variable: " + id));
        }

        // Enter the given symbol information into the symbol table.  If the given
        // symbol is already present at the current scope level, assign error.
        public void enter(string id, Attributes attr)
        {
            ScopeTable currentScopeTable = symbolTable.Pop();
            TypeDescriptor err = currentScopeTable.Add(id, attr);
            symbolTable.Push(currentScopeTable);

            if (err != null) { attr.TypeDescriptor = err; }
            if (PRINT_STATUS)
            {
                if (err != null)
                {
                    Console.WriteLine(((ErrorDescriptor)err).Message);
                }
                else
                {
                    Console.WriteLine("[\"" + id + "\"" + ", " + attr + "] " +
                                   "added to Symbol Table, scope level: " +
                                   CurrentScopeLevel);
                }
            }
        }

        public bool isDeclaredLocally(string id)
        {
            ScopeTable currentScopeTable = symbolTable.Peek();
            return currentScopeTable.Contains(id);
        }

        public Boolean updateValue(string id, Attributes attr)
        {
            foreach (var scope in symbolTable)
            {
                if (scope.Contains(id))
                {
                    return scope.UpdateValue(id, attr);
                }
            }
            return false;
        }
    }

    public class ScopeTable
    {
        readonly Dictionary<string, Attributes> _thisScope =
            new Dictionary<string, Attributes>();

        // adds a key/value pair to the symbol table at this scope
        // (keeps the original value if duplicate)
        public TypeDescriptor Add(string key, Attributes val)
        {
            if (_thisScope.ContainsKey(key))
            {
                return new ErrorDescriptor("Symbol Table already contains " +
                                           "key: " + key);
            }
            _thisScope.Add(key, val);
            return null;
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

        // updates the attribute of a symbol already in the table
        public bool UpdateValue(string id, Attributes attr)
        {
            if (_thisScope.ContainsKey(id))
            {
                _thisScope[id] = attr;
                return true;
            }
            return false;
        }
    }
}
