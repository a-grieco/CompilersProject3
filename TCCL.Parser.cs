using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Project4;

namespace ASTBuilder
{
    public partial class TCCLParser
    {
        private string _filename;
        public TCCLParser() : base(null) { }

        public void Parse(string filename)
        {
            _filename = filename.Substring(0, filename.Length - ".txt".Length);
            this.Scanner = new TCCLScanner(File.OpenRead(filename));
            this.Parse();
            DoSemantics();
            PrintTree();
            CreateIlFile();
        }

        public void Parse(Stream strm)
        {
            this.Scanner = new TCCLScanner(strm);
            this.Parse();
            DoSemantics();
            PrintTree();
            CreateIlFile();
        }


        public void PrintTree()
        {
            PrintVisitor visitor = new PrintVisitor();
            Console.WriteLine("\nStarting to print AST\n");
            visitor.PrintTree(CurrentSemanticValue);
        }

        public void DoSemantics()
        {
            Console.WriteLine("\nStarting declarations processing\n");
            SymbolTable symbolTable = new SymbolTable();
            TopDeclVisitor topDeclVisitor = new TopDeclVisitor(symbolTable);
            topDeclVisitor.CheckSemantics(CurrentSemanticValue);
            Console.WriteLine("\nErrors found:");
            ErrorList.Print();
        }

        private void CreateIlFile()
        {
            Console.WriteLine("\nCreating .il file\n");
            if (File.Exists(_filename + ".il")) { File.Delete(_filename + ".il"); }
            using (StreamWriter sw = File.CreateText(_filename + ".il"))
            {
                //CodeGenVisitor codeGenVisitor = new CodeGenVisitor(System.Console.Out);
                CodeGenVisitor codeGenVisitor = new CodeGenVisitor(sw);
                codeGenVisitor.CreateIlFileContent(CurrentSemanticValue);
            }
        }
    }
}
