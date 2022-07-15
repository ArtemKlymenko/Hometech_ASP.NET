using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

namespace Hometech.Controllers
{
    [Route("[action]")]
    public class ProductsController : Controller
    {
        private const string ConnectionString = "Server=localhost; Port=3306; Database=hometech; Uid=root; Pwd=kenowi36;";
        public async Task<IActionResult> WashMachine()
        {
            List<ProductInfo> washMachines=new();
            await using var con = new MySqlConnection(ConnectionString);
            await con.OpenAsync();
            await using var cmd = con.CreateCommand();
            cmd.CommandText = "select hc.title category, hm.title manufacturer, vendor_code, price, amount, img from hometech.stock_products left join hometech.manufacturer hm using (id_manufacturer) left join hometech.category hc using (id_category) WHERE hc.title='Стиральная машина'";
            var reader =await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var category = reader.GetString(reader.GetOrdinal("category"));
                var manufacturer = reader.GetString(reader.GetOrdinal("manufacturer"));
                var vendorCode = reader.GetString(reader.GetOrdinal("vendor_code"));
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
                    in_cart = false,
                    amount = amount,
                    number = 0,
                    img = img
                };
                washMachines.Add(product);
            }
            await reader.CloseAsync();
            foreach (var p in washMachines)
            {
                if (!User.IsInRole("Клиент")) continue;
                cmd.CommandText =
                    $"select count(*) from hometech.cart where vendor_code='{p.vendor_code}' and id_client=(select id_client from hometech.client where client_login='{User.Identity.Name}')";
                var inCart = (long)cmd.ExecuteScalar() == 1;
                p.in_cart = inCart;
            }
            return View(washMachines);
        }
        
        public async Task<IActionResult> Microwave()
        {
            List<ProductInfo> microwaves=new();
            await using var con = new MySqlConnection(ConnectionString);
            await con.OpenAsync();
            await using var cmd = con.CreateCommand();
            cmd.CommandText = "select hc.title category, hm.title manufacturer, vendor_code, price, amount, img from hometech.stock_products left join hometech.manufacturer hm using (id_manufacturer) left join hometech.category hc using (id_category) WHERE hc.title='Микроволновая печь'";
            var reader =await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var category = reader.GetString(reader.GetOrdinal("category"));
                var manufacturer = reader.GetString(reader.GetOrdinal("manufacturer"));
                var vendorCode = reader.GetString(reader.GetOrdinal("vendor_code"));
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
                    in_cart = false,
                    amount = amount,
                    number = 0,
                    img = img
                };
                microwaves.Add(product);
            }
            await reader.CloseAsync();
            foreach (var p in microwaves)
            {
                if (!User.IsInRole("Клиент")) continue;
                cmd.CommandText =
                    $"select count(*) from hometech.cart where vendor_code='{p.vendor_code}' and id_client=(select id_client from hometech.client where client_login='{User.Identity.Name}')";
                var inCart = (long)cmd.ExecuteScalar() == 1;
                p.in_cart = inCart;
            }
            return View(microwaves);
        }
        public async Task<IActionResult> Iron()
        {
            List<ProductInfo> irons=new();
            await using var con = new MySqlConnection(ConnectionString);
            await con.OpenAsync();
            await using var cmd = con.CreateCommand();
            cmd.CommandText = "select hc.title category, hm.title manufacturer, vendor_code, price, amount, img from hometech.stock_products left join hometech.manufacturer hm using (id_manufacturer) left join hometech.category hc using (id_category) WHERE hc.title='Утюг'";
            var reader = await cmd.ExecuteReaderAsync();
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
                    category=category,
                    manufacturer=manufacturer, 
                    vendor_code = vendorCode,
                    price = price,
                    is_available = isAvailable,
                    in_cart = false,
                    amount = amount,
                    number = 0,
                    img = img
                };
                irons.Add(product);
            }
            await reader.CloseAsync();
            foreach (var p in irons)
            {
                if (!User.IsInRole("Клиент")) continue;
                cmd.CommandText =
                    $"select count(*) from hometech.cart where vendor_code='{p.vendor_code}' and id_client=(select id_client from hometech.client where client_login='{User.Identity.Name}')";
                var inCart = (long)cmd.ExecuteScalar() == 1;
                p.in_cart = inCart;
            }
            return View(irons);
        }
        public async Task<IActionResult> VacuumCleaner()
        {
            List<ProductInfo> vacuumCleaners=new();
            await using var con = new MySqlConnection(ConnectionString);
            await con.OpenAsync();
            await using var cmd = con.CreateCommand();
            cmd.CommandText = "select hc.title category, hm.title manufacturer, vendor_code, price, amount, img from hometech.stock_products left join hometech.manufacturer hm using (id_manufacturer) left join hometech.category hc using (id_category) WHERE hc.title='Пылесос'";
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
                    category=category,
                    manufacturer=manufacturer, 
                    vendor_code = vendorCode,
                    price = price,
                    is_available = isAvailable,
                    in_cart = false,
                    amount = amount,
                    number = 0,
                    img = img
                };
                vacuumCleaners.Add(product);
            }
            await reader.CloseAsync();
            foreach (var p in vacuumCleaners)
            {
                if (!User.IsInRole("Клиент")) continue;
                cmd.CommandText =
                    $"select count(*) from hometech.cart where vendor_code='{p.vendor_code}' and id_client=(select id_client from hometech.client where client_login='{User.Identity.Name}')";
                var inCart = (long)cmd.ExecuteScalar() == 1;
                p.in_cart = inCart;
            }
            return View(vacuumCleaners);
        }
        public async Task<IActionResult> Conditioner()
        {
            List<ProductInfo> conditioners=new();
            await using var con = new MySqlConnection(ConnectionString);
            await con.OpenAsync();
            await using var cmd = con.CreateCommand();
            cmd.CommandText = "select hc.title category, hm.title manufacturer, vendor_code, price, amount, img from hometech.stock_products left join hometech.manufacturer hm using (id_manufacturer) left join hometech.category hc using (id_category) WHERE hc.title='Кондиционер'";
            var reader =await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var category = reader.GetString(reader.GetOrdinal("category"));
                var manufacturer = reader.GetString(reader.GetOrdinal("manufacturer"));
                var vendorCode = reader.GetString(reader.GetOrdinal("vendor_code"));
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
                    in_cart = false,
                    amount = amount,
                    number = 0,
                    img = img
                };
                conditioners.Add(product);
            }
            await reader.CloseAsync();
            foreach (var p in conditioners)
            {
                if (!User.IsInRole("Клиент")) continue;
                cmd.CommandText =
                    $"select count(*) from hometech.cart where vendor_code='{p.vendor_code}' and id_client=(select id_client from hometech.client where client_login='{User.Identity.Name}')";
                var inCart = (long)cmd.ExecuteScalar() == 1;
                p.in_cart = inCart;
            }
            return View(conditioners);
        }
        public async Task<IActionResult> Tv()
        {
            List<ProductInfo> tvs=new();
            await using var con = new MySqlConnection(ConnectionString);
            await con.OpenAsync();
            await using var cmd = con.CreateCommand();
            cmd.CommandText = "select hc.title category, hm.title manufacturer, vendor_code, price, amount, img from hometech.stock_products left join hometech.manufacturer hm using (id_manufacturer) left join hometech.category hc using (id_category) WHERE hc.title='Телевизор'";
            var reader =await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var category = reader.GetString(reader.GetOrdinal("category"));
                var manufacturer = reader.GetString(reader.GetOrdinal("manufacturer"));
                var vendorCode = reader.GetString(reader.GetOrdinal("vendor_code"));
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
                    in_cart = false,
                    amount = amount,
                    number = 0,
                    img = img
                };
                tvs.Add(product);
            }
            await reader.CloseAsync();
            foreach (var p in tvs)
            {
                if (!User.IsInRole("Клиент")) continue;
                cmd.CommandText =
                    $"select count(*) from hometech.cart where vendor_code='{p.vendor_code}' and id_client=(select id_client from hometech.client where client_login='{User.Identity.Name}')";
                var inCart = (long)cmd.ExecuteScalar() == 1;
                p.in_cart = inCart;
            }
            return View(tvs);
        }
        public async Task<IActionResult> Fridge()
        {
            List<ProductInfo> fridges=new();
            await using var con = new MySqlConnection(ConnectionString);
            await con.OpenAsync();
            await using var cmd = con.CreateCommand();
            cmd.CommandText = "select hc.title category, hm.title manufacturer, vendor_code, price, amount, img from hometech.stock_products left join hometech.manufacturer hm using (id_manufacturer) left join hometech.category hc using (id_category) WHERE hc.title='Холодильник'";
            var reader =await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var category = reader.GetString(reader.GetOrdinal("category"));
                var manufacturer = reader.GetString(reader.GetOrdinal("manufacturer"));
                var vendorCode = reader.GetString(reader.GetOrdinal("vendor_code"));
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
                    in_cart = false,
                    amount = amount,
                    number = 0,
                    img = img
                };
                fridges.Add(product);
            }
            await reader.CloseAsync();
            foreach (var p in fridges)
            {
                if (!User.IsInRole("Клиент")) continue;
                cmd.CommandText =
                    $"select count(*) from hometech.cart where vendor_code='{p.vendor_code}' and id_client=(select id_client from hometech.client where client_login='{User.Identity.Name}')";
                var inCart = (long)cmd.ExecuteScalar() == 1;
                p.in_cart = inCart;
            }
            return View(fridges);
        }
    }
}