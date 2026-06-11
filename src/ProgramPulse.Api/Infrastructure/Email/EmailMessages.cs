namespace ProgramPulse.Api.Infrastructure.Email;

/// <summary>
/// A plain or HTML email message.
/// </summary>
public class EmailMessage
{
    public string To { get; set; } = string.Empty;
    public string? From { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsHtml { get; set; } = true;
    public List<string>? Cc { get; set; }
    public List<string>? Bcc { get; set; }
}

/// <summary>
/// An email message carrying one or more attachments.
/// </summary>
public class EmailMessageWithAttachment : EmailMessage
{
    public List<EmailAttachment> Attachments { get; set; } = new();
}

/// <summary>
/// A single attachment, sourced from either an in-memory <see cref="Data"/>
/// stream or a <see cref="FilePath"/> on disk.
/// </summary>
public class EmailAttachment
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/octet-stream";
    public Stream? Data { get; set; }
    public string? FilePath { get; set; }
}

/// <summary>
/// An email rendered from a Razor template file against a strongly-typed model.
/// </summary>
public class TemplatedEmailMessage
{
    public string To { get; set; } = string.Empty;
    public string? From { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string TemplatePath { get; set; } = string.Empty;
    public object? Model { get; set; }
}
