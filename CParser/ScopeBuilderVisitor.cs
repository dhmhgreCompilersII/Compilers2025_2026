using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CParser
{
    public class ScopeBuilderVisitor : BaseASTVisitor<int, ASTComposite>
    {
        public ScopeBuilderVisitor(){}

        public override int VisitTranslationUnit(TranslationUnitAST node, ASTComposite info)
        {
            CScopeSystem.GetInstance().EnterScope(ScopeType.File);
            base.VisitTranslationUnit(node, info);
            CScopeSystem.GetInstance().ExitScope();
            return 0;
        }

        public override int VisitFunctionDefinition(FunctionDefinitionAST node, ASTComposite info)
        {


            base.VisitFunctionDefinition(node, info);
            return 0;
        }
    }
}
