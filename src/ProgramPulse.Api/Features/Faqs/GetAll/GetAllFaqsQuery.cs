using Microsoft.EntityFrameworkCore;
using ProgramPulse.Api.Infrastructure.Persistence;
using ProgramPulse.Api.SharedKernel.Primitives;

namespace ProgramPulse.Api.Features.Faqs.GetAll;

public sealed record GetAllFaqsQuery;

/// <summary>
/// Returns all FAQs ordered by creation date. Soft-deleted FAQs are excluded by the
/// global query filter.
/// </summary>
public sealed class GetAllFaqsQueryHandler(IApplicationDbContext dbContext)
{
    private readonly IApplicationDbContext _dbContext = dbContext;

    public async Task<Result<IReadOnlyList<FaqResponse>>> HandleAsync(
        GetAllFaqsQuery query,
        CancellationToken cancellationToken)
    {
        var faqs = await _dbContext.Faqs
            .AsNoTracking()
            .OrderBy(f => f.CreatedDate)
            .Select(f => new FaqResponse(
                f.Id, f.Question, f.Answer, f.CreatedDate, f.LastModifiedDate))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<FaqResponse>>.Success(faqs);
    }
}
