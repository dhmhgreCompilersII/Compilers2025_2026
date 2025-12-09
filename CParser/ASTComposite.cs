using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;
using CParser;

namespace CParser {

    public abstract class ASTElement {
        private uint m_type; // type of AST element ( addition, multiplication, etc)
        private string m_name; // for use in debugging
        ASTElement? m_parent; // parent of this AST element
        public uint MType => m_type;

        public string MName => m_name;

        public ASTElement? MParent => m_parent;

        private uint m_serialNumber; // unique serial number of this AST element to distinguish it

        public T GetActualNodeType<T>(uint t) where T : Enum {
            // This will throw if the value is not a valid enum value, but the cast itself is fine
            return (T)Enum.ToObject(typeof(T), t);
        }

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

        public virtual uint GetContextForChild(IParseTree child) {
            return child switch {
                ParserRuleContext prc => GetContextForParserRuleContextChild(prc),
                ITerminalNode ttn => GetContextForTerminalNodeChild(ttn),
                _ => throw new NotImplementedException("GetContextForChild not implemented for this child type")
            };
        }

        protected abstract uint GetContextForParserRuleContextChild(ParserRuleContext parentContext);
        protected abstract uint GetContextForTerminalNodeChild(ITerminalNode ttn);


    }

    public abstract class ASTLeaf : ASTElement {
        private string m_lexeme;

        public ASTLeaf(string lexeme, uint type, string name) :
            base(type, name) {
            m_lexeme = lexeme;
        }
    }


    public class TranslationUnitAST : ASTComposite {

        public enum NodeTypes {
            TRANSLATION_UNIT = 0, DECLARATION = 1, FUNCTION_DEFINITION = 2,
            COMPOUNDSTATEMENT = 3, POINTER_TYPE = 4, FUNCTION_TYPE = 5,
            IDENTIFIER = 6, INTEGER_TYPE = 7, PARAMETER_DECLARATION = 8,
            EXPRESSION_STATEMENT = 9, EXPRESSION_IDENTIFIER = 10,
            EXPRESSION_ASSIGNMENT = 11, EXPRESSION_NUMBER = 12,
            EXPRESSION_ADDITION = 13,
            EXPRESSION_STRINGLITERAL = 14,
            EXPRESSION_MULTIPLICATION = 15, EXPRESSION_SUBTRACTION = 16,
            EXPRESSION_DIVISION = 17, EXPRESSION_MODULO = 18,
            EXPRESSION_EQUALITY_EQUAL = 19, EXPRESSION_EQUALITY_NOTEQUAL = 20,
            EXPRESSION_BITWISE_AND = 21, EXPRESSION_BITWISE_OR = 22, EXPRESSION_BITWISE_XOR = 23,

            POSTFIX_EXPRESSION_ARRAYSUBSCRIPT = 24, POSTFIX_EXPRESSION_FUNCTIONCALLNOARGS = 25,
            POSTFIX_EXPRESSION_FUNCTIONCALLWITHARGS = 26, POSTFIX_EXPRESSION_MEMBERACCESS = 27,
            POSTFIX_EXPRESSION_POINTERMEMBERACCESS = 28, POSTFIX_EXPRESSION_INCREMENT = 29,
            POSTFIX_EXPRESSION_DECREMENT = 30, EXPRESSION_COMMAEXPRESSION = 31,

            UNARY_EXPRESSION_INCREMENT = 32,
            UNARY_EXPRESSION_DECREMENT = 33,
            UNARY_EXPRESSION_UNARY_OPERATOR_AMBERSAND = 34,
            UNARY_EXPRESSION_UNARY_OPERATOR_ASTERISK = 35,
            UNARY_EXPRESSION_UNARY_OPERATOR_PLUS = 36,
            UNARY_EXPRESSION_UNARY_OPERATOR_HYPHEN = 37,
            UNARY_EXPRESSION_UNARY_OPERATOR_TILDE = 38,
            UNARY_EXPRESSION_UNARY_OPERATOR_NOT = 39,


            UNARY_EXPRESSION_SIZEOF = 40, UNARY_EXPRESSION_SIZEOF_TYPE = 41,
            EXPRESSION_RELATIONAL_SHIFT = 42, EXPRESSION_RELATIONAL_LESS = 43,
            EXPRESSION_RELATIONAL_GREATER = 44, EXPRESSION_RELATIONAL_LESS_OR_EQUAL = 45,
            EXPRESSION_RELATIONAL_GREATER_OR_EQUAL = 46, EXPRESSION_LOGICAL_AND_INCLUSIVE_OR = 47,
            EXPRESSION_LOGICAL_AND = 48, EXPRESSION_LOGICAL_OR_INCLUSIVE_OR = 49,
            EXPRESSION_LOGICAL_OR = 50, CONDITIONAL_EXPRESSION_OR = 51,
            CONDITIONAL_EXPRESSION = 52, ASSIGNMENT_EXPRESSION_CONDITIONAL = 53,
            ASSIGNMENT_EXPRESSION = 54,

            CHAR_TYPE = 55,
            UNARY_EXPRESSION_CAST = 56,
            EXPRESSION_ASSIGNMENT_MULTIPLICATION = 57, EXPRESSION_ASSIGNMENT_DIVISION = 58,
            EXPRESSION_ASSIGNMENT_MODULO = 59, EXPRESSION_ASSIGNMENT_ADDITION = 60,
            EXPRESSION_ASSIGNMENT_SUBTRACTION = 61,
            EXPRESSION_ASSIGNMENT_LEFT = 62,
            EXPRESSION_ASSIGNMENT_RIGHT = 63,
            EXPRESSION_ASSIGNMENT_AND = 64,
            EXPRESSION_ASSIGNMENT_XOR = 65,
            EXPRESSION_ASSIGNMENT_OR = 66,

            STATEMENT_EXPRESSION = 67,
            INTEGER = 68


        }


        public const int FUNCTION_DEFINITION = 0, DECLARATIONS = 1;

        public TranslationUnitAST() :
            base(2, (uint)TranslationUnitAST.NodeTypes.TRANSLATION_UNIT, "TranslationUnitAST") {
        }

        protected override uint GetContextForParserRuleContextChild(ParserRuleContext prc) {
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
        }


        protected override uint GetContextForTerminalNodeChild(ITerminalNode ttn) {
            throw new ArgumentOutOfRangeException("child", "Terminal nodes are not expected as direct children of TranslationUnitAST");
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
            base(3, (uint)TranslationUnitAST.NodeTypes.DECLARATION, "DeclarationAST") {
        }

        protected override uint GetContextForParserRuleContextChild(ParserRuleContext prc) {
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
        }

        protected override uint GetContextForTerminalNodeChild(ITerminalNode ttn) {
            switch (ttn.Symbol.Type) {
                case CGrammarLexer.IDENTIFIER:
                    return DECLARATORS;
                case CGrammarLexer.INT:
                    return DECLARATION_TYPE;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("child", "Unknown child terminal type");
            }
        }

        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            return visitor.VisitDeclaration(this, info);
        }
    }

    public class IntegerTypeAST : ASTLeaf {
        public IntegerTypeAST(string lexeme) :
            base(lexeme, (uint)TranslationUnitAST.NodeTypes.INTEGER_TYPE, lexeme) {
        }
        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            return visitor.VisitIntegerType(this, info);
        }
    }

    public class CharTypeAST : ASTLeaf {
        public CharTypeAST(string lexeme) :
            base(lexeme, (uint)TranslationUnitAST.NodeTypes.CHAR_TYPE, lexeme) {
        }
        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            return visitor.VisitCharType(this, info);
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
            base(1, (uint)TranslationUnitAST.NodeTypes.POINTER_TYPE, "PointerTypeAST") {
        }

        protected override uint GetContextForParserRuleContextChild(ParserRuleContext prc) {
            switch (prc.RuleIndex) {
                case CGrammarParser.RULE_pointer:
                case CGrammarParser.RULE_direct_declarator:
                    return POINTER_TARGER;
                default:
                    throw new ArgumentOutOfRangeException("child", "Unknown child rule index");
            }
        }

        protected override uint GetContextForTerminalNodeChild(ITerminalNode ttn) {
            switch (ttn.Symbol.Type) {
                case CGrammarLexer.IDENTIFIER:
                    return POINTER_TARGER;
                default:
                    throw new ArgumentOutOfRangeException("child", "Unknown child terminal type");
            }
        }

        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            return visitor.VisitPointerType(this, info);
        }
    }

    public class ParameterDeclarationAST : ASTComposite {
        public const int DECLARATION_SPECIFIERS = 0, DECLARATOR = 1;
        public ParameterDeclarationAST() :
            base(2, (uint)TranslationUnitAST.NodeTypes.PARAMETER_DECLARATION, "ParameterDeclaration") {
        }

        protected override uint GetContextForParserRuleContextChild(ParserRuleContext prc) {
            switch (prc.RuleIndex) {
                case CGrammarParser.RULE_pointer:
                case CGrammarParser.RULE_direct_declarator:
                    return DECLARATOR;
                default:
                    throw new ArgumentOutOfRangeException("child", "Unknown child rule index");
            }
        }

        protected override uint GetContextForTerminalNodeChild(ITerminalNode ttn) {
            switch (ttn.Symbol.Type) {
                case CGrammarLexer.INT:
                case CGrammarLexer.CHAR:
                    return DECLARATION_SPECIFIERS;
                case CGrammarLexer.IDENTIFIER:
                    return DECLARATOR;
                default:
                    throw new ArgumentOutOfRangeException("child", "Unknown child terminal type");

            }
        }

        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            return visitor.VisitParameterDeclaration(this, info);
        }
    }

    public class FunctionTypeAST : ASTComposite {
        public const int FUNCTION_TYPE = 0, FUNCTION_NAME = 1, FUNCTION_PARAMETERS = 2;


        public FunctionTypeAST() :
            base(3, (uint)TranslationUnitAST.NodeTypes.FUNCTION_TYPE, "FunctionTypeAST") {
        }

        protected override uint GetContextForParserRuleContextChild(ParserRuleContext prc) {
            switch (prc.RuleIndex) {
                case CGrammarParser.RULE_pointer:
                    return FUNCTION_TYPE;
                case CGrammarParser.RULE_parameter_declaration:
                    return FUNCTION_PARAMETERS;
                default:
                    throw new ArgumentOutOfRangeException("child", "Unknown child rule index");
            }
        }

        protected override uint GetContextForTerminalNodeChild(ITerminalNode ttn) {
            switch (ttn.Symbol.Type) {
                case CGrammarLexer.IDENTIFIER:
                    return FUNCTION_NAME;
                default:
                    throw new ArgumentOutOfRangeException("child", "Unknown child terminal type");
            }
        }

        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            return visitor.VisitFunctionType(this, info);
        }
    }


    public class FunctionDefinitionAST : ASTComposite {
        public const int DECLARATION_SPECIFIERS = 0,
            DECLARATOR = 1, PARAMETER_DECLARATIONS = 2, FUNCTION_BODY = 3;
        public FunctionDefinitionAST() :
            base(4, (uint)TranslationUnitAST.NodeTypes.FUNCTION_DEFINITION, "FunctionDefinitionAST") {
        }

        protected override uint GetContextForParserRuleContextChild(ParserRuleContext prc) {
            switch (prc.RuleIndex) {
                case CGrammarParser.RULE_pointer:
                case CGrammarParser.RULE_direct_declarator:
                    return DECLARATOR;
                case CGrammarParser.RULE_parameter_declaration:
                    return PARAMETER_DECLARATIONS;
                case CGrammarParser.RULE_compound_statement:
                    return FUNCTION_BODY;
                default:
                    throw new ArgumentOutOfRangeException("child", "Unknown child rule index");
            }
        }

        protected override uint GetContextForTerminalNodeChild(ITerminalNode ttn) {
            switch (ttn.Symbol.Type) {
                case CGrammarLexer.IDENTIFIER:
                    return DECLARATOR;
                case CGrammarLexer.INT:
                    return DECLARATION_SPECIFIERS;
                default:
                    throw new ArgumentOutOfRangeException("child", "Unknown child terminal type");
            }
        }

        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            return visitor.VisitFunctionDefinition(this, info);
        }
    }


    public abstract class CExpression : ASTComposite {
        public CExpression(uint numcontexts, uint type, string name) :
            base(numcontexts, type, name) {
        }
    }

    public abstract class CStatement : ASTComposite {
        public CStatement(uint numcontexts, uint type, string name) :
            base(numcontexts, type, name) {
        }
    }

    public class Expression_Identifier : CExpression {

        public const int IDENTIFIER = 0;

        public Expression_Identifier() : base(1,
            (uint)TranslationUnitAST.NodeTypes.EXPRESSION_IDENTIFIER, "Expression_Identifier") {
        }

        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            return visitor.VisitExpressionIdentifier(this, info);
        }

        protected override uint GetContextForParserRuleContextChild(ParserRuleContext prc) {
            throw new InvalidOperationException($"{MName} does not have non-leaf children");
        }

        protected override uint GetContextForTerminalNodeChild(ITerminalNode ttn) {
            if (ttn.Symbol.Type == CGrammarParser.IDENTIFIER) {
                return IDENTIFIER;
            } else {
                throw new ArgumentOutOfRangeException("child", "Unknown child terminal type");
            }
        }
    }

    public class Expression_Number : CExpression {
        public const int NUMBER = 0;
        public Expression_Number(uint numcontexts, uint type, string name) :
            base(1, (uint)TranslationUnitAST.NodeTypes.EXPRESSION_NUMBER, name) {
        }

        protected override uint GetContextForParserRuleContextChild(ParserRuleContext prc) {
            throw new NotImplementedException();
        }

        protected override uint GetContextForTerminalNodeChild(ITerminalNode ttn) {
            switch (ttn.Symbol.Type) {
                case CGrammarLexer.INT:
                    return NUMBER;
                default:
                    throw new NotImplementedException();
            }
        }

        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            return visitor.VisitExpressionNumber(this, info);
        }
    }

    public class Expression_StringLiteral : CExpression {
        public const int STRING = 0;
        public Expression_StringLiteral() : base(1,
            (uint)TranslationUnitAST.NodeTypes.EXPRESSION_STRINGLITERAL, "StringLiteral") {
        }
        protected override uint GetContextForParserRuleContextChild(ParserRuleContext prc) {
            throw new Exception("Expression Indentifier does not have non-leaf children");
        }
        protected override uint GetContextForTerminalNodeChild(ITerminalNode ttn) {
            return STRING;
        }
        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            return visitor.VisitExpressionStringLiteral(this, info);
        }
    }

    public class Expression_Assignment : CExpression {
        public const int LEFT = 0, RIGHT = 1;

        public Expression_Assignment() : base(2,
            (uint)TranslationUnitAST.NodeTypes.EXPRESSION_ASSIGNMENT, "Expression_Assignment") {
        }

        protected override uint GetContextForParserRuleContextChild(ParserRuleContext prc) {
            switch (prc.RuleIndex) {
                
                default:
                    throw new NotImplementedException();

            }
        }

        protected override uint GetContextForTerminalNodeChild(ITerminalNode ttn) {
            switch (ttn.Symbol.Type) {
                case CGrammarLexer.IDENTIFIER:
                    return LEFT;
                case CGrammarLexer.CONSTANT:
                    return RIGHT;
                default:
                    throw new NotImplementedException();
            }
        }

        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            return visitor.VisitExpressionAssignment(this, info);
        }
    }

    public class Expression_Addition : CExpression {
        public const int LEFT = 0, RIGHT = 1;

        public Expression_Addition() : base(2,
            (uint)TranslationUnitAST.NodeTypes.EXPRESSION_ADDITION, "Expression_Addition") {
        }

        protected override uint GetContextForParserRuleContextChild(ParserRuleContext prc) {
            throw new NotImplementedException();
        }

        protected override uint GetContextForTerminalNodeChild(ITerminalNode ttn) {
            throw new NotImplementedException();
        }

        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            return visitor.VisitExpressionAddition(this, info);
        }
    }

    public class Expression_Multiplication : CExpression {
        public const int LEFT = 0, RIGHT = 1;

        public Expression_Multiplication() : base(2,
            (uint)TranslationUnitAST.NodeTypes.EXPRESSION_MULTIPLICATION, "Expression_Multiplication") {
        }

        protected override uint GetContextForParserRuleContextChild(ParserRuleContext prc) {
            throw new NotImplementedException();
        }

        protected override uint GetContextForTerminalNodeChild(ITerminalNode ttn) {
            throw new NotImplementedException();
        }

        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            return visitor.VisitExpressionMultiplication(this, info);
        }
    }

    public class Expression_Division : CExpression {
        public const int LEFT = 0, RIGHT = 1;

        public Expression_Division() : base(2,
            (uint)TranslationUnitAST.NodeTypes.EXPRESSION_DIVISION, "Expression_Division") {
        }

        protected override uint GetContextForParserRuleContextChild(ParserRuleContext prc) {
            throw new NotImplementedException();
        }

        protected override uint GetContextForTerminalNodeChild(ITerminalNode ttn) {
            throw new NotImplementedException();
        }

        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            return visitor.VisitExpressionDivision(this, info);
        }
    }

    public class Expression_Modulo : CExpression {
        public const int LEFT = 0, RIGHT = 1;

        public Expression_Modulo() : base(2,
            (uint)TranslationUnitAST.NodeTypes.EXPRESSION_MODULO, "Expression_Modulo") {
        }

        protected override uint GetContextForParserRuleContextChild(ParserRuleContext prc) {
            throw new NotImplementedException();
        }

        protected override uint GetContextForTerminalNodeChild(ITerminalNode ttn) {
            throw new NotImplementedException();
        }

        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            return visitor.VisitExpressionModulo(this, info);
        }
    }

    public class Expression_Subtraction : CExpression {
        public const int LEFT = 0, RIGHT = 1;

        public Expression_Subtraction() : base(2,
            (uint)TranslationUnitAST.NodeTypes.EXPRESSION_SUBTRACTION, "Expression_Subtraction") {
        }

        protected override uint GetContextForParserRuleContextChild(ParserRuleContext prc) {
            throw new NotImplementedException();
        }

        protected override uint GetContextForTerminalNodeChild(ITerminalNode ttn) {
            throw new NotImplementedException();
        }

        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            return visitor.VisitExpressionSubtraction(this, info);
        }
    }

    public class Expression_EqualityEqual : CExpression {
        public const int LEFT = 0, RIGHT = 1;

        public Expression_EqualityEqual() : base(2,
            (uint)TranslationUnitAST.NodeTypes.EXPRESSION_EQUALITY_EQUAL, "Expression_Equality_Equal") {
        }

        protected override uint GetContextForParserRuleContextChild(ParserRuleContext prc) {
            throw new NotImplementedException();
        }

        protected override uint GetContextForTerminalNodeChild(ITerminalNode ttn) {
            throw new NotImplementedException();
        }

        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            return visitor.VisitExpressionEqualityEqual(this, info);
        }
    }

    public class Expression_EqualityNotEqual : CExpression {
        public const int LEFT = 0, RIGHT = 1;

        public Expression_EqualityNotEqual() : base(2,
            (uint)TranslationUnitAST.NodeTypes.EXPRESSION_EQUALITY_NOTEQUAL, "Expression_Equality_NotEqual") {
        }

        protected override uint GetContextForParserRuleContextChild(ParserRuleContext prc) {
            throw new NotImplementedException();
        }

        protected override uint GetContextForTerminalNodeChild(ITerminalNode ttn) {
            throw new NotImplementedException();
        }

        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            return visitor.VisitExpressionEqualityNotEqual(this, info);
        }
    }

    public class Expression_BitwiseAND : CExpression {
        public const int LEFT = 0, RIGHT = 1;

        public Expression_BitwiseAND() : base(2,
            (uint)TranslationUnitAST.NodeTypes.EXPRESSION_BITWISE_AND, "Expression_Bitwise_AND") {
        }

        protected override uint GetContextForParserRuleContextChild(ParserRuleContext prc) {
            throw new NotImplementedException();
        }

        protected override uint GetContextForTerminalNodeChild(ITerminalNode ttn) {
            throw new NotImplementedException();
        }

        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            return visitor.VisitExpressionBitwiseAND(this, info);
        }
    }

    public class Expression_BitwiseOR : CExpression {
        public const int LEFT = 0, RIGHT = 1;

        public Expression_BitwiseOR() : base(2,
            (uint)TranslationUnitAST.NodeTypes.EXPRESSION_BITWISE_OR, "Expression_Bitwise_OR") {
        }

        protected override uint GetContextForParserRuleContextChild(ParserRuleContext prc) {
            throw new NotImplementedException();
        }

        protected override uint GetContextForTerminalNodeChild(ITerminalNode ttn) {
            throw new NotImplementedException();
        }

        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            return visitor.VisitExpressionBitwiseOR(this, info);
        }
    }


    public class Expression_BitwiseXOR : CExpression {
        public const int LEFT = 0, RIGHT = 1;

        public Expression_BitwiseXOR() : base(2,
            (uint)TranslationUnitAST.NodeTypes.EXPRESSION_BITWISE_XOR, "Expression_Bitwise_XOR") {
        }

        protected override uint GetContextForParserRuleContextChild(ParserRuleContext prc) {
            throw new NotImplementedException();
        }

        protected override uint GetContextForTerminalNodeChild(ITerminalNode ttn) {
            throw new NotImplementedException();
        }

        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            return visitor.VisitExpressionBitwiseXOR(this, info);
        }
    }





    public abstract class Statement : ASTComposite {
        public Statement(uint numcontexts, uint type, string name) :
            base(numcontexts, type, name) {
        }
    }

    public class CompoundStatement : Statement {
        public const int STATEMENTS = 0, DECLARATIONS = 1;
        public CompoundStatement() : base(2,
            (uint)TranslationUnitAST.NodeTypes.COMPOUNDSTATEMENT, "CompoundStatement") {
        }

        protected override uint GetContextForParserRuleContextChild(ParserRuleContext parentContext) {
            switch (parentContext.RuleIndex) {
                case CGrammarParser.RULE_compound_statement:
                case CGrammarParser.RULE_expression_statement:
                    return STATEMENTS;
                case CGrammarParser.RULE_declaration:
                    return DECLARATIONS;
                default:
                    throw new ArgumentOutOfRangeException("child", "Unknown child rule index");
            }
        }

        protected override uint GetContextForTerminalNodeChild(ITerminalNode ttn) {
            throw new ArgumentOutOfRangeException("child", "CompoundStatement child cannot be terminal");
        }

        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            return visitor.VisitCompoundStatement(this, info);
        }
    }

    public class ExpressionStatement : Statement {
        public const int EXPRESSION = 0;
        public ExpressionStatement() : base(1,
            (uint)TranslationUnitAST.NodeTypes.EXPRESSION_STATEMENT, "ExpressionStatement") {
        }

        protected override uint GetContextForParserRuleContextChild(ParserRuleContext prc) {
            return EXPRESSION;
        }

        protected override uint GetContextForTerminalNodeChild(ITerminalNode ttn) {
            return EXPRESSION;
        }

        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            return visitor.VisitExpressionStatement(this, info);
        }
    }

    public class UnaryExpressionIncrement : CExpression {
        public UnaryExpressionIncrement() : base(2, (uint)TranslationUnitAST.NodeTypes.UNARY_EXPRESSION_INCREMENT, "UnaryExpressionIncrement") {

        }

        protected override uint GetContextForParserRuleContextChild(ParserRuleContext prc) {
            throw new NotImplementedException();
        }

        protected override uint GetContextForTerminalNodeChild(ITerminalNode ttn) {
            throw new NotImplementedException();
        }

        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            throw new NotImplementedException();
        }
    }

    public class UnaryExpressionDecrement : CExpression {
        public UnaryExpressionDecrement() : base(2, (uint)TranslationUnitAST.NodeTypes.UNARY_EXPRESSION_DECREMENT, "UnaryExpressionDecrement") {
        }
        protected override uint GetContextForParserRuleContextChild(ParserRuleContext prc) {
            throw new NotImplementedException();
        }
        protected override uint GetContextForTerminalNodeChild(ITerminalNode ttn) {
            throw new NotImplementedException();
        }
        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            throw new NotImplementedException();
        }
    }

    public class UnaryExpressionUnaryOperatorAmbersand : CExpression {

        public UnaryExpressionUnaryOperatorAmbersand() :
            base(2,
                (uint)TranslationUnitAST.NodeTypes.UNARY_EXPRESSION_UNARY_OPERATOR_AMBERSAND,
                "UnaryExpressionUnaryOperatorAmbersand") {
        }
        protected override uint GetContextForParserRuleContextChild(ParserRuleContext prc) {
            throw new NotImplementedException();
        }
        protected override uint GetContextForTerminalNodeChild(ITerminalNode ttn) {
            throw new NotImplementedException();
        }
        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            throw new NotImplementedException();
        }
    }

    public class UnaryExpressionUnaryOperatorAsterisk : CExpression {

        public UnaryExpressionUnaryOperatorAsterisk() :
            base(2,
                (uint)TranslationUnitAST.NodeTypes.UNARY_EXPRESSION_UNARY_OPERATOR_ASTERISK,
                "UnaryExpressionUnaryOperatorAsterisk") {
        }
        protected override uint GetContextForParserRuleContextChild(ParserRuleContext prc) {
            throw new NotImplementedException();
        }
        protected override uint GetContextForTerminalNodeChild(ITerminalNode ttn) {
            throw new NotImplementedException();
        }
        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            throw new NotImplementedException();
        }
    }

    public class UnaryExpressionUnaryOperatorPLUS : CExpression {

        public UnaryExpressionUnaryOperatorPLUS() :
            base(2,
                (uint)TranslationUnitAST.NodeTypes.UNARY_EXPRESSION_UNARY_OPERATOR_PLUS,
                "UnaryExpressionUnaryOperatorPLUS") {
        }
        protected override uint GetContextForParserRuleContextChild(ParserRuleContext prc) {
            throw new NotImplementedException();
        }
        protected override uint GetContextForTerminalNodeChild(ITerminalNode ttn) {
            throw new NotImplementedException();
        }
        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            throw new NotImplementedException();
        }
    }


    public class UnaryExpressionUnaryOperatorMINUS : CExpression {

        public UnaryExpressionUnaryOperatorMINUS() :
            base(2,
                (uint)TranslationUnitAST.NodeTypes.UNARY_EXPRESSION_UNARY_OPERATOR_HYPHEN,
                "UnaryExpressionUnaryOperatorMinus") {
        }
        protected override uint GetContextForParserRuleContextChild(ParserRuleContext prc) {
            throw new NotImplementedException();
        }
        protected override uint GetContextForTerminalNodeChild(ITerminalNode ttn) {
            throw new NotImplementedException();
        }
        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            throw new NotImplementedException();
        }
    }


    public class UnaryExpressionUnaryOperatorTilde : CExpression {

        public UnaryExpressionUnaryOperatorTilde() :
            base(2,
                (uint)TranslationUnitAST.NodeTypes.UNARY_EXPRESSION_UNARY_OPERATOR_TILDE,
                "UnaryExpressionUnaryOperatorTilde") {
        }
        protected override uint GetContextForParserRuleContextChild(ParserRuleContext prc) {
            throw new NotImplementedException();
        }
        protected override uint GetContextForTerminalNodeChild(ITerminalNode ttn) {
            throw new NotImplementedException();
        }
        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            throw new NotImplementedException();
        }
    }


    public class UnaryExpressionUnaryOperatorNOT : CExpression {

        public UnaryExpressionUnaryOperatorNOT() :
            base(2,
                (uint)TranslationUnitAST.NodeTypes.UNARY_EXPRESSION_UNARY_OPERATOR_NOT,
                "UnaryExpressionUnaryOperatorNOT") {
        }
        protected override uint GetContextForParserRuleContextChild(ParserRuleContext prc) {
            throw new NotImplementedException();
        }
        protected override uint GetContextForTerminalNodeChild(ITerminalNode ttn) {
            throw new NotImplementedException();
        }
        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            throw new NotImplementedException();
        }
    }

    public class UnaryExpressionSizeOfExpression : CExpression {
        public UnaryExpressionSizeOfExpression() : base(2, (uint)TranslationUnitAST.NodeTypes.UNARY_EXPRESSION_SIZEOF, "UnaryExpressionSizeOf") {
        }
        protected override uint GetContextForParserRuleContextChild(ParserRuleContext prc) {
            throw new NotImplementedException();
        }
        protected override uint GetContextForTerminalNodeChild(ITerminalNode ttn) {
            throw new NotImplementedException();
        }
        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            throw new NotImplementedException();
        }
    }

    public class UnaryExpressionSizeOfTypeName : CExpression {
        public UnaryExpressionSizeOfTypeName() : base(2, (uint)TranslationUnitAST.NodeTypes.UNARY_EXPRESSION_SIZEOF_TYPE, "UnaryExpressionSizeOfType") {
        }
        protected override uint GetContextForParserRuleContextChild(ParserRuleContext prc) {
            throw new NotImplementedException();
        }
        protected override uint GetContextForTerminalNodeChild(ITerminalNode ttn) {
            throw new NotImplementedException();
        }
        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            throw new NotImplementedException();
        }
    }

    public class ExpressionRelationalShift : CExpression {
        public ExpressionRelationalShift() : base(2, (uint)TranslationUnitAST.NodeTypes.EXPRESSION_RELATIONAL_SHIFT, "ExpressionRelationalShift") {
        }
        protected override uint GetContextForParserRuleContextChild(ParserRuleContext prc) {
            throw new NotImplementedException();
        }
        protected override uint GetContextForTerminalNodeChild(ITerminalNode ttn) {
            throw new NotImplementedException();
        }
        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            throw new NotImplementedException();
        }
    }

    public class ExpressionRelationalLess : CExpression {
        public ExpressionRelationalLess() : base(2, (uint)TranslationUnitAST.NodeTypes.EXPRESSION_RELATIONAL_LESS, "ExpressionRelationalLess") {
        }
        protected override uint GetContextForParserRuleContextChild(ParserRuleContext prc) {
            throw new NotImplementedException();
        }
        protected override uint GetContextForTerminalNodeChild(ITerminalNode ttn) {
            throw new NotImplementedException();
        }
        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            throw new NotImplementedException();
        }
    }

    public class ExpressionRelationalGreater : CExpression {
        public ExpressionRelationalGreater() : base(2, (uint)TranslationUnitAST.NodeTypes.EXPRESSION_RELATIONAL_GREATER, "ExpressionRelationalGreater") {
        }
        protected override uint GetContextForParserRuleContextChild(ParserRuleContext prc) {
            throw new NotImplementedException();
        }
        protected override uint GetContextForTerminalNodeChild(ITerminalNode ttn) {
            throw new NotImplementedException();
        }
        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            throw new NotImplementedException();
        }
    }

    public class ExpressionRelationalLessOrEqual : CExpression {
        public ExpressionRelationalLessOrEqual() : base(2, (uint)TranslationUnitAST.NodeTypes.EXPRESSION_RELATIONAL_LESS_OR_EQUAL, "ExpressionRelationalLessOrEqual") {
        }
        protected override uint GetContextForParserRuleContextChild(ParserRuleContext prc) {
            throw new NotImplementedException();
        }
        protected override uint GetContextForTerminalNodeChild(ITerminalNode ttn) {
            throw new NotImplementedException();
        }
        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            throw new NotImplementedException();
        }
    }

    public class ExpressionRelationalGreaterOrEqual : CExpression {
        public ExpressionRelationalGreaterOrEqual() : base(2, (uint)TranslationUnitAST.NodeTypes.EXPRESSION_RELATIONAL_GREATER_OR_EQUAL, "ExpressionRelationalGreaterOrEqual") {
        }
        protected override uint GetContextForParserRuleContextChild(ParserRuleContext prc) {
            throw new NotImplementedException();
        }
        protected override uint GetContextForTerminalNodeChild(ITerminalNode ttn) {
            throw new NotImplementedException();
        }
        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            throw new NotImplementedException();
        }
    }

    public class ExpressionLogicalAndInclusiveOr : CExpression {
        public ExpressionLogicalAndInclusiveOr() : base(2, (uint)TranslationUnitAST.NodeTypes.EXPRESSION_LOGICAL_AND_INCLUSIVE_OR, "ExpressionLogicalAndInclusiveOr") {
        }
        protected override uint GetContextForParserRuleContextChild(ParserRuleContext prc) {
            throw new NotImplementedException();
        }
        protected override uint GetContextForTerminalNodeChild(ITerminalNode ttn) {
            throw new NotImplementedException();
        }
        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            throw new NotImplementedException();
        }
    }

    public class ExpressionLogicalAnd : CExpression {
        public ExpressionLogicalAnd() : base(2, (uint)TranslationUnitAST.NodeTypes.EXPRESSION_LOGICAL_AND, "ExpressionLogicalAnd") {
        }
        protected override uint GetContextForParserRuleContextChild(ParserRuleContext prc) {
            throw new NotImplementedException();
        }
        protected override uint GetContextForTerminalNodeChild(ITerminalNode ttn) {
            throw new NotImplementedException();
        }
        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            throw new NotImplementedException();
        }
    }

    public class ExpressionLogicalOrInclusiveOr : CExpression {
        public ExpressionLogicalOrInclusiveOr() : base(2, (uint)TranslationUnitAST.NodeTypes.EXPRESSION_LOGICAL_OR_INCLUSIVE_OR, "ExpressionLogicalOrInclusiveOr") {
        }
        protected override uint GetContextForParserRuleContextChild(ParserRuleContext prc) {
            throw new NotImplementedException();
        }
        protected override uint GetContextForTerminalNodeChild(ITerminalNode ttn) {
            throw new NotImplementedException();
        }
        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            throw new NotImplementedException();
        }
    }

    public class ExpressionLogicalOr : CExpression {
        public ExpressionLogicalOr() : base(2, (uint)TranslationUnitAST.NodeTypes.EXPRESSION_LOGICAL_OR, "ExpressionLogicalOr") {
        }
        protected override uint GetContextForParserRuleContextChild(ParserRuleContext prc) {
            throw new NotImplementedException();
        }
        protected override uint GetContextForTerminalNodeChild(ITerminalNode ttn) {
            throw new NotImplementedException();
        }
        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            throw new NotImplementedException();
        }
    }

    public class ConditionalExpressionOr : CExpression {
        public ConditionalExpressionOr() : base(2, (uint)TranslationUnitAST.NodeTypes.CONDITIONAL_EXPRESSION_OR, "ConditionalExpressionOr") {
        }
        protected override uint GetContextForParserRuleContextChild(ParserRuleContext prc) {
            throw new NotImplementedException();
        }
        protected override uint GetContextForTerminalNodeChild(ITerminalNode ttn) {
            throw new NotImplementedException();
        }
        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            throw new NotImplementedException();
        }
    }

    public class ConditionalExpression : CExpression {
        public ConditionalExpression() : base(2, (uint)TranslationUnitAST.NodeTypes.CONDITIONAL_EXPRESSION, "ConditionalExpression") {
        }

        protected override uint GetContextForParserRuleContextChild(ParserRuleContext prc) {
            throw new NotImplementedException();
        }

        protected override uint GetContextForTerminalNodeChild(ITerminalNode ttn) {
            throw new NotImplementedException();
        }

        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            throw new NotImplementedException();
        }
    }

    public class AssignmentExpressionConditional : CExpression {
        public AssignmentExpressionConditional() : base(2,
            (uint)TranslationUnitAST.NodeTypes.ASSIGNMENT_EXPRESSION_CONDITIONAL, "assignment_expression_conditional") {

        }

        protected override uint GetContextForParserRuleContextChild(ParserRuleContext prc) {
            throw new NotImplementedException();
        }

        protected override uint GetContextForTerminalNodeChild(ITerminalNode ttn) {
            throw new NotImplementedException();
        }

        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            throw new NotImplementedException();
        }
    }


    public class IDENTIFIER : ASTLeaf {
        public IDENTIFIER(string lexeme) :
            base(lexeme, (uint)TranslationUnitAST.NodeTypes.IDENTIFIER, lexeme) {
        }
        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            return visitor.VisitIdentifier(this, info);
        }
    }

    public class INTEGER : ASTLeaf {
        public INTEGER(string lexeme) :
            base(lexeme, (uint)TranslationUnitAST.NodeTypes.INTEGER, lexeme) {
        }
        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            return visitor.VisitInteger(this, info);
        }
    }

    public class Postfixexpression_ArraySubscript : CExpression {
        public const int ARRAY = 0, INDEX = 1;
        public Postfixexpression_ArraySubscript() : base(2,
            (uint)TranslationUnitAST.NodeTypes.POSTFIX_EXPRESSION_ARRAYSUBSCRIPT, "postfix_expression_ArraySubscript") {
        }
        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            return visitor.VisitPostfixExpression_ArraySubscript(this, info);
        }
        protected override uint GetContextForParserRuleContextChild(ParserRuleContext prc) {
            throw new NotImplementedException();
        }
        protected override uint GetContextForTerminalNodeChild(ITerminalNode ttn) {
            throw new NotImplementedException();
        }
    }

    public class Postfixexpression_FunctionCallNoArgs : CExpression {
        public const int FUNCTION = 0;
        public Postfixexpression_FunctionCallNoArgs() : base(1,
            (uint)TranslationUnitAST.NodeTypes.POSTFIX_EXPRESSION_FUNCTIONCALLNOARGS, "postfix_expression_FunctionCallNoArgs") {
        }
        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            return visitor.Visitpostfix_expression_FunctionCallNoArgs(this, info);
        }
        protected override uint GetContextForParserRuleContextChild(ParserRuleContext prc) {
            return FUNCTION;
        }
        protected override uint GetContextForTerminalNodeChild(ITerminalNode ttn) {
            throw new NotImplementedException();
        }
    }

    public class Postfixexpression_FunctionCallWithArgs : CExpression {

        public const int FUNCTION = 0;

        public Postfixexpression_FunctionCallWithArgs() : base(1,
            (uint)TranslationUnitAST.NodeTypes.POSTFIX_EXPRESSION_FUNCTIONCALLWITHARGS, "postfix_expression_FunctionCallWithArgs") {
        }

        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            return visitor.Visitpostfix_expression_FunctionCallWithArgs(this, info);
        }

        protected override uint GetContextForParserRuleContextChild(ParserRuleContext prc) {
            return FUNCTION;
        }

        protected override uint GetContextForTerminalNodeChild(ITerminalNode ttn) {
            throw new NotImplementedException();
        }
    }

    public class Postfixexpression_MemberAccess : CExpression {

        public const int ACCESS = 0;

        public Postfixexpression_MemberAccess() : base(1,
            (uint)TranslationUnitAST.NodeTypes.POSTFIX_EXPRESSION_MEMBERACCESS, "postfix_expression_MemberAccess") {
        }

        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            return visitor.Visitpostfix_expression_MemberAccess(this, info);
        }

        protected override uint GetContextForParserRuleContextChild(ParserRuleContext prc) {
            return ACCESS;
        }

        protected override uint GetContextForTerminalNodeChild(ITerminalNode ttn) {
            throw new NotImplementedException();
        }

    }

    public class Postfixexpression_PointerMemberAccess : CExpression {
        public const int ACCESS = 0;
        public Postfixexpression_PointerMemberAccess() : base(1,
            (uint)TranslationUnitAST.NodeTypes.POSTFIX_EXPRESSION_POINTERMEMBERACCESS, "postfix_expression_PointerMemberAccess") {
        }
        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            return visitor.Visitpostfix_expression_PointerMemberAccess(this, info);
        }
        protected override uint GetContextForParserRuleContextChild(ParserRuleContext prc) {
            return ACCESS;
        }
        protected override uint GetContextForTerminalNodeChild(ITerminalNode ttn) {
            throw new NotImplementedException();
        }
    }

    public class Postfixexpression_Increment : CExpression {

        public const int ACCESS = 0;
        public Postfixexpression_Increment() : base(1,
            (uint)TranslationUnitAST.NodeTypes.POSTFIX_EXPRESSION_INCREMENT, "postfix_expression_Increment") {
        }

        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            return visitor.Visitpostfix_expression_Increment(this, info);
        }
        protected override uint GetContextForParserRuleContextChild(ParserRuleContext prc) {
            return ACCESS;
        }
        protected override uint GetContextForTerminalNodeChild(ITerminalNode ttn) {
            throw new NotImplementedException();
        }
    }

    public class Postfixexpression_Decrement : CExpression {

        public const int ACCESS = 0;
        public Postfixexpression_Decrement() : base(1,
            (uint)TranslationUnitAST.NodeTypes.POSTFIX_EXPRESSION_DECREMENT, "postfix_expression_Increment") {
        }

        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            return visitor.Visitpostfix_expression_Decrement(this, info);
        }
        protected override uint GetContextForParserRuleContextChild(ParserRuleContext prc) {
            return ACCESS;
        }
        protected override uint GetContextForTerminalNodeChild(ITerminalNode ttn) {
            throw new NotImplementedException();
        }
    }

    public class Expression_CommaExpression : CExpression {
        public const int LEFT = 0, RIGHT = 1;
        public Expression_CommaExpression() : base(2,
            (uint)TranslationUnitAST.NodeTypes.EXPRESSION_COMMAEXPRESSION, "Expression_CommaExpression") {
        }
        protected override uint GetContextForParserRuleContextChild(ParserRuleContext prc) {
            throw new NotImplementedException();
        }
        protected override uint GetContextForTerminalNodeChild(ITerminalNode ttn) {
            throw new NotImplementedException();
        }
        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            return visitor.VisitExpressionCommaExpression(this, info);
        }
    }

    public class Expression_Cast : CExpression {
        public Expression_Cast() : base(2,
            (uint)TranslationUnitAST.NodeTypes.UNARY_EXPRESSION_CAST,
            "ExpressionCast") {
        }

        protected override uint GetContextForParserRuleContextChild(ParserRuleContext prc) {
            throw new NotImplementedException();
        }

        protected override uint GetContextForTerminalNodeChild(ITerminalNode ttn) {
            throw new NotImplementedException();
        }

        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            throw new NotImplementedException();
        }
    }

    public class ExpressionMultiplication : CExpression {
        public ExpressionMultiplication() : base(2,
            (uint)TranslationUnitAST.NodeTypes.EXPRESSION_MULTIPLICATION,
            "ExpressionMultiplication") {

        }

        protected override uint GetContextForParserRuleContextChild(ParserRuleContext prc) {
            throw new NotImplementedException();
        }

        protected override uint GetContextForTerminalNodeChild(ITerminalNode ttn) {
            throw new NotImplementedException();
        }

        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            throw new NotImplementedException();
        }
    }

    public class ExpressionDivision : CExpression {
        public ExpressionDivision() : base(2,
            (uint)TranslationUnitAST.NodeTypes.EXPRESSION_DIVISION,
            "ExpressionDivision") {

        }

        protected override uint GetContextForParserRuleContextChild(ParserRuleContext prc) {
            throw new NotImplementedException();
        }

        protected override uint GetContextForTerminalNodeChild(ITerminalNode ttn) {
            throw new NotImplementedException();
        }

        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            throw new NotImplementedException();
        }
    }

    public class ExpressionModulus : CExpression {
        public ExpressionModulus() : base(2,
            (uint)TranslationUnitAST.NodeTypes.EXPRESSION_MODULO,
            "ExpressionModulo") {

        }

        protected override uint GetContextForParserRuleContextChild(ParserRuleContext prc) {
            throw new NotImplementedException();
        }

        protected override uint GetContextForTerminalNodeChild(ITerminalNode ttn) {
            throw new NotImplementedException();
        }

        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            throw new NotImplementedException();
        }
    }

    public class Expression_AssignmentLeft : CExpression {
        public Expression_AssignmentLeft() : base(2,
            (uint)TranslationUnitAST.NodeTypes.EXPRESSION_ASSIGNMENT_LEFT,
            "ExpressionAssignmentLeft") {
        }
        protected override uint GetContextForParserRuleContextChild(ParserRuleContext prc) {
            throw new NotImplementedException();
        }
        protected override uint GetContextForTerminalNodeChild(ITerminalNode ttn) {
            throw new NotImplementedException();
        }
        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            throw new NotImplementedException();
        }
    }

    public class Expression_AssignmentRight : CExpression {
        public Expression_AssignmentRight() : base(2,
            (uint)TranslationUnitAST.NodeTypes.EXPRESSION_ASSIGNMENT_RIGHT,
            "ExpressionAssignmentRight") {
        }
        protected override uint GetContextForParserRuleContextChild(ParserRuleContext prc) {
            throw new NotImplementedException();
        }
        protected override uint GetContextForTerminalNodeChild(ITerminalNode ttn) {
            throw new NotImplementedException();
        }
        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            throw new NotImplementedException();
        }
    }

    public class Expression_AssignmentAnd : CExpression {
        public Expression_AssignmentAnd() : base(2,
            (uint)TranslationUnitAST.NodeTypes.EXPRESSION_ASSIGNMENT_AND,
            "ExpressionAssignmentAND") {

        }

        protected override uint GetContextForParserRuleContextChild(ParserRuleContext prc) {
            throw new NotImplementedException();
        }

        protected override uint GetContextForTerminalNodeChild(ITerminalNode ttn) {
            throw new NotImplementedException();
        }

        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            throw new NotImplementedException();
        }
    }

    public class Expression_AssignmentXor : CExpression {
        public Expression_AssignmentXor() : base(2,
            (uint)TranslationUnitAST.NodeTypes.EXPRESSION_ASSIGNMENT_XOR,
            "ExpressionAssignmentXOR") {
        }
        protected override uint GetContextForParserRuleContextChild(ParserRuleContext prc) {
            throw new NotImplementedException();
        }
        protected override uint GetContextForTerminalNodeChild(ITerminalNode ttn) {
            throw new NotImplementedException();
        }
        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            throw new NotImplementedException();
        }
    }

    public class Expression_AssignmentOr : CExpression {
        public Expression_AssignmentOr() : base(2,
            (uint)TranslationUnitAST.NodeTypes.EXPRESSION_ASSIGNMENT_OR,
            "ExpressionAssignmentOR") {
        }
        protected override uint GetContextForParserRuleContextChild(ParserRuleContext prc) {
            throw new NotImplementedException();
        }
        protected override uint GetContextForTerminalNodeChild(ITerminalNode ttn) {
            throw new NotImplementedException();
        }
        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            throw new NotImplementedException();
        }
    }

    public class ExpressionAssignmentMultiplication : CExpression {
        public ExpressionAssignmentMultiplication() : base(2,
            (uint)TranslationUnitAST.NodeTypes.EXPRESSION_ASSIGNMENT_MULTIPLICATION,
            "ExpressionAssignmentDiv") {

        }

        protected override uint GetContextForParserRuleContextChild(ParserRuleContext prc) {
            throw new NotImplementedException();
        }

        protected override uint GetContextForTerminalNodeChild(ITerminalNode ttn) {
            throw new NotImplementedException();
        }

        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            throw new NotImplementedException();
        }
    }


    public class ExpressionAssignmentDivision : CExpression {
        public ExpressionAssignmentDivision() : base(2,
            (uint)TranslationUnitAST.NodeTypes.EXPRESSION_ASSIGNMENT_DIVISION,
            "ExpressionAssignmentDivision") {

        }

        protected override uint GetContextForParserRuleContextChild(ParserRuleContext prc) {
            throw new NotImplementedException();
        }

        protected override uint GetContextForTerminalNodeChild(ITerminalNode ttn) {
            throw new NotImplementedException();
        }

        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            throw new NotImplementedException();
        }
    }

    public class ExpressionAssignmentModulo : CExpression {
        public ExpressionAssignmentModulo() : base(2,
            (uint)TranslationUnitAST.NodeTypes.EXPRESSION_ASSIGNMENT_MODULO,
            "ExpressionAssignmentModulo") {

        }

        protected override uint GetContextForParserRuleContextChild(ParserRuleContext prc) {
            throw new NotImplementedException();
        }

        protected override uint GetContextForTerminalNodeChild(ITerminalNode ttn) {
            throw new NotImplementedException();
        }

        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            throw new NotImplementedException();
        }
    }

    public class ExpressionAssignmentAddition : CExpression {
        public ExpressionAssignmentAddition() : base(2,
            (uint)TranslationUnitAST.NodeTypes.EXPRESSION_ASSIGNMENT_ADDITION,
            "ExpressionAssignmentAddition") {

        }

        protected override uint GetContextForParserRuleContextChild(ParserRuleContext prc) {
            throw new NotImplementedException();
        }

        protected override uint GetContextForTerminalNodeChild(ITerminalNode ttn) {
            throw new NotImplementedException();
        }

        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            throw new NotImplementedException();
        }
    }

    public class ExpressionAssignmentSubtraction : CExpression {
        public ExpressionAssignmentSubtraction() : base(2,
            (uint)TranslationUnitAST.NodeTypes.EXPRESSION_ASSIGNMENT_SUBTRACTION,
            "ExpressionAssignmentSubtraction") {

        }

        protected override uint GetContextForParserRuleContextChild(ParserRuleContext prc) {
            throw new NotImplementedException();
        }

        protected override uint GetContextForTerminalNodeChild(ITerminalNode ttn) {
            throw new NotImplementedException();
        }

        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            throw new NotImplementedException();
        }
    }

    public class Statement_Expression : CStatement {
        public const uint EXPRESSION = 0;

        public Statement_Expression() :
            base(1,
                (uint)TranslationUnitAST.NodeTypes.STATEMENT_EXPRESSION, "Statement_Expression") {
        }

        protected override uint GetContextForParserRuleContextChild(ParserRuleContext prc) {
            switch (prc.RuleIndex) {
                case CGrammarParser.RULE_assignment_expression:
                    return EXPRESSION;
                default:
                    throw new NotImplementedException();
            }
        }

        protected override uint GetContextForTerminalNodeChild(ITerminalNode ttn) {
            throw new NotImplementedException();
        }

        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            return visitor.VisitStatementExpression(this, info);
        }
    }
}