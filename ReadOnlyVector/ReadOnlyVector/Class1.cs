using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadOnlyVectorTask
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var c = new ReadOnlyVector(1, 2).WithX(3);
            Console.WriteLine(c);
        }
    }
}
