using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;

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

        public abstract uint GetContextForChild(ParserRuleContext child);
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

        public override uint GetContextForChild(ParserRuleContext child) {

            switch (child.RuleIndex) {
                case CGrammarParser.RULE_declaration:
                    return DECLARATIONS;
                    break;
                case CGrammarParser.RULE_function_definition:
                    return FUNCTION_DEFINITION;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("child", "Unknown child rule index");
            }
        }

        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            return visitor.VisitTranslationUnit(this, info);
        }

    }

    public class DeclarationAST : ASTComposite {
        public const int DECLARATION_STORAGE_CLASS = 0,
            DECLARATION_TYPE = 1, DECLARATORS = 2;
        public DeclarationAST() :
            base(3, 1, "DeclarationAST") {
        }

        public override uint GetContextForChild(ParserRuleContext child) {
            throw new NotImplementedException();
        }

        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            return visitor.VisitDeclaration(this, info);
        }
    }

    public class FunctionDefinitionAST : ASTComposite {
        public const int DECLARATION_SPECIFIERS = 0,
            DECLARATOR = 1, PARAMETER_DECLARATIONS = 3, FUNCTION_BODY = 4;
        public FunctionDefinitionAST() :
            base(4, 2, "FunctionDefinitionAST") {
        }

        public override uint GetContextForChild(ParserRuleContext child) {
            throw new NotImplementedException();
        }
        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            return visitor.VisitFunctionDefinition(this, info);
        }
    }
}