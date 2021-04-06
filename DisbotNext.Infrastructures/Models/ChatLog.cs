using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisbotNext.Infrastructures.Sqlite.Models
{
    public class ChatLog
    {
        public ulong Id { get; }

        public Member Author { get; }

        public string Content { get; }

        public DateTime CreateAt { get; }

        public ChatLog(ulong id,
                       Member author, 
                       string content, 
                       DateTime createAt)
        {
            Id = id;
            Author = author;
            Content = content;
            CreateAt = createAt;
        }
    }
}
