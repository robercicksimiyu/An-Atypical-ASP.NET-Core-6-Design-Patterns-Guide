﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mediator;

public interface IChatRoom
{
    void Join(IParticipant participant);
    void Send(ChatMessage message);
}

public interface IParticipant
{
    string Name { get; }
    void Send(string message);
    void ReceiveMessage(ChatMessage message);
    void ChatRoomJoined(IChatRoom chatRoom);
}

public class ChatMessage
{
    public ChatMessage(IParticipant from, string content)
    {
        Sender = from ?? throw new ArgumentNullException(nameof(from));
        Content = content ?? throw new ArgumentNullException(nameof(content));
    }

    public IParticipant Sender { get; }
    public string Content { get; }
}

public class User : IParticipant
{
    private IChatRoom? _chatRoom;
    private readonly IMessageWriter<ChatMessage> _messageWriter;

    public User(IMessageWriter<ChatMessage> messageWriter, string name)
    {
        _messageWriter = messageWriter ?? throw new ArgumentNullException(nameof(messageWriter));
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    public string Name { get; }

    public void ChatRoomJoined(IChatRoom chatRoom)
    {
        _chatRoom = chatRoom;
    }

    public void ReceiveMessage(ChatMessage message)
    {
        _messageWriter.Write(message);
    }

    public void Send(string message)
    {
        if(_chatRoom == null)
        {
            throw new ChatRoomNotJoinedException();
        }
        _chatRoom.Send(new ChatMessage(this, message));
    }
}

public class ChatRoomNotJoinedException : Exception
{
    public ChatRoomNotJoinedException()
        : base("You must join a chat room before sending a message.") { }
}

public class ChatRoom : IChatRoom
{
    private readonly List<IParticipant> _participants = new List<IParticipant>();

    public void Join(IParticipant participant)
    {
        _participants.Add(participant);
        participant.ChatRoomJoined(this);
        Send(new ChatMessage(participant, "Has joined the channel"));
    }

    public void Send(ChatMessage message)
    {
        _participants.ForEach(participant => participant.ReceiveMessage(message));
    }
}
