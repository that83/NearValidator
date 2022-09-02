// See https://aka.ms/new-console-template for more information
using System.Net;
using System.IO;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
//using MySql.Data.MySqlClient;
using System.Data;
using System.Data.SqlClient;
using System;

using System.Web;

string API_KEY = "9d34a558-b1e1-4978-81c9-f4e1997f5585";

//var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://209.145.60.40:3030");
var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://rpc.mainnet.near.org");
httpWebRequest.ContentType = "application/json";
httpWebRequest.Method = "POST";
//string strConnection = "Data Source=.\\sqlexpress;Database=master;Integrated Security=True";

SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
builder.DataSource = "servertoni.database.windows.net";
builder.UserID = "chartio";
builder.Password = "Forever6";
builder.InitialCatalog = "near";
SqlConnection connection = new SqlConnection(builder.ConnectionString);

//get Near price
var URL = new UriBuilder("https://pro-api.coinmarketcap.com/v1/cryptocurrency/quotes/latest");
var queryString = HttpUtility.ParseQueryString(string.Empty);
queryString["symbol"] = "NEAR";
URL.Query = queryString.ToString();

var client = new WebClient();
client.Headers.Add("X-CMC_PRO_API_KEY", API_KEY);
client.Headers.Add("Accepts", "application/json");
string strResult = client.DownloadString(URL.ToString());
JsonPrice resPrice = JsonConvert.DeserializeObject<JsonPrice>(strResult);
//JObject jObject = JsonConvert.DeserializeObject<JObject>(res.result.current_validators.ToString());
string strPrice = resPrice.data.NEAR.quote.USD.price.ToString();
Decimal decPrice = Decimal.Parse(strPrice);

//get validators
//var connection = new SqlConnection(strConnection);
using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
{
    string json = "{\"jsonrpc\":\"2.0\"," +
                  "\"method\":\"validators\"," +
                  "\"id\":\"dontcare\"," +
                  "\"params\":[null]}";


    streamWriter.Write(json);
    streamWriter.Flush();
    streamWriter.Close();
}

var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
{
    var result = streamReader.ReadToEnd();
    streamReader.Close();
    //Console.WriteLine(result);
    //Debug.WriteLine(result);
    using (StreamWriter outputFile = new StreamWriter("dataReady.json"))
    {
        outputFile.WriteLine(result);
    }
    JsonResult res = JsonConvert.DeserializeObject<JsonResult>(result);
    //JObject jObject = JsonConvert.DeserializeObject<JObject>(res.result.current_validators.ToString());
    JArray jArray = JsonConvert.DeserializeObject<JArray>(res.result.current_validators.ToString());


    DateTime dateNow = System.DateTime.Now;

    connection.Open();
    foreach (JObject item in jArray)
    {
        var sql = "INSERT INTO validator(date, account_id, stake, near_price, num_expected_blocks, num_expected_chunks, num_produced_blocks, num_produced_chunks) VALUES(@date, @account_id, @stake, @near_price, @num_expected_blocks, @num_expected_chunks, @num_produced_blocks, @num_produced_chunks)";
        using (var cmd = new SqlCommand(sql, connection))
        {
            cmd.Parameters.AddWithValue("@date", dateNow);
            cmd.Parameters.AddWithValue("@account_id", item.GetValue("account_id").ToString());

            //yotto Near to Near
            string strStake = item.GetValue("stake").ToString();
            strStake = strStake.Remove(strStake.Length - 24);

            cmd.Parameters.Add("@stake", SqlDbType.Decimal);
            cmd.Parameters["@stake"].Value = Decimal.Parse(strStake);

            cmd.Parameters.AddWithValue("@near_price", decPrice);

            cmd.Parameters.Add("@num_expected_blocks", SqlDbType.Int);
            cmd.Parameters["@num_expected_blocks"].Value = Int32.Parse(item.GetValue("num_expected_blocks").ToString());

            cmd.Parameters.Add("@num_expected_chunks", SqlDbType.Int);
            cmd.Parameters["@num_expected_chunks"].Value = Int32.Parse(item.GetValue("num_expected_chunks").ToString());

            cmd.Parameters.Add("@num_produced_blocks", SqlDbType.Int);
            cmd.Parameters["@num_produced_blocks"].Value = Int32.Parse(item.GetValue("num_produced_blocks").ToString());

            cmd.Parameters.Add("@num_produced_chunks", SqlDbType.Int);
            cmd.Parameters["@num_produced_chunks"].Value = Int32.Parse(item.GetValue("num_produced_chunks").ToString());

            cmd.ExecuteNonQuery();
        }
 
    }
}
public class JsonPrice
{
    public data data { get; set; }
    //public result result { get; set; }
}

public class data
{
    //public current_fishermen current_fishermen { get; set; }
    public NEAR NEAR { get; set; }
}

public class NEAR
{
    //public current_fishermen current_fishermen { get; set; }
    public quote quote { get; set; }
}

public class quote
{
    //public current_fishermen current_fishermen { get; set; }
    public USD USD { get; set; }
}

public class USD
{
    public string price { get; set; }
}

public class JsonResult
{
    public string jsonrpc { get; set; }
    public result result { get; set; }
}

public class result
{
    //public current_fishermen current_fishermen { get; set; }
    public object current_fishermen { get; set; }
    public object current_proposals { get; set; }
    public object current_validators { get; set; }
}

public class current_fishermen
{
    public string account_id { get; set; }
    public string public_key { get; set; }
    public string stake { get; set; }
    public string validator_stake_struct_version { get; set; }
}
public class current_proposals
{
    public string account_id { get; set; }
    public string public_key { get; set; }
    public string stake { get; set; }
    public string validator_stake_struct_version { get; set; }
}
public class current_validators
{
    public string account_id { get; set; }
    public string public_key { get; set; }
    public string stake { get; set; }

}
