﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZalApiGateway.Models.ApiCommunicationModels
{
    public class ArticleTopTenRequestModel
    {
        public int[] Ids { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
