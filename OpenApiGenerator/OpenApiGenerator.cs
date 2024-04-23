using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;


// ============================================================================
// ============================================================================
// ============================================================================
namespace Com.AiricLenz.OpenApi
{
    
 
    // ============================================================================
    // ============================================================================
    // ============================================================================
    internal class OpenApiGenerator
    {

        private static Helper helper = new Helper();


        // ============================================================================
        static void Main(string[] args)
        {

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("OpenAPI Specification Generator");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Gray;

            string errorLog;

            if (!helper.GenerateOpenApiDocument(out errorLog))
            {
                Console.WriteLine();
                Console.WriteLine("There was an error generating the output file!");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(errorLog);
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            else
            {
                Console.WriteLine();
                Console.Write("You can upload the file to ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(@"https://editor.swagger.io");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine(" to view it.");
            }
                        
            Console.WriteLine();
            Console.WriteLine("Press any key to continue...");

            while (!Console.KeyAvailable) { }            
            
        }
                    
    }
}
