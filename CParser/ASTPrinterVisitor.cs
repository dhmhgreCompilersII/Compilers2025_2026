using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;

namespace CParser {
    public class ASTPrinterVisitor : BaseASTVisitor<int, ASTComposite>{
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
                m_writer.Write($"\"{child.MName}\";");
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


        public override int VisitParameterDeclaration(ParameterDeclarationAST node, ASTComposite parent) {
            // 1.Create context clusters
            CreateContext(node, ParameterDeclarationAST.DECLARATOR, "Declarators");
            CreateContext(node, ParameterDeclarationAST.TYPE_SPECIFIER, "Type Specifier");
            CreateContext(node, ParameterDeclarationAST.TYPE_QUALIFIER, "Type Qualifier");
            CreateContext(node, ParameterDeclarationAST.STORAGE_SPECIFIER, "Storage Specifier");

            // . Print graphviz edge from parent to this node
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");

            return base.VisitParameterDeclaration(node, node);
        }

        public override int VisitDeclaration(DeclarationAST node, ASTComposite parent) {


            // 1.Create context clusters
            CreateContext(node, DeclarationAST.DECLARATORS, "Declarators");
            CreateContext(node, DeclarationAST.TYPE_SPECIFIER, "Type Specifier");
            CreateContext(node, DeclarationAST.TYPE_QUALIFIER, "Type Qualifier");
            CreateContext(node, DeclarationAST.STORAGE_SPECIFIER, "Storage Specifier");

            // . Print graphviz edge from parent to this node
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");

            // 3. Visit children and print AST nodes and edges
            VisitChildren(node, node);


            return 0;
        }

        public override int VisitFunctionDefinition(FunctionDefinitionAST node, ASTComposite parent) {

            // 1.Create context clusters
            CreateContext(node, FunctionDefinitionAST.DECLARATION_SPECIFIERS, "Declaration Specifier");
            CreateContext(node, FunctionDefinitionAST.FUNCTION_BODY, "Body");
            CreateContext(node, FunctionDefinitionAST.DECLARATOR, "Name");
            CreateContext(node, FunctionDefinitionAST.PARAMETER_DECLARATIONS, "Parameter Declarations");

            // 1. Print graphviz edge from parent to this node
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;
        }

        public override int VisitStatementExpression(Statement_Expression node, ASTComposite parent) {
            // 1.Create context clusters
            CreateContext(node, Statement_Expression.EXPRESSION, "EXPRESSION_BODY");
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;
        }

        public override int VisitExpressionAssignment(Expression_Assignment node, ASTComposite parent) {
            // 1.Create context clusters
            CreateContext(node, Expression_Assignment.LEFT, "L_VALUE");
            CreateContext(node, Expression_Assignment.RIGHT, "R_VALUE");
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;
        }

        public override int VisitExpressionAddition(Expression_Addition node, ASTComposite parent) {
            // 1.Create context clusters
            CreateContext(node, Expression_Addition.LEFT, "LEFT");
            CreateContext(node, Expression_Addition.RIGHT, "RIGHT");
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;
        }


        public override int VisitCompoundStatement(CompoundStatement node, ASTComposite parent) {

            // 1.Create context clusters
            CreateContext(node, CompoundStatement.DECLARATIONS, "Declarations");
            CreateContext(node, CompoundStatement.STATEMENTS, "Statements");

            // 1. Print graphviz edge from parent to this node
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;
        }


        public override int VisitPointerType(PointerTypeAST node, ASTComposite parent) {

            // 1.Create context clusters
            CreateContext(node, PointerTypeAST.POINTER_TARGET, "Target");


            // 2. Print graphviz edge from parent to this node
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;
        }

        public override int VisitFunctionType(FunctionTypeAST node, ASTComposite parent) {

            // 1.Create context clusters
            CreateContext(node, FunctionTypeAST.FUNCTION_NAME, "Function Name");
            CreateContext(node, FunctionTypeAST.FUNCTION_TYPE, "Declarator");
            CreateContext(node, FunctionTypeAST.FUNCTION_PARAMETERS, "Function Parameters");

            // 2. Print graphviz edge from parent to this node
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;
        }

        public override int VisitIdentifier(IDENTIFIER node, ASTComposite parent) {
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            return base.VisitIdentifier(node, parent);
        }

        public override int VisitInteger(INTEGER node, ASTComposite parent) {
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            return base.VisitInteger(node, parent);
        }

        public override int VisitIntegerType(IntegerTypeAST node, ASTComposite parent) {
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            return base.VisitIntegerType(node, parent);
        }
        public override int VisitCharType(CharTypeAST node, ASTComposite parent) {
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            return base.VisitCharType(node, parent);
        }
    }
}
