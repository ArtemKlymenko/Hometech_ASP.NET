using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Hometech.Models;
using Hometech.Models.DeliveryViewModels;
using Hometech.Models.HomeViewModels;
using Microsoft.AspNetCore.Authorization;
using MySqlConnector;

namespace Hometech.Controllers
{
    public class ProductInfo
    {
        public string category { get; set; }
        public string manufacturer { get; set; }
        public string vendor_code { get; set; }
        public decimal price { get; set; }
        public bool is_available { get; set; }
        public bool in_cart { get; set; }
        public int amount { get; set; }
        public int number { get; set; }
        public string img { get; set; }
    }
    public class HomeController : Controller
    {
        private const string ConnectionString = "Server=localhost; Port=3306; Database=hometech; Uid=root; Pwd=kenowi36;";

        public async Task<IActionResult> AllProducts()
        {
            List<ProductInfo> stockProducts=new();
            await using var con = new MySqlConnection(ConnectionString);
            await con.OpenAsync();
            await using var cmd = con.CreateCommand();
            cmd.CommandText = "select category.title category, manufacturer.title manufacturer, vendor_code, price, amount, img from hometech.stock_products left join hometech.manufacturer using (id_manufacturer)left join hometech.category using (id_category)";
            var reader =await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var category = reader.GetString(reader.GetOrdinal("category"));
                var manufacturer = reader.GetString(reader.GetOrdinal("manufacturer"));
                var vendorCode = reader.GetString(reader.GetOrdinal("vendor_code"));
                var price = reader.GetDecimal(reader.GetOrdinal("price"));
                var amount = reader.GetInt32(reader.GetOrdinal("amount"));
                var img = reader.GetString(reader.GetOrdinal("img"));
                var isAvailable = amount > 0;
                var product=new ProductInfo{
                    category = category,
                    manufacturer = manufacturer, 
                    vendor_code = vendorCode,
                    price = price,
                    is_available = isAvailable,
                    in_cart = false,
                    amount = amount,
                    number = 0,
                    img = img
                };
                stockProducts.Add(product);
            }
            await reader.CloseAsync();
            foreach (var p in stockProducts)
            {
                if (!User.IsInRole("Клиент")) continue;
                cmd.CommandText =
                    $"select count(*) from hometech.cart where vendor_code='{p.vendor_code}' and id_client=(select id_client from hometech.client where client_login='{User.Identity.Name}')";
                var inCart = (long)cmd.ExecuteScalar() == 1;
                p.in_cart = inCart;
            }
            return View(stockProducts);
        }
        [Authorize(Roles="Клиент")]
        public async Task<IActionResult> Cart()
        {
            List<ProductInfo> cart=new();
            await using var con = new MySqlConnection(ConnectionString);
            await con.OpenAsync();
            await using var cmd = con.CreateCommand();
            List<string> vendorCodes = new();
            cmd.CommandText = $"select id_client from hometech.client where client_login='{User.Identity.Name}'";
            var id = (int)cmd.ExecuteScalar(); 
            cmd.CommandText = $"select vendor_code from hometech.cart c left join hometech.stock_products using (vendor_code) where amount=0 and id_client='{id}'";
            var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                vendorCodes.Add(reader.GetString(reader.GetOrdinal("vendor_code")));
            }
            await reader.CloseAsync();
            if (vendorCodes.Count > 0)
            {
                cmd.CommandText =
                    $"delete from hometech.cart c where (select amount from hometech.stock_products sp where sp.vendor_code=c.vendor_code)=0 and id_client='{id}'";
                await cmd.ExecuteNonQueryAsync();
            }
            cmd.CommandText = $"select count(*) from hometech.cart where id_client='{id}'";
            var count = (long)cmd.ExecuteScalar();
            ViewBag.EmptyCart = count == 0;
            if (ViewBag.EmptyCart) return View();
            cmd.CommandText =$"select category.title category, manufacturer.title manufacturer, vendor_code, price, amount, img from hometech.stock_products RIGHT JOIN hometech.cart using(vendor_code) left join hometech.manufacturer using (id_manufacturer) left join hometech.category using (id_category) WHERE id_client='{id}'";
            reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var category= reader.GetString(reader.GetOrdinal("category"));
                var manufacturer= reader.GetString(reader.GetOrdinal("manufacturer"));
                var vendorCode= reader.GetString(reader.GetOrdinal("vendor_code"));
                var price = reader.GetDecimal(reader.GetOrdinal("price"));
                var amount = reader.GetInt32(reader.GetOrdinal("amount"));
                var img= reader.GetString(reader.GetOrdinal("img"));
                var isAvailable = amount > 0;
                var product=new ProductInfo{
                    category=category,
                    manufacturer=manufacturer, 
                    vendor_code = vendorCode,
                    price = price,
                    is_available = isAvailable,
                    in_cart = true,
                    amount = amount,
                    number = 1,
                    img = img
                };
                cart.Add(product);
            }
            var model = new CartViewModel
            {
                products=cart
            };
            return View(model);
        }

        public async Task<IActionResult> ClearCart()
        {
            await using var con = new MySqlConnection(ConnectionString);
            await con.OpenAsync();
            await using var cmd = con.CreateCommand();
            cmd.CommandText = $"select id_client from hometech.client where client_login='{User.Identity.Name}'";
            var id = (int)cmd.ExecuteScalar(); 
            cmd.CommandText = $"delete from hometech.cart where id_client='{id}'";
            await cmd.ExecuteNonQueryAsync();
            return RedirectToAction("Cart");
        }
        public async Task<IActionResult> AddItem(string vendorCode, string a, string c)
        {
            await using var con = new MySqlConnection(ConnectionString);
            await con.OpenAsync();
            await using var cmd = con.CreateCommand();
            cmd.CommandText = $"select id_client from hometech.client where client_login='{User.Identity.Name}'";
            var id = (int)cmd.ExecuteScalar(); 
            cmd.CommandText = $"insert into hometech.cart (id_client, vendor_code) values ('{id}','{vendorCode}')";
            await cmd.ExecuteNonQueryAsync();
            return RedirectToAction(a,c,new { area = "" });
        }
        public async Task<IActionResult> DeleteItem(string vendorCode)
        {
            await using var con = new MySqlConnection(ConnectionString);
            await con.OpenAsync();
            await using var cmd = con.CreateCommand();
            cmd.CommandText = $"select id_client from hometech.client where client_login='{User.Identity.Name}'";
            var id = (int)cmd.ExecuteScalar(); 
            cmd.CommandText = $"delete from hometech.cart where id_client='{id}' and vendor_code='{vendorCode}'";
            await cmd.ExecuteNonQueryAsync();
            return RedirectToAction("Cart");
        }
        [Authorize(Roles = "Администратор")]
        public async Task<IActionResult> Orders()
        {
            List<Order> orders = new();
            await using var con = new MySqlConnection(ConnectionString);
            await con.OpenAsync();
            await using var cmd = con.CreateCommand();
            cmd.CommandText = "select count(*) from hometech.order where status!='Не оформлен'";
            var count = (long)cmd.ExecuteScalar();
            ViewBag.NoOrders = count==0;
            if (ViewBag.NoOrders) return View();
            cmd.CommandText = "select * from hometech.order where status!='Не оформлен'";
            var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var idOrder=reader.GetInt32(reader.GetOrdinal("id_order"));
                var idClient=reader.GetInt32(reader.GetOrdinal("id_client"));
                var status = reader.GetString(reader.GetOrdinal("status"));
                var totalPrice = reader.GetDecimal(reader.GetOrdinal("total_price"));
                DateTime? orderDatetime = reader.IsDBNull(reader.GetOrdinal("order_datetime"))? null:reader.GetDateTime(reader.GetOrdinal("order_datetime"));
                orders.Add(new Order
                {
                    id_client = idClient,
                    id_order = idOrder,
                    status = status,
                    total_price = totalPrice,
                    order_datetime = orderDatetime
                });
            }
            await reader.CloseAsync();
            return View(orders);
        }
        [Authorize(Roles = "Администратор")]
        public async Task<IActionResult> AcceptOrder(int idOrder)
        {
            await using var con = new MySqlConnection(ConnectionString);
            await con.OpenAsync();
            await using var cmd = con.CreateCommand();
            cmd.CommandText = $"update hometech.order set status='Принят' where id_order='{idOrder}'";
            await cmd.ExecuteNonQueryAsync();
            return RedirectToAction("Orders", "Home");
        }
        [Authorize(Roles = "Администратор")]
        public async Task<IActionResult> CancelOrder(int idOrder)
        {
            var prods = new List<ProductInfo>();
            await using var con = new MySqlConnection(ConnectionString);
            await con.OpenAsync();
            await using var cmd = con.CreateCommand();
            cmd.CommandText = $"update hometech.order set status='Отклонён' where id_order='{idOrder}'";
            await cmd.ExecuteNonQueryAsync();
            cmd.CommandText = $"select vendor_code, number, price from hometech.order_products left join hometech.stock_products using (vendor_code) where id_order='{idOrder}'";
            var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var vendorCode= reader.GetString(reader.GetOrdinal("vendor_code"));
                var number = reader.GetInt32(reader.GetOrdinal("number"));
                var price = reader.GetDecimal(reader.GetOrdinal("price"));
                prods.Add(new ProductInfo
                {
                    vendor_code = vendorCode,
                    number = number,
                    price=price
                });
            }
            await reader.CloseAsync();
            foreach (var p in prods)
            {
                cmd.CommandText = $"update hometech.stock_products set amount=amount+{p.number} where vendor_code='{p.vendor_code}'";
                await cmd.ExecuteNonQueryAsync();
            }
            return RedirectToAction("Orders");
        }
        [Authorize(Roles = "Курьер")]
        public async Task<IActionResult> CourierOrders()
        {
            await using var con = new MySqlConnection(ConnectionString);
            await con.OpenAsync();
            await using var cmd = con.CreateCommand();
            cmd.CommandText = $"select id_courier from hometech.courier where courier_login='{User.Identity.Name}'";
            var idCourier = (int)cmd.ExecuteScalar();
            cmd.CommandText =
                $"select count(*) from hometech.delivery left join hometech.order using(id_order) where id_courier='{idCourier}' and status='В пути'";
            var isInDelivery = (long)cmd.ExecuteScalar() == 1;
            ViewBag.isInDelivery = isInDelivery;
            if (!isInDelivery)
            {
                return View();
            }
            cmd.CommandText = $"select id_order from hometech.delivery left join hometech.order using(id_order) where id_courier='{idCourier}' and status='В пути'";
            var idOrder = (int)cmd.ExecuteScalar();
            cmd.CommandText = $"select id_client from hometech.order where id_order='{idOrder}'";
            var idClient = (int)cmd.ExecuteScalar();
            cmd.CommandText = $"select * from hometech.client where id_client='{idClient}'";
            var reader = await cmd.ExecuteReaderAsync();
            await reader.ReadAsync();
            var clientLogin = reader.GetString(reader.GetOrdinal("client_login"));
            var city = reader.GetString(reader.GetOrdinal("city"));
            var street = reader.GetString(reader.GetOrdinal("street"));
            var homeNumber = reader.GetInt32(reader.GetOrdinal("home_number"));
            var clientInfo = new Client
            {
                city = city, client_login = clientLogin, home_number = homeNumber, id_client = idClient, street = street
            };
            await reader.CloseAsync();
            cmd.CommandText = $"select name, surname from hometech.user_info where login='{clientLogin}'";
            reader = await cmd.ExecuteReaderAsync();
            await reader.ReadAsync();
            var name = reader.GetString(reader.GetOrdinal("name"));
            var surname = reader.GetString(reader.GetOrdinal("surname"));
            await reader.CloseAsync();
            cmd.CommandText = $"select category.title category, manufacturer.title manufacturer, vendor_code, price, number, img from hometech.stock_products right join hometech.order_products using(vendor_code) left join hometech.manufacturer using (id_manufacturer)left join hometech.category using (id_category) where id_order='{idOrder}'";
            reader = await cmd.ExecuteReaderAsync();
            List<ProductInfo> prods = new();
            while (await reader.ReadAsync())
            {
                var category = reader.GetString(reader.GetOrdinal("category"));
                var manufacturer = reader.GetString(reader.GetOrdinal("manufacturer"));
                var vendorCode = reader.GetString(reader.GetOrdinal("vendor_code"));
                var number = reader.GetInt32(reader.GetOrdinal("number"));
                var price = reader.GetDecimal(reader.GetOrdinal("price")); 
                var img= reader.GetString(reader.GetOrdinal("img"));
                prods.Add(new ProductInfo{
                    category=category,
                    manufacturer=manufacturer, 
                    vendor_code = vendorCode,
                    price = price,
                    number = number,
                    img = img
                });
            }
            await reader.CloseAsync();
            var model = new CourierOrders {Prods = prods,ClientInfo = clientInfo,Name = name,Surname = surname, IdOrder = idOrder};
            return View(model);
        }
        public IActionResult Index()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }
    }
}