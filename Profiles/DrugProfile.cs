using AutoMapper;
using SearchTool_ServerSide.Dtos;
using SearchTool_ServerSide.Dtos.DrugDtos;
using SearchTool_ServerSide.Dtos.UserDtos;
using SearchTool_ServerSide.Models;
using ServerSide.Models;

namespace SearchTool_ServerSide.Profiles
{
    public class DrugProfile : Profile
    {
        public DrugProfile()
        {
            CreateMap<DrugInsurance, DrugsAlternativesReadDto>()
                      // Map the drug name from the Drug navigation property
                      .ForMember(dest => dest.DrugName, opt => opt.MapFrom(src => src.Drug != null ? src.Drug.Name : string.Empty))
                      // Map the drug class name from the Drug's DrugClass navigation property
                      .ForMember(dest => dest.DrugClass, opt => opt.MapFrom(src =>
                          src.Drug != null && src.Drug.DrugClass != null ? src.Drug.DrugClass.Name : string.Empty))
                    .ForMember(dest => dest.DrugClassV2Id, opt => opt.MapFrom(src =>
                          src.Drug != null && src.Drug.DrugClassV2 != null ? src.Drug.DrugClassV2.Id : 0))
                    .ForMember(dest => dest.DrugClassV3Id, opt => opt.MapFrom(src =>
                          src.Drug != null && src.Drug.DrugClassV3 != null ? src.Drug.DrugClassV3.Id : 0))
                      // Map the insurance name from the Insurance navigation property
                      .ForMember(dest => dest.insuranceName, opt => opt.MapFrom(src => src.Insurance != null ? src.Insurance.RxGroup : string.Empty))
                      // Map the branch name from the Branch navigation property
                      .ForMember(dest => dest.branchName, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : string.Empty));

            // Reverse mapping if needed
            CreateMap<DrugsAlternativesReadDto, DrugInsurance>();
            CreateMap<DrugInsurance, DrugInsuranceReadDto>()
               .ForMember(dest => dest.Insurance, opt => opt.MapFrom(src => src.Insurance != null ? src.Insurance.RxGroup : null))
               .ForMember(dest => dest.Drug, opt => opt.MapFrom(src => src.Drug != null ? src.Drug.Name : null))
               .ForMember(dest => dest.Branch, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : null));
            CreateMap<DrugMedi, DrugMediReadDto>();
            CreateMap<Drug, FullDrugReadDto>().ReverseMap();

        }
    }
}