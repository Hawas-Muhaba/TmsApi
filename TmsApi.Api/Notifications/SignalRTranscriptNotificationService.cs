using Microsoft.AspNetCore.SignalR;
using TmsApi.Api.Hubs;
using TmsApi.Application.Hubs;
using TmsApi.Application.Notifications;

namespace TmsApi.Api.Notifications;

public class SignalRTranscriptNotificationService(
    IHubContext<TmsHub, ITmsHubClient> hubContext) : ITranscriptNotificationService
{
    public Task NotifyTranscriptReadyAsync(int studentId, string reportId, string downloadUrl) =>
        hubContext.Clients
            .Group(GroupNames.Student(studentId.ToString()))
            .ReceiveTranscriptReady(reportId, downloadUrl);
}
