using System;

namespace ProjectMaui.Models
{
    public class DoctorScheduleModel
    {
        public int ScheduleId { get; set; }
        public int DoctorId { get; set; }
        public int DayOfWeek { get; set; } 
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        // Navigation property
        public DoctorInfoModel Doctor { get; set; }

        // Computed properties
        public string DayOfWeekName
        {
            get
            {
                return DayOfWeek switch
                {
                    2 => "Thứ Hai",
                    3 => "Thứ Ba",
                    4 => "Thứ Tư",
                    5 => "Thứ Năm",
                    6 => "Thứ Sáu",
                    7 => "Thứ Bảy",
                    8 => "Chủ Nhật",
                    _ => ""
                };
            }
        }

        public string TimeDisplay => $"{StartTime:hh\\:mm} - {EndTime:hh\\:mm}";
        public string ScheduleDisplay => $"{DayOfWeekName}: {TimeDisplay}";
    }
}