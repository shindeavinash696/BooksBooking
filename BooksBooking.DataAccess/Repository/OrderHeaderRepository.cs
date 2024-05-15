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
    public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {
        private ApplicationDbContext _db;
        public OrderHeaderRepository(ApplicationDbContext db): base(db)
        {
                _db = db;
        }
       

        public void Update(OrderHeader obj)
        {
           _db.OrderHeaders.Update(obj);
        }

		public void UpdateStatus(int id, string orderStatus, string? paymentStatus = null)
		{
            var orderFromDb = _db.OrderHeaders.FirstOrDefault(u => id == u.OrderHeaderId);

            if(orderFromDb != null)
            {
                orderFromDb.OrderStatus = orderStatus;
                if (!string.IsNullOrEmpty(paymentStatus))
                {
                    orderFromDb.PaymentStatus = paymentStatus;
                }
            }
		}

		public void UpdateStripePaymentID(int id, string sessionId, string paymentIntenId)
		{
			var orderFromDb = _db.OrderHeaders.FirstOrDefault(u => id == u.OrderHeaderId);
            if (!string.IsNullOrEmpty(sessionId))
            {
                orderFromDb.SessionId = sessionId;
            }
			if (!string.IsNullOrEmpty(paymentIntenId))
			{
				orderFromDb.PaymentIntenId = paymentIntenId;
                orderFromDb.PaymentDate = DateTime.Now;
			}
		}
	}
}
