using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.Configuration;
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

        public void AddChild(ASTElement child, uint? context) {
            if (context == null || context >= m_contexts) {
                throw new ArgumentOutOfRangeException("context", "Context index out of range");
            }
            m_children[(uint)context].Add(child);
        }
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
            EXPRESSION_RELATIONAL_SHIFTL, EXPRESSION_RELATIONAL_SHIFTR,
            EXPRESSION_RELATIONAL_LESS,
            EXPRESSION_RELATIONAL_GREATER, EXPRESSION_RELATIONAL_LESS_OR_EQUAL,
            EXPRESSION_RELATIONAL_GREATER_OR_EQUAL, 
            EXPRESSION_LOGICAL_AND, 
            EXPRESSION_LOGICAL_OR, 
            CONDITIONAL_EXPRESSION, 
            ASSIGNMENT_EXPRESSION,

            CHAR_TYPE,
            UNARY_EXPRESSION_CAST,
            EXPRESSION_ASSIGNMENT_MULTIPLICATION, EXPRESSION_ASSIGNMENT_DIVISION,
            EXPRESSION_ASSIGNMENT_MODULO, EXPRESSION_ASSIGNMENT_ADDITION,
            EXPRESSION_ASSIGNMENT_SUBTRACTION,
            EXPRESSION_ASSIGNMENT_LEFT,
            EXPRESSION_ASSIGNMENT_RIGHT,
            EXPRESSION_ASSIGNMENT_AND,
            EXPRESSION_ASSIGNMENT_XOR,
            EXPRESSION_ASSIGNMENT_OR,

            STATEMENT_EXPRESSION,
            INTEGER


        }


        public const uint FUNCTION_DEFINITION = 0, DECLARATIONS = 1;

        public TranslationUnitAST() :
            base(2, (uint)TranslationUnitAST.NodeTypes.TRANSLATION_UNIT, "TranslationUnitAST") {
        }

        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            return visitor.VisitTranslationUnit(this, info);
        }

    }

    public class DeclarationAST : ASTComposite {
        public const int TYPE_SPECIFIER = 0, DECLARATORS = 1, TYPE_QUALIFIER = 2, STORAGE_SPECIFIER = 3;

        public enum TYPE_SPECIFIER_ENUM {
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

        public enum STORAGE_CLASS_ENUM {
            STATIC,
            EXTERN,
            AUTO,
            REGISTER
        }

        public enum TYPE_QUALIFIER_ENUM {
            CONST,
            RESTRICT,
            VOLATILE,
            ATOMIC
        }

        private TYPE_SPECIFIER_ENUM m_type;
        private STORAGE_CLASS_ENUM m_class;
        private TYPE_QUALIFIER_ENUM m_qualifier;

        public TYPE_SPECIFIER_ENUM MType1 {
            get => m_type;
            internal set => m_type = value;
        }

        public STORAGE_CLASS_ENUM MClass {
            get => m_class;
            internal set => m_class = value;
        }

        public TYPE_QUALIFIER_ENUM MQualifier {
            get => m_qualifier;
            internal set => m_qualifier = value;
        }

        public DeclarationAST() :
            base(4, (uint)TranslationUnitAST.NodeTypes.DECLARATION, "DeclarationAST") {
        }

        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor,
            INFO info = default(INFO)) {
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
        public const int POINTER_TARGET = 0;
        public enum QUALIFIER {
            CONST,
            RESTRICT,
            VOLATILE,
            ATOMIC
        }
        public PointerTypeAST() :
            base(1, (uint)TranslationUnitAST.NodeTypes.POINTER_TYPE, "PointerTypeAST") {
        }
        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            return visitor.VisitPointerType(this, info);
        }
    }

    public class ParameterDeclarationAST : ASTComposite {
        public const int TYPE_SPECIFIER = 0, DECLARATOR = 1, TYPE_QUALIFIER = 2, STORAGE_SPECIFIER = 3;
        public ParameterDeclarationAST() :
            base(4, (uint)TranslationUnitAST.NodeTypes.PARAMETER_DECLARATION, "ParameterDeclaration") {
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


        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor,
            INFO info = default(INFO)) {
            return visitor.VisitFunctionType(this, info);
        }
    }


    public class FunctionDefinitionAST : ASTComposite {
        public const int DECLARATION_SPECIFIERS = 0,
            DECLARATOR = 1, PARAMETER_DECLARATIONS = 2, FUNCTION_BODY = 3;
        public FunctionDefinitionAST() :
            base(4, (uint)TranslationUnitAST.NodeTypes.FUNCTION_DEFINITION, "FunctionDefinitionAST") {
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

    }

    public class Expression_Number : CExpression {
        public const int NUMBER = 0;
        public Expression_Number(uint numcontexts, uint type, string name) :
            base(1, (uint)TranslationUnitAST.NodeTypes.EXPRESSION_NUMBER, name) {
        }

        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor,
            INFO info = default(INFO)) {
            return visitor.VisitExpressionNumber(this, info);
        }
    }

    public class Expression_StringLiteral : CExpression {
        public const int STRING = 0;
        public Expression_StringLiteral() : base(1,
            (uint)TranslationUnitAST.NodeTypes.EXPRESSION_STRINGLITERAL, "StringLiteral") {
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

        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            return visitor.VisitExpressionAssignment(this, info);
        }
    }

    public class Expression_Addition : CExpression {
        public const int LEFT = 0, RIGHT = 1;

        public Expression_Addition() : base(2,
            (uint)TranslationUnitAST.NodeTypes.EXPRESSION_ADDITION, "Expression_Addition") {
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

        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            return visitor.VisitExpressionMultiplication(this, info);
        }
    }

    public class Expression_Division : CExpression {
        public const int LEFT = 0, RIGHT = 1;

        public Expression_Division() : base(2,
            (uint)TranslationUnitAST.NodeTypes.EXPRESSION_DIVISION, "Expression_Division") {
        }

        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor,
            INFO info = default(INFO)) {
            return visitor.VisitExpressionDivision(this, info);
        }
    }

    public class Expression_Modulo : CExpression {
        public const int LEFT = 0, RIGHT = 1;

        public Expression_Modulo() : base(2,
            (uint)TranslationUnitAST.NodeTypes.EXPRESSION_MODULO, "Expression_Modulo") {
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

        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            return visitor.VisitExpressionSubtraction(this, info);
        }
    }

    public class Expression_EqualityEqual : CExpression {
        public const int LEFT = 0, RIGHT = 1;

        public Expression_EqualityEqual() : base(2,
            (uint)TranslationUnitAST.NodeTypes.EXPRESSION_EQUALITY_EQUAL, "Expression_Equality_Equal") {
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

        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            return visitor.VisitExpressionEqualityNotEqual(this, info);
        }
    }

    public class Expression_BitwiseAND : CExpression {
        public const int LEFT = 0, RIGHT = 1;

        public Expression_BitwiseAND() : base(2,
            (uint)TranslationUnitAST.NodeTypes.EXPRESSION_BITWISE_AND, "Expression_Bitwise_AND") {
        }
        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor,
            INFO info = default(INFO)) {
            return visitor.VisitExpressionBitwiseAND(this, info);
        }
    }

    public class Expression_BitwiseOR : CExpression {
        public const int LEFT = 0, RIGHT = 1;

        public Expression_BitwiseOR() : base(2,
            (uint)TranslationUnitAST.NodeTypes.EXPRESSION_BITWISE_OR, "Expression_Bitwise_OR") {
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

        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            return visitor.VisitCompoundStatement(this, info);
        }
    }

    public class ExpressionStatement : Statement {
        public const int EXPRESSION = 0;
        public ExpressionStatement() : base(1,
            (uint)TranslationUnitAST.NodeTypes.EXPRESSION_STATEMENT, "ExpressionStatement") {
        }

        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor,
            INFO info = default(INFO)) {
            return visitor.VisitExpressionStatement(this, info);
        }
    }

    public class UnaryExpressionIncrement : CExpression {
        public UnaryExpressionIncrement() : base(2, (uint)TranslationUnitAST.NodeTypes.UNARY_EXPRESSION_INCREMENT, "UnaryExpressionIncrement") {

        }

        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            return visitor.VisitUnaryExpressionIncrement(this, info);
        }
    }

    public class UnaryExpressionDecrement : CExpression {
        public UnaryExpressionDecrement() : base(2, (uint)TranslationUnitAST.NodeTypes.UNARY_EXPRESSION_DECREMENT, "UnaryExpressionDecrement") {
        }

        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            return visitor.VisitUnaryExpressionDecrement(this, info);
        }
    }

    public class UnaryExpressionUnaryOperatorAmbersand : CExpression {

        public UnaryExpressionUnaryOperatorAmbersand() :
            base(2,
                (uint)TranslationUnitAST.NodeTypes.UNARY_EXPRESSION_UNARY_OPERATOR_AMBERSAND,
                "UnaryExpressionUnaryOperatorAmbersand") {
        }

        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            return visitor.VisitUnaryExpressionOperatorAmbersand(this, info);
        }
    }

    public class UnaryExpressionUnaryOperatorAsterisk : CExpression {

        public UnaryExpressionUnaryOperatorAsterisk() :
            base(2,
                (uint)TranslationUnitAST.NodeTypes.UNARY_EXPRESSION_UNARY_OPERATOR_ASTERISK,
                "UnaryExpressionUnaryOperatorAsterisk") {
        }

        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            return visitor.VisitUnaryExpressionOperatorAsterisk(this, info);
        }
    }

    public class UnaryExpressionUnaryOperatorPLUS : CExpression {

        public UnaryExpressionUnaryOperatorPLUS() :
            base(2,
                (uint)TranslationUnitAST.NodeTypes.UNARY_EXPRESSION_UNARY_OPERATOR_PLUS,
                "UnaryExpressionUnaryOperatorPLUS") {
        }

        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor,
            INFO info = default(INFO)) {
            return visitor.VisitUnaryExpressionOperatorPLUS(this, info);
        }
    }


    public class UnaryExpressionUnaryOperatorMINUS : CExpression {

        public UnaryExpressionUnaryOperatorMINUS() :
            base(2,
                (uint)TranslationUnitAST.NodeTypes.UNARY_EXPRESSION_UNARY_OPERATOR_HYPHEN,
                "UnaryExpressionUnaryOperatorMinus") {
        }

        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor,
            INFO info = default(INFO)) {
            return visitor.VisitUnaryExpressionOperatorMINUS(this, info);
        }
    }


    public class UnaryExpressionUnaryOperatorTilde : CExpression {

        public UnaryExpressionUnaryOperatorTilde() :
            base(2,
                (uint)TranslationUnitAST.NodeTypes.UNARY_EXPRESSION_UNARY_OPERATOR_TILDE,
                "UnaryExpressionUnaryOperatorTilde") {
        }

        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor,
            INFO info = default(INFO)) {
            return visitor.VisitUnaryExpressionOperatorTilde(this, info);
        }
    }


    public class UnaryExpressionUnaryOperatorNOT : CExpression {

        public UnaryExpressionUnaryOperatorNOT() :
            base(2,
                (uint)TranslationUnitAST.NodeTypes.UNARY_EXPRESSION_UNARY_OPERATOR_NOT,
                "UnaryExpressionUnaryOperatorNOT") {
        }

        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor,
            INFO info = default(INFO)) {
            return visitor.VisitUnaryExpressionOperatorNOT(this, info);
        }
    }

    public class UnaryExpressionSizeOfExpression : CExpression {
        public UnaryExpressionSizeOfExpression() : base(2, (uint)TranslationUnitAST.NodeTypes.UNARY_EXPRESSION_SIZEOF, "UnaryExpressionSizeOf") {
        }

        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor,
            INFO info = default(INFO)) {
            return visitor.VisitUnaryExpressionSizeOfExpression(this, info);
        }
    }

    public class UnaryExpressionSizeOfTypeName : CExpression {
        public UnaryExpressionSizeOfTypeName() : base(2, (uint)TranslationUnitAST.NodeTypes.UNARY_EXPRESSION_SIZEOF_TYPE, "UnaryExpressionSizeOfType") {
        }

        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor,
            INFO info = default(INFO)) {
            return visitor.VisitUnaryExpressionSizeOfTypename(this, info);
        }
    }

    public class ExpressionShiftLeft : CExpression {
        public ExpressionShiftLeft() : base(2,
            (uint)TranslationUnitAST.NodeTypes.EXPRESSION_RELATIONAL_SHIFTL,
            "ExpressionLeftShift") {
        }

        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor,
            INFO info = default(INFO)) {
            return visitor.VisitShiftExpression_Left(this, info);
        }
    }

    public class ExpressionShiftRight : CExpression {
        public ExpressionShiftRight() : base(2, 
            (uint)TranslationUnitAST.NodeTypes.EXPRESSION_RELATIONAL_SHIFTR, 
            "ExpressionRightShift") {
        }

        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor,
            INFO info = default(INFO)) {
            return visitor.VisitShiftExpression_Right(this, info);
        }
    }

    public class ExpressionRelationalLess : CExpression {
        public ExpressionRelationalLess() : base(2, (uint)TranslationUnitAST.NodeTypes.EXPRESSION_RELATIONAL_LESS, "ExpressionRelationalLess") {
        }

        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor,
            INFO info = default(INFO)) {
            return visitor.VisitRelationalLess(this, info);
        }
    }

    public class ExpressionRelationalGreater : CExpression {
        public ExpressionRelationalGreater() : base(2, (uint)TranslationUnitAST.NodeTypes.EXPRESSION_RELATIONAL_GREATER, "ExpressionRelationalGreater") {
        }

        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor,
            INFO info = default(INFO)) {
            return visitor.VisitRelationalGreater(this, info);
        }
    }

    public class ExpressionRelationalLessOrEqual : CExpression {
        public ExpressionRelationalLessOrEqual() : base(2, (uint)TranslationUnitAST.NodeTypes.EXPRESSION_RELATIONAL_LESS_OR_EQUAL, "ExpressionRelationalLessOrEqual") {
        }

        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor,
            INFO info = default(INFO)) {
            return visitor.VisitRelationalLessEqual(this, info);
        }
    }

    public class ExpressionRelationalGreaterOrEqual : CExpression {
        public ExpressionRelationalGreaterOrEqual() : base(2, (uint)TranslationUnitAST.NodeTypes.EXPRESSION_RELATIONAL_GREATER_OR_EQUAL, "ExpressionRelationalGreaterOrEqual") {
        }

        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor,
            INFO info = default(INFO)) {
            return visitor.VisitRelationalGreaterEqual(this, info);
        }
    }

    
    public class ExpressionLogicalAnd : CExpression {
        public ExpressionLogicalAnd() : base(2, (uint)TranslationUnitAST.NodeTypes.EXPRESSION_LOGICAL_AND, "ExpressionLogicalAnd") {
        }

        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor,
            INFO info = default(INFO)) {
            throw new NotImplementedException();
        }
    }
    
    public class ExpressionLogicalOr : CExpression {
        public ExpressionLogicalOr() : base(2, (uint)TranslationUnitAST.NodeTypes.EXPRESSION_LOGICAL_OR, "ExpressionLogicalOr") {
        }

        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor,
            INFO info = default(INFO)) {
            throw new NotImplementedException();
        }
    }
    
    public class ConditionalExpression : CExpression {
        public ConditionalExpression() : base(2, (uint)TranslationUnitAST.NodeTypes.CONDITIONAL_EXPRESSION, "ConditionalExpression") {
        }


        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor,
            INFO info = default(INFO)) {
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

    }

    public class Postfixexpression_FunctionCallNoArgs : CExpression {
        public const int FUNCTION = 0;
        public Postfixexpression_FunctionCallNoArgs() : base(1,
            (uint)TranslationUnitAST.NodeTypes.POSTFIX_EXPRESSION_FUNCTIONCALLNOARGS, "postfix_expression_FunctionCallNoArgs") {
        }
        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            return visitor.Visitpostfix_expression_FunctionCallNoArgs(this, info);
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

    }

    public class Postfixexpression_MemberAccess : CExpression {

        public const int ACCESS = 0;

        public Postfixexpression_MemberAccess() : base(1,
            (uint)TranslationUnitAST.NodeTypes.POSTFIX_EXPRESSION_MEMBERACCESS, "postfix_expression_MemberAccess") {
        }

        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            return visitor.Visitpostfix_expression_MemberAccess(this, info);
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

    }

    public class Postfixexpression_Increment : CExpression {

        public const int ACCESS = 0;
        public Postfixexpression_Increment() : base(1,
            (uint)TranslationUnitAST.NodeTypes.POSTFIX_EXPRESSION_INCREMENT, "postfix_expression_Increment") {
        }

        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            return visitor.Visitpostfix_expression_Increment(this, info);
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

    }

    public class Expression_CommaExpression : CExpression {
        public const int LEFT = 0, RIGHT = 1;
        public Expression_CommaExpression() : base(2,
            (uint)TranslationUnitAST.NodeTypes.EXPRESSION_COMMAEXPRESSION, "Expression_CommaExpression") {
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

        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            throw new NotImplementedException();
        }
    }

    public class ExpressionMultiplication : CExpression {
        public ExpressionMultiplication() : base(2,
            (uint)TranslationUnitAST.NodeTypes.EXPRESSION_MULTIPLICATION,
            "ExpressionMultiplication") {

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


        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            throw new NotImplementedException();
        }
    }

    public class ExpressionModulus : CExpression {
        public ExpressionModulus() : base(2,
            (uint)TranslationUnitAST.NodeTypes.EXPRESSION_MODULO,
            "ExpressionModulo") {

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

        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            throw new NotImplementedException();
        }
    }

    public class Expression_AssignmentRight : CExpression {
        public Expression_AssignmentRight() : base(2,
            (uint)TranslationUnitAST.NodeTypes.EXPRESSION_ASSIGNMENT_RIGHT,
            "ExpressionAssignmentRight") {
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


        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            throw new NotImplementedException();
        }
    }

    public class Expression_AssignmentXor : CExpression {
        public Expression_AssignmentXor() : base(2,
            (uint)TranslationUnitAST.NodeTypes.EXPRESSION_ASSIGNMENT_XOR,
            "ExpressionAssignmentXOR") {
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

        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            throw new NotImplementedException();
        }
    }

    public class ExpressionAssignmentMultiplication : CExpression {
        public ExpressionAssignmentMultiplication() : base(2,
            (uint)TranslationUnitAST.NodeTypes.EXPRESSION_ASSIGNMENT_MULTIPLICATION,
            "ExpressionAssignmentDiv") {

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



        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            throw new NotImplementedException();
        }
    }

    public class ExpressionAssignmentModulo : CExpression {
        public ExpressionAssignmentModulo() : base(2,
            (uint)TranslationUnitAST.NodeTypes.EXPRESSION_ASSIGNMENT_MODULO,
            "ExpressionAssignmentModulo") {

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


        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            throw new NotImplementedException();
        }
    }

    public class ExpressionAssignmentSubtraction : CExpression {
        public ExpressionAssignmentSubtraction() : base(2,
            (uint)TranslationUnitAST.NodeTypes.EXPRESSION_ASSIGNMENT_SUBTRACTION,
            "ExpressionAssignmentSubtraction") {

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


        public override Result Accept<Result, INFO>(BaseASTVisitor<Result, INFO> visitor, INFO info = default(INFO)) {
            return visitor.VisitStatementExpression(this, info);
        }
    }
}