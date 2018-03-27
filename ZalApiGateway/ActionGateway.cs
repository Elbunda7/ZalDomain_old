﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZalApiGateway.ApiTools;
using ZalApiGateway.Models;
using ZalApiGateway.Models.NonSqlModels;

namespace ZalApiGateway
{
    public class ActionGateway
    {
        private JsonFormator jsonFormator;

        public ActionGateway() {
            jsonFormator = new JsonFormator(API.ENDPOINT.ACTIONS);
        }

        public async Task<ActionModel> GetAsync(int id) {
            string tmp = jsonFormator.CreateApiRequestString(API.METHOD.GET, id);
            tmp = await ApiClient.PostRequest(tmp);
            ActionModel respond = JsonConvert.DeserializeObject<ActionModel>(tmp);
            return respond;
        }

        public async Task<Collection<ActionModel>> GetAllAsync() {
            string tmp = jsonFormator.CreateApiRequestString(API.METHOD.GET_ALL);
            tmp = await ApiClient.PostRequest(tmp);
            Collection<ActionModel> respond = JsonConvert.DeserializeObject<Collection<ActionModel>>(tmp);
            return respond;
        }

        public async Task<Collection<ActionModel>> GetAllByYearAsync(ActionRequestModel model) {
            string tmp = jsonFormator.CreateApiRequestString(API.METHOD.GET_ALL_BY_YEAR, model);
            tmp = await ApiClient.PostRequest(tmp);
            Collection<ActionModel> respond = JsonConvert.DeserializeObject<Collection<ActionModel>>(tmp);
            return respond;
        }

        public async Task<bool> AddAsync(ActionModel model) {
            string tmp = jsonFormator.CreateApiRequestString(API.METHOD.ADD, model);
            tmp = await ApiClient.PostRequest(tmp);
            int respond = JsonConvert.DeserializeObject<int>(tmp);
            if (respond != -1) {
                model.Id = respond;
                return true;
            }
            return false;
        }

        public async Task<bool> JoinAsync(int idAction, int idUser) {
            string tmp = jsonFormator.CreateApiRequestString(API.METHOD.JOIN, new Action_UserModel { Id_Action = idAction, Id_User = idUser });
            tmp = await ApiClient.PostRequest(tmp);
            bool result = JsonConvert.DeserializeObject<bool>(tmp);
            return result;
        }

        public async Task<bool> DeleteAsync(int idAction) {
            string tmp = jsonFormator.CreateApiRequestString(API.METHOD.DELETE, idAction);
            tmp = await ApiClient.PostRequest(tmp);
            bool result = JsonConvert.DeserializeObject<bool>(tmp);
            return result;
        }

        public async Task<List<int>> GetParticipatingUsersAsync(int id) {
            throw new NotImplementedException();
        }

        public async Task<bool> UpdateAsync(ActionModel model) {
            string tmp = jsonFormator.CreateApiRequestString(API.METHOD.UPDATE, model);
            tmp = await ApiClient.PostRequest(tmp);
            bool result = JsonConvert.DeserializeObject<bool>(tmp);
            return result;
        }

        public async Task<ActionModel> GetChangedAsync(int id, DateTime lastCheck) {
            throw new NotImplementedException();
        }

        public async Task<ChangesRespondModel<ActionModel>> GetAllChangedAsync(ChangesRequestModel requestModel) {
            string tmp = jsonFormator.CreateApiRequestString(API.METHOD.GET_CHANGED, requestModel);
            tmp = await ApiClient.PostRequest(tmp);
            var result = JsonConvert.DeserializeObject<ChangesRespondModel<ActionModel>>(tmp);
            return result;
        }

    }
}
