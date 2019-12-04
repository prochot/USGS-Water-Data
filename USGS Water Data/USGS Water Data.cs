using System.IO;
using System.Data;
using System.Net;
using System.Xml;
using System.Text;
using Microsoft.SqlServer.Server;
using System.Data.SqlTypes;

public partial class StoredProcedures
{
    [Microsoft.SqlServer.Server.SqlProcedure]
    public static void UspGetUSGSDataBySite(SqlInt32 site)
    {
        ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
        HttpWebRequest request =
            (HttpWebRequest)WebRequest.Create("https://waterservices.usgs.gov/nwis/iv/?format=waterml,2.0&indent=on&sites=" + site + "&siteStatus=all");

        request.Method = "GET";
        request.ContentLength = 0;
        request.ContentType = "application/xml";
        request.Accept = "application/xml";

        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
        {
            using (Stream receiveStream = response.GetResponseStream())
            {
                using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                {
                    string strContent = readStream.ReadToEnd();
                    XmlDocument xdoc = new XmlDocument();
                    xdoc.LoadXml(strContent);
                    SqlDataRecord record = new SqlDataRecord(new SqlMetaData("USGS_XML", SqlDbType.Xml));
                    record.SetSqlXml(0, new SqlXml(new XmlNodeReader(xdoc)));
                    SqlContext.Pipe.Send(record);
                }
            }
        }
    }
}
