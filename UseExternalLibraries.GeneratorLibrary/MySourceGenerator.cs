using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics;
using System.Linq;

//can't use global namespaces.   otherwise, the getclasssymbol does not work properly.
namespace UseExternalLibraries.GeneratorLibrary;
[Generator] //this is important so it knows this class is a generator which will generate code for a class using it.
public class MySourceGenerator : ISourceGenerator
{
    //this is important to get information about the class including namespacename and even figuring out methods.


    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxReceiver is not SyntaxReceiver receiver)
        {
            return;
        }
        Compilation compilation = context.Compilation;
        foreach (var ourClass in receiver.CandidateClasses)
        {
            var ss = compilation.GetClassSymbol(ourClass);
            //the code below could be cast to something (iffy).
            bool rets = ss.GetMembers().Any(xx => xx.Name == "HelloFrom");
            //the sample uses HelloFrom as a starting point.  you can change to what you want.
            if (rets == false)
            {
                //shows how you can continue the loop so if a condition is not met, then won't do the code below but can loop to the next item.
                continue;
            }
            int value = 1;
            string results = Newtonsoft.Json.JsonConvert.SerializeObject(value);
            if (results == "")
            {
                continue;
            }
            string className = ourClass.Identifier.ValueText;
            string nameSpaceName = ss.ContainingNamespace.ToDisplayString();
            string source = $@"
namespace {nameSpaceName};
public partial class {className}
{{
    public partial void HelloFrom(string name)
    {{
        Console.WriteLine($""Generator 3 says: Hi from '{{name}}'"");
    }}
}}
";
            context.AddSource($"generatedSource{className}.g", source);
        }
    }

    public void Initialize(GeneratorInitializationContext context)
    {
        //does need to comment to start with.  otherwise, when opening new project, will have those problems.
        //therefore, comment out if you need to step through it.
        //the code below is for debugging support.  make sure to comment out before closing out.  otherwise, has many problems.

        context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
    }
}