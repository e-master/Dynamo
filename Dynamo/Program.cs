using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dynamo
{
    class Program
    {
        static void Main(string[] args)
        {
            var compiler = new Compiler();

            var asm = compiler.CreateAssembly(@"C:\Code\dynamo\test.cs");
            dynamic test1 = asm.CreateInstance("Test.Test1");

            Console.ReadLine();
            test1.RunTest(12);

            //CompilerResult dll = compiler.Compile(@"C:\Code\dynamo\test.cs");

            //Assembly asm = Assembly.Load(dll.PeStream, dll.PdbStream);
            //dynamic test1 = asm.CreateInstance("Test.Test1");

            //File.WriteAllBytes(@"C:\temp\test.pdb", dll.PdbStream);
            //test1.RunTest();

            // var runner = new Runner();
        }
    }
}
