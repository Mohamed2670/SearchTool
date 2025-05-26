using System.Transactions;
using AutoMapper;
using SearchTool_ServerSide.Dtos.OrderDtos;
using SearchTool_ServerSide.Dtos.OrderItemDtos;
using SearchTool_ServerSide.Dtos.SearchLogDtos;
using SearchTool_ServerSide.Models;
using SearchTool_ServerSide.Repository;

namespace SearchTool_ServerSide.Services
{
    public class OrderService(DrugRepository _drugrepository, InsuranceRepository _insuranceRepository, OrderRepository _orderRepository, OrderItemRepository _orderItemRepository, IMapper _mapper, SearchLogRepository _searchLogRepository)
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

        internal async Task<ICollection<OrderHistoryReadDto>> GetAllOrdersByUserId(int userId)
        {
            var orders = await _orderRepository.GetAllOrdersByUserId(userId);
            if (orders == null || !orders.Any())
            {
                return null;
            }
            var orderHistoryReadDtos = new List<OrderHistoryReadDto>();
            foreach (var order in orders)
            {
                var orderItemReadDtos = new List<OrderItemReadDto>();
                foreach (var item in order.OrderItems)
                {
                    var searchLog = await _searchLogRepository.GetByOrderItemId(item.Id);
                    item.SearchLogReadDto = _mapper.Map<SearchLogReadDto>(searchLog);
                    var drug = await _drugrepository.GetById(searchLog.DrugId);
                    var rxGroup = await _insuranceRepository.GetRXById(searchLog.RxgroupId ?? 0);
                    var pcn = await _insuranceRepository.GetPCNById(searchLog.PcnId ?? 0);
                    var bin = await _insuranceRepository.GetById(searchLog.BinId ?? 0);
                    item.SearchLogReadDto.DrugName = drug?.Name;
                    item.SearchLogReadDto.NDC = drug?.NDC;
                    item.SearchLogReadDto.RxgroupName = rxGroup?.RxGroup;
                    item.SearchLogReadDto.PcnName = pcn?.PCN;
                    item.SearchLogReadDto.BinName = bin?.Bin;
                    var orderItemReadDto = _mapper.Map<OrderItemReadDto>(item);
                    var OrderDrug = await _drugrepository.GetById(item.DrugId);
                    orderItemReadDto.DrugName = OrderDrug.Name;
                    orderItemReadDto.NDC = OrderDrug.NDC;
                    var insurance = await _insuranceRepository.GetRXById(item.InsuranceRxId ?? 0);
                    orderItemReadDto.InsuranceRxName = insurance.RxGroup;

                    orderItemReadDtos.Add(orderItemReadDto);
                }
                var orderHistoryReadDto = _mapper.Map<OrderHistoryReadDto>(order);
                orderHistoryReadDto.OrderItemReadDtos = orderItemReadDtos;
                orderHistoryReadDtos.Add(orderHistoryReadDto);
            }
            return orderHistoryReadDtos;
        }


    }


}