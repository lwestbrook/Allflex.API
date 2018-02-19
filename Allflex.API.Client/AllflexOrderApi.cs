using System;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Xml.Linq;
using AllfleXML.FlexOrderStatus;

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
        /// Sets the API status.
        /// </summary>
        /// <param name="orderStatus">The order status.</param>
        /// <param name="status">The status.</param>
        public void SetApiStatus(AllfleXML.FlexOrderStatus.OrderStatus orderStatus, OrderStatusEnum status)
        {
            #region Send status to API

            try
            {
                var statusstring = GetStatusString(status);
                
                if (!string.IsNullOrWhiteSpace(orderStatus.WSOrderId))
                {
                    // send the status
                    var msg = "";
                    if (SendStatus(orderStatus))
                    {
                        msg = $"Status of '{orderStatus} sent to Api for {orderStatus.WSOrderId}, Allflex Order {orderStatus.OrderId}'";
                    }
                    else
                    {
                        msg = $"Error sending Api Status for Api Order {orderStatus.WSOrderId}";
                    }
                }
            }
            catch (Exception e)
            {
                var message = $"There was an issue setting the status for the API for order {orderStatus.OrderId}, with API id {orderStatus.WSOrderId}:\n\n";
                message += $"{e.Message} - {e.InnerException.Message}";
                Console.WriteLine(message);
            }
            #endregion
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
        public bool SendStatus(AllfleXML.FlexOrderStatus.OrderStatus status)
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
    }
}
