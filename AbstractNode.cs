using System;
using System.Collections;
using System.Diagnostics;

namespace Project3
{
    [DebuggerDisplay("AbstractNodeType: {ToString()}")]

    /* All AST nodes are subclasses of this node.  This node knows how to
     * link itself with other siblings and adopt children. Each node gets 
     * a node number to help identify it distinctly in an AST.  */

    public abstract class AbstractNode : IVisitableNode
    {
        private static int nodeNums = 0;
        private int nodeNum;
        private AbstractNode mysib;
        private AbstractNode parent;
        private AbstractNode child;
        private AbstractNode firstSib;
        private Type type;

        public TypeDescriptor TypeDescriptor { get; set; }  //TODO, delete
        public Attributes AttributesRef { get; set; }

        public AbstractNode()
        {
            parent = null;
            mysib = null;
            firstSib = this;
            child = null;
            nodeNum = ++nodeNums;
        }

        /// <summary>
        /// Join the end of this sibling's list with the supplied sibling's list </summary>
        public virtual AbstractNode makeSibling(AbstractNode sib)
        {
            if (sib == null)
            {
                throw new Exception("Call to makeSibling supplied null-valued parameter");
            }
            AbstractNode appendAt = this;
            while (appendAt.mysib != null)
            {
                appendAt = appendAt.mysib;
            }
            appendAt.mysib = sib.firstSib;


            AbstractNode ans = sib.firstSib;
            ans.firstSib = appendAt.firstSib;
            while (ans.mysib != null)
            {
                ans = ans.mysib;
                ans.firstSib = appendAt.firstSib;
            }
            return (ans);
        }

        /// <summary>
        /// Adopt the supplied node and all of its siblings under this node </summary>
        public virtual AbstractNode adoptChildren(AbstractNode n)
        {
            if (n != null)
            {
                if (this.child == null)
                {
                    this.child = n.firstSib;
                }
                else
                {
                    this.child.makeSibling(n);
                }
            }
            for (AbstractNode c = this.child; c != null; c = c.mysib)
            {
                c.parent = this;
            }
            return this;
        }

        public virtual AbstractNode orphan()
        {
            mysib = parent = null;
            firstSib = this;
            return this;
        }

        public virtual AbstractNode abandonChildren()
        {
            child = null;
            return this;
        }

        private AbstractNode Parent
        {
            set
            {
                this.parent = value;
            }
            get
            {
                return (parent);
            }
        }


        public virtual AbstractNode Sib
        {
            get
            {
                return (mysib);
            }
            protected set
            {
                this.mysib = value;
            }
        }

        public virtual AbstractNode Child
        {
            get
            {
                return (child);
            }
        }

        public virtual AbstractNode First
        {
            get
            {
                return (firstSib);
            }
        }

        public virtual Type NodeType
        {
            get
            {
                return type;
            }
            set
            {
                this.type = value;
            }
        }

        public virtual string ClassName()
        {
            return this.GetType().Name;
        }

        public virtual int NodeNum
        {
            get
            {
                return nodeNum;
            }
        }

        public override string ToString()
        {
            return GetType().Name;
        }

        private static Type objectClass = (new object()).GetType();

        /// <summary>
        /// Indicate the class of "this" node </summary>
        public virtual string whatAmI()
        {
            string ans = this.GetType().ToString();
            return ans;
        }


        /// <summary>
        /// Visitor pattern component </summary>
        public virtual void Accept(IReflectiveVisitor v)
        {
            v.Visit(this);
        }

        /*
         * method?
        typeVisitor ← new TypeVisitor() 56
        call md.returnType.accept(typeVisitor )
        attr ← new Attributes(MethodAttributes )
        attr.returnType ← md.returnType.type
        attr.modi f iers ← md.modi f iers
        attr.isDe f inedIn ← getCurrentClass( )
        attr.locals ← new SymbolTable()
        call currentSymbolTable.enterSymbol(name.name, attr )
        md.name.attributeRe f ← attr
        call openScope(attr.locals )

        oldCurrentMethod ← getCurrentMethod()
        call setCurrentMethod(attr )
        call md.parameters.accept(this ) 57
        attr.signature ← parameters.signature.addReturn(attr.returntype )
        call md.body.accept( this ) 58
        call setCurrentMethod(oldCurrentMethod )
        call closeScope()

        procedure visit(ClassDeclaring cd )
        typeRe f ← new TypeDescriptor (ClassType ) 51
        typeRe f.names ← new SymbolTable ( )
        attr ← new Attributes ( ClassAttributes )
        attr.classType ← typeRe f
        call currentSymbolTable.enterSymbol( name.name, attr )
        call setCurrentClass( attr )
        if cd.parentclass = null 52
        then cd.parentclass ← getRefToObject( )
        else
        typeVisitor ← new TypeVisitor ( )
        call cd.parentclass.accept( typeVisitor )
        if cd.parentclass.type = errorType
        then attr.classtype ← errorType
        else
        if cd.parentclass.type.kind  classType
        then
        attr.classtype ← errorType
        call error( parentClass.name, ”does not name a class”)
        else
        typeRe f.parent ← cd.parentClass.attributeRe f 53
        typeRe f.isFinal ← memberOf( cd.modi f iers,final )
        typeRe f.isAbstractl ← memberOf( cd.modi f iers,abstract )
        call typeRe f.names.incorporate( cd.parentclass.type.names ) 54
        call openScope( typeRe f.names )
        call cd. f ields.accept( this ) 55
        call cd.constructors.accept( this )
        call cd.methods.accept( this )
        call closeScope( )
        call setCurrentClass( null )
        end
        */


}

}