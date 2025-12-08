using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime.Tree;

namespace CParser {
    public class ANLTRST2ASTGenerationVisitor : CGrammarParserBaseVisitor<int>
    {

        ASTComposite m_root;
        private Stack<ASTComposite> m_parents = new Stack<ASTComposite>();
        PointerTypeAST m_currentPointerBottom = null;

        public ASTComposite Root
        {
            get => m_root;
        }

        public ANLTRST2ASTGenerationVisitor()
        {
            m_root = null;
        }

        public override int VisitTranslation_unit(CGrammarParser.Translation_unitContext context)
        {

            // 1. Create TranslationUnitAST node
            TranslationUnitAST tuNode = new TranslationUnitAST();
            m_root = tuNode;

            //2. Visit children and populate the AST node
            m_parents.Push(tuNode);
            VisitChildren(context);
            m_parents.Pop();

            return 0;
        }

        public override int VisitDeclaration(CGrammarParser.DeclarationContext context)
        {

            // 1. Get current parent node
            ASTComposite parent = m_parents.Peek();

            // 2. Create DeclarationAST node
            DeclarationAST declNode = new DeclarationAST();

            // 3. Add DeclarationAST node to parent
            parent.AddChild(declNode, parent.GetContextForChild(context)); // assuming context DECLARATORS for simplicity

            m_parents.Push(declNode);
            base.VisitDeclaration(context);
            m_parents.Pop();

            return 0;
        }




        public override int VisitPointer(CGrammarParser.PointerContext context)
        {

            ASTComposite parent = m_parents.Peek();

            PointerTypeAST pointerNode = new PointerTypeAST();
            parent.AddChild(pointerNode, parent.GetContextForChild(context)); // assuming context POINTER_TARGER for simplicity
            m_parents.Push(pointerNode);

            if (context.pointer() == null)
            {
                m_currentPointerBottom = pointerNode;
            }

            base.VisitPointer(context);
            m_parents.Pop();

            return 0;
        }

        public override int VisitFunctionWithNOArguments(CGrammarParser.FunctionWithNOArgumentsContext context)
        {

            ASTComposite parent = m_parents.Peek();


            switch (parent.MType)
            {
                case (uint)TranslationUnitAST.NodeTypes.FUNCTION_DEFINITION:
                    base.VisitFunctionWithNOArguments(context);
                    break;
                default:
                    FunctionTypeAST funcTypeNode = new FunctionTypeAST();

                    parent.AddChild(funcTypeNode, parent.GetContextForChild(context)); // assuming context FUNCTION_TYPE for simplicity

                    m_parents.Push(funcTypeNode);
                    base.VisitFunctionWithNOArguments(context);
                    m_parents.Pop();
                    break;
            }

            return 0;
        }

        public override int VisitFunctionWithArguments(CGrammarParser.FunctionWithArgumentsContext context)
        {
            ASTComposite parent = m_parents.Peek();

            switch (parent.MType)
            {
                case (uint)TranslationUnitAST.NodeTypes.FUNCTION_DEFINITION:
                    base.VisitFunctionWithArguments(context);
                    break;
                default:
                    FunctionTypeAST funcTypeNode = new FunctionTypeAST();

                    parent.AddChild(funcTypeNode, parent.GetContextForChild(context)); // assuming context FUNCTION_TYPE for simplicity

                    m_parents.Push(funcTypeNode);
                    base.VisitFunctionWithArguments(context);
                    m_parents.Pop();
                    break;

            }

            return 0;
        }

        public override int VisitParameter_declaration(CGrammarParser.Parameter_declarationContext context)
        {
            ASTComposite parent = m_parents.Peek();
            ParameterDeclarationAST pardecl = new ParameterDeclarationAST();
            parent.AddChild(pardecl, parent.GetContextForChild(context)); // assuming context PARAMETER_DECLARATION for simplicity
            m_parents.Push(pardecl);
            base.VisitParameter_declaration(context);
            m_parents.Pop();
            return 0;
        }


        public override int VisitDeclarator(CGrammarParser.DeclaratorContext context)
        {


            // 1. Visit Pointer if exists and derived the pointer chain as a tree
            if (context.pointer() != null)
            {
                Visit(context.pointer());
                // 2. Make the bottom of the pointer chain the current parent
                m_parents.Push(m_currentPointerBottom);
            }


            // 3. Visit DirectDeclarator to link the rest of the declarator
            // at the bottom of the pointer chain
            Visit(context.direct_declarator());

            // 4. If pointer was visited, pop the pointer chain bottom
            if (context.pointer() != null)
            {
                m_parents.Pop();
                m_currentPointerBottom = null;
            }



            return 0;
        }



        public override int VisitArrayDimensionWithSIZE(CGrammarParser.ArrayDimensionWithSIZEContext context)
        {
            return base.VisitArrayDimensionWithSIZE(context);
        }

        public override int VisitArrayDimensionWithNOSIZE(CGrammarParser.ArrayDimensionWithNOSIZEContext context)
        {
            return base.VisitArrayDimensionWithNOSIZE(context);
        }

        public override int VisitTerminal(ITerminalNode node)
        {

            switch (node.Symbol.Type)
            {
                case CGrammarParser.IDENTIFIER:
                    {
                        ASTComposite parent = m_parents.Peek();
                        IDENTIFIER idNode = new IDENTIFIER(node.GetText());
                        parent.AddChild(idNode, parent.GetContextForChild(node)); // assuming context IDENTIFIER for simplicity
                    }
                    break;
                case CGrammarParser.INT:
                    {
                        ASTComposite parent = m_parents.Peek();
                        IntegerTypeAST intNode = new IntegerTypeAST(node.GetText());
                        parent.AddChild(intNode, parent.GetContextForChild(node)); // assuming context INT for simplicity
                    }
                    break;
                case CGrammarParser.CHAR:
                    {
                        ASTComposite parent = m_parents.Peek();
                        CharTypeAST intNode = new CharTypeAST(node.GetText());
                        parent.AddChild(intNode, parent.GetContextForChild(node)); // assuming context INT for simplicity
                    }
                    break;
                // Handle other terminal types as needed
                default:
                    break;
            }

            return base.VisitTerminal(node);
        }

        public override int VisitFunction_definition(CGrammarParser.Function_definitionContext context)
        {

            // 1. Get current parent node
            ASTComposite parent = m_parents.Peek();

            // 2. Create FunctionDefinitionAST node
            FunctionDefinitionAST funcDefNode = new FunctionDefinitionAST();

            // 3. Add FunctionDefinitionAST node to parent
            parent.AddChild(funcDefNode, parent.GetContextForChild(context)); // assuming context FUNCTION_DEFINITION for simplicity

            m_parents.Push(funcDefNode);
            base.VisitFunction_definition(context);
            m_parents.Pop();

            return 0;
        }

        public override int VisitCompound_statement(CGrammarParser.Compound_statementContext context)
        {

            // 1. Get current parent node
            ASTComposite parent = m_parents.Peek();

            // 2. Create FunctionDefinitionAST node
            CompoundStatement compStmtNode = new CompoundStatement();

            // 3. Add FunctionDefinitionAST node to parent
            parent.AddChild(compStmtNode, parent.GetContextForChild(context)); // assuming context

            m_parents.Push(compStmtNode);
            base.VisitCompound_statement(context);
            m_parents.Pop();


            return 0;
        }

        public override int VisitPostfix_expression_ArraySubscript(CGrammarParser.Postfix_expression_ArraySubscriptContext context)
        {
            return base.VisitPostfix_expression_ArraySubscript(context);
        }

        public override int VisitPostfix_expression_Decrement(CGrammarParser.Postfix_expression_DecrementContext context)
        {
            return base.VisitPostfix_expression_Decrement(context);
        }

        public override int VisitPostfix_expression_Increment(CGrammarParser.Postfix_expression_IncrementContext context)
        {
            return base.VisitPostfix_expression_Increment(context);
        }

        public override int VisitPostfix_expression_FunctionCallNoArgs(CGrammarParser.Postfix_expression_FunctionCallNoArgsContext context)
        {
            return base.VisitPostfix_expression_FunctionCallNoArgs(context);
        }

        public override int VisitPostfix_expression_FunctionCallWithArgs(CGrammarParser.Postfix_expression_FunctionCallWithArgsContext context)
        {
            return base.VisitPostfix_expression_FunctionCallWithArgs(context);
        }

        public override int VisitPostfix_expression_PointerMemberAccess(CGrammarParser.Postfix_expression_PointerMemberAccessContext context)
        {
            return base.VisitPostfix_expression_PointerMemberAccess(context);
        }

        public override int VisitPostfix_expression_MemberAccess(CGrammarParser.Postfix_expression_MemberAccessContext context)
        {
            return base.VisitPostfix_expression_MemberAccess(context);
        }

        public override int VisitUnary_expression_Increment(CGrammarParser.Unary_expression_IncrementContext context)
        {
            // 1. Get current parent node
            ASTComposite parent = m_parents.Peek();

            // 2. Create FunctionDefinitionAST node
            UnaryExpressionIncrement unaryIncrement = new UnaryExpressionIncrement();

            // 3. Add FunctionDefinitionAST node to parent
            parent.AddChild(unaryIncrement, parent.GetContextForChild(context)); // assuming context

            m_parents.Push(unaryIncrement);
            base.VisitUnary_expression_Increment(context);
            m_parents.Pop();


            return 0;
        }

        public override int VisitUnary_expression_Decrement(CGrammarParser.Unary_expression_DecrementContext context)
        {
            // 1. Get current parent node
            ASTComposite parent = m_parents.Peek();

            // 2. Create FunctionDefinitionAST node
            UnaryExpressionDecrement unaryDecrement = new UnaryExpressionDecrement();
            
            // 3. Add FunctionDefinitionAST node to parent
            parent.AddChild(unaryDecrement, parent.GetContextForChild(context)); // assuming context
            
            m_parents.Push(unaryDecrement);
            base.VisitUnary_expression_Decrement(context);
            m_parents.Pop();
            
            return 0;
        }

        public override int VisitUnary_expression_SizeofExpression(CGrammarParser.Unary_expression_SizeofExpressionContext context)
        {
            // 1. Get current parent node
            ASTComposite parent = m_parents.Peek();

            // 2. Create FunctionDefinitionAST node
            UnaryExpressionSizeOfExpression UnarySizeOfExpression = new UnaryExpressionSizeOfExpression();

            // 3. Add FunctionDefinitionAST node to parent
            parent.AddChild(UnarySizeOfExpression, parent.GetContextForChild(context)); // assuming context

            m_parents.Push(UnarySizeOfExpression);
            base.VisitUnary_expression_SizeofExpression(context);
            m_parents.Pop();

            return 0;
        }

        public override int VisitUnary_expression_SizeofTypeName(CGrammarParser.Unary_expression_SizeofTypeNameContext context)
        {
            // 1. Get current parent node
            ASTComposite parent = m_parents.Peek();

            // 2. Create FunctionDefinitionAST node
            UnaryExpressionSizeOfTypeName UnarySizeOfType = new UnaryExpressionSizeOfTypeName();

            // 3. Add FunctionDefinitionAST node to parent
            parent.AddChild(UnarySizeOfType, parent.GetContextForChild(context)); // assuming context
            
            m_parents.Push(UnarySizeOfType);
            base.VisitUnary_expression_SizeofTypeName(context);
            m_parents.Pop();
            
            return 0;
        }

        public override int VisitAnd_expression_BitwiseAND(CGrammarParser.And_expression_BitwiseANDContext context) {
            // 1. Get current parent node
            ASTComposite parent = m_parents.Peek();

            // 2. Create FunctionDefinitionAST node
            Expression_BitwiseAND AndExpression = new Expression_BitwiseAND();

            // 3. Add FunctionDefinitionAST node to parent
            parent.AddChild(AndExpression, parent.GetContextForChild(context)); // assuming context

            m_parents.Push(AndExpression);
            base.VisitAnd_expression_BitwiseAND(context);
            m_parents.Pop();

            return 0;
        }

        public override int VisitExclusive_or_expression_BitwiseXOR(CGrammarParser.Exclusive_or_expression_BitwiseXORContext context) {
            // 1. Get current parent node
            ASTComposite parent = m_parents.Peek();

            // 2. Create FunctionDefinitionAST node
            Expression_BitwiseXOR XORExpression = new Expression_BitwiseXOR();

            // 3. Add FunctionDefinitionAST node to parent
            parent.AddChild(XORExpression, parent.GetContextForChild(context)); // assuming context

            m_parents.Push(XORExpression);
            base.VisitExclusive_or_expression_BitwiseXOR(context);
            m_parents.Pop();

            return 0;
        }

        public override int VisitInclusive_or_expression_BitwiseOR(CGrammarParser.Inclusive_or_expression_BitwiseORContext context) {
            
            // 1. Get current parent node
            ASTComposite parent = m_parents.Peek();

            // 2. Create FunctionDefinitionAST node
            Expression_BitwiseOR ORExpression = new Expression_BitwiseOR();

            // 3. Add FunctionDefinitionAST node to parent
            parent.AddChild(ORExpression, parent.GetContextForChild(context)); // assuming context

            m_parents.Push(ORExpression);
            base.VisitInclusive_or_expression_BitwiseOR(context);
            m_parents.Pop();

            return 0;
        }


    }

}
