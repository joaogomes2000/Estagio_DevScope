using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.Contructor
{
    class constructor
    {

        public class Appointment
        {
            public string? Subject { get; set; }
            public string? Organizer { get; set; }
            public long Start { get; set; }
            public long End { get; set; }
            public bool Private { get; set; }
        }

        public class MeetingRoom
        {
            public int id { get; set; } = 0;
            public string? Roomlist { get; set; }
            public string? Name { get; set; }
            public string? RoomAlias { get; set; }
            public string? Email { get; set; }
            public List<Appointment> Appointments { get; set; } = new List<Appointment>();
            public bool Busy { get; set; }
        }
    }
}
