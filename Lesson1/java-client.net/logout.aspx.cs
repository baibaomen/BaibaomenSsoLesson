using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class logout : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Session.Clear();
        Response.Write("<script>if(window.top !== window && window.top.postMessage){window.top.postMessage('logout:" + Request.Url.Host + "','*');}else{window.location='http://sso.baibaomen.com/logout';}</script>");
    }
}