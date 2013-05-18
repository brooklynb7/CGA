using System;
using System.Data;
using System.Configuration;
using System.Xml;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Security.Permissions;
using System.Web;
using System.Security.Cryptography.X509Certificates;


/// <summary>
/// User RSA to digital sign the SamlResponse
/// </summary>
public class XmlDigitalSigner
{
    /// <summary>
    /// Sign the XML String of SamlResponse
    /// </summary>
    /// <param name="xmlString"></param>
    /// <returns>Digital signed SamlReponse string</returns>
    public static String signSamlElement(String xmlString)
    {
        // Create a new CspParameters object to specify a key container.
         CspParameters cspParams = new CspParameters();

         cspParams.Flags = CspProviderFlags.UseMachineKeyStore;
     //    cspParams.KeyContainerName = "GOOGLE";
    //     cspParams.ProviderName = "HF";

         string pb = "<RSAKeyValue><Modulus>wTI341fDKEG9mV9VDFRj/XKf5nZxfadISavENRbwPlKZBipYAi6zgVNPJ7nhSH4qdXqphOreXFFmwsg8JzxHLJRJ8yjIfiG3ORuRaHO0dpTslSQ4wz5qVroj4avI3m5pL6jFgtaWJkWlr7uzq4xrdKwu+wZiOaNNCFjqUo18ycE=</Modulus><Exponent>AQAB</Exponent><P>86FAPMQJey3PrI+PBPnMgn8xzR3qy/WBjUihKn+Fb9tP7GipWD9oi3tkdR/KZfBcvSoacDQqxMg8Y+aY90glGQ==</P><Q>ywFrg+GccDYsFwOZJsgzC8FXBQf9jalFbfuRdjrrH0Cd2JqHXC/nrpc7YB3qOORaWSuxWorGdN3+o42qszX26Q==</Q><DP>L3udDnrSsjxCfopYQIsDDegGZ8jN60SFJGkkaCkEc8GVuSjI4JczJAQ/lwhEJUwMdx3Om1G/iCzSgFIAPCnGeQ==</DP><DQ>HX0nUREE2IgF/5HWPXv3bk23hlOS0XE1VLSmfLYyUWfhhgVshEexL/tn9J5j17/UH//o0241ReS5iKibk0zTgQ==</DQ><InverseQ>IC++K/C2NT5w01BYp5dcB1sXmWH32oFB1bmgcAkwK2VbQm9a9Xt1YdXtMVUEkxln7Inciny8oEfwdDiUjc82KQ==</InverseQ><D>byvGnTvTQUcTIz6IYh/tqdpbyPI/PF8Wac49iY85j6NYCwQywI6/HJwj4GhGCsEPDasYATRl4Bm3WD6A3tMA4NUw/RYfdutL2vDXjYXZMETWnABeeTdPK9haPw/NrcvhWRkGqNyeHG1soqmrF/x/0Xh5EYTv4KtrIJrPpKFajAE=</D></RSAKeyValue>";
        // Create a new RSA signing key and save it in the container.  
         RSACryptoServiceProvider rsaKey = new RSACryptoServiceProvider();

        
            /*  RSACryptoServiceProvider rsaKey;
         if (System.Web.HttpContext.Current == null) // WinForm
             rsaKey = new RSACryptoServiceProvider();
         else // WebForm - Uses Machine store for keys
             rsaKey = new RSACryptoServiceProvider(cspParams);
        */
        
         rsaKey.FromXmlString(pb);

         
      /*  X509Certificate2 cert = new X509Certificate2(@"C:\certificate.pfx", "");
        RSACryptoServiceProvider rsaKey = cert.PrivateKey as RSACryptoServiceProvider;*/


       // Create a new XML document
        XmlDocument xmlDoc = new XmlDocument();

        // Load an XML String into the XmlDocument object
        xmlDoc = Util.createXmlDom(xmlString);
     
        // Sign the XML document
        SignXml(xmlDoc, rsaKey);
        
        // convert XmlDocument to String
   /*     MemoryStream stream = new MemoryStream();

        XmlTextWriter writer = new XmlTextWriter(stream, null);

        writer.Formatting = Formatting.Indented;

        xmlDoc.Save(writer);

        StreamReader sr = new StreamReader(stream, System.Text.Encoding.UTF8);

        stream.Position = 0;

        string res = sr.ReadToEnd();

        sr.Close();

        stream.Close();

        return res;
    * */

        return xmlDoc.OuterXml;
    }

    // Sign an XML file.This document cannot be verified unless the verifying code has the key with which it was signed.
    public static void SignXml(XmlDocument doc, RSA key)
    {
        // Create a SignedXml object
        SignedXml signedXml = new SignedXml(doc);

        // Add the key to the SignedXml document
        signedXml.KeyInfo = new KeyInfo();
        signedXml.KeyInfo.AddClause(new RSAKeyValue(key));
        signedXml.SigningKey = key;

        // Create a reference to be signed
        Reference reference = new Reference("");

       // reference.Uri = "";

        // Add an enveloped transformation to the reference
        XmlDsigEnvelopedSignatureTransform env = new XmlDsigEnvelopedSignatureTransform();

        reference.AddTransform(env);

        // Set the KeyInfo to the SignedXml object
   //     KeyInfo ki = new KeyInfo();

    //    ki.AddClause(new RSAKeyValue(key));

     //   signedXml.SigningKey = key;
    //    signedXml.KeyInfo = ki;

        // Add the reference to the SignedXml object
        signedXml.AddReference(reference);

        // Compute the signature
        signedXml.ComputeSignature();

        // Get the XML representation of the signature and save it to an XmlElement object.
     //   XmlElement xmlDigitalSignature = signedXml.GetXml();

        // Append the element to the XML document
      //  doc.DocumentElement.AppendChild(doc.ImportNode(xmlDigitalSignature, true));
        doc.DocumentElement.PrependChild(signedXml.GetXml());
    }
}
