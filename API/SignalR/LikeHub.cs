using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
    public class LikeHub: Hub
    {
        private static readonly Dictionary<string, List<string>> OnlineUser = 
            new Dictionary<string, List<string>>();
        public override async Task OnConnectedAsync()
        {
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            
        }
    }
}