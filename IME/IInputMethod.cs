using System.Collections.Generic;

namespace NaiveIME
{
    public interface IInputMethod
	{
	    string Name { get; }

		void Clear();

		void Input(char c);

		string InputString { get;}

		IEnumerable<string> Results { get;}

        // below: for future use
		IEnumerable<string> SubResult { get; }
		void ConfirmSubResult(int index);
	}
}
