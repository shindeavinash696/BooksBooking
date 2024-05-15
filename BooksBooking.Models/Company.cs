using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BooksBooking.Models
{
    public class Company
    {

        public int CompanyId { get; set; }
        [Required]
        public string Name { get; set; }
        public string? StreetAddress { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostCode { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
