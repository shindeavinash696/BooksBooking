using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BooksBooking.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required]
        [DisplayName("Category Name")]
        public string Name { get; set; }

        [DisplayName("Display Order")]
        [Range(1, 100, ErrorMessage = "Display order must be beetween 1 to 100 only !!")]
        public int DisplayOrder { get; set; }
        public DateTime CreatedDateTime { get; set; } = DateTime.Now;
    }
}
