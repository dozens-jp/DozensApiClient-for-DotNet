﻿using DozensAPI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Configuration;

namespace DozensAPI.Test
{
    [TestClass()]
    public class DozensTest
    {
        public TestContext TestContext { get; set; }

        public string DozensId { get { return ConfigurationManager.AppSettings["DozensId"]; } }
        public string APIKey { get { return ConfigurationManager.AppSettings["APIKey"]; } }

        #region 追加のテスト属性
        // 
        //テストを作成するときに、次の追加属性を使用することができます:
        //
        //クラスの最初のテストを実行する前にコードを実行するには、ClassInitialize を使用
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //クラスのすべてのテストを実行した後にコードを実行するには、ClassCleanup を使用
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //各テストを実行する前にコードを実行するには、TestInitialize を使用
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //各テストを実行した後にコードを実行するには、TestCleanup を使用
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion

        Dozens_Accessor CreateTarget()
        {
            var target = new Dozens_Accessor(this.DozensId, this.APIKey);
            if (ConfigurationManager.AppSettings["UseMock"].ToLower() == "true")
            {
                target._APIEndPoint = new MockEndPoint();
            }
            return target;
        }

        static bool RegexIsMatch(string expectedPattern, string actual)
        {
            return Regex.IsMatch(actual, expectedPattern);
        }

        [TestMethod]
        public void AuthTest()
        {
            var target = this.CreateTarget();
            target.Token.IsNull();

            target.Auth();
            target.Token.IsNotNull();
        }

        [TestMethod()]
        public void GetZonesTest()
        {
            var target = this.CreateTarget();

            VerifyInitialZonez(target);
        }

        public void VerifyInitialZonez(Dozens_Accessor target)
        {
            var zones = target
                .GetZones()
                .Select(zone => zone.ToString())
                .ToArray();
            zones.Is(@"{Id = 200, Name = ""jsakamoto.info""}");
        }

        [TestMethod]
        public void CreateAndDeleteZoneByIdTest()
        {
            var target = this.CreateTarget();
            var zones = CreateZoneTest(target);

            var zoneId = zones.First(z => z.Name == "subdomain.jsakamoto.info").Id;
            target.DeleteZone(zoneId);
            VerifyInitialZonez(target);
        }

        private DozensZone[] CreateZoneTest(Dozens_Accessor target)
        {
            VerifyInitialZonez(target);

            var zones = target.CreateZone("subdomain.jsakamoto.info");
            zones
                .Select(z => z.ToString())
                .Is(new[] { 
                @"{Id = \d+, Name = ""subdomain\.jsakamoto\.info""}",
                @"{Id = 200, Name = ""jsakamoto\.info""}" 
                }, RegexIsMatch);
            return zones;
        }

        [TestMethod]
        public void CreateAndDeleteZoneByNameTest()
        {
            var target = this.CreateTarget();
            CreateZoneTest(target);

            target.DeleteZone("subdomain.jsakamoto.info");
            VerifyInitialZonez(target);
        }

        [TestMethod()]
        public void GetRecordsTest()
        {
            var target = this.CreateTarget();
            VerifyInitialRecords(target);
        }

        public void VerifyInitialRecords(Dozens_Accessor target)
        {
            var records = target
                .GetRecords("jsakamoto.info")
                .Select(record => record.ToString())
                .ToArray();
            records.Is(new[] {
                @"{Id = \d+, Name = www.jsakamoto.info, Type = A, Content = 192.168.0.101, Prio = 0, TTL = 7200}" 
                }, RegexIsMatch);
        }

        [TestMethod()]
        public void UpdateRecordsByIdTest()
        {
            var target = this.CreateTarget();
            VerifyInitialRecords(target);
            target.UpdateRecord(6654, 1, "192.168.0.201", 7200);
            
            var records = target.GetRecords("jsakamoto.info")
            .Select(record => record.ToString())
            .ToArray();
            records.Is(@"{Id = 6654, Name = www.jsakamoto.info, Type = A, Content = 192.168.0.201, Prio = 1, TTL = 7200}");

            target.UpdateRecord(6654, 0, "192.168.0.101", 7200);
            VerifyInitialRecords(target);
        }

        [TestMethod()]
        public void UpdateRecordsByNameTest()
        {
            var target = this.CreateTarget();
            VerifyInitialRecords(target);
            target.UpdateRecord("jsakamoto.info", "www", 1, "192.168.0.201", 7200);

            var records = target.GetRecords("jsakamoto.info")
            .Select(record => record.ToString())
            .ToArray();
            records.Is(@"{Id = 6654, Name = www.jsakamoto.info, Type = A, Content = 192.168.0.201, Prio = 1, TTL = 7200}");

            target.UpdateRecord("jsakamoto.info", "www", 0, "192.168.0.101", 7200);
            VerifyInitialRecords(target);
        }

        [TestMethod()]
        public void UpdateRecordsByFQDNTest()
        {
            var target = this.CreateTarget();
            VerifyInitialRecords(target);
            target.UpdateRecord("jsakamoto.info", "www.jsakamoto.info", 1, "192.168.0.201", 7200);

            var records = target.GetRecords("jsakamoto.info")
            .Select(record => record.ToString())
            .ToArray();
            records.Is(@"{Id = 6654, Name = www.jsakamoto.info, Type = A, Content = 192.168.0.201, Prio = 1, TTL = 7200}");

            target.UpdateRecord("jsakamoto.info", "www.jsakamoto.info", 0, "192.168.0.101", 7200);
            VerifyInitialRecords(target);
        }

        [TestMethod()]
        public void UpdateRecordsByFQDNOnlyTest()
        {
            var target = this.CreateTarget();
            VerifyInitialRecords(target);
            target.UpdateRecord("www.jsakamoto.info", 1, "192.168.0.201", 7200);

            var records = target.GetRecords("jsakamoto.info")
            .Select(record => record.ToString())
            .ToArray();
            records.Is(@"{Id = 6654, Name = www.jsakamoto.info, Type = A, Content = 192.168.0.201, Prio = 1, TTL = 7200}");

            target.UpdateRecord("www.jsakamoto.info", 0, "192.168.0.101", 7200);
            VerifyInitialRecords(target);
        }

        [TestMethod()]
        public void CreateAndDeleteRecordByIdTest()
        {
            var target = this.CreateTarget();
            VerifyInitialRecords(target);
            target.CreateRecord("jsakamoto.info", "", "MX", 0, "smtp.jsakamoto.info", 7200);

            var records = target.GetRecords("jsakamoto.info");
            records.Select(r => r.ToString())
                .ToArray()
                .Is(new[]{
                    @"{Id = \d+, Name = www.jsakamoto.info, Type = A, Content = 192.168.0.101, Prio = 0, TTL = 7200}",                    
                    @"{Id = \d+, Name = jsakamoto\.info, Type = MX, Content = smtp\.jsakamoto\.info, Prio = 0, TTL = 7200}"
                }, RegexIsMatch);

            var record = records.First(r => r.Type == "MX");
            target.DeleteRecord(record.Id);
            VerifyInitialRecords(target);
        }

        [TestMethod()]
        public void CreateAndDeleteRecordByNameTest()
        {
            var target = this.CreateTarget();
            CreateCNAMETest(target);

            target.DeleteRecord("jsakamoto.info", "pop3");
            VerifyInitialRecords(target);
        }

        [TestMethod()]
        public void CreateAndDeleteRecordByFQDNTest()
        {
            var target = this.CreateTarget();
            CreateCNAMETest(target);

            target.DeleteRecord("jsakamoto.info", "pop3.jsakamoto.info");
            VerifyInitialRecords(target);
        }

        [TestMethod()]
        public void CreateAndDeleteRecordByFQDNOnlyTest()
        {
            var target = this.CreateTarget();
            CreateCNAMETest(target);

            target.DeleteRecord("pop3.jsakamoto.info");
            VerifyInitialRecords(target);
        }

        private DozensRecord[] CreateCNAMETest(Dozens_Accessor target)
        {
            VerifyInitialRecords(target);
            target.CreateRecord("jsakamoto.info", "pop3", "CNAME", 10, "imap4.jsakamoto.info", 7200);

            var records = target.GetRecords("jsakamoto.info");
            records.Select(r => r.ToString())
                .ToArray()
                .Is(new[]{
                    @"{Id = \d+, Name = www\.jsakamoto\.info, Type = A, Content = 192\.168\.0\.101, Prio = 0, TTL = 7200}",
                    @"{Id = \d+, Name = pop3\.jsakamoto\.info, Type = CNAME, Content = imap4\.jsakamoto\.info, Prio = 10, TTL = 7200}"
                }, RegexIsMatch);

            return records;
        }
    }
}
