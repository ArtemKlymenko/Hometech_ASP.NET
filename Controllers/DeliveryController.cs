using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hometech.Models;
using Hometech.Models.DeliveryViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using Courier = Hometech.Models.Courier;

namespace Hometech.Controllers
{
    [Authorize(Roles = "Администратор, Курьер")]
    public class DeliveryController : Controller
    {
        private const string ConnectionString = "Server=localhost; Port=3306; Database=hometech; Uid=root; Pwd=kenowi36;";
        [Authorize(Roles = "Администратор")]
        public async Task<IActionResult> Couriers(int idOrder)
        {
            await using var con = new MySqlConnection(ConnectionString);
            await con.OpenAsync();
            await using var cmd = con.CreateCommand();
            cmd.CommandText = $"select id_admin from hometech.admin where admin_login='{User.Identity.Name}'";
            var idAdmin = (int)cmd.ExecuteScalar();
            cmd.CommandText = $"select * from hometech.courier where id_admin='{idAdmin}'";
            var reader = await cmd.ExecuteReaderAsync();
            List<Courier> couriers = new();
            while (await reader.ReadAsync())
            {
                var idCourier = reader.GetInt32(reader.GetOrdinal("id_courier"));
                var courierLogin = reader.GetString(reader.GetOrdinal("courier_login"));
                var status = reader.GetString(reader.GetOrdinal("status"));
                couriers.Add(new Courier {
                    courier_login = courierLogin,
                    id_admin = idAdmin,
                    id_courier = idCourier,
                    status = status=="Доступен" ? Status.Available : Status.Unavailable
                });
            }
            await reader.CloseAsync();
            List<CourierInfo> courierInfos=new();
            foreach (var c in couriers)
            {
                cmd.CommandText = $"select name, surname from hometech.user_info where login='{c.courier_login}'";
                reader=await cmd.ExecuteReaderAsync();
                await reader.ReadAsync();
                var name= reader.GetString(reader.GetOrdinal("name"));
                var surname= reader.GetString(reader.GetOrdinal("surname"));
                await reader.CloseAsync();
                courierInfos.Add(new CourierInfo
                {
                    id_courier = c.id_courier,
                    name = name,
                    status = c.status,
                    surname = surname
                });
            }
            ViewBag.IdOrder = idOrder;
            var model = new CourierViewModel{ Couriers = courierInfos };
            return View(model);
        }
        
        [Authorize(Roles = "Администратор")]
        public async Task<IActionResult> Assign(int idOrder,int idCourier)
        {  
            await using var con = new MySqlConnection(ConnectionString);
            await con.OpenAsync();
            await using var cmd = con.CreateCommand();
            cmd.CommandText = $"insert into hometech.delivery (id_order, id_courier) values ({idOrder},{idCourier})";
            await cmd.ExecuteNonQueryAsync();
            cmd.CommandText = $"update hometech.courier set status='Недоступен' where id_courier='{idCourier}'";
            await cmd.ExecuteNonQueryAsync();
            cmd.CommandText = $"update hometech.order set status='В пути' where id_order='{idOrder}'";
            await cmd.ExecuteNonQueryAsync();
            return RedirectToAction("Orders","Home");
        }
        
        [Authorize(Roles = "Курьер")]
        public async Task<IActionResult> ConfirmDelivery(int idOrder)
        {
            await using var con = new MySqlConnection(ConnectionString);
            await con.OpenAsync();
            await using var cmd = con.CreateCommand();
            cmd.CommandText = $"update hometech.order set status='Доставлен' where id_order='{idOrder}'";
            await cmd.ExecuteNonQueryAsync();
            cmd.CommandText =
                $"update hometech.courier set status='Доступен' where courier_login='{User.Identity.Name}'";
            await cmd.ExecuteNonQueryAsync();
            cmd.CommandText = $"update hometech.delivery set delivery_datetime='{DateTime.Now:yyyy-MM-dd HH:mm:ss}' where id_order='{idOrder}'";
            await cmd.ExecuteNonQueryAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}