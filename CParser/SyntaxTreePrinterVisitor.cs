using Antlr4.Runtime.Tree;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;

namespace CParser {
    public class SyntaxTreePrinterVisitor : CGrammarParserBaseVisitor<int>{
        private string m_dotFileName;
        StreamWriter m_streamWriter;
        private Stack<string> m_parentsNames = new Stack<string>();
        private static int ms_nodeCounter;
        

        public SyntaxTreePrinterVisitor(string dotFileName) {
            m_dotFileName = dotFileName;
            m_parentsNames.Clear();
        }


        public override int VisitTranslation_unit(CGrammarParser.Translation_unitContext context) {
            ms_nodeCounter = 0;
            
            // 1. Open a DOT graph file
            m_streamWriter = new StreamWriter(m_dotFileName);
            m_streamWriter.WriteLine("digraph G {");

            m_parentsNames.Push("TranslationUnit_"+ms_nodeCounter++);
            
            // 2. Visit the children of the translation_unit node
            VisitChildren(context);

            m_parentsNames.Pop();
            m_streamWriter.WriteLine("}");

            // 3. Close the DOT graph file
            m_streamWriter.Close();

            // 4. Call Graphviz to generate a PNG from the DOT file
            // Prepare the process dot to run
            ProcessStartInfo start = new ProcessStartInfo();
            string m_dotFileNamePath = "\"" + m_dotFileName + ".dot" + "\"";
            // Enter in the command line arguments, everything you would enter after the executable name itself
            start.Arguments = "-Tgif " +
                              Path.GetFileName(m_dotFileNamePath) + " -o " +
                              Path.GetFileNameWithoutExtension("test") + ".gif";
            // Enter the executable to run, including the complete path
            start.FileName = "dot";
            // Do you want to show a console window?
            start.WindowStyle = ProcessWindowStyle.Hidden;
            start.CreateNoWindow = true;
            int exitCode;

            // Run the external process & wait for it to finish
            using (Process proc = Process.Start(start)) {
                proc.WaitForExit();

                // Retrieve the app's exit code
                exitCode = proc.ExitCode;
            }


            return 0;
        }

        public override int Visit(IParseTree tree) {

            RuleContext ruleContext = tree as RuleContext;

            if (ruleContext.RuleIndex == CGrammarParser.RULE_translation_unit) {
                return VisitTranslation_unit(ruleContext as CGrammarParser.Translation_unitContext);
            }
            else {
                // 1. Print an edge from parent to this node
                string nodeName = CGrammarParser.ruleNames[ruleContext.RuleIndex] + "_" + ms_nodeCounter++;
                m_streamWriter.WriteLine($"\"{m_parentsNames.Peek()}\"->\"{nodeName}\"");
                m_parentsNames.Push(nodeName);
                // 2. Visit children
                base.Visit(tree);
                m_parentsNames.Pop();

                return 0;
            }
        }
    }
}
