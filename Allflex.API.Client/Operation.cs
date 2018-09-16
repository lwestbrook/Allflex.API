using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Allflex.API.Client
{
    public class Operation : IDisposable
    {
        HttpClient _client;

        /// <summary>
        /// Initializes a new instance of the <see cref="Client"/> class.
        /// </summary>
        /// <param name="apiKey">The API key.</param>
        /// <param name="apiUrl">The API URL.</param>
        public Operation(string apiKey, string apiUrl)
        {
            _client = new HttpClient { BaseAddress = new Uri(apiUrl) };
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Add("Authorization", "Bearer " + apiKey);
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
        }

        /// <summary>
        /// Gets the status string.
        /// </summary>
        /// <param name="status">The status.</param>
        /// <returns></returns>
        public static string GetStatusString(OrderStatusEnum status) => Enum.GetName(typeof(OrderStatusEnum), status);

        /// <summary>
        /// Gets the status from a string.
        /// </summary>
        /// <param name="status">The status</param>
        /// <returns></returns>
        public static OrderStatusEnum GetStatusEnum(string status) => (OrderStatusEnum)Enum.Parse(typeof(OrderStatusEnum), status);

        /// <summary>
        /// Sends the status.
        /// </summary>
        /// <param name="status">The status.</param>
        /// <returns></returns>
        public bool PostStatus(AllfleXML.FlexOrderStatus.OrderStatus status)
        {
            return PostStatusAsync(status).Result;
        }

        /// <summary>
        /// Sends the status.
        /// </summary>
        /// <param name="status">The status.</param>
        /// <returns></returns>
        public async Task<bool> PostStatusAsync(AllfleXML.FlexOrderStatus.OrderStatus status)
        {
            var endpoint = $"/api/admin/orders/status/{status.WSOrderId}";
            var body = AllfleXML.FlexOrderStatus.Parser.Export(status).ToString();
            var putMessage = new StringContent(body, Encoding.UTF8, "application/xml");

            var response = await _client.PostAsync(endpoint, putMessage);
            if (!response.IsSuccessStatusCode)
            {
                var httpErrorObject = await response.Content.ReadAsStringAsync();
                var uri = new Uri(_client.BaseAddress, endpoint);
                throw new Exception($"Bad response from {uri.ToString()}: {response.StatusCode} - {response.ReasonPhrase}\nThere was an issue with sending the status for the API for order {status.OrderId}, with API id {status.WSOrderId}\n{httpErrorObject}");
            }
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Gets the order status.
        /// </summary>
        /// <param name="wsOrderId">The ws order identifier.</param>
        /// <returns></returns>
        public AllfleXML.FlexOrderStatus.OrderStatus GetOrderStatus(string wsOrderId)
        {
            return GetOrderStatusAsync(wsOrderId).Result;
        }

        /// <summary>
        /// Gets the order status.
        /// </summary>
        /// <param name="wsOrderId">The ws order identifier.</param>
        /// <returns></returns>
        public async Task<AllfleXML.FlexOrderStatus.OrderStatus> GetOrderStatusAsync(string wsOrderId)
        {
            var endpoint = $"/api/orders/status/{wsOrderId}";
            var response = await _client.GetAsync(endpoint);

            if (!response.IsSuccessStatusCode)
            {
                var httpErrorObject = await response.Content.ReadAsStringAsync();
                var uri = new Uri(_client.BaseAddress, endpoint);
                throw new Exception($"Bad response from {uri.ToString()}: {response.StatusCode} - {response.ReasonPhrase}\nThere was an issue with pulling the order status for {wsOrderId} from the API for processing\n{httpErrorObject}");
            }

            var result = await response.Content.ReadAsStringAsync();
            return AllfleXML.FlexOrderStatus.Parser.Import(XDocument.Parse(result));
        }

        // TODO: Quote Order

        /// <summary>
        /// Posts the order to flex service.
        /// </summary>
        /// <param name="order">The order.</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public AllfleXML.FlexOrder.OrderHeader PostOrder(AllfleXML.FlexOrder.OrderHeader order)
        {
            return PostOrderAsync(order).Result;
        }

        /// <summary>
        /// Posts the order to flex service asynchronous.
        /// </summary>
        /// <param name="order">The order.</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<AllfleXML.FlexOrder.OrderHeader> PostOrderAsync(AllfleXML.FlexOrder.OrderHeader order)
        {
            var endpoint = "api/orders";
            var body = AllfleXML.FlexOrder.Parser.Export(order).ToString();
            var putMessage = new StringContent(body, Encoding.UTF8, "text/xml");
            var response = await _client.PostAsync(endpoint, putMessage);

            if (!response.IsSuccessStatusCode)
            {
                var httpErrorObject = await response.Content.ReadAsStringAsync();
                var uri = new Uri(_client.BaseAddress, endpoint);
                throw new Exception($"Bad response from {uri.ToString()}: {response.StatusCode} - {response.ReasonPhrase}\nThere was an issue saving the purchase order {order.PO} to the Allflex Order API\n{httpErrorObject}");
            }

            var result = await response.Content.ReadAsStringAsync();
            return AllfleXML.FlexOrder.Parser.Import(XDocument.Parse(result)).OrderHeaders.SingleOrDefault();
        }

        public AllfleXML.FlexOrder.OrderHeader ViewOrder(Guid wsOrderId)
        {
            throw new NotImplementedException();
        }

        public async Task<AllfleXML.FlexOrder.OrderHeader> ViewOrderAsync(Guid wsOrderId)
        {
            // GET
            throw new NotImplementedException();
        }

        public bool DeleteOrder(Guid wsOrderId)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> DeleteOrderAsync(Guid wsOrderId)
        {
            // DELETE
            throw new NotImplementedException();
        }

        /// <summary>
        /// Retrieves all orders.
        /// </summary>
        /// <param name="saveFile">if set to <c>true</c> [save file].</param>
        /// <returns></returns>
        public AllfleXML.FlexOrder.Document RetrieveOrders()
        {
            return RetrieveOrdersAsync().Result;
        }

        /// <summary>
        /// Retrieves all orders asynchronously.
        /// </summary>
        /// <param name="saveFile">if set to <c>true</c> [save file].</param>
        /// <returns></returns>
        public async Task<AllfleXML.FlexOrder.Document> RetrieveOrdersAsync()
        {
            var endpoint = "api/admin/orders/process";
            var response = await _client.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode)
            {
                var httpErrorObject = await response.Content.ReadAsStringAsync();
                var uri = new Uri(_client.BaseAddress, endpoint);
                throw new Exception($"Bad response from {uri.ToString()}: {response.StatusCode} - {response.ReasonPhrase}\nThere was an issue with pulling orders from the API for processing\n{httpErrorObject}");
            }

            var result = await response.Content.ReadAsStringAsync();
            return AllfleXML.FlexOrder.Parser.Import(XDocument.Parse(result));
        }

        public AllfleXML.FlexSpec.Specification CreateSpecification(AllfleXML.FlexSpec.Specification spec)
        {
            return CreateSpecificationAsync(spec).Result;
        }

        public async Task<AllfleXML.FlexSpec.Specification> CreateSpecificationAsync(AllfleXML.FlexSpec.Specification spec)
        {
            // admin
            // POST
            throw new NotImplementedException();
        }

        public bool DeleteSpecification(Guid specId)
        {
            return DeleteSpecificationAsync(specId).Result;
        }

        public async Task<bool> DeleteSpecificationAsync(Guid specId)
        {
            // admin
            // DELETE
            throw new NotImplementedException();
        }

        public bool DeleteSpecification(string name)
        {
            return DeleteSpecificationAsync(name).Result;
        }

        public async Task<bool> DeleteSpecificationAsync(string name)
        {
            // admin
            // DELETE
            throw new NotImplementedException();
        }

        public AllfleXML.FlexSpec.Specification GetSpecification(string name)
        {
            return GetSpecificationAsync(name).Result;
        }

        public async Task<AllfleXML.FlexSpec.Specification> GetSpecificationAsync(string name)
        {
            // GET
            throw new NotImplementedException();
        }

        public AllfleXML.FlexSpec.Specification GetSpeccification(Guid specId)
        {
            return GetSpecificationAsync(specId).Result;
        }

        public async Task<AllfleXML.FlexSpec.Specification> GetSpecificationAsync(Guid specId)
        {
            // GET
            throw new NotImplementedException();
        }
        
        // TODO: Get shilouette

        // TODO: Get Outline

        // TODO: Get Logo

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}
