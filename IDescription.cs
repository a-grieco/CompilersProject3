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

        //int Size { get; }
    }

    public abstract class Description : IDescription
    {
        public virtual int Size { get; }
    }

    public class ClassDescription : Description, IDescription
    {
        public Modifiers Modifiers { get; set; }
        public Identifier Identifier { get; set; }
        public ClassBody ClassBody { get; set; }
    }
}
