grammar calculator;

// Parser rules

compilationUnit	: ( expression ';' )+ 
	;

expression : INT
;

// Lexer rules

INT        : [0-9]+ ;
SEMICOLON   : ';' ;
WS         : [ \t\r\n]+ -> skip; 