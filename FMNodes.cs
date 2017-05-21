using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASTBuilder;


namespace Project3
{
    using static ASTBuilder.TCCLParser;
    public class FMNodes
    {
        public static AbstractNode MakeCompilationUnit(AbstractNode classDeclaration)
        {
            return new CompilationUnit(classDeclaration);
        }

        public static AbstractNode MakeClassDeclaration(AbstractNode modifiers, AbstractNode identifier, AbstractNode classBody)
        {
            return new ClassDeclaration(modifiers, identifier, classBody);
        }

        public enum ModifiersEnums { PUBLIC, PRIVATE, STATIC }
        public static AbstractNode MakeModifiers(ModifiersEnums modToken)
        {
            return new Modifiers(modToken);
        }

        public static AbstractNode MakeModifiers(AbstractNode mod, ModifiersEnums modToken)
        {
            ((Modifiers)mod).AddModifier(modToken);
            return mod;
        }

        public static AbstractNode MakeClassBody()
        {
            return new ClassBody(null);
        }

        public static AbstractNode MakeClassBody(AbstractNode fieldDeclarations)
        {
            return new ClassBody(fieldDeclarations);
        }

        public static AbstractNode MakeFieldDeclarations(AbstractNode fieldDeclaration)
        {
            return new FieldDeclarations(fieldDeclaration);
        }

        public static AbstractNode MakeFieldDeclarations(AbstractNode fieldDecls,
            AbstractNode fieldDecl)
        {
            ((FieldDeclarations)fieldDecls).AddFieldDeclaration(fieldDecl);
            return fieldDecls;
        }

        public static AbstractNode MakeStructDeclaration(AbstractNode modifiers, AbstractNode identifier, AbstractNode classBody)
        {
            return new StructDeclaration(modifiers, identifier, classBody);
        }

        public static AbstractNode MakeFieldVariableDeclaration(AbstractNode modifiers,
            AbstractNode typeSpecifier, AbstractNode fieldVariableDeclarators)
        {
            return new FieldVariableDeclaration(modifiers, typeSpecifier,
                fieldVariableDeclarators);
        }

        public static AbstractNode MakeArraySpecifier(AbstractNode typeName)
        {
            return new ArraySpecifier(typeName);
        }

        public enum PrimitiveEnums { BOOLEAN, INT, VOID }
        public static AbstractNode MakePrimitiveType(PrimitiveEnums primType)
        {
            return new PrimitiveType(primType); // BOOLEAN, INT, or VOID
        }


        private AbstractNode MakePrimitiveTypeBoolean()
        {
            return new PrimitiveTypeBoolean();
        }

        private AbstractNode MakePrimitiveTypeInt()
        {
            return new PrimitiveTypeInt();
        }

        private AbstractNode MakePrimitiveTypeVoid()
        {
            return new PrimitiveTypeVoid();
        }

        public static AbstractNode MakeFieldVariableDeclarators(AbstractNode fieldVarDeclName)
        {
            return new FieldVariableDeclarators(fieldVarDeclName);
        }

        public static AbstractNode MakeFieldVariableDeclarators(AbstractNode fieldVarDecls, AbstractNode fieldVarDeclName)
        {
            ((FieldVariableDeclarators)fieldVarDecls).AddFieldVariableDeclaratorName(fieldVarDeclName);
            return fieldVarDecls;
        }

        public static AbstractNode MakeMethodDeclaration(AbstractNode modifiers,
            AbstractNode typeSpecifier, AbstractNode methodDeclarator, AbstractNode methodBody)
        {
            return new MethodDeclaration(modifiers, typeSpecifier, methodDeclarator, methodBody);
        }

        public static AbstractNode MakeMethodDeclarator(AbstractNode methodDeclName)
        {
            return new MethodDeclarator(methodDeclName, null);
        }

        public static AbstractNode MakeMethodDeclarator(AbstractNode methodDeclName,
            AbstractNode parameterList)
        {
            return new MethodDeclarator(methodDeclName, parameterList);
        }

        public static AbstractNode MakeParameterList(AbstractNode parameter)
        {
            return new ParameterList(parameter);
        }

        public static AbstractNode MakeParameterList(AbstractNode parameterList, AbstractNode parameter)
        {
            ((ParameterList)parameterList).AddParameter(parameter);
            return parameterList;
        }

        public static AbstractNode MakeParameter(AbstractNode typeSpecifier,
            AbstractNode declaratorName)
        {
            return new Parameter(typeSpecifier, declaratorName);
        }

        public static AbstractNode MakeQualifiedName(AbstractNode identifier)
        {
            return new QualifiedName(identifier);
        }

        public static AbstractNode MakeQualifiedName(AbstractNode qualifiedName,
            AbstractNode identifier)
        {
            ((QualifiedName)qualifiedName).AddIdentifier(identifier);
            return qualifiedName;
        }

        public static AbstractNode MakeConstructorDeclaration(AbstractNode modifiers,
            AbstractNode methodDeclarator, AbstractNode block)
        {
            return new ConstructorDeclaration(modifiers, methodDeclarator, block);
        }

        public static AbstractNode MakeStaticInitializer(AbstractNode block)
        {
            return new StaticInitializer(block);
        }

        public static AbstractNode MakeBlock()
        {
            return new Block(null);
        }

        public static AbstractNode MakeBlock(AbstractNode localVarDeclOrStmnt)
        {
            return new Block(localVarDeclOrStmnt);
        }

        public static AbstractNode MakeBlock(AbstractNode localVarDeclAndStmnts,
            AbstractNode localVarDeclOrStmnt)
        {
            ((Block)localVarDeclAndStmnts).AddLocalVarDeclStmt(localVarDeclOrStmnt);
            return localVarDeclAndStmnts;
        }

        public static AbstractNode MakeLocalVariableDeclarationStatement
            (AbstractNode typeSpecifier, AbstractNode localVarDecls)
        {
            return new LocalVariableDeclarationStatement(typeSpecifier, localVarDecls, null);
        }

        public static AbstractNode MakeLocalVariableDeclarationStatement
            (AbstractNode structDeclaration)
        {
            return new LocalVariableDeclarationStatement(null, null, structDeclaration);
        }

        public static AbstractNode MakeLocalVariableDeclarators(AbstractNode localVarDeclName)
        {
            return new LocalVariableDeclarators(localVarDeclName);
        }

        public static AbstractNode MakeLocalVariableDeclarators(AbstractNode localVarDecls,
            AbstractNode localVarDeclName)
        {
            ((LocalVariableDeclarators)localVarDecls).AddLocalVariableDeclaratorName(localVarDeclName);
            return localVarDecls;
        }

        public static AbstractNode MakeEmptyStatement()
        {
            return new EmptyStatement();
        }

        public static AbstractNode MakeSelectionStatement(AbstractNode ifExpression,
            AbstractNode thenStatement)
        {
            //return new SelectionStatementNode(expression, statement);
            SelectionStatement ssNode = new SelectionStatement(ifExpression);
            ThenStatement tsNode = new ThenStatement(thenStatement);
            ssNode.AddStatement(tsNode);
            return ssNode;
        }

        public static AbstractNode MakeSelectionStatement(AbstractNode ifExpression,
            AbstractNode thenStatement, AbstractNode elseStatement)
        {
            //return new SelectionStatementNode(expression, statementIf, statementElse);
            SelectionStatement ssNode = new SelectionStatement(ifExpression);
            ThenStatement tsNode = new ThenStatement(thenStatement);
            ElseStatement esNode = new ElseStatement(elseStatement);
            ssNode.AddStatement(tsNode);
            ssNode.AddStatement(esNode);
            return ssNode;
        }

        public static AbstractNode MakeIterationStatement(AbstractNode expression,
            AbstractNode statement)
        {
            return new IterationStatement(expression, statement);
        }

        public static AbstractNode MakeReturnStatement()
        {
            return new ReturnStatement();
        }

        public static AbstractNode MakeReturnStatement(AbstractNode expression)
        {
            return new ReturnStatement(expression);
        }

        public static AbstractNode MakeArgumentList(AbstractNode expression)
        {
            return new ArgumentList(expression);
        }

        public static AbstractNode MakeArgumentList(AbstractNode argumentList,
            AbstractNode expression)
        {
            ((ArgumentList)argumentList).AddExpression(expression);
            return argumentList;
        }

        public enum ExpressionEnums
        {
            EQUALS, OP_LOR, OP_LAND, PIPE, HAT, AND, OP_EQ, OP_NE, OP_GT, OP_LT,
            OP_LE, OP_GE, PLUSOP, MINUSOP, ASTERISK, RSLASH, PERCENT, UNARY
        }

        public static AbstractNode MakeExpression(AbstractNode lhs, ExpressionEnums op, AbstractNode rhs)
        {
            return new Expression(lhs, op, rhs);
        }

        public static AbstractNode MakeExpression(AbstractNode arithmeticUnaryOperator,
            AbstractNode expression, string prec, ExpressionEnums op)
        {
            return new Expression(arithmeticUnaryOperator, expression, prec, op);
        }

        public static AbstractNode GetArithmeticUnaryOperator(ExpressionEnums op)
        {
            return new ArithmeticUnaryOperator(op);
        }

        public static AbstractNode GetLiteral(string literal)
        {
            return new Literal(literal);
        }

        public static AbstractNode MakeFieldAccess(AbstractNode notJustName, AbstractNode identifer)
        {
            return new FieldAccess(notJustName, identifer);
        }

        public static AbstractNode MakeMethodCall(AbstractNode methodReference)
        {
            return new MethodCall(methodReference);
        }

        public static AbstractNode MakeMethodCall(AbstractNode methodReference,
            AbstractNode argumentList)
        {
            return new MethodCall(methodReference, argumentList);
        }

        public enum SpecialNameEnums { THIS, NULL }
        public static AbstractNode GetSpecialName(SpecialNameEnums type)
        {
            return new SpecialName(type);
        }

        public static AbstractNode GetIdentifier(string id)
        {
            return new Identifier(id);
        }

        public static AbstractNode GetNumber(string intNumber)
        {
            return new Number(intNumber);
        }


    }


}
