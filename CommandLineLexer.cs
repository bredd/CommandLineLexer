using System;
using System.Collections;
using System.Collections.Generic;

namespace CodeBit
{
    /// <summary>
    /// Yet another command-line parsing helper class. This one
    /// uses a lexer structure and expects the application to read the
    /// command-line options in sequence. Distinction from a simple
    /// foreach loop on 'args' is that this class makes error reporting easier
    /// by tracking the context.
    /// </summary>
    class CommandLineLexer : IEnumerator<string>
    {
        string[] m_args;
        int m_currentArg;
        string m_latestOption;

        /// <summary>
        /// Initialize with the command line from the environment
        /// </summary>
        public CommandLineLexer()
            : this(Environment.GetCommandLineArgs())
        {
        }

        /// <summary>
        /// Initialize with the specified arguments.
        /// </summary>
        /// <param name="args">An array of command-line arguments.</param>
        public CommandLineLexer(string[] args)
        {
            m_args = args;
            Reset();
        }

        /// <summary>
        /// Reset to the beginning of the command line
        /// </summary>
        public void Reset()
        {
            m_currentArg = -1;
            m_latestOption = string.Empty;
        }

        /// <summary>
        /// The current argument as a string
        /// </summary>
        public string Current
        {
            get
            {
                if (m_currentArg < 0) throw new InvalidOperationException("CommandLineLexer: Must call MoveNext() before Current");
                if (m_currentArg >= m_args.Length) throw new InvalidOperationException("CommandLineLexer: All arguments have been read.");
                return m_args[m_currentArg];
            }
        }

        /// <summary>
        /// Indicates whether the current argument is an option.
        /// </summary>
        /// <remarks>
        /// A command-line argument starts with a dash.
        /// </remarks>
        public bool IsOption
        {
            get
            {
                return m_currentArg < m_args.Length && m_args[m_currentArg][0] == '-';
            }
        }

        object IEnumerator.Current => Current;

        /// <summary>
        /// Returns the current argument as an integer.
        /// </summary>
        /// <exception cref="CommandLineException">Thrown if the value is not an integer.</exception>
        public int CurrentInt
        {
            get
            {
                int value;
                if (!int.TryParse(Current, out value))
                    throw new CommandLineException($"{OptionErrPrefix}Expected integer; found \"{Current}\".");
                return value;
            }
        }

        /// <summary>
        /// The latest option is the most recent argument with a - (dash) prefix.
        /// </summary>
        /// <remarks>
        /// This is useful for error reporting with an invalid value. It may also be cleared
        /// (by setting to null or empty string) or set to a special value.
        /// </remarks>
        public string LatestOption
        {
            get
            {
                return m_latestOption;
            }

            set
            {
                m_latestOption = value ?? string.Empty;
            }
        }

        /// <summary>
        /// Disposal is not required - does nothing
        /// </summary>
        public void Dispose()
        {
            // Nothing to dispose
        }

        /// <summary>
        /// Moves to the next argument. Returns true if another argument is present. Otherwise, false.
        /// </summary>
        /// <returns></returns>
        public bool MoveNext()
        {
            if (m_currentArg >= m_args.Length) return false;
            ++m_currentArg;
            if (m_currentArg >= m_args.Length)
            {
                m_latestOption = string.Empty;
                return false;
            }
            if (IsOption)
            {
                m_latestOption = Current;
            }
            return true;
        }

        /// <summary>
        /// Moves to the next value, presumably for the current option.
        /// </summary>
        /// <remarks>
        /// Throws an <see cref="CommandLineException"/> if there are no more arguments or if the next
        /// argument is is an option (prefixed with a hyphen). When an exception is thrown, the current
        /// value does not advance.
        /// </remarks>
        /// <exception cref="CommandLineException">Thrown if at the end of the argument string
        /// or the next argument is an option.</exception>
        public void MoveNextValue()
        {
            if (m_currentArg >= m_args.Length - 1)
                ThrowValueError("Value expected.");
            if (m_args[m_currentArg+1][0] == '-')
                ThrowValueError($"Value expected. Found \"{m_args[m_currentArg + 1]}\"");
            ++m_currentArg;
        }

        /// <summary>
        /// Moves to the next argument and return it.
        /// </summary>
        /// <returns>The next argument or null if at the end of the argument list.</returns>
        public string ReadNextArg()
        {
            if (!MoveNext()) return null;
            return Current;
        }

        /// <summary>
        /// Advances to the next value and returns it.
        /// </summary>
        /// <returns>The string value of the next argument.</returns>
        /// <remarks>
        /// The next argument must be a value, not an option. That is, it must not be
        /// prefixed with a hyphen. If at the end of the argument string or the value
        /// is an option then an <see cref="CommandLineException"/> is thrown.
        /// </remarks>
        /// <exception cref="CommandLineException">Thrown if at the end of the argument string
        /// or the next argument is an option.</exception>
        public string ReadNextValue()
        {
            MoveNextValue();
            return Current;
        }

        /// <summary>
        /// Advances to the next value and returns it as an integer.
        /// </summary>
        /// <returns>The string value of the next argument.</returns>
        /// <remarks>
        /// The next argument must be a value, not an option. That is, it must not be
        /// prefixed with a hyphen. If at the end of the argument string or the value
        /// is an option then an <see cref="CommandLineException"/> is thrown.
        /// </remarks>
        /// <exception cref="CommandLineException">Thrown if at the end of the argument string
        /// or the next argument is an option.</exception>
        public int ReadNextValueInt()
        {
            MoveNextValue();
            return CurrentInt;
        }

        /// <summary>
        /// Throws a <see cref="CommandLineException"/> with the specified error message.
        /// </summary>
        /// <param name="message">An error message.</param>
        /// <remarks>
        /// The message is prefixed with "Command Line Error: "
        /// </remarks>
        public void ThrowError(string message)
        {
            throw new CommandLineException("Command Line Error: " + message);
        }

        /// <summary>
        /// Throws a <see cref="CommandLineException"/> regarding the <see cref="LatestOption"/>
        /// </summary>
        /// <param name="message">An error message.</param>
        /// <remarks>The error message is prefixed with "Command Line Error on Option" with the
        /// option name if CurrentOption has a value. Otherwise prefixed with "Command Line Error"
        /// </remarks>
        public void ThrowValueError(string message)
        {
            throw new CommandLineException(OptionErrPrefix + message);
        }

        private string OptionErrPrefix
        {
            get
            {
                return (string.IsNullOrEmpty(m_latestOption))
                    ? "Command Line Error: "
                    : $"Command Line Error on Option \"{m_latestOption}\": ";
            }
        }

    }

    /// <summary>
    /// An error found when parsing the command line.
    /// </summary>
    /// <remarks>
    /// The <see cref="Message"/> value is always suitable for reporting to the user.
    /// </remarks>
    class CommandLineException : Exception
    {
        public CommandLineException(string errorMessage)
            : base(errorMessage)
        {
        }

    }
}
