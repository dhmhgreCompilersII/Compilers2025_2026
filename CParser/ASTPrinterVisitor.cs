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

        public override int VisitExpressionSubtraction(Expression_Subtraction node, ASTComposite parent) {
            // 1.Create context clusters
            CreateContext(node, Expression_Subtraction.LEFT, "LEFT");
            CreateContext(node, Expression_Subtraction.RIGHT, "RIGHT");
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;
        }

        public override int VisitExpressionMultiplication(Expression_Multiplication node, ASTComposite parent) {
            // 1.Create context clusters
            CreateContext(node, Expression_Multiplication.LEFT, "LEFT");
            CreateContext(node, Expression_Multiplication.RIGHT, "RIGHT");
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;
        }

        public override int VisitLogicalAND(ExpressionLogicalAnd node, ASTComposite parent) {
            // 1.Create context clusters
            CreateContext(node, ExpressionLogicalAnd.LEFT, "LEFT");
            CreateContext(node, ExpressionLogicalAnd.RIGHT, "RIGHT");
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;
        }

        public override int VisitLogicalOR(ExpressionLogicalOr node, ASTComposite parent) {
            // 1.Create context clusters
            CreateContext(node, ExpressionLogicalOr.LEFT, "LEFT");
            CreateContext(node, ExpressionLogicalOr.RIGHT, "RIGHT");
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;
        }

        public override int VisitExpressionDivision(Expression_Division node, ASTComposite parent) {
            // 1.Create context clusters
            CreateContext(node, Expression_Division.LEFT, "LEFT");
            CreateContext(node, Expression_Division.RIGHT, "RIGHT");
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;
        }

        public override int VisitExpressionModulo(Expression_Modulo node, ASTComposite parent) {
            // 1.Create context clusters
            CreateContext(node, Expression_Modulo.LEFT, "LEFT");
            CreateContext(node, Expression_Modulo.RIGHT, "RIGHT");
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;
        }

        public override int VisitShiftExpression_Left(ExpressionShiftLeft node, ASTComposite parent) {
            // 1.Create context clusters
            CreateContext(node, ExpressionShiftLeft.LEFT, "LEFT");
            CreateContext(node, ExpressionShiftLeft.RIGHT, "RIGHT");
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;

        }

        public override int VisitShiftExpression_Right(ExpressionShiftRight node, ASTComposite parent) {
            // 1.Create context clusters
            CreateContext(node, ExpressionShiftRight.LEFT, "LEFT");
            CreateContext(node, ExpressionShiftRight.RIGHT, "RIGHT");
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;
        }

        public override int VisitExpressionAssignmentAdd(ExpressionAssignmentAddition node, ASTComposite parent) {
            // 1.Create context clusters
            CreateContext(node, ExpressionAssignmentAddition.LEFT, "LEFT");
            CreateContext(node, ExpressionAssignmentAddition.RIGHT, "RIGHT");
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;
        }

        public override int VisitExpressionAssignmentSub(ExpressionAssignmentSubtraction node, ASTComposite parent) {
            // 1.Create context clusters
            CreateContext(node, ExpressionAssignmentSubtraction.LEFT, "LEFT");
            CreateContext(node, ExpressionAssignmentSubtraction.RIGHT, "RIGHT");
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;
        }

        public override int VisitExpressionAssignmentMul(ExpressionAssignmentMultiplication node, ASTComposite parent) {
            // 1.Create context clusters
            CreateContext(node, ExpressionAssignmentMultiplication.LEFT, "LEFT");
            CreateContext(node, ExpressionAssignmentMultiplication.RIGHT, "RIGHT");
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;
        }

        public override int VisitExpressionAssignmentDiv(ExpressionAssignmentDivision node, ASTComposite parent) {
            // 1.Create context clusters
            CreateContext(node, ExpressionAssignmentDivision.LEFT, "LEFT");
            CreateContext(node, ExpressionAssignmentDivision.RIGHT, "RIGHT");
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;
        }

        public override int VisitExpressionAssignmentMod(ExpressionAssignmentModulo node, ASTComposite parent) {
            // 1.Create context clusters
            CreateContext(node, ExpressionAssignmentModulo.LEFT, "LEFT");
            CreateContext(node, ExpressionAssignmentModulo.RIGHT, "RIGHT");
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;
        }

        public override int VisitExpressionAssignmentLeft(Expression_AssignmentLeft node, ASTComposite parent) {
            // 1.Create context clusters
            CreateContext(node, Expression_AssignmentLeft.LEFT, "LEFT");
            CreateContext(node, Expression_AssignmentLeft.RIGHT, "RIGHT");
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;
        }

        public override int VisitExpressionAssignmentRight(Expression_AssignmentRight node, ASTComposite parent) {
            // 1.Create context clusters
            CreateContext(node, Expression_AssignmentRight.LEFT, "LEFT");
            CreateContext(node, Expression_AssignmentRight.RIGHT, "RIGHT");
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;
        }

        public override int VisitExpressionAssignmentAnd(Expression_AssignmentAnd node, ASTComposite parent) {
            // 1.Create context clusters
            CreateContext(node, Expression_AssignmentAnd.LEFT, "LEFT");
            CreateContext(node, Expression_AssignmentAnd.RIGHT, "RIGHT");
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;
        }

        public override int VisitExpressionAssignmentOr(Expression_AssignmentOr node, ASTComposite parent) {
            // 1.Create context clusters
            CreateContext(node, Expression_AssignmentOr.LEFT, "LEFT");
            CreateContext(node, Expression_AssignmentOr.RIGHT, "RIGHT");
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;
        }

        public override int VisitExpressionAssignmentXor(Expression_AssignmentXor node, ASTComposite parent) {
            // 1.Create context clusters
            CreateContext(node, Expression_AssignmentXor.LEFT, "LEFT");
            CreateContext(node, Expression_AssignmentXor.RIGHT, "RIGHT");
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;
        }



        public override int VisitUnaryExpressionIncrement(UnaryExpressionIncrement node, ASTComposite parent) {
            // 1.Create context clusters
            CreateContext(node, UnaryExpressionIncrement.OPERAND, "UnaryIncrement");
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;

        }

        public override int VisitUnaryExpressionDecrement(UnaryExpressionDecrement node, ASTComposite parent) {
            // 1.Create context clusters
            CreateContext(node, UnaryExpressionDecrement.OPERAND, "UnaryDecrement");
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;

        }

        public override int VisitUnaryExpressionOperatorAmbersand(UnaryExpressionUnaryOperatorAmbersand node, ASTComposite parent) {
            // 1.Create context clusters
            CreateContext(node, UnaryExpressionUnaryOperatorAmbersand.OPERAND, "AddressOf");
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;
        }

        public override int VisitUnaryExpressionOperatorAsterisk(UnaryExpressionUnaryOperatorAsterisk node, ASTComposite parent) {
            // 1.Create context clusters
            CreateContext(node, UnaryExpressionUnaryOperatorAsterisk.OPERAND, "PointerDereference");
            m_writer.WriteLine( $"    \"{parent.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;
        }

        public override int VisitUnaryExpressionOperatorPLUS(UnaryExpressionUnaryOperatorPLUS node, ASTComposite parent) {
            // 1.Create context clusters
            CreateContext(node, UnaryExpressionUnaryOperatorPLUS.OPERAND, "UnaryPlus");
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;
        }

        public override int VisitUnaryExpressionOperatorMINUS(UnaryExpressionUnaryOperatorMINUS node, ASTComposite parent) {
            // 1.Create context clusters
            CreateContext(node, UnaryExpressionUnaryOperatorMINUS.OPERAND, "UnaryMinus");
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;
        }

        public override int VisitUnaryExpressionOperatorTilde(UnaryExpressionUnaryOperatorTilde node, ASTComposite parent) {
            // 1.Create context clusters
            CreateContext(node, UnaryExpressionUnaryOperatorTilde.OPERAND, "BitwiseNOT");
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;
        }

        public override int VisitUnaryExpressionOperatorNOT(UnaryExpressionUnaryOperatorNOT node, ASTComposite parent) {
            // 1.Create context clusters
            CreateContext(node, UnaryExpressionUnaryOperatorNOT.OPERAND, "LogicalNOT");
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;
        }

        public override int VisitUnarySIZEOFExpression(UnaryExpressionSizeOfExpression node, ASTComposite parent) {
            CreateContext(node, UnaryExpressionSizeOfExpression.EXPRESSION, "SizeOfExpression");
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;
        }

        public override int VisitUnarySIZEOFTypeName(UnaryExpressionSizeOfTypeName node, ASTComposite parent) {
            CreateContext(node, UnaryExpressionSizeOfTypeName.TYPE, "SizeOfTypeName");
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;
        }

        public override int Visitpostfix_expression_Increment(Postfixexpression_Increment node, ASTComposite parent) { 
            // 1.Create context clusters
            CreateContext(node, Postfixexpression_Increment.ACCESS, "PostfixIncrement");
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;
        }

        public override int Visitpostfix_expression_Decrement(Postfixexpression_Decrement node, ASTComposite parent) {
            // 1.Create context clusters
            CreateContext(node, Postfixexpression_Decrement.ACCESS, "PostfixDecrement");
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;
        }

        public override int VisitPostfixExpression_ArraySubscript(Postfixexpression_ArraySubscript node, ASTComposite parent) {
            // 1.Create context clusters
            CreateContext(node, Postfixexpression_ArraySubscript.ARRAY, "Array");
            CreateContext(node, Postfixexpression_ArraySubscript.INDEX, "Index");
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;
        }

        public override int Visitpostfix_expression_FunctionCallNoArgs(Postfixexpression_FunctionCallNoArgs node, ASTComposite parent) {
            // 1.Create context clusters
            CreateContext(node, Postfixexpression_FunctionCallNoArgs.FUNCTION, "Function");
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;
        }

        public override int Visitpostfix_expression_FunctionCallWithArgs(Postfixexpression_FunctionCallWithArgs node, ASTComposite parent) {
            // 1.Create context clusters
            CreateContext(node, Postfixexpression_FunctionCallNoArgs.FUNCTION, "Function");
            CreateContext(node, Postfixexpression_FunctionCallWithArgs.ARGUMENTS, "Arguments");
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;
        }

        public override int Visitpostfix_expression_MemberAccess(Postfixexpression_MemberAccess node, ASTComposite parent) {
            // 1.Create context clusters
            CreateContext(node, Postfixexpression_MemberAccess.ACCESS, "MemberAccess");
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;
        }

        public override int Visitpostfix_expression_PointerMemberAccess(Postfixexpression_PointerMemberAccess node, ASTComposite parent) {
            // 1.Create context clusters
            CreateContext(node, Postfixexpression_PointerMemberAccess.ACCESS, "PointerMemberAccess");
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;
        }

        public override int VisitRelationalLess(ExpressionRelationalLess node, ASTComposite parent) {
            // 1.Create context clusters
            CreateContext(node, ExpressionRelationalLess.LEFT, "LEFT");
            CreateContext(node, ExpressionRelationalLess.RIGHT, "RIGHT");
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;
        }

        public override int VisitRelationalGreater(ExpressionRelationalGreater node, ASTComposite parent) {
            // 1.Create context clusters
            CreateContext(node, ExpressionRelationalGreater.LEFT, "LEFT");
            CreateContext(node, ExpressionRelationalGreater.RIGHT, "RIGHT");
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;
        }

        public override int VisitRelationalLessEqual(ExpressionRelationalLessOrEqual node, ASTComposite parent) {
            // 1.Create context clusters
            CreateContext(node, ExpressionRelationalLessOrEqual.LEFT, "LEFT");
            CreateContext(node, ExpressionRelationalLessOrEqual.RIGHT, "RIGHT");
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;
        }

        public override int VisitRelationalGreaterEqual(ExpressionRelationalGreaterOrEqual node, ASTComposite parent) {
            // 1.Create context clusters
            CreateContext(node, ExpressionRelationalGreaterOrEqual.LEFT, "LEFT");
            CreateContext(node, ExpressionRelationalGreaterOrEqual.RIGHT, "RIGHT");
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;
        }

        public override int VisitExpressionEqualityEqual(Expression_EqualityEqual node, ASTComposite parent) {
            // 1.Create context clusters
            CreateContext(node, Expression_EqualityEqual.LEFT, "LEFT");
            CreateContext(node, Expression_EqualityEqual.RIGHT, "RIGHT");
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;
        }

        public override int VisitExpressionEqualityNotEqual(Expression_EqualityNotEqual node, ASTComposite parent) {
            // 1.Create context clusters
            CreateContext(node, Expression_EqualityNotEqual.LEFT, "LEFT");
            CreateContext(node, Expression_EqualityNotEqual.RIGHT, "RIGHT");
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;
        }

        public override int VisitExpressionBitwiseAND(Expression_BitwiseAND node, ASTComposite parent) {
            // 1.Create context clusters
            CreateContext(node, Expression_BitwiseAND.LEFT, "LEFT");
            CreateContext(node, Expression_BitwiseAND.RIGHT, "RIGHT");
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;
        }

        public override int VisitExpressionBitwiseOR(Expression_BitwiseOR node, ASTComposite parent) {
            // 1.Create context clusters
            CreateContext(node, Expression_BitwiseOR.LEFT, "LEFT");
            CreateContext(node, Expression_BitwiseOR.RIGHT, "RIGHT");
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;
        }

        public override int VisitExpressionBitwiseXOR(Expression_BitwiseXOR node, ASTComposite parent) {
            // 1.Create context clusters
            CreateContext(node, Expression_BitwiseXOR.LEFT, "LEFT");
            CreateContext(node, Expression_BitwiseXOR.RIGHT, "RIGHT");
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;
        }

        public override int VisitExpressionCommaExpression(Expression_CommaExpression node, ASTComposite parent) {
            // 1.Create context clusters
            CreateContext(node, Expression_CommaExpression.LEFT, "LEFT");
            CreateContext(node, Expression_CommaExpression.RIGHT, "RIGHT");
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;
        }

        public override int VisitCastExpressionCast(Expression_Cast node, ASTComposite parent) {
            // 1.Create context clusters
            CreateContext(node, Expression_Cast.TYPE, "TYPE");
            CreateContext(node, Expression_Cast.EXPRESSION, "EXPRESSION");
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;
        }

        public override int VisitArrayDimensionWithSIZE(ArrayDimensionWithSIZE node, ASTComposite parent) {
            // 1.Create context clusters
            CreateContext(node, ArrayDimensionWithSIZE.ARRAY, "Array");
            CreateContext(node, ArrayDimensionWithSIZE.SIZE, "Size");
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;
        }

        public override int VisitArrayDimensionWithNOSIZE(ArrayDimensionWithNOSIZE node, ASTComposite parent) {
            // 1.Create context clusters
            CreateContext(node, ArrayDimensionWithNOSIZE.ARRAY, "Array");
            m_writer.WriteLine($"    \"{parent.MName}\" -> \"{node.MName}\";");
            VisitChildren(node, node);
            return 0;
        }

        public override int VisitConditionalExpression(ConditionalExpression node, ASTComposite parent) {

            // 1.Create context clusters
            CreateContext(node, ConditionalExpression.CONDITION, "Condition");
            CreateContext(node, ConditionalExpression.TRUE_EXPRESSION, "If Expression");
            CreateContext(node, ConditionalExpression.FALSE_EXPRESSION, "Else Expression");
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
