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
            PrintTree();
        }
        public void Parse(Stream strm)
        {
            this.Scanner = new TCCLScanner(strm);
            this.Parse();
            PrintTree();
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
            Console.WriteLine("Starting declarations processing");
            topDeclVisitor.CheckSemantics(CurrentSemanticValue);

            SemanticsVisitor semanticsVisitor = new SemanticsVisitor(symbolTable);
            Console.WriteLine("Starting semantic checking");
            semanticsVisitor.CheckSemantics(CurrentSemanticValue);

        }


    }
}
