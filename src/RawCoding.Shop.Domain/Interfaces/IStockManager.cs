﻿using System.Collections.Generic;
using System.Threading.Tasks;
using RawCoding.Shop.Domain.Models;

namespace RawCoding.Shop.Domain.Interfaces
{
    public interface IStockManager
    {
        Task<int> CreateStock(Stock stock);
        Task<int> DeleteStock(int id);
        Task<int> UpdateStockRange(IEnumerable<Stock> stockList);

        Stock GetStockWithProduct(int stockId);
        IEnumerable<Stock> GetStockWithProduct(int[] stockIds);
        bool HasStock(int stockId, int qty);
    }
}