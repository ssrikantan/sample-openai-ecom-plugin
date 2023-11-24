using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.OpenApi.Models;
using System.Globalization;
using Newtonsoft.Json;
using Entity;

namespace Contoso.Functions
{
    public class OrderProduct
    {
        private readonly ILogger _logger;
        private static List<Product> allProducts;

        public OrderProduct(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<SearchProductsByCategory>();
            allProducts = JsonConvert.DeserializeObject<List<Product>>(System.IO.File.ReadAllText("./data/products.json"));

        }

        [OpenApiOperation(operationId: "OrderProduct", tags: new[] { "ExecuteFunction" }, Description = "Order a product based on id and quantity.")]
        [OpenApiParameter(name: "id", Description = "product id", Required = true, In = ParameterLocation.Query)]
        [OpenApiParameter(name: "quantity", Description = "quantity", Required = true, In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "Returns the order information")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(string), Description = "Returns the error of the input.")]
        [Function("OrderProduct")]
        public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
        {
            if (req is null)
            {
                throw new System.ArgumentNullException(nameof(req));
            }

            string product_id = req.Query["id"] ?? "";
            string quantity = req.Query["quantity"] ?? "";
            int _product_id = int.TryParse(product_id, out int Try_product_id) ? Try_product_id : 0;
            int _quantity = int.TryParse(quantity, out int Try_quantity) ? Try_quantity : 0;

            if (_product_id == 0 || _quantity == 0)
            {
                HttpResponseData response = req.CreateResponse(HttpStatusCode.BadRequest);
                response.Headers.Add("Content-Type", "application/json");
                response.WriteString("Please pass valid product id and quantity on the query string or in the request body");
                return response;
            }
            else
            {
                HttpResponseData response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json");
                var _product = (allProducts.Where(c => c.id == _product_id).ToList())[0];
                var order = "{  \"orderid\": " + Guid.NewGuid().ToString() + ",\"productid\": " + _product.id + ", \"name\": \"" + _product.ProductName + "\", \"quantity\": " + _quantity + ", \"price\": " + _product.Price + ", \"total\": " + _product.Price * _quantity + " }";

                // serialize _products to string
                // var _productsAsString = JsonConvert.SerializeObject(_products);
                _logger.LogInformation($"Add function processed a request. Order: {order}");

                //response.Headers.Add("Content-Type", "application/text");
                response.WriteString(order.ToString(CultureInfo.CurrentCulture));

                return response;
            }
        }
    }
}
