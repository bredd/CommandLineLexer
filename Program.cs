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

        // This is just a non-functional sample.
        static void ParseCommandLineSample(string[] args)
        {
            var clx = new CommandLineLexer(args);

            // Fake arguments to parse
            string arg_command;
            string arg_file;
            int arg_int;
            string arg_name;

            arg_command = clx.ReadNextArg();
            while (clx.MoveNextOption())
            {
                switch(clx.Current)
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
            bool exceptionThrown = false;
            try
            {
                var cl = new CommandLineLexer(new string[0]);
            }
            catch (CommandLineException err)
            {
                Assert(err.Message.Equals("Empty Command Line."));
                exceptionThrown = true;
            }
            Assert(exceptionThrown);
        }

        static string[] Test1Args = new string[]
        {
            "Command",
            "-option1", "SpamAndEggs",
            "-numbers", "4", "5",
            "AnotherCommand"
        };

        static void Test1()
        {
            SimpleTest(Test1Args);

            // Happy Path
            var cl = new CommandLineLexer(Test1Args);
            Assert(cl.ReadNextArg() == Test1Args[0]);
            Assert(!cl.IsOption);
            Assert(cl.ReadNextArg() == Test1Args[1]);
            Assert(cl.IsOption);
            Assert(cl.ReadNextValue() == Test1Args[2]);
            Assert(!cl.IsOption);
            Assert(cl.ReadNextOption() == Test1Args[3]);
            Assert(cl.IsOption);
            Assert(cl.ReadNextValueAsInt() == 4);
            Assert(!cl.IsOption);
            Assert(cl.ReadNextValueAsInt() == 5);
            Assert(!cl.IsOption);
            Assert(cl.ReadNextValue() == Test1Args[6]);
            Assert(!cl.IsOption);
            Assert(cl.ReadNextArg() == null);

            // Error Path
            cl.Reset();
            Assert(cl.ReadNextArg() == Test1Args[0]);
            AssertCurrentIntFails(cl, Test1Args[0]);
            Assert(cl.ReadNextArg() == Test1Args[1]);
            AssertCurrentIntFails(cl, Test1Args[1]);
            AssertReadNextOptionFails(cl, Test1Args[2]);
            Assert(cl.ReadNextValue() == Test1Args[2]);
            Assert(cl.ReadNextOption() == Test1Args[3]);
            AssertCurrentIntFails(cl, Test1Args[3]);
            AssertReadNextOptionFails(cl, Test1Args[4]);
            Assert(cl.ReadNextValueAsInt() == 4);
            AssertReadNextOptionFails(cl, Test1Args[5]);
            Assert(cl.ReadNextValueAsInt() == 5);
            AssertReadNextOptionFails(cl, Test1Args[6]);
            Assert(cl.ReadNextValue() == Test1Args[6]);
            AssertCurrentIntFails(cl, Test1Args[6]);
            AssertReadNextValueFails(cl);
            Assert(cl.ReadNextArg() == null);
            Assert(cl.ReadNextOption() == null);
        }

        static void SimpleTest(string[] args)
        {
            var cl = new CommandLineLexer(Test1Args);

            foreach (var arg in Test1Args)
            {
                Assert(arg == cl.ReadNextArg());
                Assert(arg == cl.Current);
                int i;
                if (int.TryParse(arg, out i))
                {
                    Assert(i == cl.CurrentAsInt);
                }
            }
            Assert(null == cl.ReadNextArg());
        }

        static void Assert(bool success)
        {
            if (!success) throw new ApplicationException("Unit test failure.");
        }

        static void AssertReadNextValueFails(CommandLineLexer cl)
        {
            bool failure = false;
            try
            {
                cl.ReadNextValue();
                failure = true;
            }
            catch (Exception err)
            {
                if (!err.Message.EndsWith("Value expected but reached end of argument list."))
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
                if (!err.Message.EndsWith($"Option expected. Found \"{expectedArg}\""))
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
                throw new ApplicationException("CurrentInt didn't faile when it should have.");
            }
        }

    }

}
