using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.MSBuild;

await CompileAssembly("""
    using Microsoft.Maui.Controls;

    public static class C
    {
        public static Element? Test() => null;
    }
    """);

static async Task<bool> CompileAssembly(string code)
{
    MSBuildLocator.RegisterDefaults();

    using var workspace = MSBuildWorkspace.Create(new Dictionary<string, string>
    {
        ["Configuration"] = "Debug",
        ["Platform"] = "AnyCPU",
        ["TargetFramework"] = "net9.0-windows10.0.19041.0"
    });

    var project = await workspace.OpenProjectAsync("IosPlayerMaui.CompiledPages.csproj");

    var syntaxTree = CSharpSyntaxTree.ParseText(code);
    project = project.AddDocument("IosPlayerMaui.CompiledPages.cs", await syntaxTree.GetTextAsync().ConfigureAwait(false)).Project;

    if (await project.GetCompilationAsync().ConfigureAwait(false) is not { } compilation)
        return false;

    if (compilation.GetDiagnostics() is [_, ..] diagnostics)
    {
        foreach (var diagnostic in diagnostics)
            Console.Error.WriteLine(diagnostic);
        return false;
    }

    var emitResult = compilation.Emit("IosPlayerMaui.CompiledPages.dll");
    return true;
}
