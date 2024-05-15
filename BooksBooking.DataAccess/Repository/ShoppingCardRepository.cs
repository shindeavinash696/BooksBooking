using BooksBooking.DataAccess.Data;
using BooksBooking.DataAccess.Repository.IRepository;
using BooksBooking.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BooksBooking.DataAccess.Repository
{
    public class ShoppingCardRepository : Repository<ShoppingCart>,IShoppingCardRepository
    {
        private ApplicationDbContext _db;
        public ShoppingCardRepository(ApplicationDbContext db): base(db)
        {
                _db = db;
        }
       

        public void Update(ShoppingCart obj)
        {
           _db.ShoppingCarts.Update(obj);
        }
    }
}
