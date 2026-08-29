using Microsoft.AspNetCore.SignalR;
using TmsApi.Application.Hubs;

namespace TmsApi.Api.Hubs;

public class TmsHub : Hub<ITmsHubClient>
{
    public override async Task OnConnectedAsync()
    {
        var studentId = Context.GetHttpContext()?.Request.Query["studentId"].ToString();
        if (!string.IsNullOrWhiteSpace(studentId))
            await Groups.AddToGroupAsync(Context.ConnectionId, GroupNames.Student(studentId));

        await base.OnConnectedAsync();
    }

    public Task JoinCourseGroup(string courseCode) =>
        Groups.AddToGroupAsync(Context.ConnectionId, GroupNames.Course(courseCode));

    public Task LeaveCourseGroup(string courseCode) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupNames.Course(courseCode));
}

public static class GroupNames
{
    public static string Student(string studentId) => $"student-{studentId}";
    public static string Course(string courseCode) => $"course-{courseCode}";
}
