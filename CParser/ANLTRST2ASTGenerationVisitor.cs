using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

namespace CParser {

    public class ASTGenerationBuildParameters {
        private ASTComposite m_parent;
        private uint? m_context;
        public uint? Context {
            get => m_context;
            set => m_context = value;
        }
        public ASTComposite Parent {
            get => m_parent;
            set => m_parent = value;
        }
    }


    public class ANLTRST2ASTGenerationVisitor : CGrammarParserBaseVisitor<int> {

        ASTComposite m_root;
        Stack<ASTGenerationBuildParameters> m_contexts = new Stack<ASTGenerationBuildParameters>();
        private ASTGenerationBuildParameters m_lastCreatedNode = null;


        public ASTComposite Root {
            get => m_root;
        }

        public ANLTRST2ASTGenerationVisitor() {
            m_root = null;
        }

        public void VisitChildInContext(ParserRuleContext child, ASTGenerationBuildParameters p) {
            if (child != null) {
                m_contexts.Push(p);
                Visit(child);
                m_contexts.Pop();
            }
        }

        public void VisitChildrenInContext(ParserRuleContext[] children,
            ASTGenerationBuildParameters p) {
            if (children != null) {
                m_contexts.Push(p);
                foreach (var child in children) {
                    Visit(child);
                }
                m_contexts.Pop();
            }
        }

        public override int VisitTranslation_unit(CGrammarParser.Translation_unitContext context) {

            // 1. Create TranslationUnitAST node
            TranslationUnitAST tuNode = new TranslationUnitAST();
            m_root = tuNode;

            //2. Visit children and populate the AST node
            ASTGenerationBuildParameters tuContext = new ASTGenerationBuildParameters() {
                Parent = tuNode,
                Context = null
            };
            VisitChildrenInContext(context.external_declaration(), tuContext);

            return 0;
        }

        public override int VisitExternal_declaration(CGrammarParser.External_declarationContext context) {


            // 1. Visit Declarations
            ASTGenerationBuildParameters parentContextParameters = new ASTGenerationBuildParameters() {
                Parent = m_contexts.Peek().Parent,
                Context = TranslationUnitAST.DECLARATIONS
            };
            VisitChildInContext(context.declaration(), parentContextParameters);

            // 2. Visit Function Definitions
            parentContextParameters = new ASTGenerationBuildParameters() {
                Parent = m_contexts.Peek().Parent,
                Context = TranslationUnitAST.FUNCTION_DEFINITION
            };
            VisitChildInContext(context.function_definition(), parentContextParameters);

            return 0;
        }

        public override int VisitFunction_definition(CGrammarParser.Function_definitionContext context) {

            // 1. Get current parent node
            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;

            // 2. Create FunctionDefinitionAST node
            FunctionDefinitionAST funcDefNode = new FunctionDefinitionAST();
            parent.AddChild(funcDefNode, currentContext.Context);


            // 3. Visit Declaration Specifiers
            ASTGenerationBuildParameters p = new ASTGenerationBuildParameters() {
                Parent = funcDefNode,
                Context = FunctionDefinitionAST.DECLARATION_SPECIFIERS
            };
            VisitChildInContext(context.declaration_specifiers(), p);

            // 4. Visit Declarator
            p = new ASTGenerationBuildParameters() {
                Parent = funcDefNode,
                Context = null
            };
            VisitChildInContext(context.declarator(), p);

            // 5. Visit Compound Statement (Function Body)
            p = new ASTGenerationBuildParameters() {
                Parent = funcDefNode,
                Context = FunctionDefinitionAST.FUNCTION_BODY
            };
            VisitChildInContext(context.compound_statement(), p);


            return 0;
        }

        public override int VisitDeclaration(CGrammarParser.DeclarationContext context) {

            // 1. Get current parent node
            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;

            // 2. Create DeclarationAST node
            DeclarationAST declNode = new DeclarationAST();

            // 3. Add DeclarationAST node to parent
            parent.AddChild(declNode, currentContext.Context);

            ASTGenerationBuildParameters p = new ASTGenerationBuildParameters() {
                Parent = declNode,
                Context = null
            };
            VisitChildInContext(context.declaration_specifiers(), p);

            p = new ASTGenerationBuildParameters() {
                Parent = declNode,
                Context = DeclarationAST.DECLARATORS
            };
            VisitChildInContext(context.init_declarator_list(), p);

            return 0;
        }

        public override int VisitDeclaration_specifiers(CGrammarParser.Declaration_specifiersContext context) {
            // 1. Get current parent node
            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;


            // 2. Visit Declarations
            ASTGenerationBuildParameters parentContextParameters;
            if (context.type_specifier() != null && context.type_specifier().Length != 0) {
                if (parent.MType == (uint)TranslationUnitAST.NodeTypes.DECLARATION ||
                    parent.MType == (uint)TranslationUnitAST.NodeTypes.PARAMETER_DECLARATION ||
                    parent.MType == (uint)TranslationUnitAST.NodeTypes.FUNCTION_DEFINITION) {
                    parentContextParameters = new ASTGenerationBuildParameters() {
                        Parent = m_contexts.Peek().Parent,
                        Context = DeclarationAST.TYPE_SPECIFIER
                    };
                    VisitChildrenInContext(context.type_specifier(), parentContextParameters);
                }
            }

            if (context.type_qualifier() != null && context.type_qualifier().Length != 0) {
                if (parent.MType == (uint)TranslationUnitAST.NodeTypes.DECLARATION ||
                    parent.MType == (uint)TranslationUnitAST.NodeTypes.PARAMETER_DECLARATION ) {
                    parentContextParameters = new ASTGenerationBuildParameters() {
                        Parent = m_contexts.Peek().Parent,
                        Context = DeclarationAST.TYPE_QUALIFIER
                    };
                    VisitChildrenInContext(context.type_qualifier(), parentContextParameters);
                }
            }

            if (context.storage_class_specifier() != null && context.storage_class_specifier().Length != 0) {
                if (parent.MType == (uint)TranslationUnitAST.NodeTypes.DECLARATION ||
                    parent.MType == (uint)TranslationUnitAST.NodeTypes.PARAMETER_DECLARATION) {
                    parentContextParameters = new ASTGenerationBuildParameters() {
                        Parent = m_contexts.Peek().Parent,
                        Context = DeclarationAST.STORAGE_SPECIFIER
                    };
                    VisitChildrenInContext(context.storage_class_specifier(), parentContextParameters);
                }
            }


            return 0;

        }


        public override int VisitParameter_declaration(CGrammarParser.Parameter_declarationContext context) {

            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;


            ParameterDeclarationAST pardecl = new ParameterDeclarationAST();
            parent.AddChild(pardecl, currentContext.Context); // assuming context PARAMETER_DECLARATION for simplicity


            ASTGenerationBuildParameters p = new ASTGenerationBuildParameters() {
                Parent = pardecl,
                Context = null
            };
            VisitChildInContext(context.declaration_specifiers(), p);

            p = new ASTGenerationBuildParameters() {
                Parent = pardecl,
                Context = ParameterDeclarationAST.DECLARATOR
            };
            VisitChildInContext(context.declarator(), p);

            return 0;
        }

        public override int VisitPointer(CGrammarParser.PointerContext context) {

            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;

            PointerTypeAST pointerNode = new PointerTypeAST();
            parent.AddChild(pointerNode, currentContext.Context); // assuming context POINTER_TARGER for simplicity

            ASTGenerationBuildParameters p = new ASTGenerationBuildParameters() {
                Parent = pointerNode,
                Context = PointerTypeAST.POINTER_TARGET
            };

            if (context.pointer() == null) {
                m_lastCreatedNode = p;
            }

            if (context.pointer() != null) {
                VisitChildInContext(context.pointer(), p);
            }

            if (context.type_qualifier_list() != null) {
                VisitChildInContext(context.type_qualifier_list(), p);
            }

            return 0;
        }


        public override int VisitFunctionWithArguments(CGrammarParser.FunctionWithArgumentsContext context) {

            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;

            ASTGenerationBuildParameters paramContext;
            if (parent.MType == (uint)TranslationUnitAST.NodeTypes.FUNCTION_DEFINITION) {
                paramContext = new ASTGenerationBuildParameters() {
                    Parent = parent,
                    Context = FunctionDefinitionAST.DECLARATOR
                };
                VisitChildInContext(context.direct_declarator(), paramContext);

                paramContext = new ASTGenerationBuildParameters() {
                    Parent = parent,
                    Context = FunctionDefinitionAST.PARAMETER_DECLARATIONS
                };
                VisitChildInContext(context.parameter_type_list(), paramContext);
            } else {
                FunctionTypeAST funcTypeNode = new FunctionTypeAST();
                parent.AddChild(funcTypeNode, currentContext.Context); // assuming context FUNCTION_TYPE for simplicity

                paramContext = new ASTGenerationBuildParameters() {
                    Parent = parent,
                    Context = FunctionTypeAST.FUNCTION_NAME
                };
                VisitChildInContext(context.direct_declarator(), paramContext);

                paramContext = new ASTGenerationBuildParameters() {
                    Parent = funcTypeNode,
                    Context = FunctionTypeAST.FUNCTION_PARAMETERS
                };
                VisitChildInContext(context.parameter_type_list(), paramContext);

            }



            return 0;
        }



        public override int VisitTerminal(ITerminalNode node) {
            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;
            switch (node.Symbol.Type) {
                case CGrammarParser.IDENTIFIER:
                    IDENTIFIER idNode = new IDENTIFIER(node.GetText());

                    if (m_lastCreatedNode != null) {
                        m_lastCreatedNode.Parent.AddChild(idNode, m_lastCreatedNode.Context); // assuming context IDENTIFIER for simplicity
                        m_lastCreatedNode = null;
                    } else {
                        parent.AddChild(idNode, currentContext.Context); // assuming context IDENTIFIER for simplicity
                    }
                    break;
                case CGrammarParser.INT:
                    IntegerTypeAST intNode = new IntegerTypeAST(node.GetText());
                    parent.AddChild(intNode, currentContext.Context); // assuming context INT for simplicity

                    break;
                case CGrammarParser.CHAR:
                    CharTypeAST charNode = new CharTypeAST(node.GetText());
                    parent.AddChild(charNode, currentContext.Context); // assuming context INT for simplicity

                    break;
                case CGrammarParser.CONSTANT: 
                    INTEGER conNode = new INTEGER(node.GetText());
                    parent.AddChild(conNode, currentContext.Context); // assuming context INTEGER for simplicity
                    break;
                default:
                    break;
            }

            return 0;
        }


        public override int VisitCompound_statement(CGrammarParser.Compound_statementContext context) {

            // 1. Get current parent node
            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;

            // 2. Create FunctionDefinitionAST node
            CompoundStatement compStmtNode = new CompoundStatement();

            // 3. Add FunctionDefinitionAST node to parent
            parent.AddChild(compStmtNode, currentContext.Context); // assuming context


            ASTGenerationBuildParameters paramContext;
            paramContext = new ASTGenerationBuildParameters() {
                Parent = compStmtNode,
                Context = CompoundStatement.DECLARATIONS
            };
            VisitChildrenInContext(context.declaration(), paramContext);

            paramContext = new ASTGenerationBuildParameters() {
                Parent = compStmtNode,
                Context = CompoundStatement.STATEMENTS
            };
            VisitChildrenInContext(context.statement(), paramContext);


            return 0;
        }
        public override int VisitAssignment_expression_Assignment(
            CGrammarParser.Assignment_expression_AssignmentContext context) {
            // 1. Get current parent node
            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;

            // 2. Create Assignment node
            var aoperator = context.assignment_operator();
            ASTComposite aOperatorNode = null;
            switch (aoperator.op.Type) {
                case CGrammarLexer.ASSIGN:
                    aOperatorNode =
                        new Expression_Assignment();
                    break;
                case CGrammarLexer.MUL_ASSIGN:
                    aOperatorNode =
                        new ExpressionAssignmentMultiplication();
                    break;
                case CGrammarLexer.DIV_ASSIGN:
                    aOperatorNode =
                        new ExpressionAssignmentDivision();
                    break;
                case CGrammarLexer.PLUS:
                    aOperatorNode =
                        new UnaryExpressionUnaryOperatorPLUS();
                    break;
                case CGrammarLexer.ADD_ASSIGN:
                    aOperatorNode =
                        new ExpressionAssignmentAddition();
                    break;
                case CGrammarLexer.SUB_ASSIGN:
                    aOperatorNode =
                        new ExpressionAssignmentSubtraction();
                    break;
                case CGrammarLexer.MOD_ASSIGN:
                    aOperatorNode =
                        new ExpressionAssignmentModulo();
                    break;
                case CGrammarLexer.LEFT_ASSIGN:
                    aOperatorNode =
                        new Expression_AssignmentLeft();
                    break;
                case CGrammarLexer.RIGHT_ASSIGN:
                    aOperatorNode =
                        new Expression_AssignmentRight();
                    break;
                case CGrammarLexer.OR_ASSIGN:
                    aOperatorNode =
                        new Expression_AssignmentOr();
                    break;
                case CGrammarLexer.AND_ASSIGN:
                    aOperatorNode =
                        new Expression_AssignmentAnd();
                    break;
                case CGrammarLexer.XOR_ASSIGN:
                    aOperatorNode =
                        new Expression_AssignmentXor();
                    break;
                default:
                    throw new NotImplementedException("Unhandled unary operator type");

            }
            // 3. Add FunctionDefinitionAST node to parent
            parent.AddChild(aOperatorNode, currentContext.Context); // assuming context

            ASTGenerationBuildParameters paramContext;
            paramContext = new ASTGenerationBuildParameters() {
                Parent = aOperatorNode,
                Context = Expression_Assignment.LEFT
            };
            VisitChildInContext(context.unary_expression(), paramContext);
            paramContext = new ASTGenerationBuildParameters() {
                Parent = aOperatorNode,
                Context = Expression_Assignment.RIGHT
            };
            VisitChildInContext(context.assignment_expression(), paramContext);

            return 0;
        }

        public override int VisitAdditive_expression_Addition(CGrammarParser.Additive_expression_AdditionContext context) {
            // 1. Get current parent node
            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;

            // 2. Create Addition node
            Expression_Addition addition = new Expression_Addition();

            // 3. Add Addition node to parent
            parent.AddChild(addition, currentContext.Context);

            // 4. Visit left and right expressions
            ASTGenerationBuildParameters paramContext;
            paramContext = new ASTGenerationBuildParameters() {
                Parent = addition,
                Context = Expression_Addition.LEFT
            };
            VisitChildInContext(context.additive_expression(), paramContext);

            paramContext = new ASTGenerationBuildParameters() {
                Parent = addition,
                Context = Expression_Addition.RIGHT
            };
            VisitChildInContext(context.multiplicative_expression(), paramContext);


            return 0;
        }

        public override int VisitAdditive_expression_Subtraction(CGrammarParser.Additive_expression_SubtractionContext context) {
            // 1. Get current parent node
            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;

            // 2. Create Subtraction node
            Expression_Subtraction subtraction = new Expression_Subtraction();

            // 3. Add Subtraction node to parent
            parent.AddChild(subtraction, currentContext.Context);

            // 4. Visit left and right expressions
            ASTGenerationBuildParameters paramContext;
            paramContext = new ASTGenerationBuildParameters() {
                Parent = subtraction,
                Context = Expression_Subtraction.LEFT
            };
            VisitChildInContext(context.additive_expression(), paramContext);

            paramContext = new ASTGenerationBuildParameters() {
                Parent = subtraction,
                Context = Expression_Subtraction.RIGHT
            };
            VisitChildInContext(context.multiplicative_expression(), paramContext);


            return 0;
        }

        public override int VisitMultiplicative_expression_Multiplication(CGrammarParser.Multiplicative_expression_MultiplicationContext context)
        {
            // 1. Get current parent node
            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;

            // 2. Create Multiplication node
            Expression_Multiplication multiplication = new Expression_Multiplication();
            
            // 3. Add Multiplication node to parent
            parent.AddChild(multiplication, currentContext.Context);
            
            // 4. Visit left and right expressions
            ASTGenerationBuildParameters paramContext;
            paramContext = new ASTGenerationBuildParameters()
            {
                Parent = multiplication,
                Context = Expression_Multiplication.LEFT
            };
            VisitChildInContext(context.multiplicative_expression(), paramContext);
            paramContext = new ASTGenerationBuildParameters()
            {
                Parent = multiplication,
                Context = Expression_Multiplication.RIGHT
            };
            VisitChildInContext(context.cast_expression(), paramContext);

            return 0;

        }

        public override int VisitMultiplicative_expression_Division(CGrammarParser.Multiplicative_expression_DivisionContext context)
        {
            // 1. Get current parent node
            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;

            // 2. Create Division node
            Expression_Division division = new Expression_Division();
            
            // 3. Add Division node to parent
            parent.AddChild(division, currentContext.Context);
            
            // 4. Visit left and right expressions
            ASTGenerationBuildParameters paramContext;
            paramContext = new ASTGenerationBuildParameters()
            {
                Parent = division,
                Context = Expression_Division.LEFT
            };
            VisitChildInContext(context.multiplicative_expression(), paramContext);
            paramContext = new ASTGenerationBuildParameters()
                {
                Parent = division,
                Context = Expression_Division.RIGHT
            };
            VisitChildInContext(context.cast_expression(), paramContext);
            return 0;
        }

        public override int VisitMultiplicative_expression_Modulus(CGrammarParser.Multiplicative_expression_ModulusContext context)
        {
            // 1. Get current parent node
            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;

            // 2. Create Modulus node
            Expression_Modulo modulus = new Expression_Modulo();

            // 3. Add Modulus node to parent
            parent.AddChild(modulus, currentContext.Context);

            // 4. Visit left and right expressions
            ASTGenerationBuildParameters paramContext;
            paramContext = new ASTGenerationBuildParameters()
            {
                Parent = modulus,
                Context = Expression_Modulo.LEFT
            };
            VisitChildInContext(context.multiplicative_expression(), paramContext);
            paramContext = new ASTGenerationBuildParameters()
            {
                Parent = modulus,
                Context = Expression_Modulo.RIGHT
            };
            VisitChildInContext(context.cast_expression(), paramContext);
            return 0;
        }

        public override int VisitShift_expression_LeftShift(CGrammarParser.Shift_expression_LeftShiftContext context)
        {

            // 1. Get current parent node
            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;
            // 2. Create LeftShift node
            ExpressionShiftLeft leftShift = new ExpressionShiftLeft();
            // 3. Add LeftShift node to parent
            parent.AddChild(leftShift, currentContext.Context);
            // 4. Visit left and right expressions
            ASTGenerationBuildParameters paramContext;
            paramContext = new ASTGenerationBuildParameters()
            {
                Parent = leftShift,
                Context = ExpressionShiftLeft.LEFT
            };
            VisitChildInContext(context.shift_expression(), paramContext);
            paramContext = new ASTGenerationBuildParameters()
            {
                Parent = leftShift,
                Context = ExpressionShiftLeft.RIGHT
            };
            VisitChildInContext(context.additive_expression(), paramContext);

            return 0;

        }

        public override int VisitShift_expression_RightShift(CGrammarParser.Shift_expression_RightShiftContext context)
        {
            // 1. Get current parent node
            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;
            // 2. Create RightShift node
            ExpressionShiftRight rightShift = new ExpressionShiftRight();
            // 3. Add RightShift node to parent
            parent.AddChild(rightShift, currentContext.Context);
            // 4. Visit left and right expressions
            ASTGenerationBuildParameters paramContext;
            paramContext = new ASTGenerationBuildParameters()
            {
                Parent = rightShift,
                Context = ExpressionShiftRight.LEFT
            };
            VisitChildInContext(context.shift_expression(), paramContext);
            paramContext = new ASTGenerationBuildParameters()
            {
                Parent = rightShift,
                Context = ExpressionShiftRight.RIGHT
            };
            VisitChildInContext(context.additive_expression(), paramContext);
            return 0;
        }

        public override int VisitUnary_expression_Increment(CGrammarParser.Unary_expression_IncrementContext context) {
            // 1. Get current parent node
            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;

            // 2. Create FunctionDefinitionAST node
            UnaryExpressionIncrement unaryIncrement = new UnaryExpressionIncrement();

            // 3. Add FunctionDefinitionAST node to parent
            parent.AddChild(unaryIncrement, currentContext.Context); // assuming context
            ASTGenerationBuildParameters paramContext;
            paramContext = new ASTGenerationBuildParameters() {
                Parent = unaryIncrement,
                Context = UnaryExpressionIncrement.OPERAND
            };
            VisitChildInContext(context.unary_expression(), paramContext);
            return 0;
        }

        public override int VisitUnary_expression_Decrement(CGrammarParser.Unary_expression_DecrementContext context) {
            // 1. Get current parent node
            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;

            // 2. Create FunctionDefinitionAST node
            UnaryExpressionDecrement unaryDecrement = new UnaryExpressionDecrement();

            // 3. Add FunctionDefinitionAST node to parent
            parent.AddChild(unaryDecrement, currentContext.Context); // assuming context
            ASTGenerationBuildParameters paramContext;
            paramContext = new ASTGenerationBuildParameters() {
                Parent = unaryDecrement,
                Context = UnaryExpressionDecrement.OPERAND
            };
            VisitChildInContext(context.unary_expression(), paramContext);
            return 0;
        }

        public override int VisitUnary_expression_UnaryOperator(CGrammarParser.Unary_expression_UnaryOperatorContext context)
        {
            // 1. Get current parent node
            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;

            // 2. Check operator type and create corresponding node
            ASTComposite unaryOperatorNode = null;
            ASTGenerationBuildParameters paramContext;
            switch (context.unary_operator().op.Type)
            {
                case CGrammarLexer.AMBERSAND:
                    unaryOperatorNode = new UnaryExpressionUnaryOperatorAmbersand();
                    // 3. Add UnaryOperator node to parent
                    parent.AddChild(unaryOperatorNode, currentContext.Context); // assuming context
                    paramContext = new ASTGenerationBuildParameters()
                    {
                        Parent = unaryOperatorNode,
                        Context = UnaryExpressionUnaryOperatorAmbersand.OPERAND
                    };
                    VisitChildInContext(context.cast_expression(), paramContext);
                    return 0;
                case CGrammarLexer.ASTERISK:
                    unaryOperatorNode = new UnaryExpressionUnaryOperatorAsterisk();
                    // 3. Add UnaryOperator node to parent
                    parent.AddChild(unaryOperatorNode, currentContext.Context); // assuming context
                    paramContext = new ASTGenerationBuildParameters()
                    {
                        Parent = unaryOperatorNode,
                        Context = UnaryExpressionUnaryOperatorAsterisk.OPERAND
                    };
                    VisitChildInContext(context.cast_expression(), paramContext);
                    return 0;
                case CGrammarLexer.PLUS:
                    unaryOperatorNode = new UnaryExpressionUnaryOperatorPLUS();
                    // 3. Add UnaryOperator node to parent
                    parent.AddChild(unaryOperatorNode, currentContext.Context); // assuming context
                    paramContext = new ASTGenerationBuildParameters()
                    {
                        Parent = unaryOperatorNode,
                        Context = UnaryExpressionUnaryOperatorPLUS.OPERAND
                    };
                    VisitChildInContext(context.cast_expression(), paramContext);
                    return 0;
                case CGrammarLexer.HYPHEN:
                    unaryOperatorNode = new UnaryExpressionUnaryOperatorMINUS();
                    // 3. Add UnaryOperator node to parent
                    parent.AddChild(unaryOperatorNode, currentContext.Context); // assuming context
                    paramContext = new ASTGenerationBuildParameters()
                    {
                        Parent = unaryOperatorNode,
                        Context = UnaryExpressionUnaryOperatorMINUS.OPERAND
                    };
                    VisitChildInContext(context.cast_expression(), paramContext);
                    return 0;
                case CGrammarLexer.TILDE:
                    unaryOperatorNode = new UnaryExpressionUnaryOperatorTilde();
                    // 3. Add UnaryOperator node to parent
                    parent.AddChild(unaryOperatorNode, currentContext.Context); // assuming context
                    paramContext = new ASTGenerationBuildParameters()
                    {
                        Parent = unaryOperatorNode,
                        Context = UnaryExpressionUnaryOperatorTilde.OPERAND
                    };
                    VisitChildInContext(context.cast_expression(), paramContext);
                    return 0;
                case CGrammarLexer.NOT:
                    unaryOperatorNode = new UnaryExpressionUnaryOperatorNOT();
                    // 3. Add UnaryOperator node to parent
                    parent.AddChild(unaryOperatorNode, currentContext.Context); // assuming context
                    paramContext = new ASTGenerationBuildParameters()
                    {
                        Parent = unaryOperatorNode,
                        Context = UnaryExpressionUnaryOperatorNOT.OPERAND
                    };
                    VisitChildInContext(context.cast_expression(), paramContext);
                    return 0;
                default:
                    throw new NotImplementedException("Unhandled unary operator type");
            }
            
        }

        public override int VisitPostfix_expression_Decrement(
            CGrammarParser.Postfix_expression_DecrementContext context)
        {
            // 1. Get current parent node
            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;

            // 2. Create PostfixexpressionDecrement node
            Postfixexpression_Decrement postfixexpressionDecrement = new Postfixexpression_Decrement();
            
            // 3. Add PostfixexpressionDecrement node to parent
            parent.AddChild(postfixexpressionDecrement, currentContext.Context); // assuming context
            ASTGenerationBuildParameters paramContext;
            paramContext = new ASTGenerationBuildParameters() {
                Parent = postfixexpressionDecrement,
                Context = Postfixexpression_Decrement.ACCESS
            };
            VisitChildInContext(context.postfix_expression(), paramContext);


            return 0;
        }

        public override int VisitPostfix_expression_Increment(
            CGrammarParser.Postfix_expression_IncrementContext context)
        {
            // 1. Get current parent node
            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;

            // 2. Create PostfixexpressionIncrement node
            Postfixexpression_Increment postfixexpressionIncrement = new Postfixexpression_Increment();
            
            // 3. Add PostfixexpressionIncrement node to parent
            parent.AddChild(postfixexpressionIncrement, currentContext.Context); // assuming context
            ASTGenerationBuildParameters paramContext;
            paramContext = new ASTGenerationBuildParameters() {
                Parent = postfixexpressionIncrement,
                Context = Postfixexpression_Increment.ACCESS
            };
            VisitChildInContext(context.postfix_expression(), paramContext);


            return 0;
        }

        public override int VisitPostfix_expression_ArraySubscript(CGrammarParser.Postfix_expression_ArraySubscriptContext context)
        {
            // 1. Get current parent node
            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;

            // 2. Create PostfixexpressionArraySubscript node
            Postfixexpression_ArraySubscript postfixexpressionArraySubscript = new Postfixexpression_ArraySubscript();
            // 3. Add PostfixexpressionArraySubscript node to parent
            parent.AddChild(postfixexpressionArraySubscript, currentContext.Context); // assuming context
            ASTGenerationBuildParameters paramContext;
            paramContext = new ASTGenerationBuildParameters()
            {
                Parent = postfixexpressionArraySubscript,
                Context = Postfixexpression_ArraySubscript.ARRAY
            };
            VisitChildInContext(context.postfix_expression(), paramContext);
            ASTGenerationBuildParameters indexParamContext = new ASTGenerationBuildParameters()
            {
                Parent = postfixexpressionArraySubscript,
                Context = Postfixexpression_ArraySubscript.INDEX
            };
            VisitChildInContext(context.expression(),indexParamContext);
            

            return 0;

        }

        public override int VisitFunctionWithNOArguments(CGrammarParser.FunctionWithNOArgumentsContext context)
        {
            // 1. Get current parent node
            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;

            if (parent.MType == (uint)TranslationUnitAST.NodeTypes.FUNCTION_DEFINITION)
            {
                ASTGenerationBuildParameters paramContext = new ASTGenerationBuildParameters()
                {
                    Parent = parent,
                    Context = FunctionDefinitionAST.DECLARATOR
                };
                VisitChildInContext(context.direct_declarator(), paramContext);
            }
            else
            {
                FunctionTypeAST funcTypeNode = new FunctionTypeAST();
                parent.AddChild(funcTypeNode, currentContext.Context); // assuming context FUNCTION_TYPE for simplicity
                ASTGenerationBuildParameters paramContext = new ASTGenerationBuildParameters()
                {
                    Parent = parent,
                    Context = FunctionTypeAST.FUNCTION_NAME
                };
                VisitChildInContext(context.direct_declarator(), paramContext);
            }


            return 0;
        }

        public override int VisitArrayDimensionWithSIZE(CGrammarParser.ArrayDimensionWithSIZEContext context)
        {
            // 1. Get current parent node
            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;

            // 2. Create ArrayDimensionWithSIZE node
            ArrayDimensionWithSIZE arrayDimensionWithSIZE = new ArrayDimensionWithSIZE();
            // 3. Add ArrayDimensionWithSIZE node to parent
            parent.AddChild(arrayDimensionWithSIZE, currentContext.Context); // assuming context
            ASTGenerationBuildParameters paramContext;
            paramContext = new ASTGenerationBuildParameters()
            {
                Parent = arrayDimensionWithSIZE,
                Context = ArrayDimensionWithSIZE.ARRAY
            };
            VisitChildInContext(context.direct_declarator(), paramContext);
            ASTGenerationBuildParameters sizeParamContext = new ASTGenerationBuildParameters()
            {
                Parent = arrayDimensionWithSIZE,
                Context = ArrayDimensionWithSIZE.SIZE
            };
            VisitChildInContext(context.constant_expression(), sizeParamContext);

            return 0;

        }

        public override int VisitArrayDimensionWithNOSIZE(CGrammarParser.ArrayDimensionWithNOSIZEContext context)
        {
            // 1. Get current parent node
            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;

            // 2. Create ArrayDimensionWithNOSIZE node
            ArrayDimensionWithNOSIZE arrayDimensionWithNOSIZE = new ArrayDimensionWithNOSIZE();

            // 3. Add ArrayDimensionWithNOSIZE node to parent
            parent.AddChild(arrayDimensionWithNOSIZE, currentContext.Context); // assuming context
            ASTGenerationBuildParameters paramContext;
            paramContext = new ASTGenerationBuildParameters()
            {
                Parent = arrayDimensionWithNOSIZE,
                Context = ArrayDimensionWithNOSIZE.ARRAY
            };
            VisitChildInContext(context.direct_declarator(), paramContext);

            return 0;
        }

        public override int VisitPostfix_expression_FunctionCallWithArgs(CGrammarParser.Postfix_expression_FunctionCallWithArgsContext context)
        {
            // 1. Get current parent node
            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;

            // 2. Create PostfixexpressionFunctionCallWithArgs node
            Postfixexpression_FunctionCallWithArgs postfixexpressionFunctionCallWithArgs = new Postfixexpression_FunctionCallWithArgs();
            
            // 3. Add PostfixexpressionFunctionCallWithArgs node to parent
            parent.AddChild(postfixexpressionFunctionCallWithArgs, currentContext.Context); // assuming context
            ASTGenerationBuildParameters paramContext;
            paramContext = new ASTGenerationBuildParameters()
            {
                Parent = postfixexpressionFunctionCallWithArgs,
                Context = Postfixexpression_FunctionCallWithArgs.FUNCTION
            };
            VisitChildInContext(context.postfix_expression(), paramContext);
            ASTGenerationBuildParameters argsParamContext = new ASTGenerationBuildParameters()
            {
                Parent = postfixexpressionFunctionCallWithArgs,
                Context = Postfixexpression_FunctionCallWithArgs.ARGUMENTS
            };
            VisitChildInContext(context.argument_expression_list(), argsParamContext);


            return 0;
        }


        public override int VisitPostfix_expression_FunctionCallNoArgs(
            CGrammarParser.Postfix_expression_FunctionCallNoArgsContext context)
        {
            // 1. Get current parent node
            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;

            // 2. Create PostfixexpressionFunctionCallNoArgs node
            Postfixexpression_FunctionCallNoArgs postfixexpressionFunctionCallNoArgs = new Postfixexpression_FunctionCallNoArgs();

            // 3. Add PostfixexpressionFunctionCallNoArgs node to parent
            parent.AddChild(postfixexpressionFunctionCallNoArgs, currentContext.Context); // assuming context
            ASTGenerationBuildParameters paramContext;
            paramContext = new ASTGenerationBuildParameters()
            {
                Parent = postfixexpressionFunctionCallNoArgs,
                Context = Postfixexpression_FunctionCallNoArgs.FUNCTION
            };
            VisitChildInContext(context.postfix_expression(), paramContext);


            return 0;
        }



        public override int VisitPostfix_expression_PointerMemberAccess(CGrammarParser.Postfix_expression_PointerMemberAccessContext context)
        {
            // 1. Get current parent node
            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;

            // 2. Create PointerMemberAccess node
            Postfixexpression_PointerMemberAccess pointerMemberAccess = new Postfixexpression_PointerMemberAccess();

            // 3. Add PointerMemberAccess node to parent
            parent.AddChild(pointerMemberAccess, currentContext.Context); // assuming context
            ASTGenerationBuildParameters paramContext;
            paramContext = new ASTGenerationBuildParameters()
            {
                Parent = pointerMemberAccess,
                Context = Postfixexpression_PointerMemberAccess.ACCESS
            };
            VisitChildInContext(context.postfix_expression(), paramContext);

            return 0;
        }

        public override int VisitPostfix_expression_MemberAccess(CGrammarParser.Postfix_expression_MemberAccessContext context)
        {
            // 1. Get current parent node
            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;

            // 2. Create MemberAccess node
            Postfixexpression_MemberAccess memberAccess = new Postfixexpression_MemberAccess();

            // 3. Add MemberAccess node to parent
            parent.AddChild(memberAccess, currentContext.Context); // assuming context
            ASTGenerationBuildParameters paramContext;
            paramContext = new ASTGenerationBuildParameters()
            {
                Parent = memberAccess,
                Context = Postfixexpression_MemberAccess.ACCESS
            };
            VisitChildInContext(context.postfix_expression(), paramContext);

            return 0;
        }

        public override int VisitUnary_expression_SizeofExpression(CGrammarParser.Unary_expression_SizeofExpressionContext context)
        {
            // 1. Get current parent node
            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;

            // 2. Create SizeofExpression node
            UnaryExpressionSizeOfExpression sizeofExpression = new UnaryExpressionSizeOfExpression();

            // 3. Add SizeofExpression node to parent
            parent.AddChild(sizeofExpression, currentContext.Context); // assuming context
            ASTGenerationBuildParameters paramContext;
            paramContext = new ASTGenerationBuildParameters()
            {
                Parent = sizeofExpression,
                Context = UnaryExpressionSizeOfExpression.EXPRESSION
            };
            VisitChildInContext(context.unary_expression(), paramContext);

            return 0;
        }

        public override int VisitUnary_expression_SizeofTypeName(CGrammarParser.Unary_expression_SizeofTypeNameContext context)
        {
            // 1. Get current parent node
            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;

            // 2. Create SizeofTypeName node
            UnaryExpressionSizeOfTypeName sizeofExpression = new UnaryExpressionSizeOfTypeName();

            // 3. Add SizeofExpression node to parent
            parent.AddChild(sizeofExpression, currentContext.Context); // assuming context
            ASTGenerationBuildParameters paramContext;
            paramContext = new ASTGenerationBuildParameters()
            {
                Parent = sizeofExpression,
                Context = UnaryExpressionSizeOfTypeName.TYPE
            };
            VisitChildInContext(context.type_name(), paramContext);

            return 0;
        }

        public override int VisitRelational_expression_LessThan(
            CGrammarParser.Relational_expression_LessThanContext context)
        {
            // 1. Get current parent node
            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;

            // 2. Create GreaterThanOrEqual node
            ExpressionRelationalLess lessThan = new ExpressionRelationalLess();

            // 3. Add GreaterThanOrEqual node to parent
            parent.AddChild(lessThan, currentContext.Context);

            // 4. Visit left and right expressions
            ASTGenerationBuildParameters paramContext;
            paramContext = new ASTGenerationBuildParameters()
            {
                Parent = lessThan,
                Context = ExpressionRelationalLess.LEFT
            };
            VisitChildInContext(context.relational_expression(), paramContext);
            paramContext = new ASTGenerationBuildParameters()
            {
                Parent = lessThan,
                Context = ExpressionRelationalLess.RIGHT
            };
            VisitChildInContext(context.shift_expression(), paramContext);

            return 0;
        }

        public override int VisitRelational_expression_GreaterThan(CGrammarParser.Relational_expression_GreaterThanContext context)
        {
            // 1. Get current parent node
            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;

            // 2. Create GreaterThan node
            ExpressionRelationalGreater greaterThan = new ExpressionRelationalGreater();

            // 3. Add GreaterThan node to parent
            parent.AddChild(greaterThan, currentContext.Context);

            // 4. Visit left and right expressions
            ASTGenerationBuildParameters paramContext;
            paramContext = new ASTGenerationBuildParameters()
            {
                Parent = greaterThan,
                Context = ExpressionRelationalGreater.LEFT
            };
            VisitChildInContext(context.relational_expression(), paramContext);
            paramContext = new ASTGenerationBuildParameters()
            {
                Parent = greaterThan,
                Context = ExpressionRelationalGreater.RIGHT
            };
            VisitChildInContext(context.shift_expression(), paramContext);

            return 0;
        }

        public override int VisitRelational_expression_GreaterThanOrEqual(
            CGrammarParser.Relational_expression_GreaterThanOrEqualContext context)
        {
            // 1. Get current parent node
            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;

            // 2. Create GreaterThanOrEqual node
            ExpressionRelationalGreaterOrEqual greaterThanOrEqual = new ExpressionRelationalGreaterOrEqual();

            // 3. Add GreaterThanOrEqual node to parent
            parent.AddChild(greaterThanOrEqual, currentContext.Context);

            // 4. Visit left and right expressions
            ASTGenerationBuildParameters paramContext;
            paramContext = new ASTGenerationBuildParameters()
            {
                Parent = greaterThanOrEqual,
                Context = ExpressionRelationalGreaterOrEqual.LEFT
            };
            VisitChildInContext(context.relational_expression(), paramContext);
            paramContext = new ASTGenerationBuildParameters()
            {
                Parent = greaterThanOrEqual,
                Context = ExpressionRelationalGreaterOrEqual.RIGHT
            };
            VisitChildInContext(context.shift_expression(), paramContext);

            return 0;
        }

        public override int VisitRelational_expression_LessThanOrEqual(CGrammarParser.Relational_expression_LessThanOrEqualContext context)
        {
            // 1. Get current parent node
            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;

            // 2. Create GreaterThanOrEqual node
            ExpressionRelationalLessOrEqual lessThanOrEqual = new ExpressionRelationalLessOrEqual();

            // 3. Add GreaterThanOrEqual node to parent
            parent.AddChild(lessThanOrEqual, currentContext.Context);

            // 4. Visit left and right expressions
            ASTGenerationBuildParameters paramContext;
            paramContext = new ASTGenerationBuildParameters()
            {
                Parent = lessThanOrEqual,
                Context = ExpressionRelationalLessOrEqual.LEFT
            };
            VisitChildInContext(context.relational_expression(), paramContext);
            paramContext = new ASTGenerationBuildParameters()
            {
                Parent = lessThanOrEqual,
                Context = ExpressionRelationalLessOrEqual.RIGHT
            };
            VisitChildInContext(context.shift_expression(), paramContext);

            return 0;
        }

        public override int VisitEquality_expression_Equal(CGrammarParser.Equality_expression_EqualContext context)
        {
            // 1. Get current parent node
            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;

            // 2. Create Equal node
            Expression_EqualityEqual equalNode = new Expression_EqualityEqual();

            // 3. Add Equal node to parent
            parent.AddChild(equalNode, currentContext.Context);
            
            // 4. Visit left and right expressions
            ASTGenerationBuildParameters paramContext;
            paramContext = new ASTGenerationBuildParameters()
            {
                Parent = equalNode,
                Context = Expression_EqualityEqual.LEFT
            };
            VisitChildInContext(context.equality_expression(), paramContext);
            paramContext = new ASTGenerationBuildParameters()
            {
                Parent = equalNode,
                Context = Expression_EqualityEqual.RIGHT
            };
            VisitChildInContext(context.relational_expression(), paramContext);
            return 0;
        }



        public override int VisitEquality_expression_NotEqual(
            CGrammarParser.Equality_expression_NotEqualContext context)
        {
            // 1. Get current parent node
            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;

            // 2. Create NotEqual node
            Expression_EqualityNotEqual notEqualNode = new Expression_EqualityNotEqual();

            // 3. Add NotEqual node to parent
            parent.AddChild(notEqualNode, currentContext.Context);

            // 4. Visit left and right expressions
            ASTGenerationBuildParameters paramContext;
            paramContext = new ASTGenerationBuildParameters()
            {
                Parent = notEqualNode,
                Context = Expression_EqualityNotEqual.LEFT
            };
            VisitChildInContext(context.equality_expression(), paramContext);
            paramContext = new ASTGenerationBuildParameters()
            {
                Parent = notEqualNode,
                Context = Expression_EqualityNotEqual.RIGHT
            };
            VisitChildInContext(context.relational_expression(), paramContext);

            return 0;

        }

        public override int VisitAnd_expression_BitwiseAND(CGrammarParser.And_expression_BitwiseANDContext context)
        {
            // 1. Get current parent node
            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;

            // 2. Create BitwiseAND node
            Expression_BitwiseAND bitwiseAndNode = new Expression_BitwiseAND();

            // 3. Add BitwiseAND node to parent
            parent.AddChild(bitwiseAndNode, currentContext.Context);
            // 4. Visit left and right expressions
            ASTGenerationBuildParameters paramContext;
            paramContext = new ASTGenerationBuildParameters()
            {
                Parent = bitwiseAndNode,
                Context = Expression_BitwiseAND.LEFT
            };
            VisitChildInContext(context.and_expression(), paramContext);
            paramContext = new ASTGenerationBuildParameters()
            {
                Parent = bitwiseAndNode,
                Context = Expression_BitwiseAND.RIGHT
            };
            VisitChildInContext(context.equality_expression(),paramContext);

            return 0;
        }

        public override int VisitInclusive_or_expression_BitwiseOR(CGrammarParser.Inclusive_or_expression_BitwiseORContext context)
        {
            // 1. Get current parent node
            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;

            // 2. Create BitwiseAND node
            Expression_BitwiseOR bitwiseOrNode = new Expression_BitwiseOR();

            // 3. Add BitwiseAND node to parent
            parent.AddChild(bitwiseOrNode, currentContext.Context);
            // 4. Visit left and right expressions
            ASTGenerationBuildParameters paramContext;
            paramContext = new ASTGenerationBuildParameters()
            {
                Parent = bitwiseOrNode,
                Context = Expression_BitwiseOR.LEFT
            };
            VisitChildInContext(context.inclusive_or_expression(), paramContext);
            paramContext = new ASTGenerationBuildParameters()
            {
                Parent = bitwiseOrNode,
                Context = Expression_BitwiseOR.RIGHT
            };
            VisitChildInContext(context.exclusive_or_expression(), paramContext);

            return 0;
        }

        public override int VisitExclusive_or_expression_BitwiseXOR(CGrammarParser.Exclusive_or_expression_BitwiseXORContext context)
        {
            // 1. Get current parent node
            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;

            // 2. Create BitwiseXOR node
            Expression_BitwiseXOR bitwiseXorNode = new Expression_BitwiseXOR();
            // 3. Add BitwiseXOR node to parent
            parent.AddChild(bitwiseXorNode, currentContext.Context);
            // 4. Visit left and right expressions
            ASTGenerationBuildParameters paramContext;
            paramContext = new ASTGenerationBuildParameters()
            {
                Parent = bitwiseXorNode,
                Context = Expression_BitwiseXOR.LEFT
            };
            VisitChildInContext(context.exclusive_or_expression(), paramContext);
            paramContext = new ASTGenerationBuildParameters()
            {
                Parent = bitwiseXorNode,
                Context = Expression_BitwiseXOR.RIGHT
            };
            VisitChildInContext(context.and_expression(), paramContext);

            return 0;
        }

        public override int VisitLogical_and_expression_LogicalAND(CGrammarParser.Logical_and_expression_LogicalANDContext context)
        {
            // 1. Get current parent node
            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;

            // 2. Create LogicalAND node
            ExpressionLogicalAnd logicalAndNode = new ExpressionLogicalAnd();
            // 3. Add LogicalAND node to parent
            parent.AddChild(logicalAndNode, currentContext.Context);

            // 4. Visit left and right expressions
            ASTGenerationBuildParameters paramContext;
            paramContext = new ASTGenerationBuildParameters()
            {
                Parent = logicalAndNode,
                Context = ExpressionLogicalAnd.LEFT
            };
            VisitChildInContext(context.logical_and_expression(), paramContext);
            paramContext = new ASTGenerationBuildParameters()
            {
                Parent = logicalAndNode,
                Context = ExpressionLogicalAnd.RIGHT
            };
            VisitChildInContext(context.inclusive_or_expression(), paramContext);


            return 0;
        }



        public override int VisitLogical_or_expression_LogicalOR(CGrammarParser.Logical_or_expression_LogicalORContext context)
        {
            // 1. Get current parent node
            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;

            // 2. Create LogicalAND node
            ExpressionLogicalOr logicalOrNode = new ExpressionLogicalOr();
            // 3. Add LogicalAND node to parent
            parent.AddChild(logicalOrNode, currentContext.Context);

            // 4. Visit left and right expressions
            ASTGenerationBuildParameters paramContext;
            paramContext = new ASTGenerationBuildParameters()
            {
                Parent = logicalOrNode,
                Context = ExpressionLogicalOr.LEFT
            };
            VisitChildInContext(context.logical_or_expression(), paramContext);
            paramContext = new ASTGenerationBuildParameters()
            {
                Parent = logicalOrNode,
                Context = ExpressionLogicalOr.RIGHT
            };
            VisitChildInContext(context.logical_and_expression(), paramContext);

            return 0;
        }

        public override int VisitCast_expression_Cast(CGrammarParser.Cast_expression_CastContext context)
        {
            // 1. Get current parent node
            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;

            // 2. Create Cast node
            Expression_Cast castNode = new Expression_Cast();

            // 3. Add Cast node to parent
            parent.AddChild(castNode, currentContext.Context);

            // 4. Visit type name and cast expression
            ASTGenerationBuildParameters paramContext;
            paramContext = new ASTGenerationBuildParameters()
            {
                Parent = castNode,
                Context = Expression_Cast.TYPE
            };
            VisitChildInContext(context.type_name(), paramContext);
            paramContext = new ASTGenerationBuildParameters()
            {
                Parent = castNode,
                Context = Expression_Cast.EXPRESSION
            };
            VisitChildInContext(context.cast_expression(), paramContext);


            return 0;
        }

        public override int VisitExpression_CommaExpression(CGrammarParser.Expression_CommaExpressionContext context)
        {
            // 1. Get current parent node
            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;

            // 2. Create CommaExpression node
            Expression_CommaExpression commaExpressionNode = new Expression_CommaExpression();

            // 3. Add CommaExpression node to parent
            parent.AddChild(commaExpressionNode, currentContext.Context);

            // 4. Visit left and right expressions
            ASTGenerationBuildParameters paramContext;
            paramContext = new ASTGenerationBuildParameters() {
                Parent = commaExpressionNode,
                Context = Expression_CommaExpression.LEFT
            }; 
            VisitChildInContext(context.expression(), paramContext);
            paramContext = new ASTGenerationBuildParameters()
            {
                Parent = commaExpressionNode,
                Context = Expression_CommaExpression.RIGHT
            };
            VisitChildInContext(context.assignment_expression(), paramContext);
            return 0;

        }

        public override int VisitConditional_expression_Conditional(CGrammarParser.Conditional_expression_ConditionalContext context)
        {
            // 1. Get current parent node
            ASTGenerationBuildParameters currentContext = m_contexts.Peek();
            ASTComposite parent = currentContext.Parent;

            // 2. Create Conditional node
            ConditionalExpression conditionalNode = new ConditionalExpression();

            // 3. Add Conditional node to parent
            parent.AddChild(conditionalNode, currentContext.Context);

            // 4. Visit logical_or_expression, expression, and conditional_expression
            ASTGenerationBuildParameters paramContext;
            paramContext = new ASTGenerationBuildParameters()
            {
                Parent = conditionalNode,
                Context = ConditionalExpression.CONDITION
            };
            VisitChildInContext(context.logical_or_expression(), paramContext);
            paramContext = new ASTGenerationBuildParameters()
            {
                Parent = conditionalNode,
                Context = ConditionalExpression.TRUE_EXPRESSION
            };
            VisitChildInContext(context.expression(), paramContext);
            paramContext = new ASTGenerationBuildParameters()
            {
                Parent = conditionalNode,
                Context = ConditionalExpression.FALSE_EXPRESSION
            };
            VisitChildInContext(context.conditional_expression(), paramContext);

            return 0;
        }


        /*

        
           
           
           
           
           
           
           
           
           
           
           public override int VisitExpression_statement(CGrammarParser.Expression_statementContext context)
           {
               ASTComposite parent = m_contexts.Peek();
           
               Statement_Expression node = new Statement_Expression();
           
               parent.AddChild(node, parent.GetContextForChild(context));
           
               m_contexts.Push(node);
               base.VisitExpression_statement(context);
               m_contexts.Pop();
           
               return 0;
           }


        
              

        */




    }
}
