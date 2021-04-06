using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisbotNext.Infrastructures.Sqlite.Models
{
    public class ChatLog
    {
        public ulong Id { get; set; }

        public virtual Member Author { get; set; }

        public string Content { get; set; }

        public DateTime CreateAt { get; set; }

        public ChatLog()
        {

        }

        public ChatLog(ulong id,
                       string content,
                       DateTime createAt)
        {
            Id = id;
            Content = content;
            CreateAt = createAt;
        }
    }
}
