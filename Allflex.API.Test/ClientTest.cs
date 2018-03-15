using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Allflex.API.Test
{
    [TestClass]
    public class ClientTest
    {
        // TODO: fill out apiKey
        private string apiKey = string.Empty;
        private string apiUrl = "http://testapi.allflexusa.com/";

        [TestMethod]
        public void PostOrderStatusTest()
        {
            // TODO: Build status object.
            var status = new AllfleXML.FlexOrderStatus.OrderStatus();
            bool result;
            using(var c = new Allflex.API.Client(apiKey, apiUrl))
            {
                result = c.PostStatusAsync(status).Result;
            }

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void GetOrderStatusTest()
        {
            // TODO: assign wsOrderId
            var wsOrderId = string.Empty;
            AllfleXML.FlexOrderStatus.OrderStatus result;
            using (var c = new Allflex.API.Client(apiKey, apiUrl))
            {
                result = c.GetOrderStatusAsync(wsOrderId).Result;
            }

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void PostOrderTest()
        {
            // TODO: Build order object
            var order = new AllfleXML.FlexOrder.OrderHeader();
            AllfleXML.FlexOrder.OrderHeader result;
            using (var c = new Allflex.API.Client(apiKey, apiUrl))
            {
                result = c.PostOrderAsync(order).Result;
            }

            Assert.IsNotNull(result);
            Assert.AreNotEqual(order.WSOrderId, result.WSOrderId);
            order.WSOrderId = result.WSOrderId;
            Assert.AreEqual(order, result);
        }

        [TestMethod]
        public void RetrieveOrdersTest()
        {
            AllfleXML.FlexOrder.Document result;
            using (var c = new Allflex.API.Client(apiKey, apiUrl))
            {
                result = c.RetrieveOrdersAsync().Result;
            }

            Assert.IsNotNull(result);
            Assert.IsTrue(result.OrderHeaders.Count > 0);
        }
    }
}
