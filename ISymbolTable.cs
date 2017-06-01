using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project3
{
    public interface ISymbolTable
    {
        // Open a new nested symbol table
        void openScope();

        // Close an existing nested symbol table
        Dictionary<string, Attributes> closeScope();

        int CurrentScopeLevel { get; }

        Attributes lookup(string id);

        void enter(string id, Attributes s);

        /// This lets you put out a message about a node, indented by the current nest level 
        //    void @out(AbstractNode n, string message);
        //    void err(AbstractNode n, string message);
        void @out(string message);
        void err(string message);
    }
}
