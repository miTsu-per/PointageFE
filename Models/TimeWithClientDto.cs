using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobileApp.Models
{
    public class TimeWithClientDto
    {
        public int Id { get; set; }
        public int IdClient { get; set; }
        public string ClientName { get; set; }
        public string ClientEmail { get; set; }
        public DateTime Starttime { get; set; }
        public DateTime? Finishtime { get; set; }
        public int? Totaltime { get; set; }
        public bool? Approved { get; set; }
    }
}