using System.Collections.Generic;

namespace Project3
{
    class SymbolTable : AbstractSymbolTable, ISymbolTable
    {
        private int nestLevel;
        private Stack<ScopeTable> symbolTable;

        public SymbolTable()
        {
            symbolTable = new Stack<ScopeTable>();
            symbolTable.Push(new ScopeTable());
            nestLevel = 0;
        }

        // Open a new scope, retaining outer ones
        public void openScope()
        {
            nestLevel++;
            while (symbolTable.Count <= nestLevel)
            {
                symbolTable.Push(new ScopeTable());
            }
        }

        // Close the innermost scope
        public void closeScope()
        {
            nestLevel--;
            symbolTable.Pop();
        }

        public override int CurrentNestLevel
        {
            get { return nestLevel; }
        }

        // Enter the given symbol information into the symbol table.  If the given
        // symbol is already present at the current nest level, throw an exception.
        public SymbolTableEntry lookup(string id)
        {
            throw new System.NotImplementedException();
        }

        // Returns the information associated with the innermost currently valid
        // declaration of the given symbol.  If there is no such valid declaration,
        // throw an excpetion.
        public void enter(string id, SymbolTableEntry s)
        {
            throw new System.NotImplementedException();
        }
    }

    internal class ScopeTable
    {
        Dictionary<string, SymbolTableEntry> thisScope = new Dictionary<string, SymbolTableEntry>();

        // adds a key/value pair to the symbol table at this scope
        // (keeps the original value if duplicate)
        public void Add(string key, SymbolTableEntry val)
        {
            if (thisScope.ContainsKey(key))
            {
                // TODO: return an error type
            }
            else
            {
                thisScope.Add(key, val);
            }
        }

        // returns the value stored for the given key
        public SymbolTableEntry Get(string key)
        {
            if (thisScope.ContainsKey(key))
            {
                return thisScope[key];
            }
            // TODO: return error type
            return null;    // key not found 
        }

        // returns whether or not the key provided is defined in this scope
        public bool Contains(string key)
        {
            return thisScope.ContainsKey(key);
        }
    }
}
