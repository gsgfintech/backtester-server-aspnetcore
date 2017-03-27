using Capital.GSG.FX.Backtest.DataTypes;
using Capital.GSG.FX.Data.Core.ContractData;
using Capital.GSG.FX.Data.Core.OrderData;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backtester.Server.Connector.Tests
{
    [TestClass]
    public class OrdersConnectorTests
    {
        [TestMethod]
        public async Task TestPostOrder()
        {
            string controllerEndpoint = "https://localhost:44379/";

            OrdersConnector connector = OrdersConnector.GetConnector(controllerEndpoint);

            BacktestOrder order = new BacktestOrder()
            {
                Cross = Cross.EURUSD,
                LastUpdateTime = DateTime.Now,
                LimitPrice = 1.11,
                OrderId = 2,
                PlacedTime = DateTime.Now.AddMinutes(-5),
                Quantity = 20000,
                Side = OrderSide.BUY,
                Status = OrderStatusCode.PreSubmitted,
                StopPrice = 1.12,
                Type = OrderType.LIMIT,
                UsdQuantity = 1500,
                FillPrice = 1.2,
                History = new List<OrderHistoryPoint>()
                {
                    new OrderHistoryPoint() { Status = OrderStatusCode.PreSubmitted, Timestamp = DateTimeOffset.Now }
                },
                TrailingAmount = 0.005
            };

            var result = await connector.PostOrder("RegTestJob", order);

            Assert.IsNotNull(result);
        }
    }
}
