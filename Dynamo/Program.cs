using System;

namespace Dynamo
{
    class Program
    {
        static void Main(string[] args)
        {
            var compiler = new Compiler();

            Console.WriteLine("1. Run this app without the VS debugger attached (Ctrl+F5 in VS)");
            Console.WriteLine("2. Open the `/sample` subfolder in VS Code and hit F5 to attach to the process");
            Console.WriteLine("3. Choose `Dynamo.exe` from the list");
            Console.WriteLine("4. Put a breakpoint in VS Code into the `test.cs` file somewhere");

            Console.WriteLine("\r\nCompiling `/sample/test.cs`...\r\n");
            var asm = compiler.CreateAssembly(@"../../../../sample/test.cs");

            Console.WriteLine("Now hit `Enter` and watch as your breakpoint gets hit");

            Console.ReadLine();
            Console.WriteLine("Calling Test1().RunTest()");
            dynamic test1 = asm.CreateInstance("Test.Test1");
            test1.RunTest(12);

            Console.WriteLine("Done. Press enter to exit");
            Console.ReadLine();
        }
    }
}