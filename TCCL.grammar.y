%namespace ASTBuilder
%using Project3

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

ClassDeclaration	:	Modifiers CLASS Identifier ClassBody	{ $$ = FMNodes.MakeClassDeclaration($1, $3, $4); }
					;

Modifiers			:	PUBLIC		{ $$ = FMNodes.MakeModifiers(FMNodes.ModifiersEnums.PUBLIC); }
					|	PRIVATE		{ $$ = FMNodes.MakeModifiers(FMNodes.ModifiersEnums.PRIVATE); }
					|	STATIC		{ $$ = FMNodes.MakeModifiers(FMNodes.ModifiersEnums.STATIC); }
					|	Modifiers PUBLIC	{ $$ = FMNodes.MakeModifiers($1, FMNodes.ModifiersEnums.PUBLIC); }
					|	Modifiers PRIVATE	{ $$ = FMNodes.MakeModifiers($1, FMNodes.ModifiersEnums.PRIVATE); }
					|	Modifiers STATIC	{ $$ = FMNodes.MakeModifiers($1, FMNodes.ModifiersEnums.STATIC); }
					;

ClassBody			:	LBRACE FieldDeclarations RBRACE	{ $$ = FMNodes.MakeClassBody($2); }
					|	LBRACE RBRACE					{ $$ = FMNodes.MakeClassBody(); }
					;

FieldDeclarations	:	FieldDeclaration					{ $$ = FMNodes.MakeFieldDeclarations($1); }
					|	FieldDeclarations FieldDeclaration	{ $$ = FMNodes.MakeFieldDeclarations($1, $2); }
					;

FieldDeclaration	:	FieldVariableDeclaration SEMICOLON	{ $$ = $1; }
					|	MethodDeclaration					{ $$ = $1; }
					|	ConstructorDeclaration				{ $$ = $1; }
					|	StaticInitializer					{ $$ = $1; }
					|	StructDeclaration					{ $$ = $1; }
					;

StructDeclaration	:	Modifiers STRUCT Identifier ClassBody	{ $$ = FMNodes.MakeStructDeclaration($1, $3, $4); }
					;



/*
 * This isn't structured so nicely for a bottom up parse.  Recall
 * the example I did in class for Digits, where the "type" of the digits
 * (i.e., the base) is sitting off to the side.  You'll have to do something
 * here to get the information where you want it, so that the declarations can
 * be suitably annotated with their type and modifier information.
 */
FieldVariableDeclaration	:	Modifiers TypeSpecifier FieldVariableDeclarators	{ $$ = FMNodes.MakeFieldVariableDeclaration($1, $2, $3); }
							;

TypeSpecifier				:	TypeName		{ $$ = $1; }
							| 	ArraySpecifier	{ $$ = $1; }
							;

TypeName					:	PrimitiveType	{ $$ = $1; }
							|   QualifiedName	{ $$ = $1; }
							;

ArraySpecifier				: 	TypeName LBRACKET RBRACKET	{ $$ = FMNodes.MakeArraySpecifier($1); }
							;
							
PrimitiveType				:	BOOLEAN	{ $$ = FMNodes.MakePrimitiveTypeBoolean(); }
							|	INT		{ $$ = FMNodes.MakePrimitiveTypeInt(); }
							|	VOID	{ $$ = FMNodes.MakePrimitiveTypeVoid(); }
							;

FieldVariableDeclarators	:	FieldVariableDeclaratorName	{ $$ = FMNodes.MakeFieldVariableDeclarators($1); }
							|   FieldVariableDeclarators COMMA FieldVariableDeclaratorName	{ $$ = FMNodes.MakeFieldVariableDeclarators($1, $3); }
							;


MethodDeclaration			:	Modifiers TypeSpecifier MethodDeclarator MethodBody	{ $$ = FMNodes.MakeMethodDeclaration($1, $2, $3, $4); }
							;
							
MethodDeclarator			:	MethodDeclaratorName LPAREN ParameterList RPAREN	{ $$ = FMNodes.MakeMethodDeclarator($1, $3); }
							|   MethodDeclaratorName LPAREN RPAREN					{ $$ = FMNodes.MakeMethodDeclarator($1); }
							;

ParameterList				:	Parameter						{ $$ = FMNodes.MakeParameterList($1); }
							|   ParameterList COMMA Parameter	{ $$ = FMNodes.MakeParameterList($1, $3); }
							;

Parameter					:	TypeSpecifier DeclaratorName	{ $$ = FMNodes.MakeParameter($1, $2); }
							;

QualifiedName				:	Identifier						{ $$ = FMNodes.MakeQualifiedName($1); }
							|	QualifiedName PERIOD Identifier	{ $$ = FMNodes.MakeQualifiedName($1, $3); }
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

ConstructorDeclaration		:	Modifiers MethodDeclarator Block	{ $$ = FMNodes.MakeConstructorDeclaration($1, $2, $3); }
							;

StaticInitializer			:	STATIC Block	{ $$ = FMNodes.MakeStaticInitializer($2); }
							;
		
/*
 * These can't be reorganized, because the order matters.
 * For example:  int i;  i = 5;  int j = i;
 */
Block						:	LBRACE LocalVariableDeclarationsAndStatements RBRACE	{ $$ = $2; }
							|   LBRACE RBRACE	{ $$ = FMNodes.MakeBlock(); }
							;

LocalVariableDeclarationsAndStatements	:	LocalVariableDeclarationOrStatement	{ $$ = FMNodes.MakeBlock($1); }
										|   LocalVariableDeclarationsAndStatements LocalVariableDeclarationOrStatement	{ $$ = FMNodes.MakeBlock($1, $2); }
										;

LocalVariableDeclarationOrStatement	:	LocalVariableDeclarationStatement	{ $$ = $1; }
									|   Statement							{ $$ = $1; }
									;

LocalVariableDeclarationStatement	:	TypeSpecifier LocalVariableDeclarators SEMICOLON	{ $$ = FMNodes.MakeLocalVariableDeclarationStatement($1, $2); }
									|   StructDeclaration									{ $$ = FMNodes.MakeLocalVariableDeclarationStatement($1); }                  						
									;

LocalVariableDeclarators	:	LocalVariableDeclaratorName	{ $$ = FMNodes.MakeLocalVariableDeclarators($1); }
							|   LocalVariableDeclarators COMMA LocalVariableDeclaratorName	{ $$ = FMNodes.MakeLocalVariableDeclarators($1, $3); }
							;

							

Statement					:	EmptyStatement					{ $$ = $1; }
							|	ExpressionStatement SEMICOLON	{ $$ = $1; }
							|	SelectionStatement				{ $$ = $1; }
							|	IterationStatement				{ $$ = $1; }
							|	ReturnStatement					{ $$ = $1; }
							|   Block							{ $$ = $1; }
							;
							
EmptyStatement				:	SEMICOLON	{ $$ = FMNodes.MakeEmptyStatement( ); }
							;

ExpressionStatement			:	Expression	{ $$ = $1; }
							;

/*
 *  You will eventually have to address the shift/reduce error that
 *     occurs when the second IF-rule is uncommented.
 *
 */

SelectionStatement			:	IF LPAREN Expression RPAREN Statement ELSE Statement	{ $$ = FMNodes.MakeSelectionStatement($3, $5, $7); }
							|   IF LPAREN Expression RPAREN Statement	{ $$ = FMNodes.MakeSelectionStatement($3, $5); }
							;


IterationStatement			:	WHILE LPAREN Expression RPAREN Statement	{$$ = FMNodes.MakeIterationStatement($3, $5); }
							;

ReturnStatement				:	RETURN Expression SEMICOLON	{ $$ = FMNodes.MakeReturnStatement($2); }
							|   RETURN            SEMICOLON	{ $$ = FMNodes.MakeReturnStatement(); }
							;

ArgumentList				:	Expression						{ $$ = FMNodes.MakeArgumentList($1); }
							|   ArgumentList COMMA Expression	{ $$ = FMNodes.MakeArgumentList($1, $3); }
							;


Expression					:	QualifiedName EQUALS Expression	{ $$ = FMNodes.MakeExpression($1, FMNodes.ExpressionEnums.EQUALS, $3); }
							|   Expression OP_LOR Expression	{ $$ = FMNodes.MakeExpression($1, FMNodes.ExpressionEnums.OP_LOR, $3); }	/* short-circuit OR */
							|   Expression OP_LAND Expression	{ $$ = FMNodes.MakeExpression($1, FMNodes.ExpressionEnums.OP_LAND, $3); }	/* short-circuit AND */
							|   Expression PIPE Expression		{ $$ = FMNodes.MakeExpression($1, FMNodes.ExpressionEnums.PIPE, $3); }
							|   Expression HAT Expression		{ $$ = FMNodes.MakeExpression($1, FMNodes.ExpressionEnums.HAT, $3); }
							|   Expression AND Expression		{ $$ = FMNodes.MakeExpression($1, FMNodes.ExpressionEnums.AND, $3); }
							|	Expression OP_EQ Expression		{ $$ = FMNodes.MakeExpression($1, FMNodes.ExpressionEnums.OP_EQ, $3); }
							|   Expression OP_NE Expression		{ $$ = FMNodes.MakeExpression($1, FMNodes.ExpressionEnums.OP_NE, $3); }
							|	Expression OP_GT Expression		{ $$ = FMNodes.MakeExpression($1, FMNodes.ExpressionEnums.OP_GT, $3); }
							|	Expression OP_LT Expression		{ $$ = FMNodes.MakeExpression($1, FMNodes.ExpressionEnums.OP_LT, $3); }
							|	Expression OP_LE Expression		{ $$ = FMNodes.MakeExpression($1, FMNodes.ExpressionEnums.OP_LE, $3); }
							|	Expression OP_GE Expression		{ $$ = FMNodes.MakeExpression($1, FMNodes.ExpressionEnums.OP_GE, $3); }
							|   Expression PLUSOP Expression	{ $$ = FMNodes.MakeExpression($1, FMNodes.ExpressionEnums.PLUSOP, $3); }
							|   Expression MINUSOP Expression	{ $$ = FMNodes.MakeExpression($1, FMNodes.ExpressionEnums.MINUSOP, $3); }
							|	Expression ASTERISK Expression	{ $$ = FMNodes.MakeExpression($1, FMNodes.ExpressionEnums.ASTERISK, $3); }
							|	Expression RSLASH Expression	{ $$ = FMNodes.MakeExpression($1, FMNodes.ExpressionEnums.RSLASH, $3); }
							|   Expression PERCENT Expression	{ $$ = FMNodes.MakeExpression($1, FMNodes.ExpressionEnums.PERCENT, $3); }	/* remainder */
							|	ArithmeticUnaryOperator Expression  %prec UNARY { $$ = FMNodes.MakeExpression($1, $2, yytext, FMNodes.ExpressionEnums.UNARY); }	// TODO: fix me
							|	PrimaryExpression	{ $$ = $1; }	//{ $$ = FMNodes.MakeExpression($1); } TODO: fix me
							;

ArithmeticUnaryOperator		:	PLUSOP	{ $$ = FMNodes.GetArithmeticUnaryOperator(FMNodes.ExpressionEnums.PLUSOP); }
							|   MINUSOP	{ $$ = FMNodes.GetArithmeticUnaryOperator(FMNodes.ExpressionEnums.PLUSOP); }
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

ComplexPrimaryNoParenthesis	:	LITERAL		{ $$ = FMNodes.GetLiteral(yystringval); }
							|   Number		{ $$ = $1; }
							|	FieldAccess	{ $$ = $1; }
							|	MethodCall	{ $$ = $1; }
							;

FieldAccess					:	NotJustName PERIOD Identifier	{ $$ = FMNodes.MakeFieldAccess($1, $3); }
							;		

MethodCall					:	MethodReference LPAREN ArgumentList RPAREN	{ $$ = FMNodes.MakeMethodCall($1, $3); }
							|   MethodReference LPAREN RPAREN				{ $$ = FMNodes.MakeMethodCall($1); }
							;

MethodReference				:	ComplexPrimaryNoParenthesis	{ $$ = $1; }	
							|	QualifiedName				{ $$ = $1; }
							|   SpecialName					{ $$ = $1; }
							;

SpecialName					:	THIS	{ $$ = FMNodes.GetSpecialName(FMNodes.SpecialNameEnums.THIS); }
							|	NULL	{ $$ = FMNodes.GetSpecialName(FMNodes.SpecialNameEnums.NULL); }
							;

Identifier					:	IDENTIFIER	{  $$ = FMNodes.GetIdentifier(yytext); }
							;

Number						:	INT_NUMBER	{ $$ = FMNodes.GetNumber(yytext); }
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