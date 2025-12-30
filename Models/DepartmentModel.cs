namespace ProjectMaui.Models
{
    public class DepartmentModel
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public string Location { get; set; }

        // Quan trọng: Override ToString để Picker hiển thị đúng tên
        // trong trường hợp ItemDisplayBinding gặp vấn đề
        public override string ToString() => DepartmentName;
    }
}