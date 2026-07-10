using ProgramPulse.Web.Models;

namespace ProgramPulse.Web.Services;

/// <summary>
/// In-memory mock dataset for the Programmes UI. Uses stable GUIDs so drill-down
/// links resolve across pages. UI-only stub — replace with an API-backed source
/// (same method shape) once the WASM client is wired to /api/v1/programmes.
/// </summary>
public sealed class SampleData
{
    private readonly List<ProgrammeVm> _programmes;

    public SampleData()
    {
        _programmes = Build();
    }

    public IReadOnlyList<ProgrammeVm> GetProgrammes() => _programmes;

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

    public ProgrammeVm? GetProgramme(Guid id) =>
        _programmes.FirstOrDefault(i => i.Id == id);

    // ----- Dashboard summary -----
    // Mirrors the API's DashboardSummaryResponse. Status counts, health, overdue and flagged
    // are computed from the mock programmes; the 8-week trend, today's reviews and velocity
    // deltas are hand-authored because the domain has no status history or Review entity yet.
    // TODO: replace with a single GET /api/v1/dashboard call once the WASM client hits the API.
    public DashboardSummaryVm GetDashboardSummary()
    {
        const int overdueThresholdDays = KpiThresholds.OverdueMeasurementDays;
        var today = DateTime.Today;

        var allKpis = _programmes.SelectMany(i => i.AllKpis).ToList();

        var onTrack = _programmes.Count(i => i.AggregateStatus is KpiStatus.OnTrack
            or KpiStatus.Completed or KpiStatus.NotStarted);
        var atRisk = _programmes.Count(i => i.AggregateStatus == KpiStatus.AtRisk);
        var offTrack = _programmes.Count(i => i.AggregateStatus == KpiStatus.OffTrack);

        var onTrackKpis = allKpis.Count(k => k.Status == KpiStatus.OnTrack);
        var health = allKpis.Count == 0 ? 0 : (int)Math.Round((double)onTrackKpis / allKpis.Count * 100);

        var overdue = allKpis.Count(k =>
            k.Measurements.Count == 0
            || k.Measurements.Max(m => m.CreatedDate) < today.AddDays(-overdueThresholdDays));

        var flagged = _programmes
            .Where(i => i.AggregateStatus is KpiStatus.AtRisk or KpiStatus.OffTrack)
            .OrderByDescending(i => i.AggregateStatus == KpiStatus.OffTrack)
            .Select(i =>
            {
                var worst = i.AllKpis.FirstOrDefault(k => k.Status == i.AggregateStatus)
                            ?? i.AllKpis.First();
                return new FlaggedProgrammeVm(
                    i.Id,
                    i.Name,
                    i.AggregateStatus == KpiStatus.OffTrack ? "off" : "warn",
                    "Strategy office",
                    i.AggregateStatus == KpiStatus.OffTrack ? "Mara Lin" : "Devon Pell",
                    worst.Name,
                    worst.CurrentValue,
                    worst.TargetValue,
                    worst.ProgressPercent);
            })
            .ToList();

        // Hand-authored 8-week trajectory ending at the current real counts (no status history).
        var trend = new List<TrendPointVm>
        {
            new(DateOnly.FromDateTime(today.AddDays(-49)), 4, 0, 0),
            new(DateOnly.FromDateTime(today.AddDays(-42)), 4, 1, 0),
            new(DateOnly.FromDateTime(today.AddDays(-35)), 3, 1, 0),
            new(DateOnly.FromDateTime(today.AddDays(-28)), 3, 2, 0),
            new(DateOnly.FromDateTime(today.AddDays(-21)), 2, 2, 1),
            new(DateOnly.FromDateTime(today.AddDays(-14)), 2, 1, 1),
            new(DateOnly.FromDateTime(today.AddDays(-7)), Math.Max(onTrack, 1), Math.Max(atRisk - 1, 0), offTrack),
            new(DateOnly.FromDateTime(today), onTrack, atRisk, offTrack),
        };

        // No Review entity in the domain — these are illustrative until one exists.
        var reviews = new List<UpcomingReviewVm>
        {
            new("09:30", "Platform Reliability review", "Strategy office · availability off track", "off"),
            new("11:00", "Retention onboarding sync", "Growth team · 2 KPIs stale >14d", "warn"),
            new("14:30", "Expansion Revenue kickoff", "New programme · not started", "warn"),
            new("16:00", "Weekly steering call", "Programme leads · portfolio review", "track"),
        };

        var coverage = allKpis.Count == 0
            ? 0
            : (int)Math.Round((double)allKpis.Count(k => k.Measurements.Count > 0) / allKpis.Count * 100);

        var velocity = new VelocityStatsVm(
            KpisHitTargetThisWeek: allKpis.Count(k => k.Status == KpiStatus.Completed),
            KpisNewlySlipped: 1,
            MeasurementCoveragePercent: coverage,
            AvgDaysToTargetClose: 38);

        return new DashboardSummaryVm(
            TeamName: "Strategy office",
            ActiveProgrammeCount: _programmes.Count,
            GeneratedAtUtc: DateTime.UtcNow.AddMinutes(-4),
            HealthScore: health,
            HealthDeltaPercent: 6.2,
            Status: new StatusCountsVm(onTrack, atRisk, offTrack, _programmes.Count),
            AtRiskDeltaSinceLastWeek: 2,
            OverdueKpiCount: overdue,
            TotalKpiCount: allKpis.Count,
            Flagged: flagged,
            Trend: trend,
            Reviews: reviews,
            Velocity: velocity);
    }

    public ObjectiveVm? GetObjective(Guid programmeId, Guid objectiveId) =>
        GetProgramme(programmeId)?.Objectives.FirstOrDefault(o => o.Id == objectiveId);

    public KpiVm? GetKpi(Guid programmeId, Guid objectiveId, Guid kpiId) =>
        GetObjective(programmeId, objectiveId)?.Kpis.FirstOrDefault(k => k.Id == kpiId);

    // ----- Mutations (in-memory only) -----
    // An objective is created together with zero or more KPIs.
    // TODO: POST to /api/v1/programmes/{id}/objectives once the WASM client is wired to the API.
    public ObjectiveVm AddObjective(
        Guid programmeId, string name, string description,
        string kpiName, string kpiUnit, KpiDirection direction,
        decimal baseline, decimal target, decimal current, DateTime due)
    {
        var programme = GetProgramme(programmeId)
            ?? throw new InvalidOperationException("Programme not found.");

        var objectiveId = Guid.CreateVersion7();
        var kpi = new KpiVm(
            Guid.CreateVersion7(), kpiName, kpiUnit, direction, baseline, target, current, due,
            KpiStatus.NotStarted, objectiveId, DateTime.Today, null, new List<MeasurementVm>());

        var objective = new ObjectiveVm(
            objectiveId, name, description, programmeId,
            DateTime.Today, null, new List<KpiVm> { kpi });

        ((List<ObjectiveVm>)programme.Objectives).Add(objective);
        return objective;
    }

    // TODO: PUT /api/v1/programmes/{id} once the WASM client is wired to the API.
    public ProgrammeVm UpdateProgramme(
        Guid id, string name, string description, DateTime startDate, DateTime? endDate)
    {
        var index = _programmes.FindIndex(i => i.Id == id);
        if (index < 0)
        {
            throw new InvalidOperationException("Programme not found.");
        }

        var updated = _programmes[index] with
        {
            Name = name,
            Description = description,
            StartDate = startDate,
            EndDate = endDate,
            LastModifiedDate = DateTime.Today,
        };

        _programmes[index] = updated;
        return updated;
    }

    // TODO: PUT /api/v1/objectives/{id} once the WASM client is wired to the API.
    public ObjectiveVm UpdateObjective(
        Guid programmeId, Guid objectiveId, string name, string description)
    {
        var programme = GetProgramme(programmeId)
            ?? throw new InvalidOperationException("Programme not found.");

        var objectives = (List<ObjectiveVm>)programme.Objectives;
        var index = objectives.FindIndex(o => o.Id == objectiveId);
        if (index < 0)
        {
            throw new InvalidOperationException("Objective not found.");
        }

        var updated = objectives[index] with
        {
            Name = name,
            Description = description,
            LastModifiedDate = DateTime.Today,
        };

        objectives[index] = updated;
        return updated;
    }

    // TODO: POST /api/v1/kpis/{id}/measurements once the WASM client is wired to the API.
    public MeasurementVm AddMeasurement(
        Guid programmeId, Guid objectiveId, Guid kpiId, decimal value, string? notes)
    {
        var kpi = GetKpi(programmeId, objectiveId, kpiId)
            ?? throw new InvalidOperationException("KPI not found.");

        var measurement = new MeasurementVm(
            Guid.CreateVersion7(), value, notes, kpiId, DateTime.Today);

        ((List<MeasurementVm>)kpi.Measurements).Add(measurement);
        return measurement;
    }

    // TODO: PUT /api/v1/measurements/{id} once the WASM client is wired to the API.
    public MeasurementVm UpdateMeasurement(
        Guid programmeId, Guid objectiveId, Guid kpiId, Guid measurementId,
        decimal value, string? notes)
    {
        var kpi = GetKpi(programmeId, objectiveId, kpiId)
            ?? throw new InvalidOperationException("KPI not found.");

        var measurements = (List<MeasurementVm>)kpi.Measurements;
        var index = measurements.FindIndex(m => m.Id == measurementId);
        if (index < 0)
        {
            throw new InvalidOperationException("Measurement not found.");
        }

        var updated = measurements[index] with { Value = value, Notes = notes };
        measurements[index] = updated;
        return updated;
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

    private static List<ProgrammeVm> Build()
    {
        var today = DateTime.Today;

        // --- Programme 1: Customer Retention 2026 ---
        var i1 = Id("programme-retention");
        var i1o1 = Id("obj-reduce-churn");
        var i1o1b = Id("obj-raise-nps");
        var i1o2 = Id("obj-improve-onboarding");
        var i1o2b = Id("obj-time-to-value");

        var programme1 = new ProgrammeVm(
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
                        Kpi(Id("kpi-churn-save-rate"), "Save-desk save rate", "%", KpiDirection.Increase,
                            baseline: 20m, target: 45m, current: 33m, due: today.AddMonths(6), KpiStatus.AtRisk, i1o1,
                            ("Baseline", 20m), ("Jan", 27m), ("Feb", 31m), ("Mar", 33m)),
                    }),
                new(
                    i1o1b,
                    "Raise net promoter score",
                    "Turn detractors into promoters by closing the loop on the top survey themes.",
                    i1,
                    today.AddMonths(-3),
                    today.AddDays(-5),
                    [Kpi(Id("kpi-nps"), "Net promoter score", "pts", KpiDirection.Increase,
                        baseline: 32m, target: 55m, current: 41m, due: today.AddMonths(7), KpiStatus.AtRisk, i1o1b,
                        ("Baseline", 32m), ("Q1 survey", 37m), ("Q2 survey", 41m))]),
                new(
                    i1o2,
                    "Improve onboarding completion",
                    "Lift the share of new accounts that reach the activation milestone within their first 14 days.",
                    i1,
                    today.AddMonths(-2),
                    today.AddDays(-12),
                    [Kpi(Id("kpi-activation"), "14-day activation rate", "%", KpiDirection.Increase,
                        baseline: 48m, target: 75m, current: 67m, due: today.AddMonths(4), KpiStatus.OnTrack, i1o2,
                        ("Baseline", 48m), ("Cohort A", 55m), ("Cohort B", 61m), ("Cohort C", 67m))]),
                new(
                    i1o2b,
                    "Shorten time to first value",
                    "Get new accounts to their first meaningful outcome faster after signup.",
                    i1,
                    today.AddMonths(-2),
                    today.AddDays(-12),
                    [Kpi(Id("kpi-time-to-value"), "Median time to first value", "days", KpiDirection.Decrease,
                        baseline: 9.0m, target: 4.0m, current: 6.5m, due: today.AddMonths(5), KpiStatus.OnTrack, i1o2b,
                        ("Baseline", 9.0m), ("Mar", 7.8m), ("Apr", 6.5m))]),
            });

        // --- Programme 2: Platform Reliability ---
        var i2 = Id("programme-reliability");
        var i2o1 = Id("obj-uptime");
        var i2o2 = Id("obj-incident-response");
        var i2o2b = Id("obj-reduce-incidents");

        var programme2 = new ProgrammeVm(
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
                    [Kpi(Id("kpi-uptime"), "Monthly availability", "%", KpiDirection.Increase,
                        baseline: 99.5m, target: 99.95m, current: 99.6m, due: today.AddMonths(2), KpiStatus.OffTrack, i2o1,
                        ("Baseline", 99.5m), ("Region outage", 99.2m), ("Recovered", 99.6m))]),
                new(
                    i2o2,
                    "Faster incident response",
                    "Reduce mean time to recovery so customer-impacting incidents are resolved quickly.",
                    i2,
                    today.AddMonths(-4),
                    today.AddDays(-9),
                    [Kpi(Id("kpi-mttr"), "Mean time to recovery", "min", KpiDirection.Decrease,
                        baseline: 95m, target: 30m, current: 52m, due: today.AddMonths(1), KpiStatus.AtRisk, i2o2,
                        ("Baseline", 95m), ("After runbooks", 71m), ("After on-call rework", 52m))]),
                new(
                    i2o2b,
                    "Cut customer-impacting incidents",
                    "Bring down the number of customer-impacting incidents per quarter.",
                    i2,
                    today.AddMonths(-4),
                    today.AddDays(-9),
                    [Kpi(Id("kpi-incidents"), "Customer-impacting incidents", "count", KpiDirection.Decrease,
                        baseline: 12m, target: 3m, current: 3m, due: today.AddMonths(-1), KpiStatus.Completed, i2o2b,
                        ("Q3", 12m), ("Q4", 7m), ("Q1", 3m))]),
            });

        // --- Programme 3: Expansion Revenue (kick-off, nothing started) ---
        var i3 = Id("programme-expansion");
        var i3o1 = Id("obj-upsell");

        var programme3 = new ProgrammeVm(
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
                    [Kpi(Id("kpi-nrr"), "Net revenue retention", "%", KpiDirection.Increase,
                        baseline: 104m, target: 118m, current: 104m, due: today.AddMonths(11), KpiStatus.NotStarted, i3o1,
                        ("Baseline", 104m))]),
            });

        return new List<ProgrammeVm> { programme1, programme2, programme3 };
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
