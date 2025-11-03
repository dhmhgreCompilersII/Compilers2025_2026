using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CParser{

    public abstract class ASTElement{
        private uint m_type; // type of AST element ( addition, multiplication, etc)
        private string m_name; // for use in debugging
        ASTElement? m_parent; // parent of this AST element

        private uint m_serialNumber; // unique serial number of this AST element to distinguish it

        // from other AST elements of the same type
        private static uint m_serialNumberCounter = 0; // static counter to generate unique serial numbers

        public ASTElement(uint type, string name) {
            m_type = type;
            m_name = name;
            m_serialNumber = m_serialNumberCounter++;
        }

        public abstract Result Accept<Result>(BaseASTVisitor<Result> visitor);

    }

    public abstract class ASTComposite : ASTElement{
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
    }

    public abstract class ASTLeaf : ASTElement{
        private string m_lexeme;

        public ASTLeaf(string lexeme, uint type, string name) :
            base(type, name) {
            m_lexeme = lexeme;
        }
    }


    public class TranslationUnitAST : ASTComposite{

        public const int FUNCTION_DEFINITION_CONTEXT = 0, DECLARATIONS = 1;

        public TranslationUnitAST() :
            base(2, 0, "TranslationUnitAST") {
        }

        public override Result Accept<Result>(BaseASTVisitor<Result> visitor) {
            return visitor.VisitTranslationUnit(this);
        }
        
    }
}