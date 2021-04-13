using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CSharp;

namespace DynamoNet452
{
    class Program
    {
        static void Main(string[] args)
        {
            var csc = new CSharpCodeProvider(new Dictionary<string, string>() { { "CompilerVersion", "v3.5" } });
            var parameters = new CompilerParameters(new[] { "mscorlib.dll", "System.Core.dll" }, "foo.exe", true);
            parameters.GenerateInMemory = true;
            parameters.CompilerOptions = "-debug:portable";

            CompilerResults results = csc.CompileAssemblyFromFile(parameters, @"C:\Code\dynamo\test.cs");
            results.Errors.Cast<CompilerError>().ToList().ForEach(error => Console.WriteLine(error.ErrorText));

            Assembly asm = results.CompiledAssembly;
            dynamic program = asm.CreateInstance("Test.Test1");

            Console.ReadLine();

            program.RunTest(56);
        }
    }
}
