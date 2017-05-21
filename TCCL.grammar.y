%namespace ASTBuilder
%using Project3
%using static Project3.FMNodes; 

%partial
%parsertype TCCLParser
%visibility public
%tokentype Token
%YYSTYPE AbstractNode

%{ 
%}

%start CompilationUnit

%token STATIC, STRUCT, QUESTION, RSLASH, MINUSOP, NULL, INT, OP_EQ, OP_LT, COLON, OP_LOR
%token ELSE, PERCENT, THIS, CLASS, PIPE, PUBLIC, PERIOD, HAT, COMMA, VOID, TILDE
%token LPAREN, RPAREN, OP_GE, SEMICOLON, IF, NEW, WHILE, PRIVATE, BANG, OP_LE, AND 
%token LBRACE, RBRACE, LBRACKET, RBRACKET, BOOLEAN, INSTANCEOF, ASTERISK, EQUALS, PLUSOP
%token RETURN, OP_GT, OP_NE, OP_LAND, INT_NUMBER, IDENTIFIER, LITERAL, SUPER

%right EQUALS
%left  OP_LOR
%left  OP_LAND
%left  PIPE
%left  HAT
%left  AND
%left  OP_EQ, OP_NE
%left  OP_GT, OP_LT, OP_LE, OP_GE
%left  PLUSOP, MINUSOP
%left  ASTERISK, RSLASH, PERCENT
%left  UNARY 

%%

CompilationUnit		:	ClassDeclaration	{ $$ = new CompilationUnit($1); }
					;

ClassDeclaration	:	Modifiers CLASS Identifier ClassBody	{ $$ = MakeClassDeclaration($1, $3, $4); }
					;

Modifiers			:	PUBLIC		{ $$ = MakeModifiers(ModifiersEnums.PUBLIC); }
					|	PRIVATE		{ $$ = MakeModifiers(ModifiersEnums.PRIVATE); }
					|	STATIC		{ $$ = MakeModifiers(ModifiersEnums.STATIC); }
					|	Modifiers PUBLIC	{ $$ = MakeModifiers($1, ModifiersEnums.PUBLIC); }
					|	Modifiers PRIVATE	{ $$ = MakeModifiers($1, ModifiersEnums.PRIVATE); }
					|	Modifiers STATIC	{ $$ = MakeModifiers($1, ModifiersEnums.STATIC); }
					;

ClassBody			:	LBRACE FieldDeclarations RBRACE	{ $$ = MakeClassBody($2); }
					|	LBRACE RBRACE					{ $$ = MakeClassBody(); }
					;

FieldDeclarations	:	FieldDeclaration					{ $$ = MakeFieldDeclarations($1); }
					|	FieldDeclarations FieldDeclaration	{ $$ = MakeFieldDeclarations($1, $2); }
					;

FieldDeclaration	:	FieldVariableDeclaration SEMICOLON	{ $$ = $1; }
					|	MethodDeclaration					{ $$ = $1; }
					|	ConstructorDeclaration				{ $$ = $1; }
					|	StaticInitializer					{ $$ = $1; }
					|	StructDeclaration					{ $$ = $1; }
					;

StructDeclaration	:	Modifiers STRUCT Identifier ClassBody	{ $$ = MakeStructDeclaration($1, $3, $4); }
					;



/*
 * This isn't structured so nicely for a bottom up parse.  Recall
 * the example I did in class for Digits, where the "type" of the digits
 * (i.e., the base) is sitting off to the side.  You'll have to do something
 * here to get the information where you want it, so that the declarations can
 * be suitably annotated with their type and modifier information.
 */
FieldVariableDeclaration	:	Modifiers TypeSpecifier FieldVariableDeclarators	{ $$ = MakeFieldVariableDeclaration($1, $2, $3); }
							;

TypeSpecifier				:	TypeName		{ $$ = $1; }
							| 	ArraySpecifier	{ $$ = $1; }
							;

TypeName					:	PrimitiveType	{ $$ = $1; }
							|   QualifiedName	{ $$ = $1; }
							;

ArraySpecifier				: 	TypeName LBRACKET RBRACKET	{ $$ = MakeArraySpecifier($1); }
							;
							
PrimitiveType				:	BOOLEAN	{ $$ = MakePrimitiveTypeBoolean(); }
							|	INT		{ $$ = MakePrimitiveTypeInt(); }
							|	VOID	{ $$ = MakePrimitiveTypeVoid(); }
							;

FieldVariableDeclarators	:	FieldVariableDeclaratorName	{ $$ = MakeFieldVariableDeclarators($1); }
							|   FieldVariableDeclarators COMMA FieldVariableDeclaratorName	{ $$ = MakeFieldVariableDeclarators($1, $3); }
							;


MethodDeclaration			:	Modifiers TypeSpecifier MethodDeclarator MethodBody	{ $$ = MakeMethodDeclaration($1, $2, $3, $4); }
							;

MethodDeclarator			:	MethodDeclaratorName LPAREN ParameterList RPAREN	{ $$ = MakeMethodDeclarator($1, $3); }
							|   MethodDeclaratorName LPAREN RPAREN					{ $$ = MakeMethodDeclarator($1); }
							;

ParameterList				:	Parameter						{ $$ = MakeParameterList($1); }
							|   ParameterList COMMA Parameter	{ $$ = MakeParameterList($1, $3); }
							;

Parameter					:	TypeSpecifier DeclaratorName	{ $$ = MakeParameter($1, $2); }
							;

QualifiedName				:	Identifier						{ $$ = MakeQualifiedName($1); }
							|	QualifiedName PERIOD Identifier	{ $$ = MakeQualifiedName($1, $3); }
							;

DeclaratorName				:	Identifier	{ $$ = $1; }	
							;

MethodDeclaratorName		:	Identifier	{ $$ = $1; }
							;

FieldVariableDeclaratorName	:	Identifier	{ $$ = $1; }
							;

LocalVariableDeclaratorName	:	Identifier	{ $$ = $1; }
							;

MethodBody					:	Block		{ $$ = $1; }
							;

ConstructorDeclaration		:	Modifiers MethodDeclarator Block	{ $$ = MakeConstructorDeclaration($1, $2, $3); }
							;

StaticInitializer			:	STATIC Block	{ $$ = MakeStaticInitializer($2); }
							;
		
/*
 * These can't be reorganized, because the order matters.
 * For example:  int i;  i = 5;  int j = i;
 */
Block						:	LBRACE LocalVariableDeclarationsAndStatements RBRACE	{ $$ = $2; }
							|   LBRACE RBRACE	{ $$ = MakeBlock(); }
							;

LocalVariableDeclarationsAndStatements	:	LocalVariableDeclarationOrStatement	{ $$ = MakeBlock($1); }
										|   LocalVariableDeclarationsAndStatements LocalVariableDeclarationOrStatement	{ $$ = MakeBlock($1, $2); }
										;

LocalVariableDeclarationOrStatement	:	LocalVariableDeclarationStatement	{ $$ = $1; }
									|   Statement							{ $$ = $1; }
									;

LocalVariableDeclarationStatement	:	TypeSpecifier LocalVariableDeclarators SEMICOLON	{ $$ = MakeLocalVariableDeclarationStatement($1, $2); }
									|   StructDeclaration									{ $$ = MakeLocalVariableDeclarationStatement($1); }                  						
									;

LocalVariableDeclarators	:	LocalVariableDeclaratorName	{ $$ = MakeLocalVariableDeclarators($1); }
							|   LocalVariableDeclarators COMMA LocalVariableDeclaratorName	{ $$ = MakeLocalVariableDeclarators($1, $3); }
							;

							

Statement					:	EmptyStatement					{ $$ = $1; }
							|	ExpressionStatement SEMICOLON	{ $$ = $1; }
							|	SelectionStatement				{ $$ = $1; }
							|	IterationStatement				{ $$ = $1; }
							|	ReturnStatement					{ $$ = $1; }
							|   Block							{ $$ = $1; }
							;

EmptyStatement				:	SEMICOLON	{ $$ = MakeEmptyStatement(Token.SEMICOLON); }
							;

ExpressionStatement			:	Expression	{ $$ = $1; }
							;

/*
 *  You will eventually have to address the shift/reduce error that
 *     occurs when the second IF-rule is uncommented.
 *
 */

SelectionStatement			:	IF LPAREN Expression RPAREN Statement ELSE Statement	{ $$ = MakeSelectionStatement($3, $5, $7); }
							|   IF LPAREN Expression RPAREN Statement	{ $$ = MakeSelectionStatement($3, $5); }
							;


IterationStatement			:	WHILE LPAREN Expression RPAREN Statement	{$$ = MakeIterationStatement($3, $5); }
							;

ReturnStatement				:	RETURN Expression SEMICOLON	{ $$ = MakeReturnStatement($2); }
							|   RETURN            SEMICOLON	{ $$ = MakeReturnStatement(); }
							;

ArgumentList				:	Expression						{ $$ = MakeArgumentList($1); }
							|   ArgumentList COMMA Expression	{ $$ = MakeArgumentList($1, $3); }
							;


Expression					:	QualifiedName EQUALS Expression	{ $$ = MakeExpression($1, ExpressionEnums.EQUALS, $3); }
							|   Expression OP_LOR Expression	{ $$ = MakeExpression($1, ExpressionEnums.OP_LOR, $3); }	/* short-circuit OR */
							|   Expression OP_LAND Expression	{ $$ = MakeExpression($1, ExpressionEnums.OP_LAND, $3); }	/* short-circuit AND */
							|   Expression PIPE Expression		{ $$ = MakeExpression($1, ExpressionEnums.PIPE, $3); }
							|   Expression HAT Expression		{ $$ = MakeExpression($1, ExpressionEnums.HAT, $3); }
							|   Expression AND Expression		{ $$ = MakeExpression($1, ExpressionEnums.AND, $3); }
							|	Expression OP_EQ Expression		{ $$ = MakeExpression($1, ExpressionEnums.OP_EQ, $3); }
							|   Expression OP_NE Expression		{ $$ = MakeExpression($1, ExpressionEnums.OP_NE, $3); }
							|	Expression OP_GT Expression		{ $$ = MakeExpression($1, ExpressionEnums.OP_GT, $3); }
							|	Expression OP_LT Expression		{ $$ = MakeExpression($1, ExpressionEnums.OP_LT, $3); }
							|	Expression OP_LE Expression		{ $$ = MakeExpression($1, ExpressionEnums.OP_LE, $3); }
							|	Expression OP_GE Expression		{ $$ = MakeExpression($1, ExpressionEnums.OP_GE, $3); }
							|   Expression PLUSOP Expression	{ $$ = MakeExpression($1, ExpressionEnums.PLUSOP, $3); }
							|   Expression MINUSOP Expression	{ $$ = MakeExpression($1, ExpressionEnums.MINUSOP, $3); }
							|	Expression ASTERISK Expression	{ $$ = MakeExpression($1, ExpressionEnums.ASTERISK, $3); }
							|	Expression RSLASH Expression	{ $$ = MakeExpression($1, ExpressionEnums.RSLASH, $3); }
							|   Expression PERCENT Expression	{ $$ = MakeExpression($1, ExpressionEnums.PERCENT, $3); }	/* remainder */
							|	ArithmeticUnaryOperator Expression  %prec UNARY { $$ = MakeExpression($1, $2, yytext, ExpressionEnums.UNARY); }	// TODO: fix me
							|	PrimaryExpression	{ $$ = $1; }	//{ $$ = MakeExpression($1); } TODO: fix me
							;

ArithmeticUnaryOperator		:	PLUSOP	{ $$ = GetArithmeticUnaryOperator(ExpressionEnums.PLUSOP); }
							|   MINUSOP	{ $$ = GetArithmeticUnaryOperator(ExpressionEnums.PLUSOP); }
							;
							
PrimaryExpression			:	QualifiedName	{ $$ = $1; }
							|   NotJustName		{ $$ = $1; }
							;

NotJustName					:	SpecialName		{ $$ = $1; }
							|   ComplexPrimary	{ $$ = $1; }
							;

ComplexPrimary				:	LPAREN Expression RPAREN	{ $$ = $2; }
							|   ComplexPrimaryNoParenthesis	{ $$ = $1; }
							;

ComplexPrimaryNoParenthesis	:	LITERAL		{ $$ = GetLiteral(yystringval); }
							|   Number		{ $$ = $1; }
							|	FieldAccess	{ $$ = $1; }
							|	MethodCall	{ $$ = $1; }
							;

FieldAccess					:	NotJustName PERIOD Identifier	{ $$ = MakeFieldAccess($1, $3); }
							;		

MethodCall					:	MethodReference LPAREN ArgumentList RPAREN	{ $$ = MakeMethodCall($1, $3); }
							|   MethodReference LPAREN RPAREN				{ $$ = MakeMethodCall($1); }
							;

MethodReference				:	ComplexPrimaryNoParenthesis	{ $$ = $1; }	
							|	QualifiedName				{ $$ = $1; }
							|   SpecialName					{ $$ = $1; }
							;

SpecialName					:	THIS	{ $$ = GetSpecialName(SpecialNameEnums.THIS); }
							|	NULL	{ $$ = GetSpecialName(SpecialNameEnums.NULL); }
							;

Identifier					:	IDENTIFIER	{  $$ = GetIdentifier(yytext); }
							;

Number						:	INT_NUMBER	{ $$ = GetNumber(yytext); }
							;

%%

public string yytext
{
	get { return((TCCLScanner)Scanner).yytext; }
}

public string yystringval
{
	get { return((TCCLScanner)Scanner).yystringval; }
}