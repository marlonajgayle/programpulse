namespace ProgramPulse.Web.Models;

// View models for the topbar notifications panel. UI-only stub — swap SampleData
// for an API-backed source (same shape) once notifications are wired to the API.

public sealed record NotificationVm(
    Guid Id,
    string Author,
    string Initials,
    bool Unread,
    string TimeAgo,
    IReadOnlyList<NotificationSegment> Message,
    NotificationAttachment? Attachment = null);

// A run of the message line. Highlighted segments render in the accent colour
// (e.g. "Task Name", "Project Name", "due date").
public sealed record NotificationSegment(string Text, bool Highlight = false);

public enum NotificationAttachmentKind { CompletedTask, File }

public sealed record NotificationAttachment(
    NotificationAttachmentKind Kind,
    string Title,
    string? Meta = null);
