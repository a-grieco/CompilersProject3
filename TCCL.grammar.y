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

ClassDeclaration	:	Modifiers CLASS Identifier ClassBody	{ $$ = new ClassDeclaration($1, $3, $4); }
					;

Modifiers			:	PUBLIC		{ $$ = new Modifiers(ModifiersEnums.PUBLIC); }
					|	PRIVATE		{ $$ = new Modifiers(ModifiersEnums.PRIVATE); }
					|	STATIC		{ $$ = new Modifiers(ModifiersEnums.STATIC); }
					|	Modifiers PUBLIC	{ ((Modifiers)$1).AddModifier(ModifiersEnums.PUBLIC); $$ = $1; }
					|	Modifiers PRIVATE	{ ((Modifiers)$1).AddModifier(ModifiersEnums.PRIVATE); $$ = $1; }
					|	Modifiers STATIC	{ ((Modifiers)$1).AddModifier(ModifiersEnums.STATIC); $$ = $1; }
					;

ClassBody			:	LBRACE FieldDeclarations RBRACE	{ $$ = new ClassBody($2); }
					|	LBRACE RBRACE					{ $$ = new ClassBody(); }
					;

FieldDeclarations	:	FieldDeclaration					{ $$ = new FieldDeclarations($1); }
					|	FieldDeclarations FieldDeclaration	{ ((FieldDeclarations)$1).AddFieldDeclaration($2); $$ = $1; }
					;

FieldDeclaration	:	FieldVariableDeclaration SEMICOLON	{ $$ = $1; }
					|	MethodDeclaration					{ $$ = $1; }
					|	ConstructorDeclaration				{ $$ = $1; }
					|	StaticInitializer					{ $$ = $1; }
					|	StructDeclaration					{ $$ = $1; }
					;

StructDeclaration	:	Modifiers STRUCT Identifier ClassBody	{ $$ = new StructDeclaration($1, $3, $4); }
					;



/*
 * This isn't structured so nicely for a bottom up parse.  Recall
 * the example I did in class for Digits, where the "type" of the digits
 * (i.e., the base) is sitting off to the side.  You'll have to do something
 * here to get the information where you want it, so that the declarations can
 * be suitably annotated with their type and modifier information.
 */
FieldVariableDeclaration	:	Modifiers TypeSpecifier FieldVariableDeclarators	{ $$ = new FieldVariableDeclaration($1, $2, $3); }
							;

TypeSpecifier				:	TypeName		{ $$ = $1; }
							| 	ArraySpecifier	{ $$ = $1; }
							;

TypeName					:	PrimitiveType	{ $$ = $1; }
							|   QualifiedName	{ $$ = $1; }
							;

ArraySpecifier				: 	TypeName LBRACKET RBRACKET	{ $$ = new ArraySpecifier($1); }
							;
							
PrimitiveType				:	BOOLEAN	{ $$ = new PrimitiveTypeBoolean(); }
							|	INT		{ $$ = new PrimitiveTypeInt(); }
							|	VOID	{ $$ = new PrimitiveTypeVoid(); }
							;

FieldVariableDeclarators	:	FieldVariableDeclaratorName	{ $$ = new FieldVariableDeclarators($1); }
							|   FieldVariableDeclarators COMMA FieldVariableDeclaratorName	{ ((FieldVariableDeclarators)$1).AddFieldVariableDeclaratorName($3); $$ = $1; }
							;


MethodDeclaration			:	Modifiers TypeSpecifier MethodDeclarator MethodBody	{ $$ = new MethodDeclaration($1, $2, $3, $4); }
							;
							
MethodDeclarator			:	MethodDeclaratorName LPAREN ParameterList RPAREN	{ $$ = new MethodDeclarator($1, $3); }
							|   MethodDeclaratorName LPAREN RPAREN					{ $$ = new MethodDeclarator($1); }
							;

ParameterList				:	Parameter						{ $$ = new ParameterList($1); }
							|   ParameterList COMMA Parameter	{ ((ParameterList)$1).AddParameter($3); $$ = $1; }
							;

Parameter					:	TypeSpecifier DeclaratorName	{ $$ = new Parameter($1, $2); }
							;

QualifiedName				:	Identifier						{ $$ = new QualifiedName($1); }
							|	QualifiedName PERIOD Identifier	{ ((QualifiedName)$1).AddIdentifier($3); $$ = $1; }
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

ConstructorDeclaration		:	Modifiers MethodDeclarator Block	{ $$ = new ConstructorDeclaration($1, $2, $3); }
							;

StaticInitializer			:	STATIC Block	{ $$ = new StaticInitializer($2); }
							;
		
/*
 * These can't be reorganized, because the order matters.
 * For example:  int i;  i = 5;  int j = i;
 */
Block						:	LBRACE LocalVariableDeclarationsAndStatements RBRACE	{ $$ = $2; }
							|   LBRACE RBRACE	{ $$ = new Block(); }
							;

LocalVariableDeclarationsAndStatements	:	LocalVariableDeclarationOrStatement	{ $$ = new Block($1); }
										|   LocalVariableDeclarationsAndStatements LocalVariableDeclarationOrStatement	{ ((Block)$1).AddLocalVarDeclStmt($2); $$ = $1; }
										;

LocalVariableDeclarationOrStatement	:	LocalVariableDeclarationStatement	{ $$ = $1; }
									|   Statement							{ $$ = $1; }
									;

LocalVariableDeclarationStatement	:	TypeSpecifier LocalVariableDeclarators SEMICOLON	{ $$ = new LocalVariableDeclarationStatement($1, $2); }
									|   StructDeclaration									{ $$ = new LocalVariableDeclarationStatement($1); }                  						
									;

LocalVariableDeclarators	:	LocalVariableDeclaratorName	{ $$ = new LocalVariableDeclarators($1); }
							|   LocalVariableDeclarators COMMA LocalVariableDeclaratorName	{ ((LocalVariableDeclarators)$1).AddLocalVariableDeclaratorName($3); $$ = $1; }
							;
					

Statement					:	EmptyStatement					{ $$ = $1; }
							|	ExpressionStatement SEMICOLON	{ $$ = $1; }
							|	SelectionStatement				{ $$ = $1; }
							|	IterationStatement				{ $$ = $1; }
							|	ReturnStatement					{ $$ = $1; }
							|   Block							{ $$ = $1; }
							;
							
EmptyStatement				:	SEMICOLON	{ $$ = new EmptyStatement( ); }
							;

ExpressionStatement			:	Expression	{ $$ = $1; }
							;

/*
 *  You will eventually have to address the shift/reduce error that
 *     occurs when the second IF-rule is uncommented.
 *
 */

SelectionStatement			:	IF LPAREN Expression RPAREN Statement ELSE Statement	{ $$ = new SelectionStatement($3, $5, $7); }
							|   IF LPAREN Expression RPAREN Statement	{ $$ = new SelectionStatement($3, $5); }
							;


IterationStatement			:	WHILE LPAREN Expression RPAREN Statement	{$$ = new IterationStatement($3, $5); }
							;

ReturnStatement				:	RETURN Expression SEMICOLON	{ $$ = new ReturnStatement($2); }
							|   RETURN            SEMICOLON	{ $$ = new ReturnStatement(); }
							;

ArgumentList				:	Expression						{ $$ = new ArgumentList($1); }
							|   ArgumentList COMMA Expression	{ ((ArgumentList)$1).AddExpression($3); $$ = $1; }
							;


Expression					:	QualifiedName EQUALS Expression	{ $$ = new Expression($1, ExpressionEnums.EQUALS, $3); }
							|   Expression OP_LOR Expression	{ $$ = new Expression($1, ExpressionEnums.OP_LOR, $3); }	/* short-circuit OR */
							|   Expression OP_LAND Expression	{ $$ = new Expression($1, ExpressionEnums.OP_LAND, $3); }	/* short-circuit AND */
							|   Expression PIPE Expression		{ $$ = new Expression($1, ExpressionEnums.PIPE, $3); }
							|   Expression HAT Expression		{ $$ = new Expression($1, ExpressionEnums.HAT, $3); }
							|   Expression AND Expression		{ $$ = new Expression($1, ExpressionEnums.AND, $3); }
							|	Expression OP_EQ Expression		{ $$ = new Expression($1, ExpressionEnums.OP_EQ, $3); }
							|   Expression OP_NE Expression		{ $$ = new Expression($1, ExpressionEnums.OP_NE, $3); }
							|	Expression OP_GT Expression		{ $$ = new Expression($1, ExpressionEnums.OP_GT, $3); }
							|	Expression OP_LT Expression		{ $$ = new Expression($1, ExpressionEnums.OP_LT, $3); }
							|	Expression OP_LE Expression		{ $$ = new Expression($1, ExpressionEnums.OP_LE, $3); }
							|	Expression OP_GE Expression		{ $$ = new Expression($1, ExpressionEnums.OP_GE, $3); }
							|   Expression PLUSOP Expression	{ $$ = new Expression($1, ExpressionEnums.PLUSOP, $3); }
							|   Expression MINUSOP Expression	{ $$ = new Expression($1, ExpressionEnums.MINUSOP, $3); }
							|	Expression ASTERISK Expression	{ $$ = new Expression($1, ExpressionEnums.ASTERISK, $3); }
							|	Expression RSLASH Expression	{ $$ = new Expression($1, ExpressionEnums.RSLASH, $3); }
							|   Expression PERCENT Expression	{ $$ = new Expression($1, ExpressionEnums.PERCENT, $3); }	/* remainder */
							|	ArithmeticUnaryOperator Expression  %prec UNARY { $$ = new Expression($1, $2, yytext, ExpressionEnums.UNARY); }	// TODO: fix me
							|	PrimaryExpression	{ $$ = $1; }	//{ $$ = new Expression($1); }	// TODO: fix me
							;

ArithmeticUnaryOperator		:	PLUSOP	{ $$ = new ArithmeticUnaryOperator(ExpressionEnums.PLUSOP); }
							|   MINUSOP	{ $$ = new ArithmeticUnaryOperator(ExpressionEnums.PLUSOP); }
							;
							
PrimaryExpression			:	QualifiedName	{ $$ = new PrimaryExpression($1); }
							|   NotJustName		{ $$ = new PrimaryExpression($1); }
							;

NotJustName					:	SpecialName		{ $$ = $1; }
							|   ComplexPrimary	{ $$ = $1; }
							;

ComplexPrimary				:	LPAREN Expression RPAREN	{ $$ = $2; }
							|   ComplexPrimaryNoParenthesis	{ $$ = $1; }
							;

ComplexPrimaryNoParenthesis	:	LITERAL		{ $$ = new Literal(yystringval); }
							|   Number		{ $$ = $1; }
							|	FieldAccess	{ $$ = $1; }
							|	MethodCall	{ $$ = $1; }
							;

FieldAccess					:	NotJustName PERIOD Identifier	{ $$ = new FieldAccess($1, $3); }
							;		

MethodCall					:	MethodReference LPAREN ArgumentList RPAREN	{ $$ = new MethodCall($1, $3); }
							|   MethodReference LPAREN RPAREN				{ $$ = new MethodCall($1); }
							;

MethodReference				:	ComplexPrimaryNoParenthesis	{ $$ = $1; }	
							|	QualifiedName				{ $$ = $1; }
							|   SpecialName					{ $$ = $1; }
							;

SpecialName					:	THIS	{ $$ = new SpecialName(SpecialNameEnums.THIS); }
							|	NULL	{ $$ = new SpecialName(SpecialNameEnums.NULL); }
							;

Identifier					:	IDENTIFIER	{  $$ = new Identifier(yytext); }
							;

Number						:	INT_NUMBER	{ $$ = new Number(yytext); }
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