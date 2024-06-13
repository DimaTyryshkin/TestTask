using System;
using System.Collections.Generic;

namespace TestTask
{
	public class UpperFirstStringComparer : IComparer<string>
	{
		public int Compare(string x, string y)
		{
			if (x == null && y == null)
               return 0;
            else if (x == null)
               return -1;
            else if (y == null)
               return 1;
               
			int value = String.Compare(x, y, StringComparison.InvariantCultureIgnoreCase);

			if (value != 0)
				return value;
			
			if (char.IsUpper(x[0]) && char.IsLower(y[0]))
				return -1;

			if (char.IsUpper(y[0]) && char.IsLower(x[0]))
				return 1;
			
			return 0;
		}
	}
}