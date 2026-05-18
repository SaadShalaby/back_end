public class DoctorDashboardDto
{
    public int TotalSessions { get; set; }
    public int NewMessages { get; set; }
    public int TotalPatients { get; set; }
    public int UpcomingSessionsCount { get; set; }

    public List<RecentActivityDto> RecentActivities { get; set; } = new();
}

public class RecentActivityDto
{
    public string Title { get; set; } = null!;
    public string Subtitle { get; set; } = null!;
    public string TimeAgo { get; set; } = null!;
    public string Type { get; set; } = null!; // Message, Session, Task
}