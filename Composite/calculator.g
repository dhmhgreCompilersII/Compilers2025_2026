grammar calculator;

// Parser rules

primary_expression
	: IDENTIFIER
	| CONSTANT
	| STRING_LITERAL
	| '(' expression ')'
	;

postfix_expression
	: primary_expression
	| postfix_expression '[' expression ']'
	| postfix_expression '(' ')'
	| postfix_expression '(' argument_expression_list ')'
	| postfix_expression '.' IDENTIFIER
	| postfix_expression PTR_OP IDENTIFIER
	| postfix_expression INC_OP
	| postfix_expression DEC_OP
	;

argument_expression_list
	: assignment_expression
	| argument_expression_list ',' assignment_expression
	;

unary_expression
	: postfix_expression
	| INC_OP unary_expression
	| DEC_OP unary_expression
	| unary_operator cast_expression
	| SIZEOF unary_expression
	| SIZEOF '(' type_name ')'
	;

unary_operator
	: '&'
	| '*'
	| '+'
	| '-'
	| '~'
	| '!'
	;

cast_expression
	: unary_expression
	| '(' type_name ')' cast_expression
	;

multiplicative_expression
	: cast_expression
	| multiplicative_expression '*' cast_expression
	| multiplicative_expression '/' cast_expression
	| multiplicative_expression '%' cast_expression
	;

additive_expression
	: multiplicative_expression
	| additive_expression '+' multiplicative_expression
	| additive_expression '-' multiplicative_expression
	;

shift_expression
	: additive_expression
	| shift_expression LEFT_OP additive_expression
	| shift_expression RIGHT_OP additive_expression
	;

relational_expression
	: shift_expression
	| relational_expression '<' shift_expression
	| relational_expression '>' shift_expression
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
	| and_expression '&' equality_expression
	;

exclusive_or_expression
	: and_expression
	| exclusive_or_expression '^' and_expression
	;

inclusive_or_expression
	: exclusive_or_expression
	| inclusive_or_expression '|' exclusive_or_expression
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
	| logical_or_expression '?' expression ':' conditional_expression
	;

assignment_expression
	: conditional_expression
	| unary_expression assignment_operator assignment_expression
	;

assignment_operator
	: '='
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
	| expression ',' assignment_expression
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
	: POINTER direct_declarator
	| direct_declarator
	;

direct_declarator
	: IDENTIFIER
	| '(' declarator ')'
	| direct_declarator '[' constant_expression ']'
	| direct_declarator '[' ']'
	| direct_declarator '(' parameter_type_list ')'
	| direct_declarator '(' ')'
	;

type_name
	: specifier_qualifier_list
	| specifier_qualifier_list abstract_declarator
	;

pointer
	: '*'
	| '*' type_qualifier_list
	| '*' pointer
	| '*' type_qualifier_list pointer
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
	: '(' abstract_declarator ')'
	| '[' ']'
	| '[' constant_expression ']'
	| direct_abstract_declarator '[' ']'
	| direct_abstract_declarator '[' constant_expression ']'
	| '(' ')'
	| '(' parameter_type_list ')'
	| direct_abstract_declarator '(' ')'
	| direct_abstract_declarator '(' parameter_type_list ')'
	;


function_definition	: declaration_specifiers declarator compound_statement
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
	: struct_or_union IDENTIFIER? '{' struct_declaration+ '}'
	| struct_or_union IDENTIFIER
	;

struct_or_union
	: STRUCT
	| UNION
	;

struct_declaration
	: specifier_qualifier_list struct_declarator_list ';'
	;

specifier_qualifier_list : (type_specifier | type_qualifier )+
	;

struct_declarator_list	: struct_declarator (',' struct_declarator)*
	;

struct_declarator
	: declarator
	| ':' constant_expression
	| declarator ':' constant_expression
	;

enum_specifier
	: ENUM '{' enumerator_list '}'
	| ENUM IDENTIFIER '{' enumerator_list '}'
	| ENUM IDENTIFIER
	;

enumerator_list
	: enumerator
	| enumerator_list ',' enumerator
	;

enumerator
	: IDENTIFIER
	| IDENTIFIER '=' constant_expression
	;


type_qualifier	: CONST
	| VOLATILE
	;


parameter_type_list : declaration ( COMMA declaration )* (COMMA ELLIPSIS)?
	;
	
compound_statement : '{' declaration* statement+ '}'
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
	: IDENTIFIER ':' statement
	| CASE constant_expression ':' statement
	| DEFAULT ':' statement
	;


expression_statement
	: ';'
	| expression ';'
	;

selection_statement
	: IF '(' expression ')' statement
	| IF '(' expression ')' statement ELSE statement
	| SWITCH '(' expression ')' statement
	;

iteration_statement
	: WHILE '(' expression ')' statement
	| DO statement WHILE '(' expression ')' ';'
	| FOR '(' expression_statement expression_statement ')' statement
	| FOR '(' expression_statement expression_statement expression ')' statement
	;

jump_statement
	: GOTO IDENTIFIER ';'
	| CONTINUE ';'
	| BREAK ';'
	| RETURN ';'
	| RETURN expression ';'
	;


// Lexer rules

AUTO : 'auto';		
BREAK : 'break';
CASE : 'case';
CHAR : 'char';
CONST : 'const';
CONTNUE : 'continue';
DEFAULT : 'default';
DO :'do';
DOUBLE : 'double';
ELSE :'else';
ENUM: 'enum';
EXTERN : 'extern';
FLOAT :'float';
FOR :'for';
GOTO : 'goto';
IF :'if';
INT : 'int';
LONG :'long';
REGISTER : 'register';
RETURN : 'return';
SHORT :'short';
SIGNED: 'signed';
SIZEOF: 'sizeof';
STATIC : 'static';
STRUCT :'struct';
SWITCH :'switch';
TYPEDEF :'typedef';
UNION : 'union';
UNSIGNED :'unsigned';
VOID :'void';
VOLATILE :'volatile';
WHILE :'while';

ELLIPSIS :'...';
RIGHT_ASSIGN : '>>=';
LEFT_ASSIGN : '<<=';
ADD_ASSIGN : '+=';
SUB_ASSIGN : '-=';
MUL_ASSIGN : '*=';
DIV_ASSIGN :'/=';
MOD_ASSIGN : '%=';
AND_ASSIGN : '&=';
XOR_ASSIGN : '^=';
OR_ASSIGN : '|=';
RIGHT_OP : '>>';
LEFT_OP :'<<';
INC_OP :'++';	
DEC_OP :'--';
PTR_OP : '->';		
AND_OP : '&&';
OR_OP : '||';	
LE_OP :'<=';
GE_OP : '>=';
EQ_OP : '==';
NE_OP :'!=';
SEMICOLON : ';';
LBRACE : ('{'|'<%');
RBRACE : ('}'|'%>');
COMMA : ',';
COLON :':';
ASSIGN :'=';
LPAREN : '(';
RPAREN : ')';
LBRACKET :('['|'<:');
RBRACKET : (']'|':>');	
MEMBEROP : '.';
AMBERSAND :'&';
NOT : '!';
TILDE : '~';
HYPHEN : '-';
PLUS : '+';
ASTERISK : '*';
SLASH : '/';
PERCENT : '%';
LESS :'<';
GREATER :'>';
CARET : '^';
OR : '|';
QMARK :'?';

{L}({L}|{D})*		{ count(); return(check_type()); }

0[xX]{H}+{IS}?		{ count(); return(CONSTANT); }
0{D}+{IS}?		{ count(); return(CONSTANT); }
{D}+{IS}?		{ count(); return(CONSTANT); }
L?'(\\.|[^\\'])+'	{ count(); return(CONSTANT); }

{D}+{E}{FS}?		{ count(); return(CONSTANT); }
{D}*"."{D}+({E})?{FS}?	{ count(); return(CONSTANT); }
{D}+"."{D}*({E})?{FS}?	{ count(); return(CONSTANT); }

L?\"(\\.|[^\\"])*\"	{ count(); return(STRING_LITERAL); }




"/*"			{ comment(); }

[ \t\v\n\f]		{ count(); }
.			{ /* ignore bad characters */ }