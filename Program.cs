using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using Microsoft.Build.Utilities;
namespace ASTBuilder
{
    class Program
    {
        public static void ILasm(String filename)
        {
            string ilasm = Microsoft.Build.Utilities.ToolLocationHelper.
                GetPathToDotNetFrameworkFile("ilasm.exe",
                TargetDotNetFrameworkVersion.VersionLatest);
            string filenameil = Path.Combine(Directory.GetCurrentDirectory(),
                filename + ".il");
            string ilasmArg = "\"" + filenameil + "\"";
            string execArgs = "/c \"" + filename + ".exe\" &pause";

            Console.WriteLine("Invoking ILASM: {0}", ilasm + " " + ilasmArg);
            Console.WriteLine("----------------------------------------");

            Process ilProcess = new Process
            {
                StartInfo =
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    FileName = ilasm,
                    Arguments = ilasmArg
                }
            };

            ilProcess.Start();
            string output = ilProcess.StandardOutput.ReadToEnd();
            ilProcess.WaitForExit();

            Console.WriteLine(output);
            Console.WriteLine("Invoking compiled executable: {0}", filename);
            Console.WriteLine("----------------------------------------");

            Process userProcess = new Process
            {
                StartInfo = { FileName = "cmd.exe", Arguments = execArgs, },
                EnableRaisingEvents = true,
            };

            userProcess.Start();
            userProcess.WaitForExit();
        }

        private const string QUIT = "quit";

        private const Boolean TEST = false;

        static void Main(string[] args)
        {
            if (TEST)
            {
                string fileName = "test.il";

                if (File.Exists(fileName)) { File.Delete(fileName); }
                using (StreamWriter sw = File.CreateText(fileName))
                {
                    //sw.WriteLine(".assembly HelloWorld { }\n" +
                    //    ".method public static void Main() cil managed\n" +
                    //    "{\n" +
                    //    "\t.entrypoint\n" +
                    //    "\t.maxstack 1\n" +
                    //    "\tldstr \"Hello World\"\n" +
                    //    "\tcall void [mscorlib]System.Console::WriteLine(string)\n" +
                    //    "\tret\n" +
                    //    "\n}");

                    sw.WriteLine(".assembly extern mscorlib {}\n" +
                        ".assembly addnums {}\n" +  // don't forget 2 headers
                        ".class public hello\n" +
                        "{\n" +
                        ".method public void main() cil managed\n" +
                        "{\n" +
                        ".entrypoint\n" +
                        ".maxstack 42\n" +
                        "ldstr \"Hello, World Test\"\n" +
                        "call void [mscorlib]System.Console::WriteLine(string)\n" +
                        "ret\n" +
                        "\n}" +
                        "\n}");
                }
                ILasm(fileName.Substring(0, fileName.Length - ".il".Length));
            }

            else
            {
                var parser = new TCCLParser();
                bool notQuit = true;
                string fileName = "";

                while (notQuit)
                {
                    bool fileIsValid = false;
                    while (!fileIsValid)
                    {
                        Console.Write("Enter the file to parse (or 'quit'): ");
                        fileName = Console.ReadLine();
                        if (!string.IsNullOrEmpty(fileName))
                        {
                            fileName = fileName.Trim();

                            if (fileName.ToLower().Equals(QUIT))
                            {
                                fileIsValid = true;
                                notQuit = false;
                            }
                            else
                            {
                                if (fileName.Length < 4 ||
                                    !fileName.Substring(fileName.Length - 4).Equals(".txt"))
                                {
                                    fileName += ".txt";
                                }
                                if (File.Exists(fileName))
                                {
                                    fileIsValid = true;
                                }
                            }

                        }
                        if (!fileIsValid)
                        {
                            Console.WriteLine("Unable to process given file name " +
                                              "\"{0}\", please try again.", fileName);
                        }
                    }

                    if (notQuit)
                    {
                        Console.WriteLine("\nCompiling file " + fileName);
                        parser.Parse(fileName);
                        Console.WriteLine("\nAST complete");
                        ILasm(fileName.Substring(0, fileName.Length - ".txt".Length));
                    }

                    Console.WriteLine("\nProgram complete. Press any key to close.");
                }

            }
        }
    }
}