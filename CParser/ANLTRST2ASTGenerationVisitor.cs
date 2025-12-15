using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
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
                Context = FunctionDefinitionAST.DECLARATOR
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
                    parent.MType == (uint)TranslationUnitAST.NodeTypes.PARAMETER_DECLARATION) {
                    parentContextParameters = new ASTGenerationBuildParameters() {
                        Parent = m_contexts.Peek().Parent,
                        Context = DeclarationAST.TYPE_SPECIFIER
                    };
                    VisitChildrenInContext(context.type_specifier(), parentContextParameters);
                }
            }

            if (context.type_qualifier() != null && context.type_qualifier().Length != 0) {
                if (parent.MType == (uint)TranslationUnitAST.NodeTypes.DECLARATION ||
                    parent.MType == (uint)TranslationUnitAST.NodeTypes.PARAMETER_DECLARATION) {
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
                /*case CGrammarParser.CONSTANT: {
                    ASTComposite parent = m_contexts.Peek();
                    INTEGER idNode = new INTEGER(node.GetText());
                    parent.AddChild(idNode, parent.GetContextForChild(node)); // assuming context INTEGER for simplicity
                }
                    break;

                
                case CGrammarParser.CHAR: {
                    ASTComposite parent = m_contexts.Peek();
                    CharTypeAST intNode = new CharTypeAST(node.GetText());
                    parent.AddChild(intNode, parent.GetContextForChild(node)); // assuming context INT for simplicity
                }
                    break;*/
                // Handle other terminal types as needed
                default:
                    break;
            }

            return 0;
        }

        /*
        public override int VisitFunctionWithNOArguments(CGrammarParser.FunctionWithNOArgumentsContext context) {

            ASTComposite parent = m_contexts.Peek();


            switch (parent.MType) {
                case (uint)TranslationUnitAST.NodeTypes.FUNCTION_DEFINITION:
                    base.VisitFunctionWithNOArguments(context);
                    break;
                default:
                    FunctionTypeAST funcTypeNode = new FunctionTypeAST();

                    parent.AddChild(funcTypeNode, parent.GetContextForChild(context)); // assuming context FUNCTION_TYPE for simplicity

                    m_contexts.Push(funcTypeNode);
                    base.VisitFunctionWithNOArguments(context);
                    m_contexts.Pop();
                    break;
            }

            return 0;
        }

        

       


        



        public override int VisitArrayDimensionWithSIZE(CGrammarParser.ArrayDimensionWithSIZEContext context) {
            return base.VisitArrayDimensionWithSIZE(context);
        }

        public override int VisitArrayDimensionWithNOSIZE(CGrammarParser.ArrayDimensionWithNOSIZEContext context) {
            return base.VisitArrayDimensionWithNOSIZE(context);
        }

        

        

        public override int VisitCompound_statement(CGrammarParser.Compound_statementContext context) {

            // 1. Get current parent node
            ASTComposite parent = m_contexts.Peek();

            // 2. Create FunctionDefinitionAST node
            CompoundStatement compStmtNode = new CompoundStatement();

            // 3. Add FunctionDefinitionAST node to parent
            parent.AddChild(compStmtNode, parent.GetContextForChild(context)); // assuming context

            m_contexts.Push(compStmtNode);
            base.VisitCompound_statement(context);
            m_contexts.Pop();


            return 0;
        }

        public override int VisitPostfix_expression_ArraySubscript(CGrammarParser.Postfix_expression_ArraySubscriptContext context) {

            // 1. Get current parent node
            ASTComposite parent = m_contexts.Peek();

            // 2. Create FunctionDefinitionAST node
            Postfixexpression_ArraySubscript arrsbArraySubscript = new Postfixexpression_ArraySubscript();

            // 3. Add FunctionDefinitionAST node to parent
            parent.AddChild(arrsbArraySubscript, parent.GetContextForChild(context)); // assuming context

            m_contexts.Push(arrsbArraySubscript);
            base.VisitPostfix_expression_ArraySubscript(context);
            m_contexts.Pop();

            return 0;
        }

        public override int VisitPostfix_expression_Decrement(
            CGrammarParser.Postfix_expression_DecrementContext context) {
            // 1. Get current parent node
            ASTComposite parent = m_contexts.Peek();

            // 2. Create FunctionDefinitionAST node
            Postfixexpression_Decrement postfixexpressionDecrement = new Postfixexpression_Decrement();

            // 3. Add FunctionDefinitionAST node to parent
            parent.AddChild(postfixexpressionDecrement, parent.GetContextForChild(context)); // assuming context

            m_contexts.Push(postfixexpressionDecrement);
            base.VisitPostfix_expression_Decrement(context);
            m_contexts.Pop();

            return 0;
        }

        public override int VisitPostfix_expression_Increment(
            CGrammarParser.Postfix_expression_IncrementContext context) {
            // 1. Get current parent node
            ASTComposite parent = m_contexts.Peek();

            // 2. Create FunctionDefinitionAST node
            Postfixexpression_Increment postfixexpressionIncrement = new Postfixexpression_Increment();

            // 3. Add FunctionDefinitionAST node to parent
            parent.AddChild(postfixexpressionIncrement, parent.GetContextForChild(context)); // assuming context

            m_contexts.Push(postfixexpressionIncrement);
            base.VisitPostfix_expression_Increment(context);
            m_contexts.Pop();

            return 0;
        }

        public override int VisitPostfix_expression_FunctionCallNoArgs(
            CGrammarParser.Postfix_expression_FunctionCallNoArgsContext context) {

            // 1. Get current parent node
            ASTComposite parent = m_contexts.Peek();

            // 2. Create FunctionDefinitionAST node
            Postfixexpression_FunctionCallNoArgs postfixexpressionFunctionCallNoArgs =
                new Postfixexpression_FunctionCallNoArgs();

            // 3. Add FunctionDefinitionAST node to parent
            parent.AddChild(postfixexpressionFunctionCallNoArgs, parent.GetContextForChild(context)); // assuming context

            m_contexts.Push(postfixexpressionFunctionCallNoArgs);
            base.VisitPostfix_expression_FunctionCallNoArgs(context);
            m_contexts.Pop();

            return 0;
        }

        public override int VisitPostfix_expression_FunctionCallWithArgs(CGrammarParser.Postfix_expression_FunctionCallWithArgsContext context) {

            // 1. Get current parent node
            ASTComposite parent = m_contexts.Peek();

            // 2. Create FunctionDefinitionAST node
            Postfixexpression_FunctionCallWithArgs postfixexpressionFunctionCallWithArgs =
                new Postfixexpression_FunctionCallWithArgs();

            // 3. Add FunctionDefinitionAST node to parent
            parent.AddChild(postfixexpressionFunctionCallWithArgs, parent.GetContextForChild(context)); // assuming context

            m_contexts.Push(postfixexpressionFunctionCallWithArgs);
            base.VisitPostfix_expression_FunctionCallWithArgs(context);
            m_contexts.Pop();

            return 0;
        }

        public override int VisitPostfix_expression_PointerMemberAccess(CGrammarParser.Postfix_expression_PointerMemberAccessContext context) {
            // 1. Get current parent node
            ASTComposite parent = m_contexts.Peek();

            // 2. Create FunctionDefinitionAST node
            Postfixexpression_PointerMemberAccess pointerMemberAccess =
                new Postfixexpression_PointerMemberAccess();

            // 3. Add FunctionDefinitionAST node to parent
            parent.AddChild(pointerMemberAccess, parent.GetContextForChild(context)); // assuming context

            m_contexts.Push(pointerMemberAccess);
            base.VisitPostfix_expression_PointerMemberAccess(context);
            m_contexts.Pop();

            return 0;
        }

        public override int VisitPostfix_expression_MemberAccess(CGrammarParser.Postfix_expression_MemberAccessContext context) {

            // 1. Get current parent node
            ASTComposite parent = m_contexts.Peek();

            // 2. Create FunctionDefinitionAST node
            Postfixexpression_MemberAccess memberAccess =
                new Postfixexpression_MemberAccess();

            // 3. Add FunctionDefinitionAST node to parent
            parent.AddChild(memberAccess, parent.GetContextForChild(context)); // assuming context

            m_contexts.Push(memberAccess);
            base.VisitPostfix_expression_MemberAccess(context);
            m_contexts.Pop();

            return 0;
        }

        public override int VisitUnary_expression_UnaryOperator(CGrammarParser.Unary_expression_UnaryOperatorContext context) {

            // 1. Get current parent node
            ASTComposite parent = m_contexts.Peek();

            // 2. Create FunctionDefinitionAST node
            var uoperator = context.unary_operator();
            ASTComposite unaryOperatorNode = null;
            switch (uoperator.op.Type) {
                case CGrammarLexer.AMBERSAND:
                    unaryOperatorNode =
                        new UnaryExpressionUnaryOperatorAmbersand();
                    break;
                case CGrammarLexer.ASTERISK:
                    unaryOperatorNode =
                        new UnaryExpressionUnaryOperatorAsterisk();
                    break;
                case CGrammarLexer.PLUS:
                    unaryOperatorNode =
                        new UnaryExpressionUnaryOperatorPLUS();
                    break;
                case CGrammarLexer.HYPHEN:
                    unaryOperatorNode =
                        new UnaryExpressionUnaryOperatorMINUS();
                    break;
                case CGrammarLexer.TILDE:
                    unaryOperatorNode =
                        new UnaryExpressionUnaryOperatorTilde();
                    break;
                case CGrammarLexer.NOT:
                    unaryOperatorNode =
                        new UnaryExpressionUnaryOperatorNOT();
                    break;
                default:
                    throw new NotImplementedException("Unhandled unary operator type");

            }
            // 3. Add FunctionDefinitionAST node to parent
            parent.AddChild(unaryOperatorNode, parent.GetContextForChild(context)); // assuming context

            m_contexts.Push(unaryOperatorNode);
            base.VisitUnary_expression_UnaryOperator(context);
            m_contexts.Pop();

            return 0;
        }

        public override int VisitRelational_expression_LessThan(
            CGrammarParser.Relational_expression_LessThanContext context) {
            // 1. Get current parent node
            ASTComposite parent = m_contexts.Peek();

            // 2. Create FunctionDefinitionAST node
            ExpressionRelationalLess less =
                new ExpressionRelationalLess();

            // 3. Add FunctionDefinitionAST node to parent
            parent.AddChild(less, parent.GetContextForChild(context)); // assuming context

            m_contexts.Push(less);
            base.VisitRelational_expression_LessThan(context);
            m_contexts.Pop();
            return 0;
        }

        public override int VisitRelational_expression_GreaterThan(CGrammarParser.Relational_expression_GreaterThanContext context) {
            // 1. Get current parent node
            ASTComposite parent = m_contexts.Peek();

            // 2. Create FunctionDefinitionAST node
            ExpressionRelationalGreater greater =
                new ExpressionRelationalGreater();

            // 3. Add FunctionDefinitionAST node to parent
            parent.AddChild(greater, parent.GetContextForChild(context)); // assuming context

            m_contexts.Push(greater);
            base.VisitRelational_expression_GreaterThan(context);
            m_contexts.Pop();
            return 0;
        }

        public override int VisitRelational_expression_GreaterThanOrEqual(
            CGrammarParser.Relational_expression_GreaterThanOrEqualContext context) {
            // 1. Get current parent node
            ASTComposite parent = m_contexts.Peek();

            // 2. Create FunctionDefinitionAST node
            ExpressionRelationalGreaterOrEqual greaterOrEqual =
                new ExpressionRelationalGreaterOrEqual();

            // 3. Add FunctionDefinitionAST node to parent
            parent.AddChild(greaterOrEqual, parent.GetContextForChild(context)); // assuming context

            m_contexts.Push(greaterOrEqual);
            base.VisitRelational_expression_GreaterThanOrEqual(context);
            m_contexts.Pop();
            return 0;
        }

        public override int VisitRelational_expression_LessThanOrEqual(CGrammarParser.Relational_expression_LessThanOrEqualContext context) {
            // 1. Get current parent node
            ASTComposite parent = m_contexts.Peek();

            // 2. Create FunctionDefinitionAST node
            ExpressionRelationalLessOrEqual lessOrEqual =
                new ExpressionRelationalLessOrEqual();

            // 3. Add FunctionDefinitionAST node to parent
            parent.AddChild(lessOrEqual, parent.GetContextForChild(context)); // assuming context

            m_contexts.Push(lessOrEqual);
            base.VisitRelational_expression_LessThanOrEqual(context);
            m_contexts.Pop();
            return 0;
        }

        public override int VisitEquality_expression_Equal(CGrammarParser.Equality_expression_EqualContext context) {
            // 1. Get current parent node
            ASTComposite parent = m_contexts.Peek();

            // 2. Create FunctionDefinitionAST node
            Expression_EqualityEqual equal =
                new Expression_EqualityEqual();

            // 3. Add FunctionDefinitionAST node to parent
            parent.AddChild(equal, parent.GetContextForChild(context)); // assuming context

            m_contexts.Push(equal);
            base.VisitEquality_expression_Equal(context);
            m_contexts.Pop();
            return 0;
        }

        public override int VisitUnary_expression_Increment(CGrammarParser.Unary_expression_IncrementContext context) {
            // 1. Get current parent node
            ASTComposite parent = m_contexts.Peek();

            // 2. Create FunctionDefinitionAST node
            UnaryExpressionIncrement unaryIncrement = new UnaryExpressionIncrement();

            // 3. Add FunctionDefinitionAST node to parent
            parent.AddChild(unaryIncrement, parent.GetContextForChild(context)); // assuming context

            m_contexts.Push(unaryIncrement);
            base.VisitUnary_expression_Increment(context);
            m_contexts.Pop();


            return 0;
        }

        public override int VisitUnary_expression_Decrement(CGrammarParser.Unary_expression_DecrementContext context) {
            // 1. Get current parent node
            ASTComposite parent = m_contexts.Peek();

            // 2. Create FunctionDefinitionAST node
            UnaryExpressionDecrement unaryDecrement = new UnaryExpressionDecrement();

            // 3. Add FunctionDefinitionAST node to parent
            parent.AddChild(unaryDecrement, parent.GetContextForChild(context)); // assuming context

            m_contexts.Push(unaryDecrement);
            base.VisitUnary_expression_Decrement(context);
            m_contexts.Pop();

            return 0;
        }

        public override int VisitUnary_expression_SizeofExpression(CGrammarParser.Unary_expression_SizeofExpressionContext context) {
            // 1. Get current parent node
            ASTComposite parent = m_contexts.Peek();

            // 2. Create FunctionDefinitionAST node
            UnaryExpressionSizeOfExpression UnarySizeOfExpression = new UnaryExpressionSizeOfExpression();

            // 3. Add FunctionDefinitionAST node to parent
            parent.AddChild(UnarySizeOfExpression, parent.GetContextForChild(context)); // assuming context

            m_contexts.Push(UnarySizeOfExpression);
            base.VisitUnary_expression_SizeofExpression(context);
            m_contexts.Pop();

            return 0;
        }

        public override int VisitUnary_expression_SizeofTypeName(CGrammarParser.Unary_expression_SizeofTypeNameContext context) {
            // 1. Get current parent node
            ASTComposite parent = m_contexts.Peek();

            // 2. Create FunctionDefinitionAST node
            UnaryExpressionSizeOfTypeName UnarySizeOfType = new UnaryExpressionSizeOfTypeName();

            // 3. Add FunctionDefinitionAST node to parent
            parent.AddChild(UnarySizeOfType, parent.GetContextForChild(context)); // assuming context

            m_contexts.Push(UnarySizeOfType);
            base.VisitUnary_expression_SizeofTypeName(context);
            m_contexts.Pop();

            return 0;
        }

        public override int VisitEquality_expression_NotEqual(
            CGrammarParser.Equality_expression_NotEqualContext context) {
            // 1. Get current parent node
            ASTComposite parent = m_contexts.Peek();

            // 2. Create FunctionDefinitionAST node
            Expression_EqualityNotEqual notequal =
                new Expression_EqualityNotEqual();

            // 3. Add FunctionDefinitionAST node to parent
            parent.AddChild(notequal, parent.GetContextForChild(context)); // assuming context

            m_contexts.Push(notequal);
            base.VisitEquality_expression_NotEqual(context);
            m_contexts.Pop();
            return 0;

        }

        public override int VisitAnd_expression_BitwiseAND(CGrammarParser.And_expression_BitwiseANDContext context) {
            // 1. Get current parent node
            ASTComposite parent = m_contexts.Peek();

            // 2. Create FunctionDefinitionAST node
            Expression_BitwiseAND AndExpression = new Expression_BitwiseAND();

            // 3. Add FunctionDefinitionAST node to parent
            parent.AddChild(AndExpression, parent.GetContextForChild(context)); // assuming context

            m_contexts.Push(AndExpression);
            base.VisitAnd_expression_BitwiseAND(context);
            m_contexts.Pop();

            return 0;
        }

        public override int VisitCast_expression_Cast(CGrammarParser.Cast_expression_CastContext context) {
            ASTComposite parent = m_contexts.Peek();

            Expression_Cast node = new Expression_Cast();

            parent.AddChild(node, parent.GetContextForChild(context));

            m_contexts.Push(node);
            base.VisitCast_expression_Cast(context);
            m_contexts.Pop();

            return 0;
        }


        public override int VisitMultiplicative_expression_Division(CGrammarParser.Multiplicative_expression_DivisionContext context) {
            ASTComposite parent = m_contexts.Peek();

            ExpressionDivision node = new ExpressionDivision();

            parent.AddChild(node, parent.GetContextForChild(context));

            m_contexts.Push(node);
            base.VisitMultiplicative_expression_Division(context);
            m_contexts.Pop();

            return 0;
        }

        public override int VisitMultiplicative_expression_Multiplication(CGrammarParser.Multiplicative_expression_MultiplicationContext context) {
            ASTComposite parent = m_contexts.Peek();

            ExpressionMultiplication node = new ExpressionMultiplication();

            parent.AddChild(node, parent.GetContextForChild(context));

            m_contexts.Push(node);
            base.VisitMultiplicative_expression_Multiplication(context);
            m_contexts.Pop();

            return 0;
        }

        public override int VisitMultiplicative_expression_Modulus(CGrammarParser.Multiplicative_expression_ModulusContext context) {
            ASTComposite parent = m_contexts.Peek();

            ExpressionModulus node = new ExpressionModulus();

            parent.AddChild(node, parent.GetContextForChild(context));

            m_contexts.Push(node);
            base.VisitMultiplicative_expression_Modulus(context);
            m_contexts.Pop();

            return 0;
        }

        public override int VisitAdditive_expression_Addition(CGrammarParser.Additive_expression_AdditionContext context) {
            ASTComposite parent = m_contexts.Peek();

            Expression_Addition node = new Expression_Addition();

            parent.AddChild(node, parent.GetContextForChild(context));

            m_contexts.Push(node);
            base.VisitAdditive_expression_Addition(context);
            m_contexts.Pop();

            return 0;
        }



        public override int VisitAdditive_expression_Subtraction(CGrammarParser.Additive_expression_SubtractionContext context) {
            ASTComposite parent = m_contexts.Peek();

            Expression_Subtraction node = new Expression_Subtraction();

            parent.AddChild(node, parent.GetContextForChild(context));

            m_contexts.Push(node);
            base.VisitAdditive_expression_Subtraction(context);
            m_contexts.Pop();

            return 0;
        }



        public override int VisitLogical_and_expression_LogicalAND(CGrammarParser.Logical_and_expression_LogicalANDContext context) {
            ASTComposite parent = m_contexts.Peek();

            ExpressionLogicalAnd node = new ExpressionLogicalAnd();

            parent.AddChild(node, parent.GetContextForChild(context));

            m_contexts.Push(node);
            base.VisitLogical_and_expression_LogicalAND(context);
            m_contexts.Pop();

            return 0;
        }



        public override int VisitLogical_or_expression_LogicalOR(CGrammarParser.Logical_or_expression_LogicalORContext context) {
            ASTComposite parent = m_contexts.Peek();

            ExpressionLogicalOr node = new ExpressionLogicalOr();

            parent.AddChild(node, parent.GetContextForChild(context));

            m_contexts.Push(node);
            base.VisitLogical_or_expression_LogicalOR(context);
            m_contexts.Pop();

            return 0;
        }


        public override int VisitConditional_expression_Conditional(CGrammarParser.Conditional_expression_ConditionalContext context) {
            ASTComposite parent = m_contexts.Peek();

            ConditionalExpression node = new ConditionalExpression();

            parent.AddChild(node, parent.GetContextForChild(context));

            m_contexts.Push(node);
            base.VisitConditional_expression_Conditional(context);
            m_contexts.Pop();

            return 0;
        }

        public override int VisitAssignment_expression_Assignment(
            CGrammarParser.Assignment_expression_AssignmentContext context) {
            // 1. Get current parent node
            ASTComposite parent = m_contexts.Peek();

            // 2. Create FunctionDefinitionAST node
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
            parent.AddChild(aOperatorNode, parent.GetContextForChild(context)); // assuming context

            m_contexts.Push(aOperatorNode);
            base.VisitAssignment_expression_Assignment(context);
            m_contexts.Pop();

            return 0;
        }

        public override int VisitExpression_statement(CGrammarParser.Expression_statementContext context) {
            ASTComposite parent = m_contexts.Peek();

            Statement_Expression node = new Statement_Expression();

            parent.AddChild(node, parent.GetContextForChild(context));

            m_contexts.Push(node);
            base.VisitExpression_statement(context);
            m_contexts.Pop();

            return 0;
        }*/
    }
}
