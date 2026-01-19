using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static CParser.Symbol;

namespace CParser
{

    public record ParentInfo(ASTComposite context) {
    }


    public class ScopeBuilderVisitor : BaseASTVisitor<int, ParentInfo>
    {

        public ScopeBuilderVisitor() { }

        public override int VisitTranslationUnit(TranslationUnitAST node, ParentInfo info)
        {
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

        public override int VisitDeclarationSpecifiers(Declaration_Specifiers node, ParentInfo info) {

            var mtypes = node.MChildren[Declaration_Specifiers.SPECIFIERS]
                .OfType<ASTElement>()
                .Select(child => child.MType)
                .ToList();

            CType.TypeKind tp;
            int size;

            switch (true) {
                case true when mtypes.Contains((uint)TranslationUnitAST.NodeTypes.LONG_TYPE) ||
                               mtypes.Contains((uint)TranslationUnitAST.NodeTypes.INTEGER_TYPE):
                    tp = CType.TypeKind.Int;
                    IntegerType.IntegerKind sign;
                    if (mtypes.Contains((uint)TranslationUnitAST.NodeTypes.UNSIGNED_TYPE)) {
                        sign = IntegerType.IntegerKind.Unsigned;
                    } else {
                        sign = IntegerType.IntegerKind.Signed;
                    }
                    size = mtypes.Count(t => t == (uint)TranslationUnitAST.NodeTypes.LONG_TYPE) >= 2 ? 8 : 4;
                    IntegerType intType = new IntegerType(sign, size);
                    break;

                case true when mtypes.Contains((uint)TranslationUnitAST.NodeTypes.FLOAT_TYPE) ||
                               mtypes.Contains((uint)TranslationUnitAST.NodeTypes.DOUBLE_TYPE):
                    tp = CType.TypeKind.Float;

                    break;

                default:
                    tp = CType.TypeKind.Int;
                    size = 4;
                    break;
            }
            ;

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