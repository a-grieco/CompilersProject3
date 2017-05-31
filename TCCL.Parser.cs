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
            //PrintTree();
            DoSemantics();
            PrintTree();
        }
        public void Parse(Stream strm)
        {
            this.Scanner = new TCCLScanner(strm);
            this.Parse();
            //PrintTree();
            DoSemantics();
            PrintTree();
        }
        public void PrintTree()
        {
            PrintVisitor visitor = new PrintVisitor();
            Console.WriteLine("Starting to print AST ");
            visitor.PrintTree(CurrentSemanticValue);
        }

        public void DoSemantics()
        {
            SymbolTable symbolTable = new SymbolTable();

            TopDeclVisitor topDeclVisitor = new TopDeclVisitor(symbolTable);
            Console.WriteLine("\nStarting declarations processing");
            topDeclVisitor.CheckSemantics(CurrentSemanticValue);
            //Console.WriteLine("Errors found:");
            //ErrorList.Print();

            //SemanticsVisitor semanticsVisitor = new SemanticsVisitor(symbolTable);
            //Console.WriteLine("\nStarting semantic checking");
            //semanticsVisitor.CheckSemantics(CurrentSemanticValue);

        }


    }
}
