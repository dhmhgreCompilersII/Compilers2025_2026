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

        public override int VisitDeclaration(DeclarationAST node, ParentInfo info) {

            // 1. Visit Type Specifier
            VisitContext(node, DeclarationAST.TYPE_SPECIFIER, info);
            return 0;
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

            return base.VisitIdentifier(node, info);
        }

        public override int VisitPointerType(PointerTypeAST node, ParentInfo info) {

            // 1. 




            return base.VisitPointerType(node, info);
        }




    }
}
