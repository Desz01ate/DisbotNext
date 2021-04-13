using System;
using System.Collections.Generic;
using System.Text;

namespace DisbotNext.Infrastructures.Common.Models
{
    public class Member
    {
        public ulong Id { get; private set; }

        public int Level { get; private set; }

        public double Exp { get; private set; }

        public bool AutoMoveToChannel { get; set; }

        public virtual List<ChatLog> ChatLogs { get; set; }

        public double NextExp => Math.Round(Level + Math.Pow(Level / 2, 1.115) * Math.Sqrt(Level), 0);

        public Member()
        {

        }

        public Member(ulong id,
                      int level,
                      double exp,
                      bool autoMoveToChannel)
        {
            Id = id;
            Level = level;
            Exp = exp;
            AutoMoveToChannel = autoMoveToChannel;
        }

        public bool ExpGained(double exp)
        {
            this.Exp += exp;
            bool levelUp = false;
            while (this.Exp >= this.NextExp)
            {
                this.Level += 1;
                this.Exp = this.NextExp - this.Exp;
                if (this.Exp < 0)
                    this.Exp = 0;
                levelUp = true;
            }
            return levelUp;
        }

        internal static Member NewMember(ulong id)
        {
            return new Member(id, 1, 0, false);
        }
    }
}
