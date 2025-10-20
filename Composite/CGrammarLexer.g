lexer grammar CGrammarLexer;


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