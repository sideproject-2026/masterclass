namespace Lms.Modules.Catalog.Contracts;

/// <summary>
/// Assembly marker. Lets architecture tests and DI scanning reference this assembly
/// without depending on a specific contract type.
/// </summary>
/// <remarks>
/// This project may reference <c>Lms.SharedKernel</c> and nothing else — that is what keeps
/// the mutual Catalog/Enrollment contract edges acyclic (01-architecture.md §2.2).
/// </remarks>
public interface ICatalogContractsMarker;
