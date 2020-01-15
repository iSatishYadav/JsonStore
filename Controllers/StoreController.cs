using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using JsonStore.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JsonStore.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StoreController : ControllerBase
    {
        private readonly ILogger<StoreController> _logger;
        private readonly StoreContext _context;

        public StoreController(ILogger<StoreController> logger, StoreContext storeContext)
        {
            _logger = logger;
            _context = storeContext;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(GetAllStores());
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            string jsonString = GetStore(id);
            return Ok(jsonString);
        }

        private string GetStore(int id)
        {
            using var connection = _context.Database.GetDbConnection() as SqlConnection;
            connection.Open();
            using var cmd = new SqlCommand("dbo.GetStore", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add("@Id", SqlDbType.NVarChar).Value = id;
            var jsonString = ReadJsonFromReader(cmd);
            return jsonString;
        }

        private string GetAllStores()
        {
            using var connection = _context.Database.GetDbConnection() as SqlConnection;
            connection.Open();
            using var cmd = new SqlCommand("dbo.GetAllStores", connection)
            {
                CommandType = CommandType.StoredProcedure
            };            
            var jsonString = ReadJsonFromReader(cmd);
            return jsonString;
        }

        private static string ReadJsonFromReader(SqlCommand cmd)
        {
            var jsonResult = new StringBuilder();
            using var reader = cmd.ExecuteReader();
            if (!reader.HasRows)
            {
                jsonResult.Append("[]");
            }
            else
            {
                while (reader.Read())
                {
                    jsonResult.Append(reader.GetValue(0).ToString());
                }
            }
            return jsonResult.ToString();
        }

        [HttpPost]
        public IActionResult Save([FromBody]ExpandoObject json)
        {
            var jsonString = JsonSerializer.Serialize(json);
            var saved = _context.Database.ExecuteSqlRaw("EXEC SaveStore @Json", new SqlParameter("@Json", jsonString));
            return CreatedAtAction("Get", new { id = saved }, null);
        }
    }
}
