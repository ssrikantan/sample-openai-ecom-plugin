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
    public class SearchProductsByCategoryAndPrice
    {
        private readonly ILogger _logger;
        private static List<Product> allProducts;

        public SearchProductsByCategoryAndPrice(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<SearchProductsByCategory>();
            allProducts = JsonConvert.DeserializeObject<List<Product>>(System.IO.File.ReadAllText("./data/products.json"));

        }

        [OpenApiOperation(operationId: "SearchProductsByCategoryAndPrice", tags: new[] { "ExecuteFunction" }, Description = "Search for products based on their category and price.")]
        [OpenApiParameter(name: "category", Description = "category name", Required = true, In = ParameterLocation.Query)]
        [OpenApiParameter(name: "price", Description = "price", Required = true, In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "Returns the list of Product Names and Product Description")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(string), Description = "Returns the error of the input.")]
        [Function("SearchProductsByCategoryAndPrice")]
        public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
        {
            if (req is null)
            {
                throw new System.ArgumentNullException(nameof(req));
            }

            string category = req.Query["category"] ?? "";
            string price = req.Query["price"] ?? "";
            decimal _price = decimal.TryParse(price, out decimal Tryprice) ? Tryprice : 0;

            if (string.IsNullOrEmpty(category))
            {
                HttpResponseData response = req.CreateResponse(HttpStatusCode.BadRequest);
                response.Headers.Add("Content-Type", "application/json");
                response.WriteString("Please pass valid category name on the query string or in the request body");
                return response;
            }
            else
            {
                HttpResponseData response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "text/plain");
                var _products = allProducts.Where(c => c.Category.ToLower().Contains(category.ToLower()) & c.Price <= _price).ToList();
                  // serialize _products to string
                var _productsAsString = JsonConvert.SerializeObject(_products);
                _logger.LogInformation($"Add function processed a request. Products: {_productsAsString}");

                //response.Headers.Add("Content-Type", "application/text");
                response.WriteString(_productsAsString.ToString(CultureInfo.CurrentCulture));

                return response;
            }
        }
    }
}
