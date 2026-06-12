namespace ProgramPulse.Api.Domain.Entities.Faqs;

using ProgramPulse.Api.SharedKernel;

public sealed class Faq : AuditableEntity<Guid>
{
    // EF Core materialization
    private Faq() { }

    public string Question { get; private set; } = string.Empty;
    public string Answer { get; private set; } = string.Empty;

    public static Faq Create(string question, string answer)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(question);
        ArgumentException.ThrowIfNullOrWhiteSpace(answer);

        return new Faq
        {
            Id = Guid.CreateVersion7(),
            Question = question,
            Answer = answer
        };
    }

    public void Update(string question, string answer)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(question);
        ArgumentException.ThrowIfNullOrWhiteSpace(answer);

        Question = question;
        Answer = answer;
    }

    // MarkAsDeleted() is inherited (public) from BaseEntity<Guid>.
}
