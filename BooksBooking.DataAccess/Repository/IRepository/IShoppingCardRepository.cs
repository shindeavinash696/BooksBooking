using BooksBooking.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BooksBooking.DataAccess.Repository.IRepository
{
    public interface IShoppingCardRepository: IRepository<ShoppingCart>
    {
        void Update(ShoppingCart obj);
    }
}
