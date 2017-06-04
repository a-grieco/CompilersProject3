using System;
using System.Collections.Generic;
using System.Linq;

namespace Project4
{
    public partial class CodeGenVisitor : IReflectiveVisitor
    {
        public class LocalVariables
        {
            private int Count { get; set; }
            private Stack<List<String>> LocalVars;

            public LocalVariables()
            {
                LocalVars = new Stack<List<String>>();
                Count = 0;
            }

            public void OpenScope()
            {
                LocalVars.Push(new List<String>());
            }

            public void CloseScope()
            {
                LocalVars.Pop();
            }

            public void AddVariable(string var)
            {
                Count++;
                List<string> scopeList = LocalVars.Pop();
                scopeList.Add(var);
                LocalVars.Push(scopeList);
            }

            public void AddVariables(List<string> names)
            {
                List<string> scopeList = LocalVars.Pop();
                foreach (var name in names)
                {
                    Count++;
                    scopeList.Add(name);
                }
                LocalVars.Push(scopeList);
            }

            public int GetVarLocation(string var)
            {
                foreach (var scopeList in LocalVars)
                {
                    if (scopeList.Contains(var))
                    {
                        // monotonically increasing locations starting at 0
                        return scopeList.IndexOf(var) + 
                            Count - scopeList.Count;    
                    }
                }
                return -1;  // error flag if not found
            }
        }
    }
}
