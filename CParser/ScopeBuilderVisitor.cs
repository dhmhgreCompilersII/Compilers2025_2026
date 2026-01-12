using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static CParser.Symbol;

namespace CParser {

    public record ParentInfo(ASTComposite context) {
    }


    public class ScopeBuilderVisitor : BaseASTVisitor<int, ParentInfo> {

        public ScopeBuilderVisitor() { }

        public override int VisitTranslationUnit(TranslationUnitAST node, ParentInfo info) {
            CScopeSystem.GetInstance().EnterScope(ScopeType.File);
            base.VisitTranslationUnit(node, info);
            CScopeSystem.GetInstance().ExitScope();
            return 0;
        }

        

        public override int VisitIdentifier(IDENTIFIER node, ParentInfo info) {

            // Check if this identifier is a function parameter
            
            return base.VisitIdentifier(node, info);
        }

        public override int VisitPointerType(PointerTypeAST node, ParentInfo info) {

            // 1. 




            return base.VisitPointerType(node, info);
        }

        public override int VisitDeclaration(DeclarationAST node, ParentInfo info) {

            // 1. Visit Type Specifier
            VisitContext(node, DeclarationAST.TYPE_SPECIFIER, info);


            return 0;
        }


    }
}
