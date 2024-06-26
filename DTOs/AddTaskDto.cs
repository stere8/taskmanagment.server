namespace TaskManagment.Server.DTOs
{
    public class AddTaskDto
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public DateTime? DueDate { get; set; } // Nullable for optional due date

        public bool Completed { get; set; } = false;  // Default value set in the constructor

        public int UserId { get; set; }
    }
}