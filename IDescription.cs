using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project3
{
    public interface IDescription
    {
        EntryType EntryType { get; set; }
        DescriptionEntry DescriptionEntry { get; set; }
    }

    public abstract class DescriptionEntry
    {
        //public virtual int Size { get; }
    }

    public class ClassTypeDescriptor : DescriptionEntry
    {
        //public Modifiers Modifiers { get; set; }
        //public Identifier Identifier { get; set; }
        //public ClassBody ClassBody { get; set; }
    }
}
