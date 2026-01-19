using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static CParser.Symbol;

namespace CParser {

    public class ParentInfo(ASTComposite context) {
    }

    public class DeclarationContext : ParentInfo{
        public CType? MTypeSpecifier { get; set; }
        public CType? MTypeRoot { get; set; }
        public CType? MParent { get; set; } 
        public string MDeclarator { get; set; }

        public DeclarationContext(ASTComposite context)
            : base(context) {
        }

        public void Reset() {
            MTypeRoot = null;
            MParent = null;
            MDeclarator = string.Empty;
        }

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

            DeclarationContext declContext = new DeclarationContext(node);

            // 1. Visit Declaration Specifiers
            VisitContext(node, DeclarationAST.TYPE_SPECIFIER, declContext);

            // 2. Visit Declarators
            foreach (ASTElement astElement in node.MChildren[DeclarationAST.DECLARATORS]) {
                declContext.Reset();
                Visit(astElement, declContext);
                if (declContext.MParent == null) {
                    declContext.MTypeRoot = declContext.MTypeSpecifier;
                    declContext.MParent = declContext.MTypeSpecifier;
                }
                else {
                    declContext.MParent.AddTypeParameter(declContext.MTypeSpecifier);
                }
            }

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
                    DeclarationContext declContext = info as DeclarationContext;
                    declContext.MTypeSpecifier = intType;
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
            DeclarationContext declarationContext = info as DeclarationContext;
            declarationContext.MDeclarator = node.MLexeme;
            return 0;
        }

        public override int VisitPointerType(PointerTypeAST node, ParentInfo info) {

            DeclarationContext declContext = info as DeclarationContext;

            // Preorder actions

            // Visit children
            VisitContext(node, PointerTypeAST.POINTER_TARGET, declContext);

            // Postorder actions
            PointerType pointerType = new PointerType();
            if (declContext.MParent != null) {
                declContext.MParent.AddTypeParameter(pointerType);
            }
            else {
                declContext.MTypeRoot = pointerType;
            }
            declContext.MParent = pointerType;

            return 0;
        }




    }
}
