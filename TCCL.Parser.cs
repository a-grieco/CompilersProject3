using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Project3;

namespace ASTBuilder
{
    public partial class TCCLParser
    {

        public TCCLParser() : base(null) { }

        SymbolTable st = new SymbolTable();

        public void Parse(string filename)
        {
            this.Scanner = new TCCLScanner(File.OpenRead(filename));
            this.Parse();
            PrintTree();
            DoSemantics();
            //PrintTree();
        }
        public void Parse(Stream strm)
        {
            this.Scanner = new TCCLScanner(strm);
            this.Parse();
            PrintTree();
            DoSemantics();
            //PrintTree();
        }
        public void PrintTree()
        {
            PrintVisitor visitor = new PrintVisitor();
            Console.WriteLine("Starting to print AST ");
            visitor.PrintTree(CurrentSemanticValue);
        }

        public void DoSemantics()
        {
            SemanticsVisitor visitor = new SemanticsVisitor();
            Console.WriteLine("Starting semantic checking");
            visitor.CheckSemantics(CurrentSemanticValue);

        }


    }
}
