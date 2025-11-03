using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;

namespace CParser {
    public class ASTPrinterVisitor :BaseASTVisitor<int,ASTComposite>{
        private string m_astDOTFilename;
        StreamWriter m_writer;

        public ASTPrinterVisitor(string dotFilename) {
            m_astDOTFilename = dotFilename;
        }


        public override int VisitTranslationUnit(TranslationUnitAST node,ASTComposite parent) {

            // 1. Open DOT file for writing
            StreamWriter writer = new StreamWriter(m_astDOTFilename);

            // 2. Write DOT file header
            writer.WriteLine("digraph AST {");

            // 3. Visit children and print AST nodes and edges
            VisitChildren(node,node);

            // 4. Write DOT file footer
            writer.WriteLine("}");

            // 5. Close DOT file
            writer.Close();

            // 6. Call dot to generate PNG from DOT file
            // call dot to generate png
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "dot";
            startInfo.Arguments = $"-Tgif {m_astDOTFilename} -o {m_astDOTFilename}.gif";
            Process process = Process.Start(startInfo);
            process.WaitForExit();

            return 0;
        }

        public override int VisitDeclaration(DeclarationAST node,ASTComposite parent ) {

            // 1. Print graphviz edge from parent to this node
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");


            return 0;
        }
    }
}
