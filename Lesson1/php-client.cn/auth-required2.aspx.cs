using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class auth_required2 : System.Web.UI.Page
{
    const string SsoUrl = "https://sso.baibaomen.com/sso?returnurl=";
    const string ValidationUrl = "https://sso.baibaomen.com/validate?server-key=key2&baibaomensso=";

    protected void Page_Load(object sender, EventArgs e)
    {
        var baibaomenSso = Request.QueryString["baibaomensso"];

        if (!string.IsNullOrEmpty(baibaomenSso))
        {
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
            
            Session["Account"] = userInfoStr.Substring(accountStart, accountEnd - accountStart);

            var dest = Request.RawUrl.Replace("baibaomensso=", "").Replace(baibaomenSso, "");
            dest = dest[dest.Length - 1] == '?' ? dest.Substring(0, dest.Length - 1) : dest;
            
            Response.Redirect(dest, true);
        }
        
        if (Session["Account"] == null)
        {
            Response.Redirect(SsoUrl + HttpUtility.UrlEncode(Context.Request.Url.AbsoluteUri), true);
        }
        
        //41. 根据cookie带过来的sessionid找到了用户Session（web服务器自动处理的），判定已登录
        //42. 输出页面内容。
        lbHello.InnerText = "你好，" + Session["Account"];
    }
}