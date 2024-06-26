using System.ComponentModel.DataAnnotations;

namespace TaskManagment.Server.Models
{
    public class Task
    {
        public int Id { get; set; } // Primary key

        [Required]  // Specify data annotation for required field
        [StringLength(255)]  // Specify maximum length
        public string Title { get; set; }

        public string Description { get; set; }

        public DateTime? DueDate { get; set; } // Nullable for optional due date

        public bool Completed { get; set; } = false;  // Default value set in the constructor

        public int UserId { get; set; }
    }

}
