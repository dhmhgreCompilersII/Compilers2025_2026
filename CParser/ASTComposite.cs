using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace CParser {

    public abstract class ASTElement {
        private uint m_type; // type of AST element ( addition, multiplication, etc)
        private string m_name; // for use in debugging
        ASTElement? m_parent; // parent of this AST element
        public uint MType => m_type;

        public string MName => m_name;

        public ASTElement? MParent => m_parent;

        private uint m_serialNumber; // unique serial number of this AST element to distinguish it

        // from other AST elements of the same type
        private static uint m_serialNumberCounter = 0; // static counter to generate unique serial numbers

        public ASTElement(uint type, string name) {
            m_type = type;
            m_serialNumber = m_serialNumberCounter++;
            m_name = name + $"_{m_serialNumberCounter}";
        }

        public abstract Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO));

    }

    public abstract class ASTComposite : ASTElement {
        List<ASTElement>[] m_children; // children of this AST element
        private uint m_contexts;

        public List<ASTElement>[] MChildren {
            get => m_children;
        }
        public uint MContexts {
            get => m_contexts;
        }

        public ASTComposite(uint numcontexts, uint type, string name) :
            base(type, name) {
            m_children = new List<ASTElement>[numcontexts]; // assume max 10 types of children
            for (int i = 0; i < numcontexts; i++) {
                m_children[i] = new List<ASTElement>();
            }

            m_contexts = numcontexts;
        }

        public void AddChild(ASTElement child, uint context) {
            if (context >= m_contexts) {
                throw new ArgumentOutOfRangeException("context", "Context index out of range");
            }
            m_children[context].Add(child);
        }

        public abstract uint GetContextForChild(IParseTree child);
    }

    public abstract class ASTLeaf : ASTElement {
        private string m_lexeme;

        public ASTLeaf(string lexeme, uint type, string name) :
            base(type, name) {
            m_lexeme = lexeme;
        }
    }


    public class TranslationUnitAST : ASTComposite {

        public const int FUNCTION_DEFINITION = 0, DECLARATIONS = 1;

        public TranslationUnitAST() :
            base(2, 0, "TranslationUnitAST") {
        }

        public override uint GetContextForChild(IParseTree child) {

            ParserRuleContext prc = child as ParserRuleContext;
            if (prc != null) {
                switch (prc.RuleIndex) {
                    case CGrammarParser.RULE_declaration:
                        return DECLARATIONS;
                        break;
                    case CGrammarParser.RULE_function_definition:
                        return FUNCTION_DEFINITION;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("child", "Unknown child rule index");
                }
            } else {
                throw new ArgumentOutOfRangeException("child", "Child is not a ParserRuleContext");
            }

        }

        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            return visitor.VisitTranslationUnit(this, info);
        }

    }

    public class DeclarationAST : ASTComposite {
        public const int DECLARATION_STORAGE_CLASS = 0,
            DECLARATION_TYPE = 1, DECLARATORS = 2;

        public enum TYPE {
            VOID,
            CHAR,
            SHORT,
            INT,
            LONG,
            FLOAT,
            DOUBLE,
            SIGNED,
            UNSIGNED,
            STRUCT,
            UNION,
            ENUM
        }

        public enum STORAGE_CLASS {
            STATIC,
            EXTERN,
            AUTO,
            REGISTER
        }

        public enum TYPE_QUALIFIER {
            CONST,
            RESTRICT,
            VOLATILE,
            ATOMIC
        }

        private TYPE m_type;
        private STORAGE_CLASS m_class;
        private TYPE_QUALIFIER m_qualifier;

        public TYPE MType1 {
            get => m_type;
            internal set => m_type = value;
        }

        public STORAGE_CLASS MClass {
            get => m_class;
            internal set => m_class = value;
        }

        public TYPE_QUALIFIER MQualifier {
            get => m_qualifier;
            internal set => m_qualifier = value;
        }

        public DeclarationAST() :
            base(3, 1, "DeclarationAST") {
        }

        public override uint GetContextForChild(IParseTree child) {
            ParserRuleContext prc = child as ParserRuleContext;
            ITerminalNode ttn = child as ITerminalNode;
            if (prc != null) {
                switch (prc.RuleIndex) {
                    case CGrammarParser.RULE_type_specifier:
                        return DECLARATION_TYPE;
                        break;
                    case CGrammarParser.RULE_pointer:
                    case CGrammarParser.RULE_direct_declarator:
                        return DECLARATORS;
                        break;
                    case CGrammarParser.RULE_storage_class_specifier:
                        return DECLARATION_STORAGE_CLASS;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("child", "Unknown child rule index");
                }
            } else if (ttn != null) {
                switch (ttn.Symbol.Type) {
                    case CGrammarLexer.IDENTIFIER:
                        return DECLARATORS;
                    case CGrammarLexer.INT:
                        return DECLARATION_TYPE;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("child", "Unknown child terminal type");
                }
            } else {
                throw new ArgumentOutOfRangeException("child", "Child is not a ParserRuleContext");
            }
        }

        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            return visitor.VisitDeclaration(this, info);
        }
    }

    public class IntegerTypeAST : ASTLeaf {
        public IntegerTypeAST(string lexeme) :
            base(lexeme, 7, lexeme) {
        }
        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            return visitor.VisitIntegerType(this, info);
        }
    }


    public class PointerTypeAST : ASTComposite {
        public const int POINTER_TARGER = 0;
        public enum QUALIFIER {
            CONST,
            RESTRICT,
            VOLATILE,
            ATOMIC
        }
        public PointerTypeAST() :
            base(1, 4, "PointerTypeAST") {
        }
        public override uint GetContextForChild(IParseTree child) {
            ParserRuleContext prc = child as ParserRuleContext;
            ITerminalNode ttn = child as ITerminalNode;
            if (prc != null) {
                switch (prc.RuleIndex) {
                    case CGrammarParser.RULE_pointer:
                    case CGrammarParser.RULE_direct_declarator:
                        return POINTER_TARGER;
                    default:
                        throw new ArgumentOutOfRangeException("child", "Unknown child rule index");
                }
            } else if (ttn != null) {
                switch (ttn.Symbol.Type) {
                    case CGrammarLexer.IDENTIFIER:
                        return POINTER_TARGER;
                    default:
                        throw new ArgumentOutOfRangeException("child", "Unknown child terminal type");
                }
            } else {
                throw new ArgumentOutOfRangeException("child", "Child is not a ParserRuleContext");
            }
        }
        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            return visitor.VisitPointerType(this, info);
        }
    }

    public class ParameterDeclarationAST : ASTComposite{
        public const int DECLARATION_SPECIFIERS = 0, DECLARATOR = 1;
        public ParameterDeclarationAST() :
            base(2, 8, "ParameterDeclaration") {
        }
        public override uint GetContextForChild(IParseTree child) {
            ParserRuleContext prc = child as ParserRuleContext;
            ITerminalNode ttn = child as ITerminalNode;
            if (ttn != null) {
                switch (ttn.Symbol.Type) {
                    case CGrammarLexer.INT:
                        return DECLARATION_SPECIFIERS;
                    case CGrammarLexer.IDENTIFIER:
                        return DECLARATOR;
                    default:
                        throw new ArgumentOutOfRangeException("child", "Unknown child terminal type");

                }
            } else if (prc != null) {
                switch (prc.RuleIndex) {
                    case CGrammarParser.RULE_pointer:
                    case CGrammarParser.RULE_direct_declarator:
                        return DECLARATOR;
                    default:
                        throw new ArgumentOutOfRangeException("child", "Unknown child rule index");
                }
            } else {
                throw new ArgumentOutOfRangeException("child", "Child is not a ParserRuleContext");
            }

        }

        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            return visitor.VisitParameterDeclaration(this, info);
        }
    }

    public class FunctionTypeAST : ASTComposite {
        public const int FUNCTION_TYPE = 0, FUNCTION_NAME = 1, FUNCTION_PARAMETERS = 2;


        public FunctionTypeAST() :
            base(3, 5, "FunctionTypeAST") {
        }
        public override uint GetContextForChild(IParseTree child) {
            ParserRuleContext prc = child as ParserRuleContext;
            ITerminalNode ttn = child as ITerminalNode;
            if (prc != null) {
                switch (prc.RuleIndex) {
                    case CGrammarParser.RULE_pointer:
                        return FUNCTION_TYPE;
                    case CGrammarParser.RULE_parameter_declaration:
                        return FUNCTION_PARAMETERS;
                    default:
                        throw new ArgumentOutOfRangeException("child", "Unknown child rule index");
                }
            } else if (ttn != null) {
                switch (ttn.Symbol.Type) {
                    case CGrammarLexer.IDENTIFIER:
                        return FUNCTION_NAME;
                    default:
                        throw new ArgumentOutOfRangeException("child", "Unknown child terminal type");
                }
            } else {
                throw new ArgumentOutOfRangeException("child", "Child is not a ParserRuleContext");


            }
        }
        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            return visitor.VisitFunctionType(this, info);
        }
    }


    public class FunctionDefinitionAST : ASTComposite {
        public const int DECLARATION_SPECIFIERS = 0,
            DECLARATOR = 1, PARAMETER_DECLARATIONS = 3, FUNCTION_BODY = 4;
        public FunctionDefinitionAST() :
            base(4, 2, "FunctionDefinitionAST") {
        }

        public override uint GetContextForChild(IParseTree child) {
            throw new NotImplementedException();
        }
        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            return visitor.VisitFunctionDefinition(this, info);
        }
    }



    public class IDENTIFIER : ASTLeaf {
        public IDENTIFIER(string lexeme) :
            base(lexeme, 6, lexeme) {
        }
        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            return visitor.VisitIdentifier(this, info);
        }
    }
}