using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project3
{
    public abstract class TypeDescriptor { }

    public class ErrorDescriptor : TypeDescriptor { }

    public class JavaObjectDescriptor : TypeDescriptor { }

    public class IntegerTypeDescriptor : TypeDescriptor { }

    public class ArrayTypeDescriptor : TypeDescriptor { }

    public class ClassTypeDescriptor : TypeDescriptor
    {
        public List<ModifiersEnums> Modifiers { get; set; }
        public ScopeTable ClassBody { get; set; }
    }

    public class MethodTypeDescriptor : TypeDescriptor
    {
        
    }

    public class PrimitiveTypeDescriptor : TypeDescriptor
    {
        public string Name { get; set; }    // BOOLEAN, VOID, INT
    }

}
