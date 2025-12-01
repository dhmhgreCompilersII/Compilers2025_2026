using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;

namespace CParser {
    public class BaseASTVisitor<Result,INFO> {
        public BaseASTVisitor() {

        }

        public Result VisitChildren(ASTComposite node,INFO info) {
            for (int context = 0; context < node.MContexts; context++) {
                foreach (ASTElement astElement in node.MChildren[context]) {
                    Visit(astElement,info);
                }
            }
            return default(Result);
        }

        public Result Visit(ASTElement astElement,INFO info) {
            return astElement.Accept<Result,INFO>(this,info);
        }

        
        public virtual Result VisitTranslationUnit(TranslationUnitAST node,INFO info) {
            return VisitChildren(node, info);
        }

        public virtual Result VisitDeclaration(DeclarationAST node,INFO info) {
            return VisitChildren(node, info);
        }
        public virtual Result VisitPointerType(PointerTypeAST node, INFO info) {
            return VisitChildren(node, info);
        }
        public virtual Result VisitIntegerType(IntegerTypeAST node , INFO info) {
            return default(Result);
        }
        public virtual Result VisitFunctionType(FunctionTypeAST node, INFO info) {
            return VisitChildren(node, info);
        }

        public virtual Result VisitParameterDeclaration(ParameterDeclarationAST node,INFO info) {
            return VisitChildren(node, info);
        }

        public virtual Result VisitFunctionDefinition(FunctionDefinitionAST node,INFO info) {
            return VisitChildren(node, info);
        }
        public virtual Result VisitCompoundStatement(CompoundStatement node, INFO info) {
            return VisitChildren(node, info);
        }

        public virtual Result VisitExpressionStatement(ExpressionStatement node, INFO info) {
            return VisitChildren(node, info);
        }

        public virtual Result VisitExpressionIdentifier(Expression_Identifier node, INFO info) {
            return VisitChildren(node, info);
        }

        public virtual Result VisitExpressionAssignment(Expression_Assignment node, INFO info) {
            return VisitChildren(node, info);
        }

        public virtual Result VisitExpressionAddition(Expression_Addition node, INFO info) {
            return VisitChildren(node, info);
        }

        public virtual Result VisitExpressionNumber(Expression_Number node, INFO info) {
            return VisitChildren(node, info);
        }


        public virtual Result VisitIdentifier( IDENTIFIER node,INFO info) {
            return default(Result);
        }


    }
}
