using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using UserLogin;

public partial class _Default : System.Web.UI.Page 
{
    UserLogin.LoginCheck login = new UserLogin.LoginCheck();

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!Page.IsPostBack)
        {
            string returnUrl = Request.Url.AbsoluteUri;

            if (!login.UserIsLogin)
            {
                Response.Redirect("http://passport.cga.com.cn/login/loginto.aspx?returnurl=" + Server.UrlEncode(returnUrl).ToString());//参数中带有参数
            }
            else
            {
                try
                {
                    ProcessResponse res = new ProcessResponse();

                    res.doPost(Request);
                }

                catch (Exception ex)
                {
                    if (ex.Message == "正在中止线程。")
                    {
                        return;
                    }
                    else if (ex.Message == "Thread was being aborted.")
                    {
                        return;
                    }
                    else
                    {
                        Util.LogsError(login.UserName, ex.Message);

                        Response.Write(ex.Message);
                    }
                }
            }
        }
    }
}
