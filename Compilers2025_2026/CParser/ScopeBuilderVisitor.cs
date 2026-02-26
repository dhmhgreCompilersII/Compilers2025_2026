using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static CParser.CScope;
using static CParser.Symbol;

namespace CParser {

    public class ParentInfo(ASTComposite context) {
    }

    public class DeclarationContext : ParentInfo{
        public CType? MTypeSpecifier { get; set; }
        public CType? MTypeRoot { get; set; }
        public CType? MParent { get; set; } 
        public string MDeclarator { get; set; }
        public bool IsConst { get; set; } = false;
        public bool IsVolatile { get; set; } = false;
        public DeclarationAST.STORAGE_CLASS_ENUM? StorageClass { get; set; } = null;

        public DeclarationContext(ASTComposite context)
            : base(context) {
        }

        public void Reset() {
            MTypeRoot = null;
            MParent = null;
            MDeclarator = string.Empty;
            IsConst = false;
            IsVolatile = false;
            StorageClass = null;
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
                if (declContext.MTypeRoot != null) {
                    declContext.MTypeRoot.TypeDebugLog();
                }
                else {
                    Console.WriteLine("⚠️ ERROR: Το MTypeRoot είναι null! Κάποιο case στο switch της VisitDeclarationSpecifiers δεν έδωσε τιμή στο baseType!");
                }
                // ================= SYMBOL TABLE (ΓΙΑ ΜΕΤΑΒΛΗΤΕΣ) =================
                if (!string.IsNullOrEmpty(declContext.MDeclarator))
                {
                    // Εδώ δίνουμε το 'astElement' που είναι ο συγκεκριμένος κόμβος της μεταβλητής
                    Symbol varSymbol = new Symbol(declContext.MDeclarator, Symbol.SymbolType.Variable, astElement);

                    CScopeSystem.GetInstance().AddSymbol(Namespace.Ordinary, declContext.MDeclarator, varSymbol);
                }

            }

            return 0;

            
        }

        public override int VisitDeclarationSpecifiers(Declaration_Specifiers node, ParentInfo info) {

            var mtypes = node.MChildren[Declaration_Specifiers.SPECIFIERS]
                .OfType<ASTElement>()
                .Select(child => child.MType)
                .ToList();

            DeclarationContext declContext = info as DeclarationContext;
            if (declContext == null) return 0;

            if (mtypes.Contains((uint)TranslationUnitAST.NodeTypes.CONST_TYPE))
            {
                declContext.IsConst = true;
            }
            if (mtypes.Contains((uint)TranslationUnitAST.NodeTypes.VOLATILE_TYPE))
            {
                declContext.IsVolatile = true;
            }

            if (mtypes.Contains((uint)TranslationUnitAST.NodeTypes.STATIC_TYPE))
            {
                declContext.StorageClass = DeclarationAST.STORAGE_CLASS_ENUM.STATIC;
            }
            else if (mtypes.Contains((uint)TranslationUnitAST.NodeTypes.EXTERN_TYPE))
            {
                declContext.StorageClass = DeclarationAST.STORAGE_CLASS_ENUM.EXTERN;
            }
            else if (mtypes.Contains((uint)TranslationUnitAST.NodeTypes.REGISTER_TYPE))
            {
                declContext.StorageClass = DeclarationAST.STORAGE_CLASS_ENUM.REGISTER;
            }
            else if (mtypes.Contains((uint)TranslationUnitAST.NodeTypes.AUTO_TYPE))
            {
                declContext.StorageClass = DeclarationAST.STORAGE_CLASS_ENUM.AUTO;
            }

            CType.TypeKind tp;
            int size;
            CType baseType = null;

            switch (true) {
                case true when mtypes.Contains((uint)TranslationUnitAST.NodeTypes.LONG_TYPE) ||
                               mtypes.Contains((uint)TranslationUnitAST.NodeTypes.INTEGER_TYPE)||
                               mtypes.Contains((uint)TranslationUnitAST.NodeTypes.SHORT_TYPE):
                    tp = CType.TypeKind.Int;
                    IntegerType.IntegerKind sign;
                    if (mtypes.Contains((uint)TranslationUnitAST.NodeTypes.UNSIGNED_TYPE)) {
                        sign = IntegerType.IntegerKind.Unsigned;
                    } else {
                        sign = IntegerType.IntegerKind.Signed;
                    }
                    if (mtypes.Contains((uint)TranslationUnitAST.NodeTypes.LONG_TYPE)) {
                        size = 8;
                    }else if(mtypes.Count(t => t == (uint)TranslationUnitAST.NodeTypes.LONG_TYPE) >= 2) {
                        size = 8;
                    }
                    else if (mtypes.Contains((uint)TranslationUnitAST.NodeTypes.SHORT_TYPE)) {
                        size = 2;
                    } else {
                        size = 4;
                    }
                    baseType = new IntegerType(sign, size);
                    declContext = info as DeclarationContext;
                    if (declContext != null)
                    {
                        declContext.MTypeSpecifier = baseType;
                    }
                    break;

                case true when mtypes.Contains((uint)TranslationUnitAST.NodeTypes.FLOAT_TYPE) ||
                               mtypes.Contains((uint)TranslationUnitAST.NodeTypes.DOUBLE_TYPE):
                    tp = CType.TypeKind.Float;
                    size = mtypes.Contains((uint)TranslationUnitAST.NodeTypes.DOUBLE_TYPE) ? 8 : 4;
                    baseType = new FloatingPointType(size);
                    declContext = info as DeclarationContext;
                    declContext.MTypeSpecifier = baseType;
                    break;
                
                case true when mtypes.Contains((uint)TranslationUnitAST.NodeTypes.CHAR_TYPE):
                    tp = CType.TypeKind.Char;
                    size = 1;
                    
                    if (mtypes.Contains((uint)TranslationUnitAST.NodeTypes.UNSIGNED_TYPE)) {
                       baseType = new CharType(CharType.CharacterKind.Unsigned, size);
                    } else {
                       baseType = new CharType(CharType.CharacterKind.Signed, size);
                    }

                    declContext = info as DeclarationContext;
                    if (declContext != null)
                    {
                        declContext.MTypeSpecifier = baseType;
                    }

                    break;
                case true when mtypes.Contains((uint)TranslationUnitAST.NodeTypes.VOID_TYPE):
                    tp = CType.TypeKind.Void;
                    size = 0;
                    baseType = new VoidType(size);
                    declContext = info as DeclarationContext;
                    if (declContext != null)
                    {
                        declContext.MTypeSpecifier = baseType;
                    }
                    break;
                case true when mtypes.Contains((uint)TranslationUnitAST.NodeTypes.STRUCT_TYPE):
                    tp = CType.TypeKind.Struct;

                    var structNode = node.MChildren[Declaration_Specifiers.SPECIFIERS]
                                         .OfType<StructTypeAST>()
                                         .FirstOrDefault();

                    Visit(structNode, declContext);

                    baseType = declContext.MTypeSpecifier;
                    declContext.MTypeSpecifier = baseType;
                    if(declContext != null) {
                        declContext.MTypeSpecifier = baseType;
                    }
                    break;

                case true when mtypes.Contains((uint)TranslationUnitAST.NodeTypes.UNION_TYPE):
                    tp = CType.TypeKind.Union;
                    UnionType unionType = new UnionType();
                    baseType = unionType;

                    // Ψάχνουμε τον κόμβο του union
                    var unionNode = node.MChildren[Declaration_Specifiers.SPECIFIERS]
                                        .OfType<UnionTypeAST>()
                                        .FirstOrDefault();

                    if (unionNode != null)
                    {
                        declContext.MTypeSpecifier = unionType;
                        Visit(unionNode, declContext);

                        if (declContext.MTypeSpecifier != null)
                        {
                            baseType = declContext.MTypeSpecifier;
                        }
                    }

                    declContext.MTypeSpecifier = baseType;
                    break;

                case true when mtypes.Contains((uint)TranslationUnitAST.NodeTypes.ENUM_TYPE):
                    tp = CType.TypeKind.Enum;
                    EnumType enumType = new EnumType();
                    baseType = enumType;

                    // Ψάχνουμε τον κόμβο του enum
                    var enumNode = node.MChildren[Declaration_Specifiers.SPECIFIERS]
                                       .OfType<EnumTypeAST>()
                                       .FirstOrDefault();

                    if (enumNode != null)
                    {
                        declContext.MTypeSpecifier = enumType;
                        Visit(enumNode, declContext); 

                        if (declContext.MTypeSpecifier != null)
                        {
                            baseType = declContext.MTypeSpecifier;
                        }
                    }

                    declContext.MTypeSpecifier = baseType;
                    break;

                default:
                    throw new NotImplementedException("Unknown type specifier encountered in AST.");
            }
            ;

            if (baseType != null)
            {
                CType finalType = baseType;

                // Αν έχει const, φτιάχνουμε Qualifier κόμβο και του "κρεμάμε" τον βασικό τύπο
                if (mtypes.Contains((uint)TranslationUnitAST.NodeTypes.CONST_TYPE))
                {
                    QualifierType constType = new QualifierType(QualifierType.QualifierKind.Const);
                    constType.AddTypeParameter(finalType);
                    finalType = constType; // Το const γίνεται πλέον η κορυφή του δέντρου
                }

                // Αν έχει volatile, κάνουμε το ίδιο
                if (mtypes.Contains((uint)TranslationUnitAST.NodeTypes.VOLATILE_TYPE))
                {
                    QualifierType volatileType = new QualifierType(QualifierType.QualifierKind.Volatile);
                    volatileType.AddTypeParameter(finalType);
                    finalType = volatileType;
                }

                if (mtypes.Contains((uint)TranslationUnitAST.NodeTypes.STATIC_TYPE)) {
                    
                   StorageClassType storageNode = new StorageClassType("Static");
                   storageNode.AddTypeParameter(finalType);
                   finalType = storageNode; 
                    
                } else if (mtypes.Contains((uint)TranslationUnitAST.NodeTypes.EXTERN_TYPE)) {
                    
                   StorageClassType storageNode = new StorageClassType("Extern");
                   storageNode.AddTypeParameter(finalType);
                   finalType = storageNode; 
                    
                } else if (mtypes.Contains((uint)TranslationUnitAST.NodeTypes.REGISTER_TYPE)) {
                    
                   StorageClassType storageNode = new StorageClassType("Register");
                   storageNode.AddTypeParameter(finalType);
                   finalType = storageNode; 
                    
                } else if (mtypes.Contains((uint)TranslationUnitAST.NodeTypes.AUTO_TYPE)) {
                    
                   StorageClassType storageNode = new StorageClassType("Auto");
                   storageNode.AddTypeParameter(finalType);
                   finalType = storageNode;

                }


                // Τέλος, δίνουμε τον τελικό τύπο (με ή χωρίς qualifiers) στο Context
                declContext.MTypeSpecifier = finalType;
            }

            return 0;
        }




        public override int VisitIdentifier(IDENTIFIER node, ParentInfo info) {

            // Check if this identifier is a function parameter
            DeclarationContext declarationContext = info as DeclarationContext;
            if(declarationContext != null) {
                declarationContext.MDeclarator = node.MLexeme;
            }
            return 0;
        }

        public override int VisitArrayType(ArrayTypeAST node, ParentInfo info)
        {
            DeclarationContext declContext = info as DeclarationContext;

            // 1. Φτιάχνουμε το ArrayType, δίνοντάς του τον βασικό τύπο
            CType elementType = declContext.MTypeSpecifier;
            ArrayType arrayType = new ArrayType(elementType);

            // 2. Διαβάζουμε το μέγεθος του πίνακα
            if (node.MChildren[ArrayTypeAST.ARRAY_SIZE].Count > 0)
            {
                // Υποθέτουμε ότι διαβάζεις το INTEGER node και παίρνεις το νούμερο
                INTEGER sizeNode = (INTEGER)node.MChildren[ArrayTypeAST.ARRAY_SIZE][0];
                if (int.TryParse(sizeNode.MLexeme, out int size))
                {
                    arrayType.AddHigherLevelDimensionSize(size);
                }
            }

            // 3. Σύνδεση με τον γονέα (όπως στα pointers)
            if (declContext.MParent != null)
            {
                declContext.MParent.AddTypeParameter(arrayType);
            }
            else
            {
                declContext.MTypeRoot = arrayType;
            }
            declContext.MParent = arrayType;

            // 4. Επισκεπτόμαστε τον στόχο (συνήθως το όνομα του array)
            VisitContext(node, ArrayTypeAST.ARRAY_TARGET, declContext);

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

        public override int VisitStructType(StructTypeAST node, ParentInfo info)
        {
            DeclarationContext declContext = info as DeclarationContext;
            if (declContext == null) return 0;

            StructType structType = new StructType();
            string structName = null;

            if (node.MChildren[StructTypeAST.STRUCT_NAME].Count > 0)
            {
                IDENTIFIER idNode = (IDENTIFIER)node.MChildren[StructTypeAST.STRUCT_NAME][0];
                structName = idNode.MLexeme;
                structType.SetName(structName);
            }

            declContext.MTypeSpecifier = structType;

            // ================= SCOPE SYSTEM =================
            // Ανοίγουμε το Scope του Struct!
            CScopeSystem.GetInstance().EnterScope(ScopeType.StructUnionEnum, structName);
            // ==================================================

            if (node.MChildren[StructTypeAST.STRUCT_DECLARATIONS].Count > 0)
            {
                VisitContext(node, StructTypeAST.STRUCT_DECLARATIONS, info);
            }

            // ================= SCOPE SYSTEM =================
            // Τελειώσαμε με τα μέλη, άρα κλείνουμε το Scope.
            CScopeSystem.GetInstance().ExitScope();
            // ==================================================

            return 0;
        }

        public override int VisitFunctionDefinition(FunctionDefinitionAST node, ParentInfo info)
        {
            // 1. Φτιάχνουμε το ΔΙΚΟ ΜΑΣ Context για να πιάσουμε τον τύπο και το όνομα!
            DeclarationContext declContext = new DeclarationContext(node);

            // 2. Διαβάζουμε τον Τύπο Επιστροφής (π.χ. int, void)
            if (node.MChildren[FunctionDefinitionAST.DECLARATION_SPECIFIERS].Count > 0)
            {
                VisitContext(node, FunctionDefinitionAST.DECLARATION_SPECIFIERS, declContext);
            }

            // 3. Διαβάζουμε το Όνομα της Συνάρτησης
            // Αυτό θα πάει στο VisitIdentifier και (επειδή τώρα στέλνουμε το declContext) θα το αποθηκεύσει σωστά!
            if (node.MChildren[FunctionDefinitionAST.DECLARATOR].Count > 0)
            {
                VisitContext(node, FunctionDefinitionAST.DECLARATOR, declContext);
            }

            // ================= SCOPE SYSTEM =================
            // 4. Ανοίγουμε νέο Scope για τη Συνάρτηση!
            string funcName = declContext.MDeclarator ?? "unknown_function";
            CScopeSystem.GetInstance().EnterScope(ScopeType.Function, funcName);
            // ==================================================

            // 5. Διαβάζουμε τις Παραμέτρους (Arguments) - Κάθε μία έχει το δικό της context
            if (node.MChildren[FunctionDefinitionAST.PARAMETER_DECLARATIONS].Count > 0)
            {
                foreach (ASTElement paramNode in node.MChildren[FunctionDefinitionAST.PARAMETER_DECLARATIONS])
                {
                    DeclarationContext paramContext = new DeclarationContext((ASTComposite)paramNode);
                    Visit(paramNode, paramContext);
                }
            }

            // 6. Διαβάζουμε το Σώμα της Συνάρτησης (π.χ. τα { ... })
            if (node.MChildren[FunctionDefinitionAST.FUNCTION_BODY].Count > 0)
            {
                VisitContext(node, FunctionDefinitionAST.FUNCTION_BODY, info);
            }

            // ================= SCOPE SYSTEM =================
            // 7. Κλείνουμε το Scope της Συνάρτησης!
            CScopeSystem.GetInstance().ExitScope();
            // ==================================================

            // ================= SYMBOL TABLE (ΓΙΑ ΣΥΝΑΡΤΗΣΕΙΣ) =================
            funcName = declContext.MDeclarator ?? "unknown_function";
            if (funcName != "unknown_function")
            {
                // Φτιάχνουμε το σύμβολο και του λέμε ότι ο τύπος του είναι Function!
                Symbol funcSymbol = new Symbol(funcName, Symbol.SymbolType.Function, node);

                // SOS: Το βάζουμε στο ΕΞΩΤΕΡΙΚΟ scope (π.χ. στο File Scope) 
                // (Αν έχεις κλείσει το Scope της συνάρτησης, τώρα βρισκόμαστε στο σωστό, εξωτερικό)
                CScopeSystem.GetInstance().AddSymbol(Namespace.Ordinary, funcName, funcSymbol);
            }
            // =================================================================

            return 0;
        }

        public override int VisitFunctionType(FunctionTypeAST node, ParentInfo info)
        {
            DeclarationContext declContext = info as DeclarationContext;
            if (declContext == null) return 0;

            // 1. Δημιουργούμε τον τύπο της συνάρτησης
            FunctionType funcType = new FunctionType();

            // 2. Τον συνδέουμε με τον τύπο επιστροφής (π.χ. int)
            if (declContext.MParent != null)
            {
                declContext.MParent.AddTypeParameter(funcType);
            }
            else
            {
                declContext.MTypeRoot = funcType;
            }
            declContext.MParent = funcType;

            // 3. Διαβάζουμε το όνομα της συνάρτησης (π.χ. myFunc)
            if (node.MChildren[FunctionTypeAST.FUNCTION_NAME].Count > 0)
            {
                VisitContext(node, FunctionTypeAST.FUNCTION_NAME, declContext);
            }

            // ================= SCOPE SYSTEM =================
            // Ανοίγουμε νέο Scope για τις παραμέτρους (Function Prototype)
            CScopeSystem.GetInstance().EnterScope(ScopeType.FunctionPrototype, declContext.MDeclarator);
            // ==================================================

            // 4. Διαβάζουμε τις παραμέτρους (Arguments)
            if (node.MChildren[FunctionTypeAST.FUNCTION_PARAMETERS].Count > 0)
            {
                foreach (ASTElement paramNode in node.MChildren[FunctionTypeAST.FUNCTION_PARAMETERS])
                {

                    // Δίνουμε σε ΚΑΘΕ παράμετρο το δικό της, καθαρό Context!
                    DeclarationContext paramContext = new DeclarationContext((ASTComposite)paramNode);
                    Visit(paramNode, paramContext);

                    // Παίρνουμε τον τελικό τύπο της παραμέτρου
                    CType finalParamType = paramContext.MTypeRoot ?? paramContext.MTypeSpecifier;

                    if (finalParamType != null)
                    {
                        // Τον προσθέτουμε στη λίστα παραμέτρων της συνάρτησης
                        funcType.AddParameterType(finalParamType);
                    }
                }
            }

            // ================= SCOPE SYSTEM =================
            CScopeSystem.GetInstance().ExitScope();
            // ==================================================

            return 0;
        }

        public override int VisitParameterDeclaration(ParameterDeclarationAST node, ParentInfo info)
        {
            DeclarationContext declContext = info as DeclarationContext;

            // 1. Διαβάζουμε τον βασικό τύπο της παραμέτρου (π.χ. int)
            VisitContext(node, ParameterDeclarationAST.TYPE_SPECIFIER, declContext);

            // 2. Διαβάζουμε τον declarator (π.χ. αν είναι δείκτης *a)
            if (node.MChildren[ParameterDeclarationAST.DECLARATOR].Count > 0)
            {
                VisitContext(node, ParameterDeclarationAST.DECLARATOR, declContext);
            }

            // Αν δεν υπήρχαν pointers, η ρίζα είναι απλά ο βασικός τύπος
            if (declContext.MTypeRoot == null)
            {
                declContext.MTypeRoot = declContext.MTypeSpecifier;
            }

            // ================= SYMBOL TABLE (ΓΙΑ ΠΑΡΑΜΕΤΡΟΥΣ) =================
            if (!string.IsNullOrEmpty(declContext.MDeclarator))
            {
                Symbol paramSymbol = new Symbol(declContext.MDeclarator, Symbol.SymbolType.Variable, node);
                CScopeSystem.GetInstance().AddSymbol(Namespace.Ordinary,declContext.MDeclarator, paramSymbol);
            }
            // =================================================================

            return 0;
        }



    }
}
