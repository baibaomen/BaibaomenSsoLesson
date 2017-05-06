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
    const string ValidationUrl = "https://sso.baibaomen.com/validate?server-key=key1&baibaomensso=";

    protected void Page_Load(object sender, EventArgs e)
    {
        var baibaomenSso = Request.QueryString["baibaomensso"];

        //12 检测到有传递token参数，服务端访问SSO服务进行验证
        if (!string.IsNullOrEmpty(baibaomenSso))
        {
            WebClient c = new WebClient();
            c.Encoding = System.Text.Encoding.UTF8;

            var userInfoStr = c.DownloadString(ValidationUrl + baibaomenSso);

            if (string.IsNullOrEmpty(userInfoStr))//非法凭据，终止页面输出。
            {
                Response.End();
                return;
            }

            //14. token有效，SSO服务传过来了授权本站访问的用户信息。
            var accountStart = userInfoStr.IndexOf(@"""JavaSiteAccount"":""") + @"""JavaSiteAccount"":""".Length;
            var accountEnd = userInfoStr.IndexOf("\"", accountStart);

            //15. 根据token及SSO服务返回的用户信息，创建用户在本站的会话。
            Session["Account"] = userInfoStr.Substring(accountStart, accountEnd - accountStart);

            //带token参数的请求地址，替换为不带的。
            var dest = Request.RawUrl.Replace("baibaomensso=", "").Replace(baibaomenSso, "");
            dest = dest[dest.Length - 1] == '?' ? dest.Substring(0, dest.Length - 1) : dest;

            //16. 跳转到不带token的地址。
            Response.Redirect(dest, true);
        }

        //2. 用户尚无本站会话，判定为未登录
        if (Session["Account"] == null)
        {

            //3. 跳转到SSO服务做认证
            Response.Redirect(SsoUrl + HttpUtility.UrlEncode(Context.Request.Url.AbsoluteUri), true);
        }

        //22. 有Session，判定为已登录
        //23. 返回页面。
        lbHello.InnerText = "你好，" + Session["Account"];
    }
}