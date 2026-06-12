using ProgramPulse.Api.Domain.Entities.Faqs;
using ProgramPulse.Api.Infrastructure.Persistence;
using ProgramPulse.Api.SharedKernel.Primitives;

namespace ProgramPulse.Api.Features.Faqs.Create;

public sealed record CreateFaqCommand(string Question, string Answer);

/// <summary>
/// Creates a new FAQ. Audit fields are stamped automatically on save by
/// <c>ApplicationDbContext</c>.
/// </summary>
public sealed class CreateFaqCommandHandler(IApplicationDbContext dbContext)
{
    private readonly IApplicationDbContext _dbContext = dbContext;

    public async Task<Result<FaqResponse>> HandleAsync(
        CreateFaqCommand command,
        CancellationToken cancellationToken)
    {
        var faq = Faq.Create(command.Question, command.Answer);

        _dbContext.Faqs.Add(faq);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var response = new FaqResponse(
            faq.Id, faq.Question, faq.Answer, faq.CreatedDate, faq.LastModifiedDate);

        return Result<FaqResponse>.Created(response, $"/api/v1/faqs/{faq.Id}");
    }
}
