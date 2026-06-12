using Microsoft.EntityFrameworkCore;
using ProgramPulse.Api.Domain.Entities.Faqs;
using ProgramPulse.Api.Infrastructure.Persistence;
using ProgramPulse.Api.SharedKernel.Primitives;

namespace ProgramPulse.Api.Features.Faqs.Delete;

public sealed record DeleteFaqCommand(Guid Id);

/// <summary>
/// Soft-deletes a FAQ via <c>MarkAsDeleted</c>. Returns a not-found error when the FAQ
/// does not exist or has already been deleted (excluded by the global query filter).
/// </summary>
public sealed class DeleteFaqCommandHandler(IApplicationDbContext dbContext)
{
    private readonly IApplicationDbContext _dbContext = dbContext;

    public async Task<Result> HandleAsync(
        DeleteFaqCommand command,
        CancellationToken cancellationToken)
    {
        var faq = await _dbContext.Faqs
            .FirstOrDefaultAsync(f => f.Id == command.Id, cancellationToken);

        if (faq is null)
            return Result.Failure(FaqErrors.FaqNotFound(command.Id));

        faq.MarkAsDeleted();
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
