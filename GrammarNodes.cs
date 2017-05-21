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
    // Substitute your GrammarNode.cs file for this one
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

    public class ClassDeclaration : AbstractNode
    {
        public ClassDeclaration(AbstractNode modifiers, AbstractNode identifier,
            AbstractNode classBody)
        {
            adoptChildren(modifiers);
            adoptChildren(identifier);  // TODO: AbstractNode Identifiers
            adoptChildren(classBody);
        }
    }

    public class Modifiers : AbstractNode
    {
        public List<FMNodes.ModifiersEnums> ModifierTokens { get; set; }

        public Modifiers(FMNodes.ModifiersEnums modToken)
        {
            ModifierTokens = new List<FMNodes.ModifiersEnums>();
            AddModifier(modToken);
        }

        public void AddModifier(FMNodes.ModifiersEnums modToken)
        {
            ModifierTokens.Add(modToken);
            if (ModifierTokens.Contains(FMNodes.ModifiersEnums.PUBLIC) &&
                ModifierTokens.Contains(FMNodes.ModifiersEnums.PRIVATE))
            {
                throw new ArgumentException("Illegal modifiers: must be PUBLIC " +
                                            "or PRIVATE, cannot be both");
            }
        }
    }

    public class ClassBody : AbstractNode
    {
        public ClassBody(AbstractNode fieldDeclarations)
        {
            if (fieldDeclarations != null)
            {
                adoptChildren(fieldDeclarations);
            }
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

    public class PrimitiveType : AbstractNode
    {
        public FMNodes.PrimitiveEnums Type { get; set; }

        public PrimitiveType(FMNodes.PrimitiveEnums primType)
        {
            Type = primType;
        }
    }

    public class PrimitiveTypeVoid : AbstractNode
    {

    }

    public class PrimitiveTypeInt : AbstractNode
    {
    }

    public class PrimitiveTypeBoolean : AbstractNode
    {
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
        public MethodDeclarator(AbstractNode methodDeclaratorName, AbstractNode parameterList)
        {
            adoptChildren(methodDeclaratorName);
            if (parameterList != null)
            {
                adoptChildren(parameterList);
            }
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
        public Block(AbstractNode localVarDeclOrStmnt)
        {
            if (localVarDeclOrStmnt != null)
            {
                adoptChildren(localVarDeclOrStmnt);
            }
        }

        public void AddLocalVarDeclStmt(AbstractNode localVarDeclOrStmnt)
        {
            adoptChildren(localVarDeclOrStmnt);
        }
    }

    public class LocalVariableDeclarationStatement : AbstractNode
    {
        public LocalVariableDeclarationStatement(AbstractNode typeSpecifier,
            AbstractNode localVariableDeclarators, AbstractNode structDeclaration)
        {
            if (structDeclaration == null)
            {
                adoptChildren(typeSpecifier);
                adoptChildren(localVariableDeclarators);
            }
            else
            {
                adoptChildren(structDeclaration);
            }
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
        public SelectionStatement(AbstractNode ifExpression)
        {
            adoptChildren(ifExpression);
        }

        public void AddStatement(AbstractNode thenStatement)
        {
            adoptChildren(thenStatement);
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

    public class Expression : AbstractNode
    {
        public FMNodes.ExpressionEnums ExpressionType { get; set; }
        public FMNodes.ExpressionEnums ArithmeticUnaryOperator { get; set; }
        private int _precision;

        public Expression(AbstractNode primaryExpression)
        {
            adoptChildren(primaryExpression);
        }

        public Expression(AbstractNode lhs, FMNodes.ExpressionEnums op,
            AbstractNode rhs)
        {
            ExpressionType = op;
            adoptChildren(lhs);
            adoptChildren(rhs);
        }

        public Expression(AbstractNode arithmeticUnaryOperator, AbstractNode expression,
            string prec, FMNodes.ExpressionEnums op)
        {
            ExpressionType = op;
            _precision = Int32.Parse(prec);
            ArithmeticUnaryOperator = ((ArithmeticUnaryOperator)arithmeticUnaryOperator).GetOp;
            adoptChildren(expression);
        }
    }

    public class ArithmeticUnaryOperator : AbstractNode
    {
        private FMNodes.ExpressionEnums op;

        public FMNodes.ExpressionEnums GetOp
        {
            get { return op; }
        }

        public ArithmeticUnaryOperator(FMNodes.ExpressionEnums op)
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

    public class SpecialName : AbstractNode
    {
        public FMNodes.SpecialNameEnums Name { get; set; }

        public SpecialName(FMNodes.SpecialNameEnums specialName)
        {
            Name = specialName;
        }
    }

    public class Identifier : AbstractNode
    {
        public string ID { get; set; }  // TODO: Type/Object Attributes?

        public Identifier(string id)
        {
            ID = id;
        }
    }

    public class Number : AbstractNode
    {
        public int Num { get; set; }

        public Number(string intNumber)
        {
            Num = Int32.Parse(intNumber);
        }
    }
}

