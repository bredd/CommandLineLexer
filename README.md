# CommandLineLexer

This is yet another command-line parser. This one uses a reader / enumerator design pattern. It has integrated error handling and reporting by way of the custom `CommandLineException`.

Here's a sample of a typical use pattern:

```
static void ParseCommandLineSample()
{
    var clx = new CommandLineLexer();

    arg_command = clx.ReadNextArg();
    while (clx.MoveNextOption())
    {
        switch(clx.Current)
        {
            case "-File":
                arg_file = clx.ReadNextValue();
                break;

            case "-Num":
                arg_int = clx.ReadNextValueInt();
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
```

With the exception of the default option on the switch, all command-line syntax errors are automatically handled by the lexer and don't require the programmer to report those cases.
