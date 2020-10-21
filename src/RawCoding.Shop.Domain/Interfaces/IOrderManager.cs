﻿using System.Collections.Generic;
using System.Threading.Tasks;
using RawCoding.Shop.Domain.Enums;
using RawCoding.Shop.Domain.Models;

namespace RawCoding.Shop.Domain.Interfaces
{
    public interface IOrderManager
    {
        bool OrderReferenceExists(string reference);

        Order GetOrderById(string id);

        Task<int> CreateOrder(Order order);
        Task<int> AdvanceOrder(string id);

        IEnumerable<Order> OrdersForAdminPanel(OrderStatus status);
    }
}