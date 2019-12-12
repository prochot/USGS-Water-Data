using System
using System.IO;
using System.Data;
using System.Net;
using System.Xml;
using System.Text;
using Microsoft.SqlServer.Server;
using System.Data.SqlTypes;
using System.Collections;

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

public partial class UserDefinedFunctions
{
    [Microsoft.SqlServer.Server.SqlFunction(
        FillRowMethodName = "FillRow",
        TableDefinition = 
            "SiteID int, " +
            "ParameterCode int, " +
            "Time datetimeoffset(7), " +
            "Value decimal(12,4), " +
            "Qualifier nchar(1)"
        )       
    ]
    //public static IEnumerable UtvfGetUSGSDataBySite()
    //{

    //}

    public static void FillRow(object Observation
        ,out SqlInt32 SiteId
        ,out SqlInt32 ParameterCode
        ,out DateTimeOffset Time
        ,out SqlDecimal Value
        ,out SqlChars Qualifier
        )
    {
        DataRow r = (DataRow)Observation;
        SiteId = new SqlInt32(Convert.ToInt32(r["SiteID"]));
        ParameterCode = new SqlInt32(Convert.ToInt32(r["ParameterCode"]));
        Time = new DateTimeOffset(Convert.ToDateTime(r["DateTimeOffset"]));
        Value = new SqlDecimal(Convert.ToDecimal(r["Value"]));
        Qualifier = new SqlChars(Convert.ToString(r["Qualifier"]));
    }
}
