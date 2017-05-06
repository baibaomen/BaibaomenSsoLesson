using Baibaomen.SsoServer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

/// <summary>
/// 模拟用户账号数据库。
/// </summary>
public static class SampleDB
{
    static DataTable _DB;
    static object _DbLock = new object();

    public static DataTable DB {
        get {
            if (_DB == null) {
                lock (_DbLock) {
                    if (_DB == null)
                    {
                        var toReturn = new DataTable();
                        toReturn.Columns.AddRange(new string[] { "Id", "Account", "PwdHash", "Name", "JavaSiteAccount", "PhpSiteAccount" }.Select(x => new DataColumn(x)).ToArray());
                        toReturn.Rows.Add("1", "baibaomen", "123456|baibaomen|22E8ED90-E039-4020-9D2C-976975EC3CF6", "百宝门", "百宝门的Java账号", "PHP百宝门");

                        _DB = toReturn;
                    }
                }
            }

            return _DB;
        }
    }

    public static UserInfo FindAccount(string account, string pwdHash) {
        var matches = DB.Select(string.Format("Account='{0}' and PwdHash='{1}'",account,pwdHash));
        if (matches == null || matches.Length == 0) {
            return null;
        }

        return new UserInfo(matches[0]);
    }

    /// <summary>
    /// 用于AD验证已通过，只需匹配账号名称。
    /// </summary>
    /// <param name="account"></param>
    /// <returns></returns>
    public static UserInfo FindAccount(string account)
    {
        var matches = DB.Select(string.Format("Account='{0}'",account));
        if (matches == null || matches.Length == 0)
        {
            return null;
        }

        return new UserInfo(matches[0]);
    }
}