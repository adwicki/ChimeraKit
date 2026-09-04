namespace ChimeraKit.Core.SharedServices;

/// <summary>
/// Marker interface for services shared across all modules. Every shared service interface derives
/// from this, so the host can discover and register all of them automatically instead of wiring each
/// one up by hand - see <c>ServiceCollectionExtensions.AddSharedServices</c>.
/// </summary>
public interface ISharedService;
