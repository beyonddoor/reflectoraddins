using System;
using System.Text;
using System.Globalization;
using System.Collections;

/***************************************************************************
 * Based on Jonathan de Halleux's CodeMetrics CommandLineProcessor.cs File *
 ***************************************************************************/
namespace Reflector.AssemblyListEx
{
    class CommandLineManager
    {
        internal bool IsArgumentPresent(string name)
        {
            return GetArgument(name) != null;
        }

        internal string GetArgument(string name)
        {
            string[] arguments = this.GetArguments(name);
            if (arguments != null)
            {
                if (arguments.Length != 1)
                {
                    throw new InvalidOperationException();
                }

                return arguments[0];
            }

            return null;
        }

        private string[] GetArguments(string name)
        {
            name = name.ToLower(CultureInfo.InvariantCulture);

            ArrayList list = new ArrayList(0);

            string[] arguments = Environment.GetCommandLineArgs();
            for (int i = 1; i < arguments.Length; i++)
            {
                string argument = arguments[i];
                string argumentName = string.Empty;
                string argumentValue = string.Empty;

                if ((argument[0] != '/') && (argument[0] != '-'))
                {
                    argumentValue = argument;
                }
                else
                {
                    int index = argument.IndexOf(':');

                    if (index == -1)
                    {
                        // "-option" without value
                        argumentName = argument.Substring(1).ToLower(CultureInfo.InvariantCulture);

                        // Turn '-?' into '-help'
                        if (argumentName == "?")
                        {
                            argumentName = "help";
                        }
                    }
                    else
                    {
                        // "-option:value"
                        argumentName = argument.Substring(1, index - 1).ToLower(CultureInfo.InvariantCulture);
                        argumentValue = argument.Substring(index + 1);
                    }
                }

                // Add value
                if ((argumentName.Length != 0) && (name.StartsWith(argumentName)))
                {
                    list.Add(argumentValue);
                }

                if ((argumentName.Length == 0) && (name.Length == 0))
                {
                    list.Add(argumentValue);
                }
            }

            if (list.Count != 0)
            {
                string[] array = new string[list.Count];
                list.CopyTo(array, 0);
                return array;
            }

            return null;
        }

    }
}
