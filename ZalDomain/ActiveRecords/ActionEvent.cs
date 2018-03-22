﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using ZalDomain.consts;
using DAL.tableStructures;
using DAL.Gateway;
using System.Xml.Linq;
using ZalDomain.tools;
using ZalApiGateway.Models;
using ZalApiGateway;
using System.Threading.Tasks;

namespace ZalDomain.ActiveRecords
{
    public class ActionEvent : IActiveRecord
    {
        private ActionModel Model;
        //private AkceTable 
        private Collection<User> garants;
        private Collection<User> participants;
        private InfoAction info;
        private RecordAction record;

        public int Id => Model.Id;
        public string Type => Model.EventType;
        public string Name => Model.Name;
        public int Days => GetDays();
        //public int RankLeast => Model.Od_hodnosti;
        public DateTime DateFrom => Model.Date_start;
        public InfoAction Info { get { return InfoLazyLoad(); } private set { info = value; } }
        public RecordAction Record { get { return RecordLazyLoad(); } private set { record = value; } }
        public Collection<User> Garants { get { return GarantsLazyLoad(); } set { SetGarants(value); } }
        public Collection<User> Participants { get { return ParticipantsLazyLoad(); } private set { participants = value; } }
        public int ParticipantsCount => Participants.Count; //Model.NumOfParticipants
        //public bool Je_oficialni { get; set; }
        //účastním se?


        private int GetDays() {
            TimeSpan ts = Model.Date_end - Model.Date_start;
            return (int)ts.TotalDays;
        }

        private InfoAction InfoLazyLoad() {
            if (info == null) {
                info = Zal.Actualities.GetInfoForAction(Model.Id);
            }
            return info;
        }

        private RecordAction RecordLazyLoad() {
            if (record == null) {
                record = Zal.Actualities.GetZapisForAction(Model.Id);
            }
            return record;
        }


        private static ActionGateway gateway;
        private static ActionGateway Gateway => gateway ?? (gateway = new ActionGateway());

        public ActionEvent(string name, string type, DateTime start, DateTime end, int odHodnosti, bool isOfficial) {
            Model = new ActionModel {
                Id = -1,
                Name = name,
                EventType = type,
                Date_start = start,
                Date_end = end,
                //Email_vedouci = null,
                //Od_hodnosti = odHodnosti,
                IsOfficial = isOfficial,
            };
            PostModel();
        }

        public async void PostModel() {
            await Gateway.AddAsync(Model);
        }

        public ActionEvent(ActionModel model) {
            Model = model;
        }

        private Collection<User> GarantsLazyLoad() {
            /*if (garants == null) {
                if (Model.Email_vedouci != null) {
                    garants = Zal.Users.GetByEmail(Model.Email_vedouci);
                }
                else {
                    garants = User.Empty();
                }               
            }
            return garants;*/
            throw new NotImplementedException();
        }

        private Collection<User> ParticipantsLazyLoad() {
            if (participants == null) {//obnovit?
                List<string> list = gateway.GetParticipatingUsers(Model.Id);
                participants = Zal.Users.GetByEmailList(list);
            }
            return participants;
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

        public bool SetGarants(Collection<User> garants) {
            /*IntegrityCondition.UserIsLeader();
            if (Gateway.JoinLeaderToAction(garants.Email, Model.Id)) {
                Garants = garants;
                return true;
            }
            return false;*/
            throw new NotImplementedException();
        }

        public async void Aktualize(String name, String type, DateTime? start, DateTime? end, int? fromRank, bool? isOfficial) {
            IntegrityCondition.UserIsLeader();
            if (name != null) {
                Model.Name = name;
            }
            if (type != null) {
                Model.EventType = type;
            }
            if (start != null) {
                Model.Date_start = start.Value;
            }
            if (end != null) {
                Model.Date_end = end.Value;
            }
            if (fromRank != null) {
                //Model.Od_hodnosti = (int)fromRank;
            }
            if (isOfficial != null) {
                Model.IsOfficial = (bool)isOfficial;
            }
            await Gateway.UpdateAsync(Model);
        }

        public int GetNumberOfMembers() {
            throw new NotImplementedException();
            //return Gateway.getNumOfUsers(this);
        }

        public void Participate(User user, bool isGoing) {
            //Gateway.JoinAsync(user.Id, Model.Id);
        }

        public override string ToString() {
            return Model.Name;
        }

        public async void Synchronize(DateTime lastCheck) {
            Model = await Gateway.GetChangedAsync(Id, lastCheck);
        }

        public async static Task<IEnumerable<ActionEvent>> GetUpcoming(User user) {
            bool onlyOfficial = user.RankLevel < ZAL.RANK.VEDOUCI ? true : false;
            var models = await Gateway.GetUpcomingAsync(user.RankLevel, onlyOfficial);
            var actions = models.Select(model => new ActionEvent(model));
            return actions;
        }

        public async static Task<List<int>> SynchronizeAll(User user, DateTime lastCheck) {
            return await Gateway.GetAllChangedAsync(user.RankLevel, lastCheck);
        }

        public async static Task<ActionEvent> Get(int id) {
            return new ActionEvent(await Gateway.GetAsync(id));
        }

        public async Task<bool> DeleteAsync() {
            return await Gateway.DeleteAsync(Model.Id);
        }

        public XElement GetXml(string elementName) {
            /*XElement element = new XElement(elementName,
                new XElement("Id", Model.Id),
                new XElement("DateOfAction", Model.Datum_od.Ticks),
                new XElement("Name", Model.Jmeno),
                new XElement("Type", Model.Typ),
                new XElement("IsOfficial", Model.Je_oficialni),
                new XElement("FromRank", Model.Od_hodnosti),
                new XElement("Days", Model.Pocet_dni),
                new XElement("GarantEmail",Model.Email_vedouci));
            return element;*/
            throw new NotImplementedException();
        }

        public static ActionEvent LoadFromXml(XElement element) {
            /*AkceTable data = new AkceTable {
                Id = Int32.Parse(element.Element("Id").Value),
                Datum_od = new DateTime(long.Parse(element.Element("DateOfAction").Value)),
                Jmeno = element.Element("Name").Value,
                Typ = element.Element("Type").Value,
                Je_oficialni = Boolean.Parse(element.Element("IsOfficial").Value),
                Od_hodnosti = Int32.Parse(element.Element("FromRank").Value),
                Pocet_dni = Int32.Parse(element.Element("Days").Value),
            };
            if (!element.Element("GarantEmail").IsEmpty) {
                data.Email_vedouci = element.Element("GarantEmail").Value;
            }
            return new ActionEvent(data);*/
            throw new NotImplementedException();
        }
    }
}
