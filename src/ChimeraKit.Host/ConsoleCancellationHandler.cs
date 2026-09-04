namespace ChimeraKit.Host;

/// <summary>
/// Turns the first Ctrl+C into cooperative cancellation: the running module is asked to stop via its
/// <see cref="System.Threading.CancellationToken"/> rather than being killed mid-write. A second press
/// is deliberately left to the runtime, so a module that ignores the token can still be forced down.
/// </summary>
public sealed class ConsoleCancellationHandler : IDisposable
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public CancellationToken Token => _cancellationTokenSource.Token;

    public ConsoleCancellationHandler()
    {
        Console.CancelKeyPress += OnCancelKeyPress;
    }

    private void OnCancelKeyPress(object? sender, ConsoleCancelEventArgs eventArgs)
    {
        eventArgs.Cancel = true;
        _cancellationTokenSource.Cancel();
    }

    public void Dispose()
    {
        Console.CancelKeyPress -= OnCancelKeyPress;
        _cancellationTokenSource.Dispose();
    }
}
