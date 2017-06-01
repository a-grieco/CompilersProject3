using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project3
{
    public abstract class AbstractSymbolTable
    {
        public abstract int CurrentScopeLevel { get; }

        public virtual void @out(string s)
        {
            string tab = "";
            for (int i = 1; i <= CurrentScopeLevel; ++i)
            {
                tab += "  ";
            }
            Console.WriteLine(tab + s);
        }

        public virtual void err(string s)
        {
            @out("Error: " + s);
            Console.Error.WriteLine("Error: " + s);
            Environment.Exit(-1);
        }

    }
}
