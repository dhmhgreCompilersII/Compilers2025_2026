using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;

namespace CParser {
    public class ASTPrinterVisitor : BaseASTVisitor<int, ASTComposite> {
        private string m_astDOTFilename;
        StreamWriter m_writer;
        private static uint ms_clusternumber = 0;

        public ASTPrinterVisitor(string dotFilename) {
            m_astDOTFilename = dotFilename;
        }

        public void CreateContext(ASTComposite node, uint context, string ContextName) {
            m_writer.WriteLine($"subgraph cluster{ms_clusternumber++} {{");
            m_writer.WriteLine($"\t node [style=filled, color=white]; ");
            m_writer.WriteLine($"\t style=filled; color=lightgrey;");
            foreach (var child in node.MChildren[context]) {
                m_writer.Write($"{child.MName};");
            }

            m_writer.WriteLine();
            m_writer.WriteLine($"\t label = \"{ContextName}\";");
            m_writer.WriteLine("}");
        }


        public override int VisitTranslationUnit(TranslationUnitAST node, ASTComposite parent) {

            // 1. Open DOT file for writing
            m_writer = new StreamWriter(m_astDOTFilename);

            // 2. Write DOT file header
            m_writer.WriteLine("digraph AST {");

            // 2.a Add context clusters
            CreateContext(node, TranslationUnitAST.DECLARATIONS, "Declarations");
            CreateContext(node, TranslationUnitAST.FUNCTION_DEFINITION, "Function Definitions");


            // 3. Visit children and print AST nodes and edges
            VisitChildren(node, node);

            // 4. Write DOT file footer
            m_writer.WriteLine("}");

            // 5. Close DOT file
            m_writer.Close();

            // 6. Call dot to generate PNG from DOT file
            // call dot to generate png
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "dot";
            startInfo.Arguments = $"-Tgif {m_astDOTFilename} -o {m_astDOTFilename}.gif";
            Process process = Process.Start(startInfo);
            process.WaitForExit();

            return 0;
        }



        public override int VisitDeclaration(DeclarationAST node, ASTComposite parent) {

            // 1. Print graphviz edge from parent to this node
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");


            return 0;
        }

        public override int VisitFunctionDefinition(FunctionDefinitionAST node, ASTComposite parent) {
            // 1. Print graphviz edge from parent to this node
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            return 0;
        }
    }
}
