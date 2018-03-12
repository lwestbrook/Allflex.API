using System;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using AllfleXML.FlexOrderStatus;

namespace Allflex.API
{
    public class Client : IDisposable
    {
        HttpClient _client;

        /// <summary>
        /// Initializes a new instance of the <see cref="Client"/> class.
        /// </summary>
        /// <param name="apiKey">The API key.</param>
        /// <param name="apiUrl">The API URL.</param>
        public Client(string apiKey, string apiUrl)
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
            var endpoint = $"/api/orders/status/{status.WSOrderId}";
            try
            {
                var putMessage = new StringContent(status.Export().ToString(), Encoding.UTF8, "application/xml");

                var response = await _client.PostAsync(endpoint, putMessage);
                return response.IsSuccessStatusCode;
            }
            catch (Exception e)
            {
                var message = $"There was an issue with sending the status for the API for order {status.OrderId}, with API id {status.WSOrderId}:\n\n";
                message += $"{e.Message} - {e.InnerException.Message}";
                Console.WriteLine(message);
                throw;
            }

        }

        /// <summary>
        /// Gets the order status.
        /// </summary>
        /// <param name="wsOrderId">The ws order identifier.</param>
        /// <returns></returns>
        public OrderStatus GetOrderStatus(string wsOrderId)
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
            try
            {
                var response = await _client.GetAsync(endpoint);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Bad response: {response.StatusCode} - {response.ReasonPhrase}");
                }

                var result = await response.Content.ReadAsStringAsync();
                return AllfleXML.FlexOrderStatus.Parser.Import(XDocument.Parse(result));
            }
            catch (Exception e)
            {
                var message = $"There was an issue with pulling the order status for {wsOrderId} from the API for processing:\n\n";
                message += $"{e.Message} - {e.InnerException?.Message}";
                Console.WriteLine(message);
                throw;
            }
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
            var endpoint = "api/orders/process";
            try
            {
                var response = await _client.GetAsync(endpoint);
                if (!response.IsSuccessStatusCode)
                {
                    var uri = new Uri(_client.BaseAddress, endpoint);
                    throw new Exception($"Bad response from {uri.ToString()}: {response.StatusCode} - {response.ReasonPhrase}");
                }

                var result = await response.Content.ReadAsStringAsync();
                return AllfleXML.FlexOrder.Parser.Import(XDocument.Parse(result));
            }
            catch (Exception e)
            {
                var message = $"There was an issue with pulling orders from the API for processing:\n\n";
                message += $"{e.Message} - {e.InnerException.Message}";
                Console.WriteLine(message);
                throw;
            }
        }

        /// <summary>
        /// Posts the order to flex service.
        /// </summary>
        /// <param name="order">The order.</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public AllfleXML.FlexOrder.Document PostOrder(AllfleXML.FlexOrder.OrderHeader order)
        {
            return PostOrderAsync(order).Result;
        }

        /// <summary>
        /// Posts the order to flex service asynchronous.
        /// </summary>
        /// <param name="order">The order.</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<AllfleXML.FlexOrder.Document> PostOrderAsync(AllfleXML.FlexOrder.OrderHeader order)
        {
            var document = new AllfleXML.FlexOrder.Document
            {
                OrderHeaders = new List<AllfleXML.FlexOrder.OrderHeader> { order }
            };

            return await PostOrderAsync(document);
        }

        /// <summary>
        /// Posts the order to flex service.
        /// </summary>
        /// <param name="order">The order.</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public AllfleXML.FlexOrder.Document PostOrder(AllfleXML.FlexOrder.Document document)
        {
            return PostOrderAsync(document).Result;
        }

        /// <summary>
        /// Posts the order to flex service asynchronous.
        /// </summary>
        /// <param name="order">The order.</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<AllfleXML.FlexOrder.Document> PostOrderAsync(AllfleXML.FlexOrder.Document document)
        {
            var endpoint = "api/orders";
            try
            {
                var putMessage = new StringContent(AllfleXML.FlexOrder.Parser.Export(document).ToString(), Encoding.UTF8, "application/xml");

                var response = await _client.PostAsync(endpoint, putMessage);
                if (!response.IsSuccessStatusCode)
                {
                    var uri = new Uri(_client.BaseAddress, endpoint);
                    var message = $"Error at {uri.ToString()}";
                    foreach (var order in document.OrderHeaders)
                    {
                        message += $"\nCould not post {order.PO}";
                    }
                    throw new Exception(message);
                }

                var result = await response.Content.ReadAsStringAsync();
                return AllfleXML.FlexOrder.Parser.Import(XDocument.Parse(result));
            }
            catch (Exception e)
            {
                var message = $"There was an issue saving the order to the Allflex Order API:\n\n";
                message += $"{e.Message} - {e.InnerException?.Message}";
                Console.WriteLine(message);
                throw;
            }
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}
