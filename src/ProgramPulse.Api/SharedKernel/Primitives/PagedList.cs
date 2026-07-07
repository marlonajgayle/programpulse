namespace ProgramPulse.Api.SharedKernel.Primitives;

/// <summary>
/// A single page of results plus the paging metadata a client needs to render
/// pagination controls. <see cref="TotalCount"/> is the total number of items
/// across all pages, not just the length of <see cref="Items"/>.
/// </summary>
public sealed record PagedList<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount)
{
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}
