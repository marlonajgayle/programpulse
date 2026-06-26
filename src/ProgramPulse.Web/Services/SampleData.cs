using ProgramPulse.Web.Models;

namespace ProgramPulse.Web.Services;

/// <summary>
/// In-memory mock dataset for the Initiatives UI. Uses stable GUIDs so drill-down
/// links resolve across pages. UI-only stub — replace with an API-backed source
/// (same method shape) once the WASM client is wired to /api/v1/initiatives.
/// </summary>
public sealed class SampleData
{
    private readonly List<InitiativeVm> _initiatives;

    public SampleData()
    {
        _initiatives = Build();
    }

    public IReadOnlyList<InitiativeVm> GetInitiatives() => _initiatives;

    // ----- Notifications (in-memory only) -----
    private readonly List<NotificationVm> _notifications = BuildNotifications();

    public IReadOnlyList<NotificationVm> GetNotifications() => _notifications;

    public int UnreadNotificationCount => _notifications.Count(n => n.Unread);

    public void MarkAllNotificationsRead()
    {
        for (var i = 0; i < _notifications.Count; i++)
        {
            if (_notifications[i].Unread)
            {
                _notifications[i] = _notifications[i] with { Unread = false };
            }
        }
    }

    private static List<NotificationVm> BuildNotifications() =>
    [
        new(Guid.CreateVersion7(), "Damien Green", "DG", Unread: true, "5 min ago",
            [
                new("Just completed "),
                new("Task Name", Highlight: true),
                new(" from "),
                new("Project Name", Highlight: true),
            ],
            new NotificationAttachment(NotificationAttachmentKind.CompletedTask,
                "Completed the prototypes for new product")),

        new(Guid.CreateVersion7(), "Sarah Smith", "SS", Unread: false, "1 hour ago",
            [
                new("Updated the "),
                new("due date", Highlight: true),
                new(" for "),
                new("Task Name Here", Highlight: true),
            ]),

        new(Guid.CreateVersion7(), "Allen Greenspan", "AG", Unread: false, "1 hour ago",
            [
                new("Added a file into "),
                new("Project Name", Highlight: true),
            ],
            new NotificationAttachment(NotificationAttachmentKind.File,
                "filename.jpg", "18 Apr, 2021 - 2847kb")),

        new(Guid.CreateVersion7(), "Karen Hayden", "KH", Unread: false, "1 hour ago",
            [
                new("Assigned 9 points to the upcoming "),
                new("Task Name", Highlight: true),
            ]),
    ];

    public InitiativeVm? GetInitiative(Guid id) =>
        _initiatives.FirstOrDefault(i => i.Id == id);

    public ObjectiveVm? GetObjective(Guid initiativeId, Guid objectiveId) =>
        GetInitiative(initiativeId)?.Objectives.FirstOrDefault(o => o.Id == objectiveId);

    // ----- Mutations (in-memory only) -----
    // TODO: POST to /api/v1/initiatives/{id}/objectives once the WASM client is wired to the API.
    public ObjectiveVm AddObjective(Guid initiativeId, string name, string description)
    {
        var initiative = GetInitiative(initiativeId)
            ?? throw new InvalidOperationException("Initiative not found.");

        var objective = new ObjectiveVm(
            Guid.CreateVersion7(), name, description, initiativeId,
            DateTime.Today, null, new List<KpiVm>());

        ((List<ObjectiveVm>)initiative.Objectives).Add(objective);
        return objective;
    }

    // TODO: POST to /api/v1/objectives/{id}/kpis once the WASM client is wired to the API.
    public KpiVm AddKpi(
        Guid initiativeId, Guid objectiveId, string name, string unit,
        KpiDirection direction, decimal baseline, decimal target, decimal current, DateTime due)
    {
        var objective = GetObjective(initiativeId, objectiveId)
            ?? throw new InvalidOperationException("Objective not found.");

        var kpi = new KpiVm(
            Guid.CreateVersion7(), name, unit, direction, baseline, target, current, due,
            KpiStatus.NotStarted, objectiveId, DateTime.Today, null, new List<MeasurementVm>());

        ((List<KpiVm>)objective.Kpis).Add(kpi);
        return kpi;
    }

    // ----- Mock dataset -----

    private static Guid Id(string seed)
    {
        // Deterministic GUID from a seed string so links stay stable between renders.
        // FNV-1a over the seed fills the 16 bytes — no crypto APIs (unsupported in WASM).
        var bytes = new byte[16];
        ulong hash = 14695981039346656037UL;
        for (var i = 0; i < seed.Length; i++)
        {
            hash = (hash ^ seed[i]) * 1099511628211UL;
        }

        for (var i = 0; i < 16; i++)
        {
            bytes[i] = (byte)(hash >> ((i % 8) * 8));
            if (i == 7)
            {
                // re-mix so the upper and lower halves differ
                hash ^= (ulong)seed.Length * 0x9E3779B97F4A7C15UL;
            }
        }

        return new Guid(bytes);
    }

    private static List<InitiativeVm> Build()
    {
        var today = DateTime.Today;

        // --- Initiative 1: Customer Retention 2026 ---
        var i1 = Id("initiative-retention");
        var i1o1 = Id("obj-reduce-churn");
        var i1o2 = Id("obj-improve-onboarding");

        var initiative1 = new InitiativeVm(
            i1,
            "Customer Retention 2026",
            "Reduce churn and deepen engagement across the existing customer base through proactive success programmes and a redesigned onboarding journey.",
            today.AddMonths(-3),
            today.AddMonths(9),
            today.AddMonths(-3).AddDays(-2),
            today.AddDays(-5),
            new List<ObjectiveVm>
            {
                new(
                    i1o1,
                    "Reduce monthly churn",
                    "Bring logo churn down by addressing the top three cancellation drivers identified in Q4 research.",
                    i1,
                    today.AddMonths(-3),
                    today.AddDays(-5),
                    new List<KpiVm>
                    {
                        Kpi(Id("kpi-churn-rate"), "Monthly logo churn", "%", KpiDirection.Decrease,
                            baseline: 4.8m, target: 2.5m, current: 3.4m, due: today.AddMonths(6), KpiStatus.OnTrack, i1o1,
                            ("Q4 baseline", 4.8m), ("Jan", 4.3m), ("Feb", 3.9m), ("Mar", 3.4m)),
                        Kpi(Id("kpi-nps"), "Net promoter score", "pts", KpiDirection.Increase,
                            baseline: 32m, target: 55m, current: 41m, due: today.AddMonths(7), KpiStatus.AtRisk, i1o1,
                            ("Baseline", 32m), ("Q1 survey", 37m), ("Q2 survey", 41m)),
                    }),
                new(
                    i1o2,
                    "Improve onboarding completion",
                    "Lift the share of new accounts that reach the activation milestone within their first 14 days.",
                    i1,
                    today.AddMonths(-2),
                    today.AddDays(-12),
                    new List<KpiVm>
                    {
                        Kpi(Id("kpi-activation"), "14-day activation rate", "%", KpiDirection.Increase,
                            baseline: 48m, target: 75m, current: 67m, due: today.AddMonths(4), KpiStatus.OnTrack, i1o2,
                            ("Baseline", 48m), ("Cohort A", 55m), ("Cohort B", 61m), ("Cohort C", 67m)),
                        Kpi(Id("kpi-time-to-value"), "Median time to first value", "days", KpiDirection.Decrease,
                            baseline: 9.0m, target: 4.0m, current: 6.5m, due: today.AddMonths(5), KpiStatus.OnTrack, i1o2,
                            ("Baseline", 9.0m), ("Mar", 7.8m), ("Apr", 6.5m)),
                    }),
            });

        // --- Initiative 2: Platform Reliability ---
        var i2 = Id("initiative-reliability");
        var i2o1 = Id("obj-uptime");
        var i2o2 = Id("obj-incident-response");

        var initiative2 = new InitiativeVm(
            i2,
            "Platform Reliability",
            "Harden the platform to meet enterprise SLA commitments — raise availability, cut incident frequency, and tighten mean time to recovery.",
            today.AddMonths(-5),
            today.AddMonths(2),
            today.AddMonths(-5).AddDays(-1),
            today.AddDays(-2),
            new List<ObjectiveVm>
            {
                new(
                    i2o1,
                    "Hit 99.95% availability",
                    "Sustain four-nines-five monthly availability across all production regions.",
                    i2,
                    today.AddMonths(-5),
                    today.AddDays(-2),
                    new List<KpiVm>
                    {
                        Kpi(Id("kpi-uptime"), "Monthly availability", "%", KpiDirection.Increase,
                            baseline: 99.5m, target: 99.95m, current: 99.6m, due: today.AddMonths(2), KpiStatus.OffTrack, i2o1,
                            ("Baseline", 99.5m), ("Region outage", 99.2m), ("Recovered", 99.6m)),
                    }),
                new(
                    i2o2,
                    "Faster incident response",
                    "Reduce mean time to recovery and the number of customer-impacting incidents per quarter.",
                    i2,
                    today.AddMonths(-4),
                    today.AddDays(-9),
                    new List<KpiVm>
                    {
                        Kpi(Id("kpi-mttr"), "Mean time to recovery", "min", KpiDirection.Decrease,
                            baseline: 95m, target: 30m, current: 52m, due: today.AddMonths(1), KpiStatus.AtRisk, i2o2,
                            ("Baseline", 95m), ("After runbooks", 71m), ("After on-call rework", 52m)),
                        Kpi(Id("kpi-incidents"), "Customer-impacting incidents", "count", KpiDirection.Decrease,
                            baseline: 12m, target: 3m, current: 3m, due: today.AddMonths(-1), KpiStatus.Completed, i2o2,
                            ("Q3", 12m), ("Q4", 7m), ("Q1", 3m)),
                    }),
            });

        // --- Initiative 3: Expansion Revenue (kick-off, nothing started) ---
        var i3 = Id("initiative-expansion");
        var i3o1 = Id("obj-upsell");

        var initiative3 = new InitiativeVm(
            i3,
            "Expansion Revenue",
            "Grow net revenue retention by building a repeatable upsell and cross-sell motion for the install base.",
            today.AddDays(-7),
            null,
            today.AddDays(-7),
            null,
            new List<ObjectiveVm>
            {
                new(
                    i3o1,
                    "Launch upsell playbook",
                    "Stand up the data, triggers, and enablement needed for a systematic upsell motion.",
                    i3,
                    today.AddDays(-7),
                    null,
                    new List<KpiVm>
                    {
                        Kpi(Id("kpi-nrr"), "Net revenue retention", "%", KpiDirection.Increase,
                            baseline: 104m, target: 118m, current: 104m, due: today.AddMonths(11), KpiStatus.NotStarted, i3o1,
                            ("Baseline", 104m)),
                    }),
            });

        return new List<InitiativeVm> { initiative1, initiative2, initiative3 };
    }

    private static KpiVm Kpi(
        Guid id,
        string name,
        string unit,
        KpiDirection direction,
        decimal baseline,
        decimal target,
        decimal current,
        DateTime due,
        KpiStatus status,
        Guid objectiveId,
        params (string Label, decimal Value)[] measurements)
    {
        var created = DateTime.Today.AddMonths(-3);
        var list = new List<MeasurementVm>();
        for (var index = 0; index < measurements.Length; index++)
        {
            var (label, value) = measurements[index];
            list.Add(new MeasurementVm(
                Id($"{id}-m{index}"),
                value,
                label,
                id,
                created.AddDays(index * 14)));
        }

        return new KpiVm(
            id, name, unit, direction, baseline, target, current, due, status, objectiveId,
            created, list.Count > 1 ? created.AddDays((list.Count - 1) * 14) : null, list);
    }
}
