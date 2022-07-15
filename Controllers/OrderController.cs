using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hometech.Models;
using Hometech.Models.OrderViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

namespace Hometech.Controllers
{
    public class OrderController : Controller
    {
        private readonly UserManager<user_info> _userManager;
        private const string ConnectionString = "Server=localhost; Port=3306; Database=hometech; Uid=root; Pwd=kenowi36;";
        public OrderController(UserManager<user_info> userManager)
        {
            _userManager = userManager;
        }
        [HttpGet]
        [Authorize(Roles = "Клиент")]
        public async Task<IActionResult> Products()
        {
            List<ProductInfo> stockProducts=new();
            await using var con = new MySqlConnection(ConnectionString);
            await con.OpenAsync();
            await using var cmd = con.CreateCommand();
            cmd.CommandText = $"select id_client from hometech.client where client_login='{User.Identity.Name}'";
            var id = (int)cmd.ExecuteScalar(); 
            cmd.CommandText =$"select category.title category, manufacturer.title manufacturer, vendor_code, price,amount, img from hometech.stock_products right join hometech.cart using(vendor_code) left join hometech.manufacturer using (id_manufacturer) left join hometech.category using (id_category) WHERE id_client='{id}'";
            MySqlDataReader reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var category = reader.GetString(reader.GetOrdinal("category"));
                var manufacturer = reader.GetString(reader.GetOrdinal("manufacturer"));
                var vendorCode = reader.GetString(reader.GetOrdinal("vendor_code"));
                var price = reader.GetDecimal(reader.GetOrdinal("price"));
                var amount = reader.GetInt32(reader.GetOrdinal("amount"));
                var img= reader.GetString(reader.GetOrdinal("img"));
                var product=new ProductInfo{
                    category=category,
                    manufacturer=manufacturer, 
                    vendor_code = vendorCode,
                    price = price,
                    amount = amount,
                    img = img
                }; 
                stockProducts.Add(product);
            }
            await reader.CloseAsync();
            var model = new ProductsViewModel{Prods = stockProducts};
            return View(model);
        }
        
        [HttpPost]
        [Authorize(Roles = "Клиент")]
        public async Task<IActionResult> Products(ProductsViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
            await using var con =
                new MySqlConnection(ConnectionString);
            await using var cmd = con.CreateCommand();
            await con.OpenAsync();
            cmd.CommandText = $"select id_client from hometech.client where client_login='{user.login}'";
            var id = (int)cmd.ExecuteScalar();
            cmd.CommandText = $"select id_order from hometech.order where id_client='{id}' and status='Не оформлен'";
            var idOrder = (int)cmd.ExecuteScalar();
            foreach (var p in model.Prods)
            {
                cmd.CommandText = $"update hometech.order_products set number='{p.number}' where vendor_code='{p.vendor_code}' and id_order='{idOrder}'";
                await cmd.ExecuteNonQueryAsync();
            }
            return RedirectToAction("Checkout");
        }
        [HttpGet]
        [Authorize(Roles = "Клиент")]
        public async Task<IActionResult> Checkout()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
            await using var con =
                new MySqlConnection(ConnectionString);
            await using var cmd = con.CreateCommand();
            await con.OpenAsync();
            cmd.CommandText = $"select city,street,home_number from hometech.client where client_login='{user.login}'";
            var reader = await cmd.ExecuteReaderAsync();
            await reader.ReadAsync();
            var city = reader.GetString(reader.GetOrdinal("city"));
            var street = reader.GetString(reader.GetOrdinal("street"));
            var homeNumber = reader.GetInt32(reader.GetOrdinal("home_number"));
            var model = new CheckoutViewModel
            {
                Name = user.name,
                Surname = user.surname,
                Email = user.email,
                PhoneNumber = user.phone_number,
                City = city,
                Street = street,
                HomeNumber = homeNumber,
            };
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            await using var con =
                new MySqlConnection(ConnectionString);
            await using var cmd = con.CreateCommand();
            await con.OpenAsync();
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
            var email = user.email;
            if (model.Email != email)
            {
                var setEmailResult = await _userManager.SetEmailAsync(user, model.Email);
                if (!setEmailResult.Succeeded)
                {
                    throw new ApplicationException($"Unexpected error occurred setting email for user with ID '{user.id_user}'.");
                }
            }
            var phoneNumber = user.phone_number;
            if (model.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, model.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    throw new ApplicationException($"Unexpected error occurred setting phone number for user with ID '{user.id_user}'.");
                }
            } 
            var name = user.name;
            var surname = user.surname;
            if (model.Name != name || model.Surname != surname)
            {
                cmd.CommandText =
                    $"update hometech.user_info set name='{model.Name}', surname='{model.Surname}' where id_user={user.id_user}";
                await cmd.ExecuteNonQueryAsync();
            }
            cmd.CommandText =
                $"select city,street,home_number from hometech.client where client_login='{user.login}'";
            var reader = await cmd.ExecuteReaderAsync();
            await reader.ReadAsync();
            var city = reader.GetString(reader.GetOrdinal("city"));
            var street = reader.GetString(reader.GetOrdinal("street"));
            var homeNumber = reader.GetInt32(reader.GetOrdinal("home_number"));
            reader.Close();
            if (model.City != city || model.Street != street ||model.HomeNumber != homeNumber)
            {
                cmd.CommandText =
                    $"update hometech.client set city='{model.City}', street='{model.Street}', home_number={model.HomeNumber} where client_login='{user.login}'";
                await cmd.ExecuteNonQueryAsync();
            }
            cmd.CommandText = $"select id_client from hometech.client where client_login='{user.login}'";
            var id = (int)cmd.ExecuteScalar();
            cmd.CommandText = $"select id_order from hometech.order where id_client='{id}' and status='Не оформлен'";
            var idOrder = (int)cmd.ExecuteScalar();
            cmd.CommandText = $"select number, vendor_code from hometech.order_products where id_order='{idOrder}'";
            reader = await cmd.ExecuteReaderAsync();
            var productsNumber = new Dictionary<string, int>();
            while (await reader.ReadAsync())
            {
                var number = reader.GetInt32(reader.GetOrdinal("number"));
                var vendorCode = reader.GetString(reader.GetOrdinal("vendor_code"));
                productsNumber.Add(vendorCode,number);
            }
            await reader.CloseAsync();
            foreach (var (key, value) in productsNumber)
            {
                cmd.CommandText = $"update hometech.stock_products set amount=amount-{value} where vendor_code='{key}'";
                await cmd.ExecuteNonQueryAsync();
            }
            cmd.CommandText = $"update hometech.order set status='Оформлен', order_datetime='{DateTime.Now:yyyy-MM-dd HH:mm:ss}' where id_order='{idOrder}'"; 
            await cmd.ExecuteNonQueryAsync();
            cmd.CommandText = $"delete from hometech.cart where id_client='{id}'";
            await cmd.ExecuteNonQueryAsync();
            return RedirectToAction("Index","Home");
        }
    }
}