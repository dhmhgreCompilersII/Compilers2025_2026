using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using static calculatorParser;

namespace Composite
{

    public abstract class ASTElement{
        private uint m_type; // type of AST element ( addition, multiplication, etc)
        private string m_name; // for use in debugging
        ASTElement m_parent; // parent of this AST element
        private uint m_serialNumber; // unique serial number of this AST element to distinguish it
                                     // from other AST elements of the same type
        private static uint m_serialNumberCounter = 0; // static counter to generate unique serial numbers

        public ASTElement(uint type, string name){
            m_type = type;
            m_name = name;
            m_serialNumber = m_serialNumberCounter++;
        }
    }

    public abstract class ASTComposite : ASTElement{
        List<ASTElement>[] m_children; // children of this AST element
        private uint m_contexts;
        public ASTComposite(uint numcontexts,uint type, string name) :
            base(type, name){
            m_children = new List<ASTElement>[numcontexts]; // assume max 10 types of children
            for(int i = 0; i < numcontexts; i++){
                m_children[i] = new List<ASTElement>();
            }
            m_contexts = numcontexts;
        }
    }

    public abstract class ASTLeaf : ASTElement{
        private string m_lexeme;
        public ASTLeaf(string lexeme,uint type, string name) :
            base(type, name){
            m_lexeme = lexeme;
        }
    }



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
            calculatorLexer lexer = new calculatorLexer(inputStream);
            // Create a token stream that feeds from the lexer
            CommonTokenStream tokenStream = new CommonTokenStream(lexer);
            // Create a parser that feeds from the token stream
            calculatorParser parser = new calculatorParser(tokenStream);
            // Ask the parser to start parsing at rule 'compilationUnit'
            IParseTree  syntaxTree = parser.compilationUnit();
            // Print the tree in LISP format
            Console.WriteLine(syntaxTree.ToStringTree());
        }


    }
}


