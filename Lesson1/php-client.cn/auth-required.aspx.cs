using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class auth_required : System.Web.UI.Page
{
    const string SsoUrl = "https://sso.baibaomen.com/sso?returnurl=";
    const string ValidationUrl = "https://sso.baibaomen.com/validate?server-key=key2&baibaomensso=";

    protected void Page_Load(object sender, EventArgs e)
    {
        var baibaomenSso = Request.QueryString["baibaomensso"];
        
        if (!string.IsNullOrEmpty(baibaomenSso))
        {
            //31. 检查到有token参数，访问SSO服务进行验证。
            WebClient c = new WebClient();
            c.Encoding = System.Text.Encoding.UTF8;

            var userInfoStr = c.DownloadString(ValidationUrl + baibaomenSso);

            if (string.IsNullOrEmpty(userInfoStr))
            {
                Response.End();
                return;
            }
            
            var accountStart = userInfoStr.IndexOf(@"""PhpSiteAccount"":""") + @"""PhpSiteAccount"":""".Length;
            var accountEnd = userInfoStr.IndexOf("\"", accountStart);
            
            //34. 根据token及SSO服务返回的用户其它信息，创建用户在本站的会话。
            Session["Account"] = userInfoStr.Substring(accountStart, accountEnd - accountStart);
            
            var dest = Request.RawUrl.Replace("baibaomensso=", "").Replace(baibaomenSso, "");
            dest = dest[dest.Length - 1] == '?' ? dest.Substring(0, dest.Length - 1) : dest;
            
            //35. 跳转到不带token参数的url。因为创建了Session，服务器也会自动请求浏览器创建sessionid对应cookie。
            Response.Redirect(dest, true);
        }

        //25. 用户尚无本站会话，判定为未登录
        if (Session["Account"] == null)
        {
            //26. 请求跳转到SSO获取用户token
            Response.Redirect(SsoUrl + HttpUtility.UrlEncode(Context.Request.Url.AbsoluteUri), true);
        }

        //38. 有session，判定用户已登录
        //39. 输出页面。
        lbHello.InnerText = "你好，" + Session["Account"];
    }
}