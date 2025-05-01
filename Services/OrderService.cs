using System.Transactions;
using AutoMapper;
using SearchTool_ServerSide.Dtos.OrderDtos;
using SearchTool_ServerSide.Dtos.OrderItemDtos;
using SearchTool_ServerSide.Dtos.SearchLogDtos;
using SearchTool_ServerSide.Models;
using SearchTool_ServerSide.Repository;

namespace SearchTool_ServerSide.Services
{
    public class OrderService(DrugRepository _drugrepository,OrderRepository _orderRepository, OrderItemRepository _orderItemRepository, IMapper _mapper, SearchLogRepository _searchLogRepository)
    {
        internal async Task CreateOrder(ICollection<OrderItemAddDto> orderItemAddDtos, string userId, ICollection<SearchLogAddDto> searchLogAddDtos)
        {
            using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    var orderAddDto = new OrderAddDto
                    {
                        UserId = int.Parse(userId),
                        Date = DateTime.UtcNow,
                        TotalNet = orderItemAddDtos.Sum(x => x.NetPrice * x.Amount),
                        TotalPatientPay = orderItemAddDtos.Sum(x => x.PatientPay * x.Amount),
                        TotalInsurancePay = orderItemAddDtos.Sum(x => x.InsurancePay * x.Amount),
                        TotalAcquisitionCost = orderItemAddDtos.Sum(x => x.AcquisitionCost * x.Amount),
                        AdditionalCost = orderItemAddDtos.Sum(x => x.AdditionalCost * x.Amount)
                    };
                    var oreder = _mapper.Map<Order>(orderAddDto);
                    await _orderRepository.Add(oreder);

                    for (int i = 0; i < orderItemAddDtos.Count; i++)
                    {
                        var orderItemAddDto = orderItemAddDtos.ElementAt(i);
                        var searchLogAddDto = searchLogAddDtos.ElementAt(i);
                        var orderItem = _mapper.Map<OrderItem>(orderItemAddDto);
                        // Console.WriteLine($"RxgroupId is {orderItem.InsuranceRxId}, skipping this order item.");
                        // Console.ReadKey();
                        orderItem.OrderId = oreder.Id;
                        await _orderItemRepository.Add(orderItem);
                        var searchLog = _mapper.Map<SearchLog>(searchLogAddDto);
                        // if (searchLog.RxgroupId == 0)
                        // {
                        //     searchLog.RxgroupId = 1;
                        // }
                        // if (searchLog.PcnId == 0)
                        // {
                        //     searchLog.PcnId = 1;
                        // }
                        // if (searchLog.BinId == 0)
                        // {
                        //     searchLog.BinId = 1;
                        // }
                        searchLog.OrderItemId = orderItem.Id;
                        searchLog.UserId = int.Parse(userId);
                        searchLog.Date = DateTime.UtcNow;
                        await _searchLogRepository.Add(searchLog);
                    }
                    transactionScope.Complete();
                }
                catch
                {
                    // Log the exception if needed
                    throw;
                }
            }
        }
    }


}