using Baibaomen.SsoServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for Util
/// </summary>
public static class Util
{
    const string PasswordSalt = "22E8ED90-E039-4020-9D2C-976975EC3CF6";
    static Random rnd = new Random(DateTime.Now.GetHashCode());

    /// <summary>
    /// 计算密码哈希。
    /// </summary>
    /// <param name="rawPassword"></param>
    /// <returns></returns>
    public static string ComputePasswordHash(string rawPassword, string account) {
        //todo:真实场景中，应该将密码拼接上用户账号（避免数据库管理人员利用其它账号的密码哈希登录本账号），然后再拼接上一个salt（避免密码过于简单时，被字典枚举攻击）后算出哈希。
        //return $"{rawPassword}|{account}|{PasswordSalt}";
        return string.Format("{0}|{1}|{2}", rawPassword, account, PasswordSalt);
    }

    /// <summary>
    /// 根据用户账号计算出token。
    /// </summary>
    /// <param name="accountId"></param>
    /// <returns></returns>
    public static string GetTokenForAccount(UserInfo account) {
        // 真实场景中，该值应该是账号Id拼接时间戳后加密生成。
        return string.Format("{0}|{1}",account.Id,DateTime.Now.Ticks);
    }

    /// <summary>
    /// 根据token到数据库查找拼装用户信息。
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public static UserInfo FindAccountForToken(string token) {
        var accountId = token.Split('|')[0];
        return new UserInfo(SampleDB.DB.Select(string.Format("Id='{0}'",accountId))[0]);
    }

    /// <summary>
    /// 解密Validation请求并返回匹配的用户信息。
    /// </summary>
    /// <param name="requestUrl"></param>
    /// <returns></returns>
    public static string DecryptTokenForValidationRequest(HttpRequest request) {
        //真实场景中，应根据server-key找到对应的secret，用它解密出待验证的token。
        var token = request.QueryString["baibaomensso"];
        return token;
    }
}