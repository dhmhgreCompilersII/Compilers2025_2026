using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CParser {
    public class ANLTRST2ASTGenerationVisitor : CGrammarParserBaseVisitor<int>{

        ASTComposite m_root;
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
            VisitChildren(context);

            return 0;
        }


    }
}
