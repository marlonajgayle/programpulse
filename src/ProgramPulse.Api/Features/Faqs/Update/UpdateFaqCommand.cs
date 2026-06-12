using Microsoft.EntityFrameworkCore;
using ProgramPulse.Api.Domain.Entities.Faqs;
using ProgramPulse.Api.Infrastructure.Persistence;
using ProgramPulse.Api.SharedKernel.Primitives;

namespace ProgramPulse.Api.Features.Faqs.Update;

public sealed record UpdateFaqCommand(Guid Id, string Question, string Answer);

/// <summary>
/// Updates an existing FAQ's question and answer. Returns a not-found error when the
/// FAQ does not exist or has been soft-deleted (excluded by the global query filter).
/// </summary>
public sealed class UpdateFaqCommandHandler(IApplicationDbContext dbContext)
{
    private readonly IApplicationDbContext _dbContext = dbContext;

    public async Task<Result> HandleAsync(
        UpdateFaqCommand command,
        CancellationToken cancellationToken)
    {
        var faq = await _dbContext.Faqs
            .FirstOrDefaultAsync(f => f.Id == command.Id, cancellationToken);

        if (faq is null)
            return Result.Failure(FaqErrors.FaqNotFound(command.Id));

        faq.Update(command.Question, command.Answer);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
