using System;
using System.Net;
using Newtonsoft.Json;
using System.IO;

namespace ForgottenArts.RestClient
{
	public class RestClient
	{
		WebClientEx wc = new WebClientEx ();
		string server;

		public string Server {
			get {
				return this.server;
			}
			set {
				server = value;
			}
		}
		
		public RestClient (string server)
		{
			this.server = server;
			wc.Headers.Add ("Content-Type", "application/json");
		}
		
		public void AddHeader (string name, string val)
		{
			wc.Headers.Add (name, val);
		}
		
		public DynamicDictionary Get (string endpoint)
		{
			JsonSerializer ser = new JsonSerializer ();
			return ser.Deserialize<DynamicDictionary> (new JsonTextReader (new StreamReader (wc.OpenRead (server + endpoint))));
		}
		
		public dynamic Post (string endpoint, object postData)
		{
			string jsonPost =  JsonConvert.SerializeObject (postData);
			//Console.WriteLine ("Posting {0} to {1}", jsonPost, server + endpoint);
			string resp = null;
			try
			{				
				resp = wc.UploadString (server + endpoint, "POST", jsonPost);
			}
			catch (WebException we)
			{
				StreamReader sr = new StreamReader (we.Response.GetResponseStream ());
				Console.Error.WriteLine ( sr.ReadToEnd ());
				throw new Exception ("Error in POST", we);
			}
			return JsonConvert.DeserializeObject<DynamicDictionary>(resp);
		}
		
		public DynamicDictionary Put (string endpoint, object postData)
		{
			string resp = wc.UploadString (server + endpoint, "PUT", JsonConvert.SerializeObject (postData));
			return JsonConvert.DeserializeObject<DynamicDictionary>(resp);
		}
		
		public DynamicDictionary Delete (string endpoint)
		{
			var req= wc.GetRequest (server + endpoint);
			req.Method = "DELETE";
			var resp = req.GetResponse ();
			JsonSerializer ser = new JsonSerializer ();
			return ser.Deserialize<DynamicDictionary> (new JsonTextReader (new StreamReader (resp.GetResponseStream ())));
		}
	}
	
	class WebClientEx : WebClient
    {
        public static CookieContainer CookieContainer { get; private set; }

        public WebClientEx()
        {
            CookieContainer = new CookieContainer();
			this.UseDefaultCredentials = true;
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = base.GetWebRequest(address);
            if (request is HttpWebRequest)
            {
                (request as HttpWebRequest).CookieContainer = CookieContainer;
            }
            return request;
        }
		
		public WebRequest GetRequest (string address)
		{
			return GetWebRequest (new Uri (address));
		}
    }
}

