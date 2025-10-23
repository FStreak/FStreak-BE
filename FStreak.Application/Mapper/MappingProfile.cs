﻿using AutoMapper;
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
        }
    }
}
