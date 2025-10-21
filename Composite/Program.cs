using Antlr4.Runtime;
using Antlr4.Runtime.Tree;


namespace Composite
{



    public class Program
    {
        static void Main(string[] args)
        {
            // C# class to read from a text file
            StreamReader streamReader = new StreamReader(args[0]);
            // Read the entire file and place it in a string
            string input = streamReader.ReadToEnd();
            // Create an ANTLR input stream from the string
            AntlrInputStream inputStream = new AntlrInputStream(input);
            // Create a lexer that feeds from that stream
            CGrammarLexer lexer = new CGrammarLexer(inputStream);
            // Create a token stream that feeds from the lexer
            CommonTokenStream tokenStream = new CommonTokenStream(lexer);
            CGrammarParser parser = new CGrammarParser(tokenStream);
            // Ask the parser to start parsing at rule 'compilationUnit'
            IParseTree  syntaxTree = parser.translation_unit();
            // Print the tree in LISP format
            Console.WriteLine(syntaxTree.ToStringTree());
        }


    }
}


