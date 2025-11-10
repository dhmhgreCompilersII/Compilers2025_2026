using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime.Tree;

namespace CParser {
    public class ANLTRST2ASTGenerationVisitor : CGrammarParserBaseVisitor<int>{

        ASTComposite m_root;
        private Stack<ASTComposite> m_parents = new Stack<ASTComposite>();


        public ASTComposite Root {
            get => m_root;
        }

        public ANLTRST2ASTGenerationVisitor() {
            m_root = null;
        }

        public override int VisitTranslation_unit(CGrammarParser.Translation_unitContext context) {

            // 1. Create TranslationUnitAST node
            TranslationUnitAST tuNode = new TranslationUnitAST();
            m_root = tuNode;

            //2. Visit children and populate the AST node
            m_parents.Push(tuNode);
            VisitChildren(context);
            m_parents.Pop();

            return 0;
        }

        public override int VisitDeclaration(CGrammarParser.DeclarationContext context) {

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

        public override int VisitPointer(CGrammarParser.PointerContext context) {

            ASTComposite parent = m_parents.Peek();

            PointerTypeAST pointerNode = new PointerTypeAST();

            parent.AddChild(pointerNode, parent.GetContextForChild(context)); // assuming context POINTER_TARGER for simplicity

            m_parents.Push(pointerNode);
            base.VisitPointer(context);
            m_parents.Pop();

            return 0;
        }

        public override int VisitFunctionWithNOArguments(CGrammarParser.FunctionWithNOArgumentsContext context) {

            ASTComposite parent = m_parents.Peek();

            FunctionTypeAST funcTypeNode = new FunctionTypeAST();

            parent.AddChild(funcTypeNode, parent.GetContextForChild(context)); // assuming context FUNCTION_TYPE for simplicity

            m_parents.Push(funcTypeNode);
            base.VisitFunctionWithNOArguments(context);
            m_parents.Pop();

            return 0;
        }

        public override int VisitFunctionWithArguments(CGrammarParser.FunctionWithArgumentsContext context) {
            ASTComposite parent = m_parents.Peek();

            FunctionTypeAST funcTypeNode = new FunctionTypeAST();

            parent.AddChild(funcTypeNode, parent.GetContextForChild(context)); // assuming context FUNCTION_TYPE for simplicity

            m_parents.Push(funcTypeNode);
            base.VisitFunctionWithArguments(context);
            m_parents.Pop();

            return 0;
        }

        public override int VisitArrayDimensionWithSIZE(CGrammarParser.ArrayDimensionWithSIZEContext context) {
            return base.VisitArrayDimensionWithSIZE(context);
        }

        public override int VisitArrayDimensionWithNOSIZE(CGrammarParser.ArrayDimensionWithNOSIZEContext context) {
            return base.VisitArrayDimensionWithNOSIZE(context);
        }

        public override int VisitTerminal(ITerminalNode node) {
            return base.VisitTerminal(node);
        }

        public override int VisitFunction_definition(CGrammarParser.Function_definitionContext context) {

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
    }
}
