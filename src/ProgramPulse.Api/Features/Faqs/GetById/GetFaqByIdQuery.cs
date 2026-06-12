using Microsoft.EntityFrameworkCore;
using ProgramPulse.Api.Domain.Entities.Faqs;
using ProgramPulse.Api.Infrastructure.Persistence;
using ProgramPulse.Api.SharedKernel.Primitives;

namespace ProgramPulse.Api.Features.Faqs.GetById;

public sealed record GetFaqByIdQuery(Guid Id);

/// <summary>
/// Returns a single FAQ by id. Soft-deleted FAQs are excluded by the global query
/// filter and therefore surface as a not-found error.
/// </summary>
public sealed class GetFaqByIdQueryHandler(IApplicationDbContext dbContext)
{
    private readonly IApplicationDbContext _dbContext = dbContext;

    public async Task<Result<FaqResponse>> HandleAsync(
        GetFaqByIdQuery query,
        CancellationToken cancellationToken)
    {
        var faq = await _dbContext.Faqs
            .AsNoTracking()
            .Where(f => f.Id == query.Id)
            .Select(f => new FaqResponse(
                f.Id, f.Question, f.Answer, f.CreatedDate, f.LastModifiedDate))
            .FirstOrDefaultAsync(cancellationToken);

        if (faq is null)
            return Result<FaqResponse>.Failure(FaqErrors.FaqNotFound(query.Id));

        return Result<FaqResponse>.Success(faq);
    }
}
