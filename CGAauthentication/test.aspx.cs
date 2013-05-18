using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Net;
using System.Security.Cryptography;
using Google.GData.Apps;
using UserLogin;

public partial class test : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string returnUrl = Request.Url.AbsoluteUri;

        UserLogin.LoginCheck ul = new UserLogin.LoginCheck();

        if (ul.UserIsLogin)
        {
            ProcessResponse pr = new ProcessResponse();

            string a = pr.createSamlResponse(Util.HashUserName(ul.UserName).ToLower(), "0", "0", "0", "0");



            Response.Write("<script>alert('" + Util.GetUserNameInGoogleUser(ul.UserName) + "');</script>");
       }
        else
        {
            Response.Redirect("http://passport.cga.com.cn/login/loginto.aspx?returnurl=" + Server.UrlEncode(returnUrl).ToString());//参数中带有参数
        }
    }
}

