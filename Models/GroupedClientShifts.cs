using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobileApp.Models
{
    public class GroupedClientShifts
    {
        public int ClientId { get; set; }
        public string ClientName { get; set; }
        public string TotalTimeFormatted { get; set; }
        public List<ShiftInfo> Shifts { get; set; }
        public bool? AllApproved { get; set; }
    }

    public class ShiftInfo
    {
        public DateTime Starttime { get; set; }
        public DateTime? Finishtime { get; set; }
        public int Totaltime { get; set; }
        public bool? Approved { get; set; }

        public string ShiftLabel =>
            $"De {Starttime:t} à {(Finishtime.HasValue ? Finishtime.Value.ToString("t") : "en cours")} ({Totaltime} sec)";
    }

}
