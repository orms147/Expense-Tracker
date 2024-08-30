using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Expense_Tracker.Models
{
    public class Category
    {
        [Key] public int CategoryId { get; set; }


        [Column(TypeName = "nvarchar(50)")]
        [Required] public string Title { get; set; }


        [Column(TypeName = "nvarchar(10)")]
        [Required] public string Type { get; set; } = "Expense";


        [Column(TypeName = "nvarchar(10)")]
        [Required] public string Icon { get; set; } = "";

        [NotMapped]
        public string? TittleAndIcon
        {
            get
            {
                return this.Icon + " " + this.Title;
            }
        }
    }
}
