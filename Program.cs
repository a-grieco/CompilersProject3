using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASTBuilder;

namespace Project3
{
    class Program
    {
        private const string QUIT = "quit";

        static void Main(string[] args)
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
                    Console.WriteLine("Parsing file " + fileName);
                    parser.Parse(fileName);
                    Console.WriteLine("Parsing complete\n");
                }
            }

            Console.WriteLine("\nProgram complete. Press any key to close.");
            //Console.ReadKey();
        }
    }
}
