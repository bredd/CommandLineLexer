using System;
using System.Diagnostics;
using CodeBit;

namespace UnitTest
{
    /// <summary>
    /// Unit test for CommandLineLexer
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Test0();
                Test1();

                Console.WriteLine("All tests passed.");
            }
            catch (Exception err)
            {
                Console.WriteLine(err.ToString());
            }
        }

        // This is just a non-functional sample of how to use the CommandLineLexer.
        static void ParseCommandLineSample(string args)
        {
            var clx = new CommandLineLexer(args);

            // Fake arguments to parse
            string arg_command;
            string arg_file;
            int arg_int;
            string arg_name;

            arg_command = clx.ReadNextArg();
            while (clx.MoveNext())
            {
                clx.ThrowIfNotOption();
                switch (clx.Current)
                {
                    case "-File":
                        arg_file = clx.ReadNextValue();
                        break;

                    case "-Num":
                        arg_int = clx.ReadNextValueAsInt();
                        break;

                    case "-Name":
                        arg_name = clx.ReadNextValue();
                        break;

                    default:
                        clx.ThrowUnexpectedArgError();
                        break; // Not necessary except for compiler warning.
                }
            }
        }

        static void Test0()
        {
            var cl = new CommandLineLexer(String.Empty);
            Assert(cl.IsEmptyCommandLine);
        }

        static string Test1Args = "Command -option1 SpamAndEggs -numbers 4 5 AnotherCommand -option2 \"-notanoption.jpg\"  ";

        static void Test1()
        {
            // Happy Path
            var cl = new CommandLineLexer(Test1Args);
            Assert(cl.ReadNextArg() == "Command");
            Assert(!cl.IsOption);
            cl.ThrowIfNotValue();
            Assert(cl.ReadNextArg() == "-option1");
            Assert(cl.IsOption);
            cl.ThrowIfNotOption();
            Assert(cl.ReadNextValue() == "SpamAndEggs");
            Assert(!cl.IsOption);
            cl.ThrowIfNotValue();
            Assert(cl.ReadNextOption() == "-numbers");
            Assert(cl.IsOption);
            Assert(cl.ReadNextValueAsInt() == 4);
            Assert(!cl.IsOption);
            Assert(cl.ReadNextValueAsInt() == 5);
            Assert(!cl.IsOption);
            Assert(cl.ReadNextValue() == "AnotherCommand");
            Assert(!cl.IsOption);
            Assert(cl.ReadNextOption() == "-option2");
            Assert(cl.ReadNextValue() == "-notanoption.jpg");
            Assert(cl.ReadNextOption() == null);

            // Error Path
            cl.Reset();
            AssertReadNextOptionFails(cl, "Command");
            AssertCurrentIntFails(cl, "Command");
            AssertReadNextValueFails(cl, "-option1");
            AssertCurrentIntFails(cl, "-option1");
            AssertReadNextOptionFails(cl, "SpamAndEggs");
            AssertReadNextValueFails(cl, "-numbers");
            AssertCurrentIntFails(cl, "-numbers");
            AssertReadNextOptionFails(cl, "4");
            AssertReadNextOptionFails(cl, "5");
            AssertReadNextOptionFails(cl, "AnotherCommand");
            AssertCurrentIntFails(cl, "AnotherCommand");
            AssertReadNextValueFails(cl, "-option2");
            AssertReadNextOptionFails(cl, "-notanoption.jpg");
            AssertThrows(() => cl.ReadNextValue());
            Assert(cl.ReadNextArg() == null);
            Assert(cl.ReadNextOption() == null);
        }

        static void Assert(bool success)
        {
            if (!success) throw new ApplicationException("Unit test failure.");
        }

        static void AssertReadNextValueFails(CommandLineLexer cl, string expectedOption)
        {
            bool failure = false;
            try
            {
                cl.ReadNextValue();
                failure = true;
            }
            catch (Exception err)
            {
                if (!err.Message.EndsWith($"Expected value but found option \"{expectedOption}\"."))
                {
                    throw new ApplicationException("ReadNextValue Failure: Error message mismatch: " + err.Message);
                }
            }

            if (failure)
            {
                throw new ApplicationException("ReadNextValue didn't fail when it should have.");
            }
        }

        static void AssertReadNextOptionFails(CommandLineLexer cl, string expectedArg)
        {
            bool failure = false;
            try
            {
                cl.ReadNextOption();
                failure = true;
            }
            catch (Exception err)
            {
                if (!err.Message.EndsWith($"Expected option but found argument \"{expectedArg}\"."))
                {
                    throw new ApplicationException("ReadNextOption Failure: Error message mismatch: " + err.Message);
                }
            }

            if (failure)
            {
                throw new ApplicationException("ReadNextOption didn't fail when it should have.");
            }
        }

        static void AssertCurrentIntFails(CommandLineLexer cl, string expectedArg)
        {
            bool failure = false;
            try
            {
                Console.WriteLine(cl.CurrentAsInt.ToString());
                failure = true;
            }
            catch (Exception err)
            {
                if (!err.Message.EndsWith($"Expected integer; found \"{expectedArg}\"."))
                {
                    throw new ApplicationException("CurrentInt Failure: Error message mismatch: " + err.Message);
                }
            }

            if (failure)
            {
                throw new ApplicationException("CurrentInt didn't fail when it should have.");
            }
        }

        static void AssertThrows(Action action)
        {
            bool failure = false;
            try
            {
                action();
                failure = true;
            }
            catch (Exception)
            {
                // Do nothing - failure was expected
            }

            if (failure)
                throw new ApplicationException("Action didn't throw when it should have.");
        }
    }
}
