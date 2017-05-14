using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace Baibaomen.SsoServer
{
    /// <summary>
    ////1.www.baidu.com,代理检测到没登录，跳转到本页面，action=verifylogin&loginpage=https://login.baidu.com；
    ////2.1verifylogin检测到已登录，把cookie中的usertoken带做参数跳转到https://login.baidu.com/?baibaomensso=123456；
    ////2.2代理检测到百度登录页面即将输出，拦截后后台用webclient根据baibaomensso访问本页action=getuserinfo&baibaomen=123456，后台将返回其用户在百度的账号、密码。代理获取后，改造登录页面，在输入框中填入账号密码，隐藏相关控件后自动提交登录，从而完成到百度的自动登录。
    ////3.1verifylogin检测到未登录，跳转到登录页面，url带上登录成功后的跳转站点。
    ////3.2登录页面提交到login，完成登录，写sso cookie，把cookie中的usertoken带做参数跳转到https://login.baidu.com/?baibaomensso=123456.
    /// </summary>
    public class SsoManager
    {
        public void Execute(HttpContext context) {
            context.Response.Clear();
            context.Response.ContentType = "text/plain";
            context.Response.ContentEncoding = System.Text.Encoding.UTF8;

            var rawUrl = new Uri(context.Request.Url, context.Request.RawUrl);

            switch (rawUrl.LocalPath.ToLower())
            {
                case "/":
                    context.Response.Redirect("login.html");
                    break;
                case "/login":
                    HandleLogin(context);
                    break;
                case "/sso":
                    HandleSso(context);
                    break;
                case "/logout":
                    HandleLogout(context);
                    break;
                case "/delete-session":
                    HandleDeleteSession(context);
                    break;
                case "/validate":
                    HandleValidate(context);
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }

        private void HandleDeleteSession(HttpContext context)
        {
            context.Response.SetCookie(new HttpCookie("baibaomensso") { Expires = DateTime.Now.AddYears(-10) });
            context.Response.SetCookie(new HttpCookie("username") { Expires = DateTime.Now.AddYears(-10) });
        }

        private void HandleValidate(HttpContext context)
        {
            var theToken = Util.DecryptTokenForValidationRequest(context.Request);

            UserInfo theAccount = null;

            if (!string.IsNullOrEmpty(theToken))
            {
                theAccount = Util.FindAccountForToken(theToken);
            }
            else
            {
                //todo:要检查账号所在域名是否正确。
                string accountStr = null;
                if (context.User != null && context.User.Identity != null)
                {
                    accountStr = context.User.Identity.Name.Split('\\')[1].ToLower();
                }

                if (!string.IsNullOrEmpty(accountStr))
                {
                    theAccount = SampleDB.FindAccount(accountStr);
                }
            }

            if (theAccount != null) {
                //13. 验证通过。
                //14. 告知token有效，并附带允许站点获取的用户信息。
                //32. 同13。
                //33. 同14。

                //todo:实际场景中，应该根据请求的server-key，只传递该server能看到的用户信息。
                context.Response.Write(Newtonsoft.Json.JsonConvert.SerializeObject(theAccount));
            }
            context.Response.End();
        }

        /// <summary>
        /// 注销登录。
        /// </summary>
        /// <param name="context"></param>
        private void HandleLogout(HttpContext context)
        {
            context.Response.Redirect("/login.html?action=logout",true);
        }

        /// <summary>
        /// 子站点通过浏览器跳转过来的SSO请求。
        /// </summary>
        /// <param name="context"></param>
        public void HandleSso(HttpContext context)
        {
            string token = null;

            if (context.Request.Cookies["baibaomensso"] != null)
            {
                token = context.Request.Cookies["baibaomensso"].Value;
            }

            if (!string.IsNullOrEmpty(token) && Util.FindAccountForToken(token) != null)//28 有token，且找到匹配用户。
            {
                //29 跳转到returnurl，并带上token参数。
                context.Response.Redirect(MakeReturnUrl(context.Request["returnurl"], token), true);
            }
            else//5 浏览器没传过来baibaomensso这个cookie，判定为未登录
            {
                //6 跳转到登录页面，带上登录成功后的返回地址
                context.Response.Redirect("login.html?returnurl=" + HttpUtility.UrlEncode(context.Request.QueryString["returnurl"]), true);
            }
        }

        /// <summary>
        /// 登录页面提交登录请求。
        /// </summary>
        /// <param name="context"></param>
        public void HandleLogin(HttpContext context)
        {
            var account = SampleDB.FindAccount(context.Request["account"], Util.ComputePasswordHash(context.Request["pwd"], context.Request["account"]));

            if (account != null)
            {
                //8 登录成功，创建用户账号对应的token xxx
                var token = Util.GetTokenForAccount(account);

                //9 把token写到本站cookie；
                context.Response.SetCookie(new HttpCookie("baibaomensso", token));

                //这个cookie和sso流程无关，是方便SSO的login.html前端页面显示用户名用的。
                context.Response.SetCookie(new HttpCookie("username", account.Name));

                //9 跳转到returnurl并带上token。此处只输出token，在前端页面回调中执行跳转。
                context.Response.Write(token);
                context.Response.End();
                //context.Response.Redirect(MakeReturnUrl(context.Request["returnurl"],token),true);
            }
            else
            {
                //不应该用401，不合理而且会导致浏览器弹出登录框：http://stackoverflow.com/questions/1959947/whats-an-appropriate-http-status-code-to-return-by-a-rest-api-service-for-a-val
                //context.Response.StatusCode = 401;
                context.Response.StatusCode = 422;
            }
        }

        public string MakeReturnUrl(string returnUrlQuery, string token) {
            var decoded = HttpUtility.UrlDecode(returnUrlQuery);
            var toReturn = decoded.Contains('?') ? string.Format("{0}&baibaomensso={1}",decoded,token) : string.Format("{0}?baibaomensso={1}",decoded,token);
            return toReturn;
        }
    }
}