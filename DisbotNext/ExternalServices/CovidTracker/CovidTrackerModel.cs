using System.Text;

namespace DisbotNext.ExternalServices.CovidTracker
{
    public class CovidTrackerModel
    {
        public string Country { get; set; }

        public int Cases { get; set; }

        public int TodayCases { get; set; }

        public int Deaths { get; set; }

        public int TodayDeaths { get; set; }

        public int Recovered { get; set; }

        public int Active { get; set; }

        public int Critical { get; set; }

        public int TotalTests { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine($"😷 จำนวนผู้ติดเชื้อที่พบวันนี้ {this.TodayCases:n0}");
            sb.AppendLine($"😷 จำนวนผู้ติดเชื้อที่พบทั้งสิ้น {this.Cases:n0}");
            sb.AppendLine($"💀 จำนวนผู้เสียชีวิตวันนี้ {this.TodayDeaths:n0}");
            sb.AppendLine($"💀 จำนวนผู้เสียชีวิตทั้งสิ้น {this.Deaths:n0}");
            sb.AppendLine($"🏨 จำนวนผู้ป่วยอาการวิกฤตทั้งสิ้น {this.Critical:n0}");
            sb.AppendLine($"🏨 จำนวนผู้ป่วยที่กำลังรักษาตัว {this.Active:n0}");
            sb.AppendLine($"👌 จำนวนผู้ป่วยที่รักษาหายแล้วทั้งสิ้น {this.Recovered:n0}");

            return sb.ToString();
        }
    }
}