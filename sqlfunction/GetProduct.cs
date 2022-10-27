using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace sqlfunction
{
    public static class GetProduct
    {
        //functie care rezulta lista tuturor itemilor
        [FunctionName("GetProducts")]
        public static async Task<IActionResult> RunProducts(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            List<Product> _product_lst = new List<Product>();
            string _statement = "SELECT ProductID,ProductName,Quantity FROM Products";
            SqlConnection _connection = GetConnection();

            _connection.Open();

            SqlCommand _sqlcommand = new SqlCommand(_statement, _connection);

            using (SqlDataReader _reader = _sqlcommand.ExecuteReader())
            {
                while (_reader.Read())
                {
                    Product _product = new Product()
                    {
                        ProductID = _reader.GetInt32(0),
                        ProductName = _reader.GetString(1),
                        Quantity = _reader.GetInt32(2),
                    };
                    _product_lst.Add(_product);
                }
            }
            _connection.Close();

            return new OkObjectResult(_product_lst);
        }

        private static SqlConnection GetConnection()
        {
            string connectionString = Environment.GetEnvironmentVariable("SQLAZURECONNSTR_SQLConnectionString");
            return new SqlConnection(connectionString);
        }

        [FunctionName("GetProduct")]
        public static async Task<IActionResult> RunProduct(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            int productId = int.Parse(req.Query["id"]);
            string _statement = String.Format("SELECT ProductID,ProductName,Quantity FROM Products WHERE ProductId={0}", productId);
            SqlConnection _connection = GetConnection();

            _connection.Open();

            SqlCommand _sqlcommand = new SqlCommand(_statement, _connection);
            Product product = new Product();

            try
            {
                using (SqlDataReader _reader = _sqlcommand.ExecuteReader())
                {
                    _reader.Read();
                    product.ProductID = _reader.GetInt32(0);
                    product.ProductName = _reader.GetString(1);
                    product.Quantity = _reader.GetInt32(2);
                    var response = product;
                    _connection.Close();
                    return new OkObjectResult(response);
                }
            }
            catch (Exception e)
            {
                var response = "No records found";
                _connection.Close();
                return new OkObjectResult(response);
            }
        }
    }
}
