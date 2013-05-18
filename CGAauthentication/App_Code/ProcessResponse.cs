using System;
using System.Data;
using System.Xml;
using System.Text;
using System.IO;
using System.Web.UI;
using System.Web;
using System.Net;
using System.Configuration;
using System.Security.Cryptography;
using Google.GData.Apps;
/// <summary>
/// Takes in a SAML AuthnRequest and verifies the user login credentials. Upon succesful
/// user login, it generates and signs the corresponding SAML Response, which is
/// then redirected to the specified Assertion Consumer Service.
/// </summary>
public class ProcessResponse : Page
{
    private string SamlResponseTemplateFile = "SamlResponseTemplate.xml";
  // private string SamlResponseTemplateFile = "AurthnRequestTemplate.xml";
    public static readonly string DateFormatter = "yyyy-MM-ddTHH:mm:ssZ";
    private System.Collections.Specialized.NameValueCollection Inputs = new System.Collections.Specialized.NameValueCollection();
    private string Url = "";
    private string Method = "post";
    private string FormName = "form1";

    /// <summary>
    /// Create Google Apps
    /// </summary>
    /// <param name="username"></param>
    private void CreateUserAccount(string username)
    {
        if (Util.UserAccountIsExist(username))
            return;
        else
        {
            string Email = Util.HashUserName(username).ToLower();

            string First = username + "@HF";

            string pwd = Util.getRandomPwd();

            Google.GData.Apps.AppsService service = new Google.GData.Apps.AppsService("cga.com.cn", "bzhang@cga.com.cn", "test2006");

            service.CreateUser(Email, First, "HoldFast", pwd);

            System.Threading.Thread.Sleep(3000);

           /* while (service.RetrieveUser(Email).Login.UserName != Email)
            {
                System.Threading.Thread.Sleep(500);
            }
            */


            Util.AddUserAccount(username, Email, First, "HoldFast", pwd);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name">saml response name</param>
    /// <param name="value">xml string</param>
    private void Add(String name, String value)
    {
        Inputs.Add(name, value);
    }

    /// <summary>
    ///  Retrieves the AuthnRequest from the encoded and compressed String extracted
    ///  from the URL. The AuthnRequest XML is retrieved in the following order: 
    ///  1. URL decode  2. Base64 decode  3. Inflate  
    ///  Returns the String format of the AuthnRequest XML.
    /// </summary>
    /// <param name="encodedRequestXmlString"></param>
    /// <returns></returns>
    public String decodeAuthnRequestXML(String encodedRequestXmlString)
    {
        //URL decode
        //   Page urlDecode = new Page();

        //   encodedRequestXmlString = urlDecode.Server.UrlDecode(encodedRequestXmlString);

        //Base64 decode
        String decode = "";

        byte[] bytes = Convert.FromBase64String(encodedRequestXmlString);

        //inflate decode
        System.IO.MemoryStream ms = new MemoryStream(bytes);

    //    ms.Position = 0;

        System.IO.Compression.DeflateStream ds = new System.IO.Compression.DeflateStream(ms, System.IO.Compression.CompressionMode.Decompress,false);

        byte[] bytes2 = new byte[20000];

        int c;

        int resultLength = 0;

        while ((c = ds.ReadByte()) >= 0)
        {
            if (resultLength >= 20000)
            {
                throw new Exception("didn't allocate enough space to hold decompressed data");
            }
            bytes2[resultLength++] = (byte)c;
        }

        //ds.Read(bytes2, 0, bytes.Length);

        //ds.Close();

        decode = System.Text.Encoding.UTF8.GetString(bytes2,0,resultLength);

        return decode;
    }

    
    /// <summary>
    /// Extracts the value under the "AssertionConsumerServiceURL" attribute
    /// </summary>
    /// <param name="xmlString"></param>
    /// <returns></returns>
    public String[] getAttributes(string xmlString)
    {
        XmlDocument dom = new XmlDocument();

        dom.LoadXml(xmlString);

        XmlNodeList samlp = dom.GetElementsByTagName("samlp:AuthnRequest");

        XmlNode node = samlp[0];

        String[] samlRequestAttributes = new String[4];

        samlRequestAttributes[0] = node.Attributes["IssueInstant"].Value;

        samlRequestAttributes[1] = node.Attributes["ProviderName"].Value;

        samlRequestAttributes[2] = node.Attributes["AssertionConsumerServiceURL"].Value;

        samlRequestAttributes[3] = node.Attributes["ID"].Value;

        return samlRequestAttributes;
    }

    /// <summary>
    /// Generates a SAML response XML
    /// </summary>
    /// <returns></returns>
    public String createSamlResponse(String authenticatedUser,String notBefore, String notOnOrAfter,String id,String acsurl)
    {
      //  string filepath = AppDomain.CurrentDomain.BaseDirectory + "xmlTemplate\\" + this.SamlResponseTemplateFile;

        string filepath = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><samlp:Response  xmlns:samlp=\"urn:oasis:names:tc:SAML:2.0:protocol\" xmlns=\"urn:oasis:names:tc:SAML:2.0:assertion\" xmlns:xenc=\"http://www.w3.org/2001/04/xmlenc#\"  ID=\"[RESPONSE_ID]\" IssueInstant=\"[ISSUE_INSTANT]\"  Version=\"2.0\"> <samlp:Status> <samlp:StatusCode Value=\"urn:oasis:names:tc:SAML:2.0:status:Success\"/>  </samlp:Status>  <Assertion ID=\"[ASSERTION_ID]\" IssueInstant=\"2003-04-17T00:46:02Z\" Version=\"2.0\"> <Issuer>https://www.opensaml.org/IDP</Issuer>  <Subject> <NameID Format=\"urn:oasis:names:tc:SAML:2.0:nameid-format:emailAddress\"> [USERNAME_STRING]  </NameID>   <SubjectConfirmation Method=\"urn:oasis:names:tc:SAML:2.0:cm:bearer\"> <SubjectConfirmationData Recipient=\"[AcsUrl]\"   NotOnOrAfter=\"[NotOnOrAfter1]\"  InResponseTo=\"[InResponseTo]\" /></SubjectConfirmation></Subject>  <Conditions NotBefore=\"[NOT_BEFORE]\" NotOnOrAfter=\"[NOT_ON_OR_AFTER]\">  </Conditions>  <AuthnStatement AuthnInstant=\"[AUTHN_INSTANT]\">   <AuthnContext>     <AuthnContextClassRef>  urn:oasis:names:tc:SAML:2.0:ac:classes:Password</AuthnContextClassRef></AuthnContext></AuthnStatement></Assertion></samlp:Response>";

     //   string samlresponse = Util.readFileContents(filepath);       

        string samlresponse = filepath;

        samlresponse = samlresponse.Replace("[USERNAME_STRING]", authenticatedUser);

        samlresponse = samlresponse.Replace("[RESPONSE_ID]", Util.createID());

        samlresponse = samlresponse.Replace("[ISSUE_INSTANT]",Util.getDateAndTime());

        samlresponse = samlresponse.Replace("[AUTHN_INSTANT]", Util.getDateAndTime());

        samlresponse = samlresponse.Replace("[NOT_BEFORE]", notBefore);

        samlresponse = samlresponse.Replace("[NOT_ON_OR_AFTER]", notOnOrAfter);

        samlresponse = samlresponse.Replace("[ASSERTION_ID]", Util.createID());

        samlresponse = samlresponse.Replace("[AcsUrl]", acsurl);

        samlresponse = samlresponse.Replace("[NotOnOrAfter1]",notOnOrAfter);

        samlresponse = samlresponse.Replace("[InResponseTo]",id);



        return samlresponse;
    }

   

    /// <summary>
    /// Signs the SAML response XML with the specified private key, and embeds with public key. 
    /// Uses helper class XmlDigitalSigner to digitally sign the XML.
    /// </summary>
    /// <param name="response"></param>
    /// <returns></returns>
    public String signResponse(String response)
    {
        return XmlDigitalSigner.signSamlElement(response);
    }

    /// <summary>
    /// return html code of post
    /// </summary>
    private void Post()
    {
        System.Web.HttpContext.Current.Response.Clear();

        System.Web.HttpContext.Current.Response.Write("<html><head><title>浩方验证</title>");

        System.Web.HttpContext.Current.Response.Write(string.Format("</head><body onload=\"document.{0}.submit()\">", FormName));

        System.Web.HttpContext.Current.Response.Write(string.Format("<form name=\"{0}\" method=\"{1}\" action=\"{2}\">", FormName, Method, Url));

        for (int i = 0; i < Inputs.Keys.Count; i++)
        {
            System.Web.HttpContext.Current.Response.Write(string.Format("<input name=\"{0}\" type=\"hidden\" value=\"{1}\">", Inputs.Keys[i], Inputs[Inputs.Keys[i]]));
        }

        System.Web.HttpContext.Current.Response.Write("</form></body></html>");

        //System.Web.HttpContext.Current.ApplicationInstance.CompleteRequest();
        System.Web.HttpContext.Current.Response.End();
    }

    private void Post1(string samlresponse,string relaystate)
    {
        System.Web.HttpContext.Current.Response.Clear();

        System.Web.HttpContext.Current.Response.Write("<html><head><title>浩方验证</title>");

        System.Web.HttpContext.Current.Response.Write(string.Format("</head><body><center><font color=\"red\">正在登录请稍等....</font></center>"));

        System.Web.HttpContext.Current.Response.Write(string.Format("<form name=\"acsForm\" method=\"{0}\" action=\"{1}\">", Method, Url));

        System.Web.HttpContext.Current.Response.Write(string.Format("<div style=\"display:none\"><textarea rows=10 cols=80 name=\"SAMLResponse\">{0}</textarea>",samlresponse));

        System.Web.HttpContext.Current.Response.Write(string.Format("<textarea rows=10 cols=80 name=\"RelayState\">{0}</textarea>",relaystate));

        System.Web.HttpContext.Current.Response.Write("</div></form></body></html>");

        System.Web.HttpContext.Current.Response.Write("<script language=\"javascript\">document.acsForm.submit();</script>");

        //System.Web.HttpContext.Current.ApplicationInstance.CompleteRequest();
        System.Web.HttpContext.Current.Response.End();
    }

    /// <summary>
    /// Post the response to the acs
    /// </summary>
    /// <param name="request"></param>
    /// <param name="response"></param>         
    public void doPost(HttpRequest request)
    {

        String samlRequest = request.QueryString["SAMLRequest"].ToString();

        String relayStateURL = request.QueryString["RelayState"].ToString();

        if (samlRequest != null)
        {
            String requestXmlString = decodeAuthnRequestXML(samlRequest);

            String[] samlRequestAttributes = getAttributes(requestXmlString);

            String issueInstant = samlRequestAttributes[0];

            String providerName = samlRequestAttributes[1];

            String acsURL = samlRequestAttributes[2];

            String userName = Util.getUserName();

            CreateUserAccount(userName);

            userName = Util.GetUserNameInGoogleUser(userName);
                        
            String notBefore = issueInstant;           

            String id = samlRequestAttributes[3];

          //  String notOnOrAfter = Util.getDateAndTime(notBefore,5);

            String notOnOrAfter = Util.getDateAndTime(5);

            String responseXmlString = createSamlResponse(Util.HashUserName(userName).ToLower(),notBefore, notOnOrAfter,id,acsURL);

            String signedSamlResponse = signResponse(responseXmlString);  

            Url = acsURL;

            Post1(signedSamlResponse, relayStateURL);
        }
    }
}
