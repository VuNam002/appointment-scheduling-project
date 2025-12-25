using System;
using System.Collections.Generic;

namespace ProjectMaui.Models.DTOs
{
    public class DoctorScheduleResponse
    {
        public int DoctorId { get; set; }
        public string DoctorName { get; set; }
        public string DepartmentName { get; set; }
        public List<ScheduleSlot> Schedules { get; set; }
    }

    public class ScheduleSlot
    {
        public string DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsAvailable { get; set; }
    }
}