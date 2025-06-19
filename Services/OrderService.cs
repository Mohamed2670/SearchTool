using System.Transactions;
using AutoMapper;
using SearchTool_ServerSide.Dtos.OrderDtos;
using SearchTool_ServerSide.Dtos.OrderItemDtos;
using SearchTool_ServerSide.Dtos.SearchLogDtos;
using SearchTool_ServerSide.Models;
using SearchTool_ServerSide.Repository;

namespace SearchTool_ServerSide.Services
{
    public class OrderService(DrugRepository _drugrepository, UserRepository _userRepository, InsuranceRepository _insuranceRepository, OrderRepository _orderRepository, OrderItemRepository _orderItemRepository, IMapper _mapper, SearchLogRepository _searchLogRepository)
    {
        internal async Task CreateOrder(ICollection<OrderItemAddDto> orderItemAddDtos, string userEmail, ICollection<SearchLogAddDto> searchLogAddDtos)
        {
            var user = await _userRepository.GetUserByEmail(userEmail);

            using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    if (user == null || string.IsNullOrWhiteSpace(user.Email))
                    {
                        Console.WriteLine($"❌ User with email {userEmail} not found or missing email.");
                        throw new InvalidOperationException("User not found or missing email");
                    }
                    else
                    {
                        Console.WriteLine($"✅ User with email {userEmail} found: {user.Email} with ID {user.Id}.");
                    }



                    // create order DTO
                    var orderAddDto = new OrderAddDto
                    {
                        Date = DateTime.UtcNow,
                        TotalNet = orderItemAddDtos.Sum(x => x.NetPrice * x.Amount),
                        TotalPatientPay = orderItemAddDtos.Sum(x => x.PatientPay * x.Amount),
                        TotalInsurancePay = orderItemAddDtos.Sum(x => x.InsurancePay * x.Amount),
                        TotalAcquisitionCost = orderItemAddDtos.Sum(x => x.AcquisitionCost * x.Amount),
                        AdditionalCost = orderItemAddDtos.Sum(x => x.AdditionalCost * x.Amount)
                    };
                    var order = new Order
                    {
                        UserEmail = user.Email,
                        Date = orderAddDto.Date,
                        TotalNet = orderAddDto.TotalNet,
                        TotalPatientPay = orderAddDto.TotalPatientPay,
                        TotalInsurancePay = orderAddDto.TotalInsurancePay,
                        TotalAcquisitionCost = orderAddDto.TotalAcquisitionCost,
                        AddtionalCost = orderAddDto.AdditionalCost
                    };


                    await _orderRepository.Add(order);

                    for (int i = 0; i < orderItemAddDtos.Count; i++)
                    {
                        var orderItemAddDto = orderItemAddDtos.ElementAt(i);
                        var searchLogAddDto = searchLogAddDtos.ElementAt(i);
                        var orderItem = _mapper.Map<OrderItem>(orderItemAddDto);
                        // Console.WriteLine($"RxgroupId is {orderItem.InsuranceRxId}, skipping this order item.");
                        // Console.ReadKey();
                        orderItem.OrderId = order.Id;
                        var drug = await _drugrepository.GetDrugByNdc(orderItemAddDto.DrugNDC);
                        orderItem.DrugName = drug.Name;
                        await _orderItemRepository.Add(orderItem);
                        var searchLog = _mapper.Map<SearchLog>(searchLogAddDto);

                        var rxGroup = await _insuranceRepository.GetRXById(searchLog.RxgroupId ?? 0);
                        searchLog.RxgroupId = rxGroup?.Id ?? 0; // Default to 0 if not found
                        searchLog.PcnId = rxGroup?.InsurancePCNId ?? 1; // Default to 1 if not found
                        searchLog.BinId = rxGroup?.InsurancePCN?.InsuranceId ?? 1; // Default to 1 if not found

                        searchLog.OrderItemId = orderItem.Id;
                        searchLog.UserEmail = order.UserEmail;
                        searchLog.User = user;
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

        internal async Task<ICollection<OrderHistoryReadDto>> GetAllOrdersByUserId(string userEmail)
        {
            var orders = await _orderRepository.GetAllOrdersByUserId(userEmail);
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
                    var drug = await _drugrepository.GetDrugByNdc(searchLog.DrugNDC);
                    var rxGroup = await _insuranceRepository.GetRXById(searchLog.RxgroupId ?? 0);
                    var pcn = await _insuranceRepository.GetPCNById(searchLog.PcnId ?? 0);
                    var bin = await _insuranceRepository.GetById(searchLog.BinId ?? 0);
                    item.SearchLogReadDto.DrugName = drug?.Name;
                    item.SearchLogReadDto.NDC = drug?.NDC;
                    item.SearchLogReadDto.RxgroupName = rxGroup?.RxGroup;
                    item.SearchLogReadDto.PcnName = pcn?.PCN;
                    item.SearchLogReadDto.BinName = bin?.Bin;
                    var orderItemReadDto = _mapper.Map<OrderItemReadDto>(item);
                    var OrderDrug = await _drugrepository.GetDrugByNdc(item.DrugNDC);
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