using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace Baibaomen.SsoServer
{
    public class UserInfo
    {
        public string Id { get; set; }

        public string Account { get; set; }

        public string PwdHash { get; set; }

        public string Name { get; set; }

        public string JavaSiteAccount { get; set; }

        public string PhpSiteAccount { get; set; }

        public UserInfo() { }

        public UserInfo(DataRow data) {
            Id = data["Id"].ToString();
            Account = data["Account"].ToString();
            PwdHash = data["PwdHash"].ToString();
            Name = data["Name"].ToString();
            JavaSiteAccount = data["JavaSiteAccount"].ToString();
            PhpSiteAccount = data["PhpSiteAccount"].ToString();
        }
    }
}