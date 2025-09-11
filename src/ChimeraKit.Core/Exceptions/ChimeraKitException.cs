namespace ChimeraKit.Core.Exceptions;

public class ChimeraKitException : Exception
{
    public ChimeraKitException(string msg) : base(msg) { }
    
    public ChimeraKitException(Exception e, string msg) 
        : base(msg + Environment.NewLine + e.Message + Environment.NewLine + e.StackTrace) { }
}