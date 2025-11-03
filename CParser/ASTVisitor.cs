using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CParser {
    public class BaseASTVisitor<Result> {
        public BaseASTVisitor() {

        }

        public Result VisitChildren(TranslationUnitAST node) {
            for (int context = 0; context < node.MContexts; context++) {
                foreach (ASTElement astElement in node.MChildren[context]) {
                    Visit(astElement);
                }
            }
            return default(Result);
        }

        public void Visit(ASTElement astElement) {
            astElement.Accept(this);
        }


        public virtual Result VisitTranslationUnit(TranslationUnitAST node) {
            return VisitChildren(node);
        }

        
    }
}
