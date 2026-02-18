using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static CParser.Symbol;

namespace CParser
{
    public class ParentInfo(ASTComposite context)
    {
    }

    public class DeclarationContext : ParentInfo
    {
        public CType? MTypeSpecifier { get; set; }
        public CType? MTypeRoot { get; set; }
        public CType? MParent { get; set; }
        public CType? MFunctionType { get; set; }
        public string MDeclarator { get; set; }

        public DeclarationContext(ASTComposite context)
            : base(context)
        {
        }

        public void Reset()
        {
            MTypeRoot = null;
            MParent = null;
            MFunctionType = null;
            MDeclarator = string.Empty;
        }
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

        public override int VisitDeclaration(DeclarationAST node, ParentInfo info)
        {
            DeclarationContext declContext = new DeclarationContext(node);

            // 1. Visit Declaration Specifiers
            VisitContext(node, DeclarationAST.TYPE_SPECIFIER, declContext);

            /*if (declContext.MFunctionType != null)
            {
                if (declContext.MParent == null)
                {
                    declContext.MFunctionType.AddTypeParameter(declContext.MTypeSpecifier.Clone());
                }
                else
                {
                    declContext.MParent.AddTypeParameter(declContext.MTypeSpecifier.Clone());
                    declContext.MFunctionType.AddTypeParameter(declContext.MParent.Clone());
                }

                declContext.MParent = null;
            }*/
            
            // 2. Visit Declarators
            foreach (ASTElement astElement in node.MChildren[DeclarationAST.DECLARATORS])
            {
                declContext.Reset();

                Visit(astElement, declContext);
                if (declContext.MParent == null)
                {
                    if (declContext.MTypeSpecifier is VoidType)
                    {
                        throw new ArgumentException();
                    }

                    declContext.MTypeRoot = declContext.MTypeSpecifier;
                    declContext.MParent = declContext.MTypeSpecifier;
                }
                else
                {
                    declContext.MParent.AddTypeParameter(declContext.MTypeSpecifier);
                }
                declContext.MTypeRoot.TypeDebugLog();
            }

            return 0;
        }

        private void SpecifierOrParamater(CType type, DeclarationContext context)
        {
            if (context.MFunctionType != null)
            {
                if (context.MParent == null)
                {
                    context.MFunctionType.AddTypeParameter(type.Clone());
                }
                else
                {
                    context.MParent.AddTypeParameter(type.Clone());
                    context.MFunctionType.AddTypeParameter(context.MTypeRoot.Clone());
                }
                context.MParent = null;
                return;
            }

            context.MTypeSpecifier = type;
        }

        public override int VisitDeclarationSpecifiers(Declaration_Specifiers node, ParentInfo info)
        {
            var mtypes = node.MChildren[Declaration_Specifiers.SPECIFIERS]
                .OfType<ASTElement>()
                .Select(child => child.MType)
                .ToList();

            CType.TypeKind tp;
            int size;

            switch (true)
            {
                case true when (mtypes.Contains((uint)TranslationUnitAST.NodeTypes.LONG_TYPE) ||
                               mtypes.Contains((uint)TranslationUnitAST.NodeTypes.INTEGER_TYPE) ||
                               mtypes.Contains((uint)TranslationUnitAST.NodeTypes.SHORT_TYPE)) &&
                               !mtypes.Contains((uint)TranslationUnitAST.NodeTypes.DOUBLE_TYPE):
                    tp = CType.TypeKind.Int;
                    IntegerType.IntegerKind sign;
                    if (mtypes.Contains((uint)TranslationUnitAST.NodeTypes.UNSIGNED_TYPE))
                    {
                        sign = IntegerType.IntegerKind.Unsigned;
                    }
                    else
                    {
                        sign = IntegerType.IntegerKind.Signed;
                    }
                    size = mtypes.Count(t => t == (uint)TranslationUnitAST.NodeTypes.LONG_TYPE) >= 2 ? 8 : 4;
                    size = mtypes.Contains((uint)TranslationUnitAST.NodeTypes.SHORT_TYPE) ? 2 : size;

                    IntegerType intType = new IntegerType(sign, size);
                    DeclarationContext declContext = info as DeclarationContext;
                    //declContext.MTypeSpecifier = intType;
                    SpecifierOrParamater(intType, declContext);
                    break;
                    
                case true when mtypes.Contains((uint)TranslationUnitAST.NodeTypes.FLOAT_TYPE) ||
                               mtypes.Contains((uint)TranslationUnitAST.NodeTypes.DOUBLE_TYPE) ||
                               mtypes.Contains((uint)TranslationUnitAST.NodeTypes.LONG_TYPE):
                    tp = mtypes.Contains((uint)TranslationUnitAST.NodeTypes.FLOAT_TYPE) ? CType.TypeKind.Float : CType.TypeKind.Double;
                    
                    size = mtypes.Contains((uint)TranslationUnitAST.NodeTypes.FLOAT_TYPE) ? 4 : 8;
                    size = mtypes.Contains((uint)TranslationUnitAST.NodeTypes.LONG_TYPE) && 
                        tp != CType.TypeKind.Float ? 10 : size;
                    
                    string name = tp == CType.TypeKind.Float ? "float" : "double";

                    FloatingPointType floatType = new FloatingPointType(size, name, tp);
                    DeclarationContext declContext2 = info as DeclarationContext;
                    //declContext2.MTypeSpecifier = floatType;
                    SpecifierOrParamater(floatType, declContext2);
                    break;

                case true when mtypes.Contains((uint)TranslationUnitAST.NodeTypes.STRUCT_TYPE):
                    IDENTIFIER struct_id = node.MChildren[Declaration_Specifiers.SPECIFIERS][1] as IDENTIFIER;
                    string struct_name = struct_id.MLexeme;

                    StructType structType = new StructType(struct_name);
                    DeclarationContext declContext3 = info as DeclarationContext;
                    //declContext3.MTypeSpecifier = structType;
                    SpecifierOrParamater(structType, declContext3);
                    break;

                case true when mtypes.Contains((uint)TranslationUnitAST.NodeTypes.UNION_TYPE):

                    IDENTIFIER union_id = node.MChildren[Declaration_Specifiers.SPECIFIERS][1] as IDENTIFIER;
                    string union_name = union_id.MLexeme;

                    UnionType unionType = new UnionType(union_name);
                    DeclarationContext declContext4 = info as DeclarationContext;
                    //declContext4.MTypeSpecifier = unionType;
                    SpecifierOrParamater(unionType, declContext4);
                    break;

                case true when mtypes.Contains((uint)TranslationUnitAST.NodeTypes.VOID_TYPE):
                    VoidType voidType = new VoidType();
                    DeclarationContext declContext5 = info as DeclarationContext;
                    //declContext5.MTypeSpecifier = voidType;
                    SpecifierOrParamater(voidType, declContext5);
                    break;

                default:
                    tp = CType.TypeKind.Int;
                    size = 4;
                    break;
            }

            return 0;
        }

        public override int VisitIdentifier(IDENTIFIER node, ParentInfo info)
        {
            // Check if this identifier is a function parameter
            DeclarationContext declarationContext = info as DeclarationContext;
            declarationContext.MDeclarator = node.MLexeme;
            return 0;
        }

        public override int VisitFunctionType(FunctionTypeAST node, ParentInfo info)
        {
            DeclarationContext declContext = info as DeclarationContext;

            FunctionType funcType = new FunctionType();
            funcType.AddTypeParameter(declContext.MTypeSpecifier.Clone());

            declContext.MFunctionType = funcType;
            base.VisitFunctionType(node, info);

            declContext.MTypeSpecifier = funcType;

            return 0;
        }

        public override int VisitPointerType(PointerTypeAST node, ParentInfo info)
        {

            DeclarationContext declContext = info as DeclarationContext;

            // Preorder actions

            // Visit children
            VisitContext(node, PointerTypeAST.POINTER_TARGET, declContext);

            // Postorder actions
            PointerType pointerType = new PointerType();
            if (declContext.MParent != null)
            {
                declContext.MParent.AddTypeParameter(pointerType);
            }
            else
            {
                declContext.MTypeRoot = pointerType;
            }
            declContext.MParent = pointerType;

            return 0;
        }
    }
}