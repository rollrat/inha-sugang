/***

   Copyright (C) 2020. rollrat. All Rights Reserved.
   
   Author: Jeong HyunJun

***/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Sugang.EVERYTIME_API
{
    public class EverytimeSession
    {
        public CookieCollection Cookies { get; private set; }

        public static EverytimeSession Create(string id, string pwd)
        {
            var request = (HttpWebRequest)WebRequest.Create("https://everytime.kr/user/login");

            request.Method = "POST";
            request.Headers.Add("Origin", "https://everytime.kr/");
            request.Referer = "https://everytime.kr/";
            request.ContentType = "application/x-www-form-urlencoded";
            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0";
            request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate, br");
            request.Headers.Add(HttpRequestHeader.AcceptLanguage, "ko,en-US;q=0.7,en;q=0.3");
            request.Host = "everytime.kr";
            request.CookieContainer = new CookieContainer();
            request.AllowAutoRedirect = false;

            var request_stream = new StreamWriter(request.GetRequestStream());
            request_stream.Write($"userid={id}&password={pwd}&redirect=%2F");
            request_stream.Close();

            //
            //  Create Session
            //

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                var cookies = response.Cookies;

                // Fake response
                var res = new StreamReader(response.GetResponseStream()).ReadToEnd();

                if (cookies.Count == 0 || cookies[0].Name != "etsid")
                    return null;

                var ms = new EverytimeSession { Cookies = cookies };

                return ms;
            }
        }

        public HttpWebRequest CreatePostRequest(string url)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";

            request.Headers.Add("Origin", "https://everytime.kr/");
            request.Referer = "https://everytime.kr/";
            request.ContentType = "application/x-www-form-urlencoded";
            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0";
            request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate, br");
            request.Headers.Add(HttpRequestHeader.AcceptLanguage, "ko,en-US;q=0.7,en;q=0.3");
            request.Host = "everytime.kr";
            request.CookieContainer = new CookieContainer();
            request.CookieContainer.Add(Cookies[0]);

            return request;
        }
    }
}
