using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BooksBooking.Models.ViewModels
{
    public class ShoppingCartVM
    {
        public IEnumerable<ShoppingCart> ShoppingCartList { get; set; }
        public OrderHeader OrderHeader { get; set; }
        public OrderHeader OrderTotal { get; set; }  
    }
}
