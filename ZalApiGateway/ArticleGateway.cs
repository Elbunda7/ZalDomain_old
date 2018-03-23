﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZalApiGateway.ApiTools;
using ZalApiGateway.Models;

namespace ZalApiGateway
{
    public class ArticleGateway
    {
        private JsonFormator jsonFormator;

        public ArticleGateway() {
            jsonFormator = new JsonFormator(API.ENDPOINT.ARTICLES);
        }


        public async Task<bool> AddAsync(ArticleModel model) {
            string tmp = jsonFormator.CreateApiRequestString(API.METHOD.ADD, model);
            tmp = await ApiClient.PostRequest(tmp);
            model.Id = JsonConvert.DeserializeObject<int>(tmp);
            return true;
        }

        public async Task<bool> DeleteAsync(int id) {
            string tmp = jsonFormator.CreateApiRequestString(API.METHOD.DELETE, id);
            tmp = await ApiClient.PostRequest(tmp);
            bool result = JsonConvert.DeserializeObject<bool>(tmp);
            return result;
        }

        public async Task<string> CheckForChanges(string userEmail, DateTime lastCheck) {
            throw new NotImplementedException();
        }

        public async Task<List<int>> GetChanged(string userEmail, DateTime lastCheck) {
            throw new NotImplementedException();
        }

        public ArticleModel SelectGeneral(int id) {
            throw new NotImplementedException();
        }

        public async Task<Collection<ArticleModel>> SelectAllGeneralFor(string uzivatelEmail) {
            throw new NotImplementedException();
        }

        public async Task<bool> Update(ArticleModel model) {
            throw new NotImplementedException();
        }

        public async Task<ArticleModel> GetAsync(int id) {
            throw new NotImplementedException();
        }

    }
}