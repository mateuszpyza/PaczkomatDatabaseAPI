using AutoMapper;
using PaczkomatDatabaseAPI.Models;

namespace PaczkomatDatabaseAPI
{
    public class FailureMappingProfile : Profile
    {
        public FailureMappingProfile() 
        {
            CreateMap<MachineEventDto, Failure>()
                .ForMember(failure => failure.Id, opt => opt.MapFrom((src, failure, destMember, context) => context.Items["Id"]))
                .ForMember(failure => failure.OccurDate, opt => opt.MapFrom((src, failure, destMember, context) => context.Items["OccurDate"]));
        }
    }
}
