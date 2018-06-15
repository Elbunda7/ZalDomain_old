﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using ZalDomain.consts;
using System.Xml.Linq;
using ZalDomain.tools;
using ZalApiGateway.Models;
using ZalApiGateway;
using System.Threading.Tasks;
using ZalApiGateway.Models.ApiCommunicationModels;
using Newtonsoft.Json.Linq;
using ZalDomain.Models;

namespace ZalDomain.ActiveRecords
{
    public class ActionEvent : IActiveRecord
    {
        private ActionModel Model;

        private IEnumerable<User> garants;
        private IEnumerable<User> members;
        private Article report;
        private Article info;
        
        public int Id => Model.Id;
        public string Type => Model.EventType;
        public string Name => Model.Name;
        public int Days => GetDays();
        public int FromRank => Model.FromRank;
        public DateTime DateFrom => Model.Date_start;
        public DateTime DateTo => Model.Date_end;
        public int MembersCount => Model.MemberCount;
        public bool IsOfficial => Model.IsOfficial;//přejmenovat nebo přidat IsPublished
        //účastním se?

        //public int Price { get { return Data.Price; } }
        //public decimal GPS_lon { get { return Data.GPS_lon; } }
        //public decimal GPS_lat { get { return Data.GPS_lat; } }
        public bool HasInfo => Model.Id_Info.HasValue;
        public bool HasReport => Model.Id_Report.HasValue;

        private static ActionGateway gateway;
        private static ActionGateway Gateway => gateway ?? (gateway = new ActionGateway());

        private UnitOfWork<ActionModel> unitOfWork;
        public UnitOfWork<ActionModel> UnitOfWork => unitOfWork ?? (unitOfWork = new UnitOfWork<ActionModel>(Model, OnUpdateCommited));

        private int GetDays() {
            TimeSpan ts = Model.Date_end - Model.Date_start;
            return (int)ts.TotalDays;
        }

        public async Task<Article> InfoLazyLoad() {
            if (HasInfo && info == null) {
                info = await Zal.Actualities.GetArticleAsync(Model.Id_Info.Value);
            }
            return info;
        }

        public async Task<Article> ReportLazyLoad() {
            if (HasReport && report == null) {
                report = await Zal.Actualities.GetArticleAsync(Model.Id_Report.Value);
            }
            return report;
        }

        internal static async Task<ChangedActiveRecords<ActionEvent>> GetChangedAsync(int userRank, DateTime lastCheck, int currentYear, int count) {
            var requestModel = new ChangesRequestModel {
                Rank = userRank,
                LastCheck = lastCheck,
                Year = currentYear,
                Count = count
            };
            var rawChanges = await Gateway.GetAllChangedAsync(requestModel);
            return new ChangedActiveRecords<ActionEvent>(rawChanges);
        }

        public ActionEvent(IModel model) {
            Model = model as ActionModel;
        }

        public static async Task<ActionEvent> AddAsync(string name, string type, DateTime start, DateTime end, int fromRank, bool isOfficial = true) {
            ActionModel model = new ActionModel {
                Id = -1,
                Name = name,
                EventType = type,
                Date_start = start,
                Date_end = end,
                FromRank = fromRank,
                IsOfficial = isOfficial,
            };
            if (await Gateway.AddAsync(model)) {
                return new ActionEvent(model);
            }
            return null;
        }

        public async Task<IEnumerable<User>> MembersLazyLoad(ZAL.ActionUserRole role, bool reload = false) {
            await MembersLazyLoad(reload);
            switch (role) {
                case ZAL.ActionUserRole.Garant: return garants;
                case ZAL.ActionUserRole.Member: return members;
                default: return garants.Union(members);
            }
        }

        private async Task MembersLazyLoad(bool reload = false) {
            if (reload || (garants == null && members == null)) {
                var respond = await Gateway.GetUsersOnAction(Id);
                garants = respond.Where(x => x.IsGarant).Select(x => new User(x.Member)).ToList();
                members = respond.Where(x => !x.IsGarant).Select(x => new User(x.Member)).ToList();
            }
        }

        public async Task<bool> AddNewInfoAsync(string title, string text) {
            //token uživatele
            return await AddNewInfoAsync(Zal.Session.CurrentUser, title, text);
        }

        public async Task<bool> AddNewInfoAsync(User author, string title, string text) {
            //token uživatele
            info = await Zal.Actualities.CreateNewArticle(author, title, text, FromRank);//new article + action.Id_foreign = najednou
            if (info != null) {
                Model.Id_Info = info.Id;
                return await Gateway.UpdateAsync(Model);
            }
            return false;
        }

        public async Task<bool> AddNewReportAsync(string title, string text) {
            //token uživatele
            //přepsat stávající?
            return await AddNewReportAsync(Zal.Session.CurrentUser, title, text);
        }

        public async Task<bool> AddNewReportAsync(User author, string title, string text) {
            //token uživatele
            report = await Zal.Actualities.CreateNewArticle(author, title, text, FromRank);
            if (report != null) {
                Model.Id_Report = report.Id;
                return await Gateway.UpdateAsync(Model);
            }
            return false;
        }

        /*public bool Has(int key, int value) {
            switch (key) {
                case CONST.AKCE.ID: return Model.Id == value;
                case CONST.AKCE.POCET_DNI: return Model.Pocet_dni == value;
                case CONST.AKCE.OD_HODNOSTI: return Model.Od_hodnosti == value;
                default: return false;
            }
        }

        public bool Has(int key, String value) {
            switch (key) {
                case CONST.AKCE.JMENO: return value.Equals(Model.Jmeno);
                case CONST.AKCE.TYP: return value.Equals(Model.Typ);
                case CONST.AKCE.EMAIL_VEDOUCI: return value.Equals(Model.Email_vedouci);
                default: return false;
            }
        }

        public bool Has(int key, bool value) {
            switch (key) {
                case CONST.AKCE.JE_OFICIALNI: return Model.Je_oficialni == value;
                default: return false;
            }
        }

        public bool Has(int key, DateTime value) {
            switch (key) {
                case CONST.AKCE.DATUM: return Model.Datum_od.Date == value.Date;
                case CONST.AKCE.DATUM_DO: return Model.Datum_od.Date <= value.Date;
                case CONST.AKCE.DATUM_OD: return Model.Datum_od.Date >= value.Date;
                default: return false;
            }
        }*/


        private Task<bool> OnUpdateCommited() {
            return Gateway.UpdateAsync(Model);
        }

        [Obsolete]
        public async Task<bool> AktualizeAsync(String name, String type, DateTime? start, DateTime? end, int? fromRank, bool? isOfficial) {
            UserPermision.HasRank(Zal.Session.CurrentUser, ZAL.RANK.VEDOUCI);
            if (name != null) Model.Name = name;
            if (type != null) Model.EventType = type;
            if (end != null) Model.Date_end = end.Value;
            if (start != null) Model.Date_start = start.Value;
            if (fromRank != null) Model.FromRank = fromRank.Value;
            if (isOfficial != null) Model.IsOfficial = isOfficial.Value;
            return await Gateway.UpdateAsync(Model);
        }

        public Task<bool> Join(bool asGarant = false) {
            //token uživatele
            return Join(Zal.Session.CurrentUser, asGarant);
        }

        public async Task<bool> Join(User user, bool asGarant = false) {
            await MembersLazyLoad();
            UpdateLocalMember(user, asGarant);
            var requestModel = new ActionUserJoinModel {
                Id_User = user.Id,
                Id_Action = Id,
                IsGarant = asGarant,
            };
            return await Gateway.Join(requestModel);
        }

        public Task<bool> UnJoin() {
            return UnJoin(Zal.Session.CurrentUser);
        }

        public async Task<bool> UnJoin(User user) {
            await MembersLazyLoad();
            RemoveLocalMember(user);
            var requestModel = new ActionUserModel {
                Id_User = user.Id,
                Id_Action = Id,
            };
            return await Gateway.UnJoin(requestModel);
        }

        private void UpdateLocalMember(User user, bool asGarant) {
            RemoveLocalMember(user);
            if (asGarant) {
                (garants as List<User>).Add(user);
            }
            else {
                (members as List<User>).Add(user);
            }
        }

        private void RemoveLocalMember(User user) {
            if (garants.Contains(user, ActiveRecordEqualityComparer.Instance)) {
                (garants as List<User>).Remove(garants.Single(x => x.Id == user.Id));
            }
            else if (members.Contains(user, ActiveRecordEqualityComparer.Instance)) {
                (members as List<User>).Remove(members.Single(x => x.Id == user.Id));
            }
        }

        public override string ToString() {
            return Model.Name;
        }

        public async void Synchronize(DateTime lastCheck) {
            Model = await Gateway.GetChangedAsync(Id, lastCheck);
        }

        public async static Task<AllActiveRecords<ActionEvent>> GetAllByYear(int userRank, int year) {//vrátit respond model se serverovým časem 
            var requestModel = new ActionRequestModel {
                Rank = userRank,
                Year = year
            };
            var rawRespondModel = await Gateway.GetAllByYearAsync(requestModel);
            return new AllActiveRecords<ActionEvent>(rawRespondModel);
        }

        public async static Task<ActionEvent> Get(int id) {
            return new ActionEvent(await Gateway.GetAsync(id));
        }

        public Task<bool> DeleteAsync() {
            return Gateway.DeleteAsync(Model.Id);
        }

        internal JToken GetJson() {
            return JObject.FromObject(Model);
        }

        internal static ActionEvent LoadFrom(JToken json) {
            var model = json.ToObject<ActionModel>();
            return new ActionEvent(model);
        }
    }
}
