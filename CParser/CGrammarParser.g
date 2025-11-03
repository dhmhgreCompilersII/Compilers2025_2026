parser grammar CGrammarParser;

options { tokenVocab = CGrammarLexer; }

// Parser rules

primary_expression
	: IDENTIFIER
	| CONSTANT
	| STRING_LITERAL
	| LPAREN expression RPAREN
	;

postfix_expression
	: primary_expression
	| postfix_expression LBRACKET expression RBRACKET
	| postfix_expression LPAREN RPAREN
	| postfix_expression LPAREN argument_expression_list RPAREN
	| postfix_expression MEMBEROP IDENTIFIER
	| postfix_expression PTR_OP IDENTIFIER
	| postfix_expression INC_OP
	| postfix_expression DEC_OP
	;

argument_expression_list : assignment_expression (COMMA assignment_expression)*
	;

unary_expression
	: postfix_expression
	| INC_OP unary_expression
	| DEC_OP unary_expression
	| unary_operator cast_expression
	| SIZEOF unary_expression
	| SIZEOF LPAREN type_name RPAREN
	;

unary_operator
	: AMBERSAND
	| ASTERISK
	| PLUS
	| HYPHEN
	| TILDE
	| NOT
	;

cast_expression
	: unary_expression
	| LPAREN type_name RPAREN cast_expression
	;

multiplicative_expression
	: cast_expression
	| multiplicative_expression ASTERISK cast_expression
	| multiplicative_expression SLASH cast_expression
	| multiplicative_expression PERCENT cast_expression
	;

additive_expression
	: multiplicative_expression
	| additive_expression PLUS multiplicative_expression
	| additive_expression HYPHEN multiplicative_expression
	;

shift_expression
	: additive_expression
	| shift_expression LEFT_OP additive_expression
	| shift_expression RIGHT_OP additive_expression
	;

relational_expression
	: shift_expression
	| relational_expression LESS shift_expression
	| relational_expression GREATER shift_expression
	| relational_expression LE_OP shift_expression
	| relational_expression GE_OP shift_expression
	;

equality_expression
	: relational_expression
	| equality_expression EQ_OP relational_expression
	| equality_expression NE_OP relational_expression
	;

and_expression
	: equality_expression
	| and_expression AMBERSAND equality_expression
	;

exclusive_or_expression
	: and_expression
	| exclusive_or_expression CARET and_expression
	;

inclusive_or_expression
	: exclusive_or_expression
	| inclusive_or_expression OR exclusive_or_expression
	;

logical_and_expression
	: inclusive_or_expression
	| logical_and_expression AND_OP inclusive_or_expression
	;

logical_or_expression
	: logical_and_expression
	| logical_or_expression OR_OP logical_and_expression
	;

conditional_expression
	: logical_or_expression
	| logical_or_expression QMARK expression COLON conditional_expression
	;

assignment_expression
	: conditional_expression
	| unary_expression assignment_operator assignment_expression
	;

assignment_operator
	: ASSIGN
	| MUL_ASSIGN
	| DIV_ASSIGN
	| MOD_ASSIGN
	| ADD_ASSIGN
	| SUB_ASSIGN
	| LEFT_ASSIGN
	| RIGHT_ASSIGN
	| AND_ASSIGN
	| XOR_ASSIGN
	| OR_ASSIGN
	;

expression
	: assignment_expression
	| expression COMMA assignment_expression
	;

constant_expression
	: conditional_expression
	;

translation_unit : external_declaration+ EOF
;

external_declaration : function_definition
					 | declaration
					 ;

declaration	: declaration_specifiers init_declarator_list? SEMICOLON
	;

init_declarator_list: init_declarator (COMMA init_declarator)*
	;

init_declarator	:  declarator (ASSIGN initializer)?
	;

initializer
	: assignment_expression
	| LBRACE initializer_list RBRACE	
	;

initializer_list: initializer (COMMA initializer)*
	;

declarator
	: pointer direct_declarator
	| direct_declarator
	;

direct_declarator
	: IDENTIFIER
	| LPAREN declarator RPAREN
	| direct_declarator LBRACKET constant_expression RBRACKET
	| direct_declarator LBRACKET RBRACKET
	| direct_declarator LPAREN parameter_type_list RPAREN
	| direct_declarator LPAREN RPAREN
	;

type_name
	: specifier_qualifier_list
	| specifier_qualifier_list abstract_declarator
	;

pointer
	: ASTERISK
	| ASTERISK type_qualifier_list
	| ASTERISK pointer
	| ASTERISK type_qualifier_list pointer
	;

type_qualifier_list
	: type_qualifier
	| type_qualifier_list type_qualifier
	;

abstract_declarator
	: pointer
	| direct_abstract_declarator
	| pointer direct_abstract_declarator
	;

direct_abstract_declarator
	: LPAREN abstract_declarator RPAREN
	| LBRACKET RBRACKET
	| LBRACKET constant_expression RBRACKET
	| direct_abstract_declarator LBRACKET RBRACKET
	| direct_abstract_declarator LBRACKET constant_expression RBRACKET
	| LPAREN RPAREN
	| LPAREN parameter_type_list RPAREN
	| direct_abstract_declarator LPAREN RPAREN
	| direct_abstract_declarator LPAREN parameter_type_list RPAREN
	;


function_definition	: declaration_specifiers  declarator  compound_statement
					;

declaration_specifiers : (storage_class_specifier|type_specifier|type_qualifier)+
	;

storage_class_specifier : TYPEDEF
	| EXTERN
	| STATIC
	| AUTO
	| REGISTER;

type_specifier :  VOID
	| CHAR
	| SHORT
	| INT
	| LONG
	| FLOAT
	| DOUBLE
	| SIGNED
	| UNSIGNED
	| struct_or_union_specifier
	| enum_specifier
	| TYPE_NAME
	;

struct_or_union_specifier
	: struct_or_union IDENTIFIER? LBRACE struct_declaration+ RBRACE
	| struct_or_union IDENTIFIER
	;

struct_or_union
	: STRUCT
	| UNION
	;

struct_declaration
	: specifier_qualifier_list struct_declarator_list SEMICOLON
	;

specifier_qualifier_list : (type_specifier | type_qualifier )+
	;

struct_declarator_list	: struct_declarator (COMMA struct_declarator)*
	;

struct_declarator
	: declarator
	| COLON constant_expression
	| declarator COLON constant_expression
	;

enum_specifier
	: ENUM LBRACE enumerator_list RBRACE
	| ENUM IDENTIFIER LBRACE enumerator_list RBRACE
	| ENUM IDENTIFIER
	;

enumerator_list : enumerator (COMMA enumerator)* ;
	

enumerator:  IDENTIFIER (ASSIGN constant_expression)?
	;


type_qualifier	: CONST
	| VOLATILE
	;


parameter_type_list : parameter_declaration ( COMMA parameter_declaration )* (COMMA ELLIPSIS)?
	;

parameter_declaration : declaration_specifiers declarator
	| declaration_specifiers abstract_declarator?
	;
	
compound_statement : LBRACE declaration* statement+ RBRACE
	;

statement
	: labeled_statement
	| compound_statement
	| expression_statement
	| selection_statement
	| iteration_statement
	| jump_statement
	;

labeled_statement
	: IDENTIFIER COLON statement
	| CASE constant_expression COLON statement
	| DEFAULT COLON statement
	;


expression_statement
	: SEMICOLON
	| expression SEMICOLON
	;

selection_statement
	: IF LPAREN expression RPAREN statement
	| IF LPAREN expression RPAREN statement ELSE statement
	| SWITCH LPAREN expression RPAREN statement
	;

iteration_statement
	: WHILE LPAREN expression RPAREN statement
	| DO statement WHILE LPAREN expression RPAREN SEMICOLON
	| FOR LPAREN expression_statement expression_statement RPAREN statement
	| FOR LPAREN expression_statement expression_statement expression RPAREN statement
	;

jump_statement
	: GOTO IDENTIFIER SEMICOLON
	| CONTINUE SEMICOLON
	| BREAK SEMICOLON
	| RETURN SEMICOLON
	| RETURN expression SEMICOLON
	;


