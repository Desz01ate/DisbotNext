using System;

namespace DisbotNext.Infrastructures.Common.Models
{
    public class ChatLog
    {
        public ChatLog()
        {
        }

        public ChatLog(ulong id, ulong memberId, string content, DateTime createAt)
        {
            Id = id;
            MemberId = memberId;
            Content = content;
            CreateAt = createAt;
        }

        public ulong Id { get; }

        public ulong MemberId { get; }

        public string Content { get; }

        public DateTime CreateAt { get; }

        public virtual Member Member { get; protected set; }
    }
}
