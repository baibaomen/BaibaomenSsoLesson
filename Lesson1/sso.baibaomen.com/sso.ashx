<%@ WebHandler Language="C#" Class="sso" %>

using System;
using System.Web;

public class sso : IHttpHandler {

    public void ProcessRequest (HttpContext context) {
            new Baibaomen.SsoServer.SsoManager().Execute(context);
    }

    public bool IsReusable {
        get {
            return false;
        }
    }

}