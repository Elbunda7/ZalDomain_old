﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZalApiGateway.Models.ApiCommunicationModels
{
    public class BadgeRequestModel
    {
        public DateTime LastCheck { get; set; }
        public int Rank { get; set; }
    }
}
