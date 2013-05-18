using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.IO;
using System.Text;
using System.Xml;
using System.Web.UI;
using System.Web.Security;
using System.Security.Cryptography;
using Com.Holdfast.DB;
using UserLogin;

/// <summary>
/// Methods that are used for the SAML transactions.
/// </summary>
public class Util
{
    private static Random random = new Random();
    private static char[] charMapping = {
        'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p'};

    /// <summary>
    /// Contents of XML file 
    /// </summary>
    /// <param name="filepath">specified path</param>
    /// <returns>return a string containing the contents of the file</returns>
    public static string readFileContents(string filepath)
    {
        StringBuilder xml = new StringBuilder();
    
        StreamReader sr = new StreamReader(filepath);

        string input = null;

        try
        {
            while ((input = sr.ReadLine()) != null)
            {
                xml.Append(input);
            }

            sr.Close();

            return xml.ToString();
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    /// <summary>
    /// Load an XML String into the XmlDocument object
    /// </summary>
    /// <param name="xmlString">XML String</param>
    /// <returns>XmlDocument Object</returns>
    public static XmlDocument createXmlDom(String xmlString)
    {
        XmlDocument dom = new XmlDocument();

        dom.LoadXml(xmlString);

        return dom;
    }

    /// <summary>
    ///  Create a randomly generated string conforming to the xsd:ID datatype.
    ///  containing 160 bits of non-cryptographically strong pseudo-randomness, 
    ///  as suggested by SAML 2.0 core 1.2.3. This will also apply to version 1.1
    /// </summary>
    /// <returns></returns>
    public static String createID()
    {
        byte[] bytes = new byte[20];// = 160 bits

        StringBuilder res = new StringBuilder();

        random.NextBytes(bytes);

        char[] chars = new char[40];

        for (int i = 0; i < bytes.Length; i++)
        {
            int left = (bytes[i] >> 4) & 0x0f; //

            int right = bytes[i] & 0x0f;

            chars[i * 2] = charMapping[left];

            chars[i * 2 + 1] = charMapping[right];
        }

        for (int j = 0; j < chars.Length; j++)
        {
            res.Append(chars[j]);
        }

        return res.ToString();
    }

    /// <summary>
    ///  Gets the current date and time in the format specified by xsd:dateTime in
    ///  UTC form, as described in SAML 2.0 core 1.2.2 This will also apply to Version 1.1
    /// </summary>
    /// <returns></returns>
    public static String getDateAndTime()
    {
        DateTime day = DateTime.Now;
        DateTime time = DateTime.Now;

        string dateformat = "yyyy-MM-ddTHH:mm:ssZ";

      //  return day.ToString(dateformat);
        return day.ToUniversalTime().ToString(dateformat);
      //  return day.ToShortDateString() + "T" + time.ToLongTimeString() + "Z";
    }

    public static String getDateAndTime(int num) 
    {
        DateTime day = DateTime.Now;
        DateTime time = DateTime.Now;

        string dateformat = "yyyy-MM-ddTHH:mm:ssZ";

        return day.ToUniversalTime().AddMinutes(num).ToString(dateformat);
       // return day.ToUniversalTime().ToString(dateformat);
        //  return day.ToShortDateString() + "T" + time.ToLongTimeString() + "Z";
    }

    /// <summary>
    /// NotOnOrAfter
    /// </summary>
    /// <param name="num"></param>
    /// <returns></returns>
    public static String getDateAndTime(string i,int num)
    {
        i = i.Replace("T"," ");
        i = i.Replace("Z"," ");

        System.DateTime datetime = System.Convert.ToDateTime(i);
        DateTime day = datetime;
        DateTime time = datetime;

        string dateformat = "yyyy-MM-ddTHH:mm:ssZ";

        return day.AddMinutes(num).ToString(dateformat);

//        return day.ToShortDateString() + "T" + time.AddMinutes(num).ToLongTimeString() + "Z";
     
    }

    /// <summary>
    /// show xml in html page
    /// </summary>
    /// <param name="xmlString"></param>
    /// <returns></returns>
    public static String htmlEncode(String xmlString)
    {
        StringBuilder encodedString = new StringBuilder();

        char[] chars = xmlString.ToCharArray();

        for (int i = 0; i < chars.Length; i++)
        {
            if (chars[i] == '<')
            {
                encodedString.Append("&lt;");
            }
            else if (chars[i] == '>')
            {
                encodedString.Append("&gt;");
            }
            else if (chars[i] == '"')
            {
                encodedString.Append("&quot;");
            }
            else
            {
                encodedString.Append(chars[i]);
            }
        }
        return encodedString.ToString();
    }


    /// <summary>
    /// Check the User Account
    /// </summary>
    /// <param name="username"></param>
    /// <returns></returns>
    public static Boolean UserAccountIsExist(string username)
    {
        
            using (DbAction db = new DbAction("connStr1"))
            {
                string sql = string.Format("select * from GoogleApp_Users_t where username='{0}'", username);

                SqlCommand cmd = new SqlCommand(sql);

                if (db.ExecuteScalar(cmd) != null)

                    return true;

                else

                    return false;
            }
        
  
    }

    /// <summary>
    /// add user to the GoogleApp
    /// </summary>
    /// <param name="username"></param>
    public static void AddUserAccount(string username, string email,string firstname,string lastname, string password)
    {
        using (DbAction db = new DbAction("connStr1"))
        {
            string sql = string.Format("insert into GoogleApp_Users_t(username,email,firstname,lastname,password,createtime) values('{0}','{1}','{2}','{3}','{4}',getdate())", username, email,firstname,lastname, password);

            SqlCommand cmd = new SqlCommand(sql);

            db.ExecuetNonQuery(cmd);
        }
    }


    /// <summary>
    /// distinguish capital
    /// </summary>
    /// <param name="username"></param>
    /// <param name="error"></param>
    public static String GetUserNameInGoogleUser(string username)
    {
        using (DbAction db = new DbAction("connStr1"))
        {
            string sql = string.Format("select username from GoogleApp_Users_t where username = '{0}'", username);

            SqlCommand cmd = new SqlCommand(sql);

            return db.ExecuteScalar(cmd).ToString();
        }
    }

    public static void LogsError(string username, string error)
    {
        using (DbAction db = new DbAction("connStr1"))
        {
            string sql = string.Format("insert into GoogleApp_Errors_t(username,error,adddate) values('{0}','{1}',getdate())",username,error);

            SqlCommand cmd = new SqlCommand(sql);

            db.ExecuetNonQuery(cmd);
        }
    }

    /// <summary>
    /// Holdfast provide the default password for the dasher account.
    /// </summary>
    /// <returns></returns>
    public static String getRandomPwd()
    {
        Random rand = new Random();

        return rand.Next(10000000, 99999999).ToString();
    }

    /// <summary>
    /// get the logined UserName
    /// </summary>
    /// <returns></returns>
    public static String getUserName()
    {
        UserLogin.LoginCheck login = new UserLogin.LoginCheck();

        return login.UserName;
    }

    /// <summary>
    /// HashedUserName
    /// </summary>
    /// <param name="username"></param>
    /// <returns></returns>
    public static String HashUserName(string username)
    {
        return FormsAuthentication.HashPasswordForStoringInConfigFile(username, "MD5");
    }

    public static String encodeMessage(string xmlString)
    {
        string encode = "";

        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(xmlString);

        try
        {
            System.IO.MemoryStream ms = new MemoryStream();

            System.IO.Compression.DeflateStream ds = new System.IO.Compression.DeflateStream(ms, System.IO.Compression.CompressionMode.Compress);

            ds.Write(bytes, 0, bytes.Length);

            ds.Close();

            encode = Convert.ToBase64String(ms.ToArray());

            Page urlEncode = new Page();

            encode = urlEncode.Server.UrlEncode(encode);
        }
        catch
        {
            encode = xmlString;
        }
        return encode;
    }
}
