using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Antlr4.Runtime.Atn;


namespace CParser
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


            /*tokenStream.Fill();
            Console.WriteLine("TOKENS:");
            foreach (var t in tokenStream.GetTokens()) {
                var name = lexer.Vocabulary.GetSymbolicName(t.Type)
                           ?? lexer.Vocabulary.GetDisplayName(t.Type);
                Console.WriteLine($"{name,-16} '{t.Text}'");
            }*/
            CGrammarParser parser = new CGrammarParser(tokenStream);

            
            // Ask the parser to start parsing at rule 'compilationUnit'
            parser.Profile = true;
            IParseTree  syntaxTree = parser.translation_unit();
            // Print the tree in LISP format
            //Console.WriteLine(syntaxTree.ToStringTree());

            SyntaxTreePrinterVisitor syntaxTreePrinterVisitor = 
                new SyntaxTreePrinterVisitor("test");
            syntaxTreePrinterVisitor.Visit(syntaxTree);



        }


    }
}


