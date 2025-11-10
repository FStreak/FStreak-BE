using AutoMapper;
using FStreak.Application.DTOs;
using FStreak.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FStreak.Application.Mapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ApplicationUser, UserDto>();

            CreateMap<StudyRoom, StudyRoomDto>()
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy))
                .ForMember(dest => dest.RoomUsers, opt => opt.MapFrom(src => src.RoomUsers));

            // Lesson mappings
            CreateMap<LessonCreateDto, Lesson>()
                .ForMember(dest => dest.DocumentUrl, opt => opt.MapFrom(src => src.DocumentUrl))
                .ForMember(dest => dest.VideoUrl, opt => opt.MapFrom(src => src.VideoUrl))
                .ForMember(dest => dest.DocumentType, opt => opt.MapFrom(src => src.DocumentType));

            CreateMap<LessonUpdateDto, Lesson>()
                .ForMember(dest => dest.DocumentUrl, opt => opt.MapFrom(src => src.DocumentUrl))
                .ForMember(dest => dest.VideoUrl, opt => opt.MapFrom(src => src.VideoUrl))
                .ForMember(dest => dest.DocumentType, opt => opt.MapFrom(src => src.DocumentType));

            CreateMap<Lesson, LessonReadDto>()
                .ForMember(dest => dest.CreatedByName, opt => opt.MapFrom(src => $"{src.CreatedBy.FirstName} {src.CreatedBy.LastName}"))
                .ForMember(dest => dest.DocumentUrl, opt => opt.MapFrom(src => src.DocumentUrl))
                .ForMember(dest => dest.VideoUrl, opt => opt.MapFrom(src => src.VideoUrl))
                .ForMember(dest => dest.DocumentType, opt => opt.MapFrom(src => src.DocumentType));

            // Achievement mappings
            CreateMap<CreateAchievementDto, Achievement>();
            CreateMap<Achievement, AchievementDto>();
            CreateMap<UserAchievement, UserAchievementDto>()
                .ForMember(dest => dest.Achievement, opt => opt.MapFrom(src => src.Achievement));

            // Shop mappings
            CreateMap<CreateShopItemDto, ShopItem>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => Enum.Parse<ShopItemType>(src.Type)));
            CreateMap<ShopItem, ShopItemDto>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()));
            CreateMap<ShopOrder, ShopOrderDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.OrderItems, opt => opt.MapFrom(src => src.OrderItems));
            CreateMap<ShopOrderItem, ShopOrderItemDto>()
                .ForMember(dest => dest.ShopItem, opt => opt.MapFrom(src => src.ShopItem));
        }
    }
}
