using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASTBuilder;
using Project3;

namespace Project3
{
    public class CompilationUnit : AbstractNode
    {
        // just for the compilation unit because it's the top node
        //public override AbstractNode LeftMostSibling => this;
        public override AbstractNode Sib => null;

        public CompilationUnit(AbstractNode classDecl)
        {
            adoptChildren(classDecl);
        }

    }

    public class ClassDeclaration : AbstractNode, IDescription
    {
        public ClassDeclaration(AbstractNode modifiers, AbstractNode identifier,
            AbstractNode classBody)
        {
            adoptChildren(modifiers);
            adoptChildren(identifier);  // TODO: AbstractNode Identifiers
            adoptChildren(classBody);
        }

        public EntryType EntryType
        {
            get { return EntryType.ClassType; }
            set { throw new NotImplementedException(); }
        }

        public DescriptionEntry DescriptionEntry { get; set; }
    }

    public enum ModifiersEnums { PUBLIC, PRIVATE, STATIC }
    public class Modifiers : AbstractNode
    {
        public List<ModifiersEnums> ModifierTokens { get; set; }

        public Modifiers(ModifiersEnums modToken)
        {
            ModifierTokens = new List<ModifiersEnums>();
            AddModifier(modToken);
        }

        public void AddModifier(ModifiersEnums modToken)
        {
            ModifierTokens.Add(modToken);
            if (ModifierTokens.Contains(ModifiersEnums.PUBLIC) &&
                ModifierTokens.Contains(ModifiersEnums.PRIVATE))
            {
                throw new ArgumentException("Illegal modifiers: must be PUBLIC " +
                                            "or PRIVATE, cannot be both");
            }
        }
    }

    public class ClassBody : AbstractNode
    {
        public ClassBody() { }

        public ClassBody(AbstractNode fieldDeclarations)
        {
            adoptChildren(fieldDeclarations);
        }
    }

    public class FieldDeclarations : AbstractNode
    {
        public FieldDeclarations(AbstractNode fieldDeclaration)
        {
            adoptChildren(fieldDeclaration);
        }

        public void AddFieldDeclaration(AbstractNode fieldDeclaration)
        {
            adoptChildren(fieldDeclaration);
        }
    }

    public class StructDeclaration : AbstractNode
    {
        public StructDeclaration(AbstractNode modifiers, AbstractNode identifier,
            AbstractNode classBody)
        {
            adoptChildren(modifiers);
            adoptChildren(identifier);
            adoptChildren(classBody);
        }
    }

    public class FieldVariableDeclaration : AbstractNode
    {
        public FieldVariableDeclaration(AbstractNode modifiers, AbstractNode typeSpecifier,
            AbstractNode fieldVariableDeclarators)
        {
            adoptChildren(modifiers);
            adoptChildren(typeSpecifier);
            adoptChildren(fieldVariableDeclarators);
        }
    }

    public class ArraySpecifier : AbstractNode
    {
        public ArraySpecifier(AbstractNode typeName)
        {
            adoptChildren(typeName);
        }
    }

    //public class PrimitiveType : AbstractNode, IDescription
    //{
    //    public FMNodes.PrimitiveEnums Type { get; set; }

    //    public PrimitiveType(FMNodes.PrimitiveEnums primType)
    //    {
    //        Type = primType;
    //    }

    //    public EntryType EntryType { get; set; }
    //    public DescriptionEntry DescriptionEntry { get; set; }
    //}

    public class PrimitiveTypeVoid : AbstractNode, IDescription
    {
        public EntryType EntryType
        {
            get { return EntryType.PrimitiveTypeVoid; }
            set { throw new NotImplementedException(); }
        }

        public DescriptionEntry DescriptionEntry { get; set; }
    }

    public class PrimitiveTypeInt : AbstractNode, IDescription
    {
        public EntryType EntryType
        {
            get { return EntryType.PrimitiveTypeInt; }
            set { throw new NotImplementedException(); }
        }
        public DescriptionEntry DescriptionEntry { get; set; }
    }

    public class PrimitiveTypeBoolean : AbstractNode
    {
        public EntryType EntryType
        {
            get { return EntryType.PrimitiveTypeBoolean; }
            set { throw new NotImplementedException(); }
        }
        public DescriptionEntry DescriptionEntry { get; set; }
    }

    public class FieldVariableDeclarators : AbstractNode
    {
        public FieldVariableDeclarators(AbstractNode fieldVariableDeclaratorName)
        {
            adoptChildren(fieldVariableDeclaratorName);
        }

        public void AddFieldVariableDeclaratorName(AbstractNode identifier)
        {
            adoptChildren(identifier);
        }
    }

    public class MethodDeclaration : AbstractNode
    {
        public MethodDeclaration(AbstractNode modifiers, AbstractNode typeSpecifier, AbstractNode methodDeclarator, AbstractNode methodBody)
        {
            adoptChildren(modifiers);
            adoptChildren(typeSpecifier);
            adoptChildren(methodDeclarator);
            adoptChildren(methodBody);
        }
    }

    public class MethodDeclarator : AbstractNode
    {
        public MethodDeclarator(AbstractNode methodDeclaratorName)
        {
            adoptChildren(methodDeclaratorName);
        }

        public MethodDeclarator(AbstractNode methodDeclaratorName, AbstractNode parameterList)
        {
            adoptChildren(methodDeclaratorName);
            adoptChildren(parameterList);
        }
    }

    public class ParameterList : AbstractNode
    {
        public ParameterList(AbstractNode parameter)
        {
            adoptChildren(parameter);
        }

        public void AddParameter(AbstractNode parameter)
        {
            adoptChildren(parameter);
        }
    }

    public class Parameter : AbstractNode
    {
        public Parameter(AbstractNode typeSpecifier, AbstractNode declaratorName)
        {
            adoptChildren(typeSpecifier);
            adoptChildren(declaratorName);
        }
    }

    public class QualifiedName : AbstractNode
    {
        public QualifiedName(AbstractNode identifier)
        {
            adoptChildren(identifier);
        }

        public void AddIdentifier(AbstractNode identifier)
        {
            adoptChildren(identifier);
        }
    }

    public class ConstructorDeclaration : AbstractNode
    {
        public ConstructorDeclaration(AbstractNode modifiers,
            AbstractNode methodDeclarator, AbstractNode block)
        {
            adoptChildren(modifiers);
            adoptChildren(methodDeclarator);
            adoptChildren(block);
        }
    }

    public class StaticInitializer : AbstractNode
    {
        public StaticInitializer(AbstractNode block)
        {
            adoptChildren(block);
        }
    }

    public class Block : AbstractNode
    {
        public Block() { }

        public Block(AbstractNode localVarDeclOrStmt)
        {
            adoptChildren(localVarDeclOrStmt);
        }

        public void AddLocalVarDeclStmt(AbstractNode localVarDeclOrStmt)
        {
            adoptChildren(localVarDeclOrStmt);
        }
    }

    public class LocalVariableDeclarationStatement : AbstractNode
    {

        public LocalVariableDeclarationStatement(AbstractNode typeSpecifier,
            AbstractNode localVariableDeclarators)
        {
            adoptChildren(typeSpecifier);
            adoptChildren(localVariableDeclarators);
        }
        public LocalVariableDeclarationStatement(AbstractNode structDeclaration)
        {
            adoptChildren(structDeclaration);
        }
    }

    public class LocalVariableDeclarators : AbstractNode
    {
        public LocalVariableDeclarators(AbstractNode localVarDeclName)
        {
            adoptChildren(localVarDeclName);
        }

        public void AddLocalVariableDeclaratorName(AbstractNode localVarDeclName)
        {
            adoptChildren(localVarDeclName);
        }
    }

    public class EmptyStatement : AbstractNode
    {

        public EmptyStatement()
        {
        }
    }

    public class SelectionStatement : AbstractNode
    {
        public SelectionStatement(AbstractNode ifExpression, 
            AbstractNode thenStatement, AbstractNode elseStatement)
        {
            adoptChildren(ifExpression);
            adoptChildren(new ThenStatement(thenStatement));
            adoptChildren(new ElseStatement(elseStatement));
        }

        public SelectionStatement(AbstractNode ifExpression, 
            AbstractNode thenStatement)
        {
            adoptChildren(ifExpression);
            adoptChildren(new ThenStatement(thenStatement));
        }
    }

    public class ThenStatement : AbstractNode
    {
        public ThenStatement(AbstractNode thenStatement)
        {
            adoptChildren(thenStatement);
        }
    }

    public class ElseStatement : AbstractNode
    {
        public ElseStatement(AbstractNode elseStatement)
        {
            adoptChildren(elseStatement);
        }
    }

    public class IterationStatement : AbstractNode
    {
        public IterationStatement(AbstractNode expression,
            AbstractNode statement)
        {
            adoptChildren(expression);
            adoptChildren(statement);
        }
    }

    public class ReturnStatement : AbstractNode
    {
        public ReturnStatement() { }

        public ReturnStatement(AbstractNode expression)
        {
            adoptChildren(expression);
        }
    }

    public class ArgumentList : AbstractNode
    {
        public ArgumentList(AbstractNode expression)
        {
            adoptChildren(expression);
        }

        public void AddExpression(AbstractNode expression)
        {
            adoptChildren(expression);
        }
    }

    public enum ExpressionEnums
    {
        EQUALS, OP_LOR, OP_LAND, PIPE, HAT, AND, OP_EQ, OP_NE, OP_GT, OP_LT,
        OP_LE, OP_GE, PLUSOP, MINUSOP, ASTERISK, RSLASH, PERCENT, UNARY
    }
    public class Expression : AbstractNode
    {
        public ExpressionEnums ExpressionType { get; }
        public ExpressionEnums ArithmeticUnaryOperator { get; set; }
        private int _precision;

        public Expression(AbstractNode primaryExpression)
        {
            adoptChildren(primaryExpression);
        }

        public Expression(AbstractNode lhs, ExpressionEnums op,
            AbstractNode rhs)
        {
            ExpressionType = op;
            adoptChildren(lhs);
            adoptChildren(rhs);
        }

        public Expression(AbstractNode arithmeticUnaryOperator, AbstractNode expression,
            string prec, ExpressionEnums op)
        {
            ExpressionType = op;
            _precision = Int32.Parse(prec);
            ArithmeticUnaryOperator = ((ArithmeticUnaryOperator)arithmeticUnaryOperator).GetOp;
            adoptChildren(expression);
        }
    }

    public class ArithmeticUnaryOperator : AbstractNode
    {
        private ExpressionEnums op;

        public ExpressionEnums GetOp
        {
            get { return op; }
        }

        public ArithmeticUnaryOperator(ExpressionEnums op)
        {
            this.op = op;
        }
    }

    public class Literal : AbstractNode
    {
        public string Lit;

        public Literal(string literal)
        {
            Lit = literal;
        }
    }

    public class FieldAccess : AbstractNode
    {
        public FieldAccess(AbstractNode notJustName, AbstractNode identifer)
        {
            adoptChildren(notJustName);
            adoptChildren(identifer);
        }
    }

    public class MethodCall : AbstractNode
    {
        public MethodCall(AbstractNode methodReference)
        {
            adoptChildren(methodReference);
        }

        public MethodCall(AbstractNode methodReference, AbstractNode argumentList)
        {
            adoptChildren(methodReference);
            adoptChildren(argumentList);
        }
    }

    public enum SpecialNameEnums { THIS, NULL }
    public class SpecialName : AbstractNode
    {
        public SpecialNameEnums Name { get; set; }

        public SpecialName(SpecialNameEnums specialName)
        {
            Name = specialName;
        }
    }

    public class Identifier : AbstractNode
    {
        public string ID { get; }  // TODO: Type/Object Attributes?

        public Identifier(string id)
        {
            ID = id;
        }
    }

    public class Number : AbstractNode
    {
        public int Num { get; }

        public Number(string intNumber)
        {
            Num = Int32.Parse(intNumber);
        }
    }
}

