using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SearchTool_ServerSide.Authentication;
using SearchTool_ServerSide.Dtos.OrderDtos;
using SearchTool_ServerSide.Dtos.OrderItemDtos;
using SearchTool_ServerSide.Dtos.SearchLogDtos;
using SearchTool_ServerSide.Services;

namespace SearchTool_ServerSide.Controllers
{
    [ApiController]
    [Route("order")]

    public class OrderController(UserAccessToken userAccessToken, OrderService _orderService) : ControllerBase
    {
        [HttpGet("GetAllOrders")]
        public IActionResult GetAllOrders()
        {
            return Ok("List of all orders.");
        }
        [HttpPost("CreateOrder")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequestDto createOrderRequest)
        {

            if (createOrderRequest == null || createOrderRequest.OrderItems == null || createOrderRequest.SearchLogs == null)
            {
                return BadRequest("Invalid order data.");
            }

            var userData = userAccessToken.tokenData();
            if (userData == null || string.IsNullOrEmpty(userData.UserId))
            {
                return Unauthorized("Invalid or missing token data");
            }

            if (!int.TryParse(userData.UserId, out int userId))
            {
                return BadRequest("Invalid user ID format");
            }

            await _orderService.CreateOrder(createOrderRequest.OrderItems, userData.UserId, createOrderRequest.SearchLogs);

            return Ok("Order created successfully.");
        }
        [HttpGet("GetAllOrdersByUserId"),AllowAnonymous]
        public async Task<IActionResult> GetAllOrdersByUserId()
        {
            var userData = userAccessToken.tokenData();
            if (userData == null || string.IsNullOrEmpty(userData.UserId))
            {
                return Unauthorized("Invalid or missing token data");
            }

            if (!int.TryParse(userData.UserId, out int userId))
            {
                return BadRequest("Invalid user ID format");
            }
            var orders = await _orderService.GetAllOrdersByUserId(userId);
            if (orders == null || orders.Count == 0)
            {
                return NotFound("No orders found for the specified user.");
            }
            return Ok(orders);
        }
    }

}