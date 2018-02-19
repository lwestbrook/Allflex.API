using System;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Xml.Linq;
using AllfleXML.FlexOrderStatus;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Allflex.API.Client
{
    public class AllflexOrderApi
    {

        private string _apiKey = "";
        private string _apiUrl = "";
        private string _orderPath = "";

        /// <summary>
        /// Initializes a new instance of the <see cref="AllflexOrderApi"/> class.
        /// </summary>
        /// <param name="apiKey">The API key.</param>
        /// <param name="apiUrl">The API URL.</param>
        /// <param name="orderPath">The order path.</param>
        public AllflexOrderApi(string apiKey, string apiUrl, string orderPath = "")
        {
            _apiKey = apiKey;
            _apiUrl = apiUrl;
            _orderPath = orderPath;
        }
        

        /// <summary>
        /// Gets the status string.
        /// </summary>
        /// <param name="status">The status.</param>
        /// <returns></returns>
        public string GetStatusString(OrderStatusEnum status)
        {
            return Enum.GetName(typeof(OrderStatusEnum), status);
        }

        public OrderStatusEnum GetStatusEnum(string status)
        {
            return (OrderStatusEnum)Enum.Parse(typeof(OrderStatusEnum), status);
        }

        /// <summary>
        /// Sends the status.
        /// </summary>
        /// <param name="status">The status.</param>
        /// <returns></returns>
        public bool PostStatus(AllfleXML.FlexOrderStatus.OrderStatus status)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    client.BaseAddress = new Uri(_apiUrl);
                    client.DefaultRequestHeaders.Accept.Clear();

                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + _apiKey);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));

                    var putMessage = new StringContent(status.Export().ToString(), Encoding.UTF8, "application/xml");

                    var response = client.PostAsync(_apiUrl + $"/api/orders/status/{status.WSOrderId}", putMessage).Result;

                    if (!response.IsSuccessStatusCode)
                    {
                        return false;
                    }
                }
                catch (Exception e)
                {
                    var message = $"There was an issue with sending the status for the API for order {status.OrderId}, with API id {status.WSOrderId}:\n\n";
                    message += $"{e.Message} - {e.InnerException.Message}";
                    Console.WriteLine(message);
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Gets the order status.
        /// </summary>
        /// <param name="wsOrderId">The ws order identifier.</param>
        /// <returns></returns>
        public OrderStatus GetOrderStatus(string wsOrderId)
        {
            var orderStatus = new OrderStatus();


            using (var client = new HttpClient())
            {
                try
                {
                    client.BaseAddress = new Uri(_apiUrl);
                    client.DefaultRequestHeaders.Accept.Clear();

                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + _apiKey);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));


                    var response = client.GetAsync(_apiUrl + $"/api/orders/status/{wsOrderId}").Result;

                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Bad response: {response.StatusCode} - {response.ReasonPhrase}");
                        return null;
                    }

                    //var orderStatus = new AllfleXML.FlexOrderStatus.OrderStatus();
                    orderStatus = AllfleXML.FlexOrderStatus.Parser.Import(XDocument.Parse(response.Content.ReadAsStringAsync().Result));                    
                }
                catch (Exception e)
                {
                    var message = $"There was an issue with pulling the order status for {wsOrderId} from the API for processing:\n\n";
                    message += $"{e.Message} - {e.InnerException?.Message}";
                    Console.WriteLine(message);
                }
            }

            return orderStatus;
        }


        /// <summary>
        /// Retrieves the orders from API.
        /// </summary>
        /// <param name="saveFile">if set to <c>true</c> [save file].</param>
        /// <returns></returns>
        public AllfleXML.FlexOrder.Document RetrieveOrdersFromAPI(bool saveFile)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    client.BaseAddress = new Uri(_apiUrl);
                    client.DefaultRequestHeaders.Accept.Clear();

                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + _apiKey);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));


                    var response = client.GetAsync(_apiUrl + "/api/orders/process/").Result;

                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Bad response: {response.StatusCode} - {response.ReasonPhrase}");
                        return null;
                    }

                    var orders = new AllfleXML.FlexOrder.Document();
                    orders = AllfleXML.FlexOrder.Parser.Import(XDocument.Parse(response.Content.ReadAsStringAsync().Result));
                    
                    foreach (var order in orders.OrderHeaders)
                    {
                        foreach (var job in order.OrderLineHeaders)
                        {
                            if(job.UserDefinedFields?.Fields.Count == 0)
                                job.UserDefinedFields = null;
                        }

                        if (saveFile && !string.IsNullOrWhiteSpace(_orderPath))
                        {
                            AllfleXML.FlexOrder.Parser.Save(order,
                                _orderPath + $"\\API_Pull_{order.CustomerNumber}_{order.PO}_{DateTime.Now:yyyyMMddHHmmssfff}.xml");
                        }
                    }

                    return orders;
                }
                catch (Exception e)
                {
                    var message = $"There was an issue with pulling orders from the API for processing:\n\n";
                    message += $"{e.Message} - {e.InnerException.Message}";
                    Console.WriteLine(message);
                    return null;
                }
            }
        }

        /// <summary>
        /// Posts the order to flex service asynchronous.
        /// </summary>
        /// <param name="order">The order.</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public AllfleXML.FlexOrder.Document PostOrder(AllfleXML.FlexOrder.OrderHeader order)
        {          

            using (var client = new HttpClient())
            {
                var secretToken = _apiKey;

                var orderReceived = new AllfleXML.FlexOrder.Document();

                try
                {

                    client.BaseAddress = new Uri(_apiUrl);
                    client.DefaultRequestHeaders.Accept.Clear();

                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + _apiKey);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));

                    var document = new AllfleXML.FlexOrder.Document
                    {
                        OrderHeaders = new List<AllfleXML.FlexOrder.OrderHeader>()
                    };

                    document.OrderHeaders.Add(order);


                    var putMessage = new StringContent(AllfleXML.FlexOrder.Parser.Export(order).ToString(), Encoding.UTF8, "application/xml");

                    var response = client.PostAsync("api/orders", putMessage).Result;
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception($"Could not post {order.PO} to {_apiUrl}/api/orders");
                    }


                    orderReceived = AllfleXML.FlexOrder.Parser.Import(XDocument.Parse(response.Content.ReadAsStringAsync().Result));
                    return orderReceived;
                }
                catch (Exception e)
                {
                    var message = $"There was an issue saving the order {order.PO} to the Allflex Order API:\n\n";
                    message += $"{e.Message} - {e.InnerException?.Message}";
                    Console.WriteLine(message);
                    return null;
                }
            }
        }
    }
}
