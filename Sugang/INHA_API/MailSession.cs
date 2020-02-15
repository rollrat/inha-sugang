/***

   Copyright (C) 2020. rollrat. All Rights Reserved.
   
   Author: Jeong HyunJun

***/

using Sugang.INHA_API.Crpyto;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Sugang.INHA_API
{
    public class MailSession
    {
        public CookieCollection Cookies { get; private set; }

        public static MailSession Create(string id, string pwd)
        {
            var wc = new WebClient();
            var pem = wc.DownloadString("http://mail.inha.ac.kr/resources/getKey");

            var rsa = RSAHelper.GetRSAProviderFromPemFile("-----BEGIN PUBLIC KEY-----\n" + pem + "\n-----END PUBLIC KEY-----");

            var username = Convert.ToBase64String(rsa.Encrypt(Encoding.ASCII.GetBytes(id), false));
            var password = Convert.ToBase64String(rsa.Encrypt(Encoding.ASCII.GetBytes(pwd), false));

            var param = new Dictionary<string, string>();
            param.Add("username", username);
            param.Add("password", password);

            var request = (HttpWebRequest)WebRequest.Create("https://mail.inha.ac.kr/customize/login.jsp");

            request.Method = "POST";
            request.Headers.Add("Origin", "https://mail.inha.ac.kr");
            request.Referer = "https://mail.inha.ac.kr/login";
            request.ContentType = "application/json";
            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0";
            request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate, br");
            request.Headers.Add(HttpRequestHeader.AcceptLanguage, "ko,en-US;q=0.7,en;q=0.3");
            request.Host = "mail.inha.ac.kr";
            request.CookieContainer = new CookieContainer();
            request.AllowAutoRedirect = false;

            var request_stream = new StreamWriter(request.GetRequestStream());
            var query = "{" + string.Join(",", param.ToList().Select(x => $"\"{x.Key}\":\"{x.Value}\"")) + "}";
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

                if (cookies.Count == 0 || cookies[0].Name != "GOSSOcookie")
                    return null;

                var ms = new MailSession { Cookies = cookies };

                return ms;
            }
        }
    }
}
