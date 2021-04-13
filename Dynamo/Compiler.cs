using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;

namespace Dynamo
{
    internal struct CompilerResult
    {
        public byte[] PeStream { get; set; }

        public byte[] PdbStream { get; set; }
    }

    internal class Compiler
    {
        public Assembly CreateAssembly(string filePath)
        {
            string code = File.ReadAllText(filePath);
            var encoding = Encoding.UTF8;

            var assemblyName = Path.GetRandomFileName();
            var symbolsName = Path.ChangeExtension(assemblyName, "pdb");
            // var sourceCodePath = "generated.cs";

            var buffer = encoding.GetBytes(code);
            var sourceText = SourceText.From(buffer, buffer.Length, encoding, canBeEmbedded: true);

            var syntaxTree = CSharpSyntaxTree.ParseText(
                sourceText,
                new CSharpParseOptions(),
                path: filePath);

            var syntaxRootNode = syntaxTree.GetRoot() as CSharpSyntaxNode;
            var encoded = CSharpSyntaxTree.Create(syntaxRootNode, null, filePath, encoding);

            var optimizationLevel = OptimizationLevel.Debug;

            var references = new MetadataReference[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.Runtime.AssemblyTargetedPatchBandAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo).Assembly.Location),
            };

            CSharpCompilation compilation = CSharpCompilation.Create(
                assemblyName,
                syntaxTrees: new[] { encoded },
                references: references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                    .WithOptimizationLevel(optimizationLevel)
                    .WithPlatform(Platform.AnyCpu)
            );

            using (var assemblyStream = new MemoryStream())
            using (var symbolsStream = new MemoryStream())
            {
                var emitOptions = new EmitOptions(
                        debugInformationFormat: DebugInformationFormat.PortablePdb,
                        pdbFilePath: symbolsName);

                var embeddedTexts = new List<EmbeddedText>
                {
                    EmbeddedText.FromSource(filePath, sourceText),
                };

                EmitResult result = compilation.Emit(
                    peStream: assemblyStream,
                    pdbStream: symbolsStream,
                    embeddedTexts: embeddedTexts,
                    options: emitOptions);

                if (!result.Success)
                {
                    var errors = new List<string>();

                    IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                        diagnostic.IsWarningAsError ||
                        diagnostic.Severity == DiagnosticSeverity.Error);

                    foreach (Diagnostic diagnostic in failures)
                        errors.Add($"{diagnostic.Id}: {diagnostic.GetMessage()}");

                    throw new Exception(String.Join("\n", errors));
                }

                Console.WriteLine(code);

                assemblyStream.Seek(0, SeekOrigin.Begin);
                symbolsStream?.Seek(0, SeekOrigin.Begin);

                var assembly = Assembly.Load(assemblyStream.ToArray(), symbolsStream.ToArray());
                return assembly;
            }
        }

        public CompilerResult Compile(string filepath)
        {
            Console.WriteLine($"Starting compilation of: '{filepath}'");

            var sourceCode = File.ReadAllText(filepath);

            using (var pdbStream = new MemoryStream())
            using (var peStream = new MemoryStream())
            {
                var emitOptions = new EmitOptions(debugInformationFormat: DebugInformationFormat.PortablePdb);
                //var sourceText = SourceText.From(sourceCode);
                var buffer = Encoding.UTF8.GetBytes(sourceCode);
                var sourceText = SourceText.From(buffer, buffer.Length, Encoding.UTF8, canBeEmbedded: true);

                var embeddedTexts = new List<EmbeddedText>
                {
                    EmbeddedText.FromSource(filepath, sourceText),
                };

                var result = GenerateCode(sourceText).Emit(
                    peStream, 
                    pdbStream, 
                    options: emitOptions);

                if (!result.Success)
                {
                    Console.WriteLine("Compilation done with error.");

                    var failures = result.Diagnostics.Where(diagnostic => diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error);

                    foreach (var diagnostic in failures)
                    {
                        Console.Error.WriteLine("{0}: {1}", diagnostic.Id, diagnostic.GetMessage());
                    }

                    return new CompilerResult();
                }

                Console.WriteLine("Compilation done without any error.");

                peStream.Seek(0, SeekOrigin.Begin);

                return new CompilerResult() { PeStream = peStream.ToArray(), PdbStream = pdbStream.ToArray() };
            }
        }

        private static CSharpCompilation GenerateCode(SourceText codeString)
        {
            var options = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp7_3);

            var parsedSyntaxTree = SyntaxFactory.ParseSyntaxTree(codeString, options);

            var references = new MetadataReference[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.Runtime.AssemblyTargetedPatchBandAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo).Assembly.Location),
            };

            return CSharpCompilation.Create("Hello.dll",
                new[] { parsedSyntaxTree },
                references: references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
                    optimizationLevel: OptimizationLevel.Debug,
                    platform: Platform.AnyCpu,
                    assemblyIdentityComparer: DesktopAssemblyIdentityComparer.Default));
        }
    }
}