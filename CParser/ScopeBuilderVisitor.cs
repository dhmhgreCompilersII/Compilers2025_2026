using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static CParser.Symbol;

namespace CParser {

    public record ParentInfo(uint context) {
    }


    public class ScopeBuilderVisitor : BaseASTVisitor<int, ParentInfo> {

        public ScopeBuilderVisitor() { }

        public override int VisitTranslationUnit(TranslationUnitAST node, ParentInfo info) {
            CScopeSystem.GetInstance().EnterScope(ScopeType.File);
            base.VisitTranslationUnit(node, info);
            CScopeSystem.GetInstance().ExitScope();
            return 0;
        }

        public override int VisitFunctionDefinition(FunctionDefinitionAST node, ParentInfo info) {

            // 1. Visit function name and place it to current scope (global scope)
            IDENTIFIER? functionName =
                node.GetChild<IDENTIFIER>(FunctionDefinitionAST.DECLARATOR);
            if (functionName == null) {
                throw new Exception("FunctionDefinitionAST has no function name.");
            }

            // 1. Add function symbol to current scope
            Symbol functionSymbol = new Symbol(functionName.MName,
                Symbol.SymbolType.Function,
                node);
            CScopeSystem.GetInstance().AddSymbol(CScope.Namespace.Ordinary,
                                                 functionName.MName,
                                                 functionSymbol);

            // 2. Enter function scope
            CScopeSystem.GetInstance().EnterScope(ScopeType.Function, functionName.MName);

            // 3. Visit parameters and place them to function scope
            ParentInfo paramInfo = new ParentInfo(FunctionDefinitionAST.PARAMETER_DECLARATIONS);
            VisitContext(node, FunctionDefinitionAST.PARAMETER_DECLARATIONS, paramInfo);

            // 4. Visit function body
            VisitContext(node, FunctionDefinitionAST.FUNCTION_BODY, info);

            // 5. Exit function scope
            CScopeSystem.GetInstance().ExitScope();

            return 0;
        }

        public override int VisitIdentifier(IDENTIFIER node, ParentInfo info) {

            // Check if this identifier is a function parameter
            if (info != null &&
                (info.context == FunctionDefinitionAST.PARAMETER_DECLARATIONS ||
                info.context == DeclarationAST.DECLARATORS)) {
                // This identifier is a function parameter
                Symbol paramSymbol = new Symbol(node.MName,
                    Symbol.SymbolType.Variable,
                    node);
                CScopeSystem.GetInstance().AddSymbol(CScope.Namespace.Ordinary,
                                                     node.MName,
                                                     paramSymbol);
            }
            return base.VisitIdentifier(node, info);
        }

        public override int VisitDeclaration(DeclarationAST node, ParentInfo info) {
            // Visit declarators to add variables to current scope
            ParentInfo declInfo = new ParentInfo(DeclarationAST.DECLARATORS);
            base.VisitDeclaration(node, declInfo);
            return 0;
        }
    }
}
