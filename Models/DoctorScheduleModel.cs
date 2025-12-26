using System;

namespace ProjectMaui.Models
{
    public class DoctorScheduleModel
    {
        public int ScheduleId { get; set; }
        public int DoctorId { get; set; }
        public int DayOfWeek { get; set; } // 2=Thứ 2, ..., 8=Chủ Nhật (Tùy quy ước DB)
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public string DayOfWeekName
        {
            get
            {
                return DayOfWeek switch
                {
                    2 => "Thứ 2",
                    3 => "Thứ 3",
                    4 => "Thứ 4",
                    5 => "Thứ 5",
                    6 => "Thứ 6",
                    7 => "Thứ 7",
                    8 => "Chủ Nhật",
                    _ => "Chủ Nhật" // Mặc định hoặc xử lý theo quy ước 0/1 của C#
                };
            }
        }

        public string TimeDisplay => $"{StartTime:hh\\:mm} - {EndTime:hh\\:mm}";
    }
}