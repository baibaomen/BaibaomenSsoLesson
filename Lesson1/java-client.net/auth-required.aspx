<%@ Page Language="C#" AutoEventWireup="true" CodeFile="auth-required.aspx.cs" Inherits="auth_required" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1, maximum-scale=1, user-scalable=no"/>
    <title>页面一|SSO演示客户端Java站点</title>
    <link href="main.css" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server">
        <div class="header">
            <label id="lbHello" runat="server"></label>
            <a href="logout.aspx">注销</a>
        </div>
        <div class="content">
            这是java-client.net的页面1
        </div>
        <div class="footer">@2017 百宝门  baibaomen@gmail.com</div>
    </form>
</body>
</html>