/***

   Copyright (C) 2020. rollrat. All Rights Reserved.
   
   Author: Jeong HyunJun

***/

using HtmlAgilityPack;
using Sugang.INHA_API.Crpyto;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Sugang.INHA_API
{
    public class SugangSession
    {
        public static SugangSession ErrorSession = new SugangSession();

        public CookieCollection Cookies { get; private set; }

        public bool IsValidITISSugangCookie { get; private set; }

        #region Login

        /// <summary>
        /// Login to sugang.inha.ac.kr
        /// </summary>
        /// <param name="id"></param>
        /// <param name="pwd"></param>
        /// <returns></returns>
        public static SugangSession Create(string id, string pwd)
        {
            //
            //  Reuqest RSA Public Key
            //

            string public_key;
            using (var client = new WebClient())
                public_key = client.DownloadString("https://sugang.inha.ac.kr/ITISWebCommon/xml/PublicKey.xml");

            var rsa = new RSACryptoServiceProvider();
            RSAKeyExtensions.FromXmlString(rsa, public_key);

            var ide = encrpyt(rsa, id);
            var pwde = encrpyt(rsa, pwd);

            //
            //  Get Parameters from Menu.aspx
            //

            string menu_aspx;
            using (var client = new WebClient())
                menu_aspx = client.DownloadString("https://sugang.inha.ac.kr/sugang/Menu.aspx?login=no");

            var document = new HtmlDocument();
            document.LoadHtml(menu_aspx);
            var root_node = document.DocumentNode;
            var form = root_node.SelectSingleNode("//form[@name='form1']");

            var param = new Dictionary<string, string>();
            foreach (var input in form.SelectNodes(".//input"))
                param.Add(input.GetAttributeValue("name", ""), input.GetAttributeValue("value", ""));
            param["hhdencId"] = ide;
            param["hhdencPw"] = pwde;

            param.Add("__EVENTTARGET", "ibtnLogin");
            param.Add("__EVENTARGUMENT", "");

            //
            //  Request Session Cookie
            //

            var request = (HttpWebRequest)WebRequest.Create("https://sugang.inha.ac.kr/sugang/Menu.aspx?login=no");
            pass_common(ref request);

            request.Method = "POST";
            request.Headers.Add("Origin", "https://sugang.inha.ac.kr");
            request.Headers.Add(HttpRequestHeader.Cookie, "this.GetLangCode=");

            var xx = request.Headers.ToString();

            var request_stream = new StreamWriter(request.GetRequestStream());
            var query = string.Join("&", param.ToList().Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value)}"));
            request_stream.Write(query);
            request_stream.Close();

            //
            //  Create Session
            //

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                var cookies = response.Cookies;

                // Fake response
                var res = new StreamReader(response.GetResponseStream()).ReadToEnd();

                if (cookies.Count == 0 || cookies[0].Name != "ITISSugangHome")
                    return ErrorSession;

                var ss =  new SugangSession { Cookies = cookies };

                try
                {
                    ss.create_itissugang_session();
                    ss.IsValidITISSugangCookie = true;
                }
                catch { }

                return ss;
            }
        }

        private static void pass_common(ref HttpWebRequest request)
        {
            request.Referer = "https://sugang.inha.ac.kr/sugang/Menu.aspx";
            request.ContentType = "application/x-www-form-urlencoded";
            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0";
            request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate, br");
            request.Headers.Add(HttpRequestHeader.AcceptLanguage, "ko,en-US;q=0.7,en;q=0.3");
            request.Headers.Add("Upgrade-Insecure-Requests", "1");
            request.Host = "sugang.inha.ac.kr";
            request.CookieContainer = new CookieContainer();
            request.AllowAutoRedirect = false;
        }

        private void create_itissugang_session()
        {
            //
            //  Create ITISSugang Session
            //

            var request = (HttpWebRequest)WebRequest.Create("https://sugang.inha.ac.kr/sugang/SU_53005/Sugang_Save.aspx");
            pass_common(ref request);

            request.Method = "GET";
            request.CookieContainer.Add(Cookies[0]);
            request.AllowAutoRedirect = false;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                var cookies = response.Cookies;
                Cookies.Add(cookies[0]);
            }
        }

        private static string encrpyt(RSACryptoServiceProvider rsa, string content)
        {
            var data = Uri.EscapeUriString(content);
            var split_size = 53;
            var rr = new List<string>();

            for (int i = 0; i <= data.Length / split_size; i++)
            {
                var dd = data.Substring(i * split_size);
                if (dd.Length > 53)
                    dd = dd.Remove(53);
                rr.Add(Convert.ToBase64String(rsa.Encrypt(Encoding.ASCII.GetBytes(dd), false)));
            }

            return string.Join("|", rr);
        }

        #endregion

        public HttpWebRequest CreateGetRequest(string url)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";

            request.CookieContainer = new CookieContainer();
            request.CookieContainer.Add(Cookies[0]);
            if (IsValidITISSugangCookie)
                request.CookieContainer.Add(Cookies[1]);

            return request;
        }
    }
}
