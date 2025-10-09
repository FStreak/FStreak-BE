using FStreak.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FStreak.Application.Services.Interface
{
    public interface IAgoraService
    {
        Task<Result<AgoraTokenResponse>> GenerateTokenAsync(string channelName, string userId);

    }
}
