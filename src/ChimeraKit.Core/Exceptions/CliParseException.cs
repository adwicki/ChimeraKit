namespace ChimeraKit.Core.Exceptions;

public class CliParseException : Exception
{
    public CliParseException(string msg) : base(msg) { }
    public CliParseException(Exception e, string msg) 
        : base(msg + Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace) { }
}