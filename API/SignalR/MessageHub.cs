using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;


namespace API.SignalR;

[Authorize]
public class MessageHub(IMessageRepository messageRepository,
    IMemberRepository memberRepository, IHubContext<PresenceHub> presenceHub) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        var otherUser = httpContext?.Request?.Query["userId"].ToString()
            ?? throw new HubException("Other user not found");
        var groupName = GetGroupName(GetUserId(), otherUser);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        await SaveConnectionToGroupAsync(groupName);

        var messages = await messageRepository.GetMessageThread(GetUserId(), otherUser);

        await Clients.Group(groupName).SendAsync("ReceiveMessageThread", messages);
    }

    public async Task SendMessage(CreateMessageDto createMessageDto)
    {
        var sender = await memberRepository.GetMemberByIdAsync(GetUserId());
        var recipient = await memberRepository.GetMemberByIdAsync(createMessageDto.RecipientId);

        if (recipient == null || sender == null || sender.Id == createMessageDto.RecipientId)
            throw new HubException("Cannot send message");

        var message = new Message
        {
            SenderId = sender.Id,
            RecipientId = recipient.Id,
            Content = createMessageDto.Content
        };

        var groupName = GetGroupName(sender.Id, recipient.Id);
        var group = await messageRepository.GetMessageGroup(groupName);
        var userInGroup = group != null && group.Connections.Any(x =>
             x.UserId == message.RecipientId); // .Any() returns bool btw

        if (userInGroup)
        {
            message.DateRead = DateTime.UtcNow;
        }

        messageRepository.AddMessage(message);

        if (await messageRepository.SaveAllAsync()) // returns true if changes were made
        {
            await Clients.Group(groupName).SendAsync("NewMessage", message.ToDto());
            var connections = await PresenceTracker.GetConnectionsForUser(recipient.Id);
            if (connections != null && connections.Count > 0 && !userInGroup)
            {
                await presenceHub.Clients.Clients(connections)
                    .SendAsync("NewMessageReceived", message.ToDto());
            }
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await messageRepository.RemoveConnection(Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
    
    private async Task<bool> SaveConnectionToGroupAsync(string groupName)
    {
        var group = await GetOrCreateGroupAsync(groupName);
        var connection = new Connection(Context.ConnectionId, GetUserId());

        group.Connections.Add(connection);
        return await messageRepository.SaveAllAsync();
    }

    private async Task<Group> GetOrCreateGroupAsync(string groupName)
    {
        var group = await messageRepository.GetMessageGroup(groupName);

        if (group == null)
        {
            group = new Group(groupName);
            messageRepository.AddGroup(group);
        }

        return group;
    }

	/// <summary>
    /// Creates a deterministic group name for a two-person conversation.
    /// Order of parameters doesn't matter—"alice" and "bob" always returns "alice-bob".
    /// This ensures both users join the same SignalR group regardless of who connects first.
    /// </summary>
    private static string GetGroupName(string? caller, string? other)
    {
        var doesCallerComeFirst = string.CompareOrdinal(caller, other) < 0;
        return doesCallerComeFirst ? $"{caller}-{other}" : $"{other}-{caller}";
    }

    private string GetUserId()
    {
        return Context.User?.GetMemberId()
            ?? throw new HubException("Cannot get member id");
    }
}