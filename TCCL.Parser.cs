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
            DoSemantics();
            PrintTree();
        }
        public void Parse(Stream strm)
        {
            this.Scanner = new TCCLScanner(strm);
            this.Parse();
            DoSemantics();
            PrintTree();
        }
        public void PrintTree()
        {
            PrintVisitor visitor = new PrintVisitor();
            Console.WriteLine("\nStarting to print AST\n");
            visitor.PrintTree(CurrentSemanticValue);
        }

        public void DoSemantics()
        {
            SymbolTable symbolTable = new SymbolTable();

            TopDeclVisitor topDeclVisitor = new TopDeclVisitor(symbolTable);
            Console.WriteLine("\nStarting declarations processing\n");
            topDeclVisitor.CheckSemantics(CurrentSemanticValue);
            Console.WriteLine("\nErrors found:");
            ErrorList.Print();
        }


    }
}
