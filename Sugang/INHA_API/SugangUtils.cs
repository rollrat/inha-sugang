/***

   Copyright (C) 2020. rollrat. All Rights Reserved.
   
   Author: Jeong HyunJun

***/

using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Sugang.INHA_API
{
    public class Subject
    {
        public string Hacksu;
        public string Name;
        public string Score;
        public string Professor;
        public string Department;
        public string Remain;
    }

    public static class SugangUtils
    {
        public static List<Subject> QureyStatusByHaksu(this SugangSession ss, string haksu)
        {
            var url = $"https://sugang.inha.ac.kr/sugang/SU_53001/Remain_Search.aspx?gb=direct&gubun=1&haksu={haksu}&objList=txtHaksu";

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";

            request.CookieContainer = new CookieContainer();
            request.CookieContainer.Add(ss.Cookies[0]);
            request.CookieContainer.Add(ss.Cookies[1]);

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                var html = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(51949)).ReadToEnd();

                var document = new HtmlDocument();
                document.LoadHtml(html);
                var root_node = document.DocumentNode;

                var table = root_node.SelectSingleNode("//*[@id=\"dgList\"]");
                var result = new List<Subject>();
                foreach (var item in table.SelectNodes("./tbody[1]/tr"))
                {
                    var subject = new Subject();
                    subject.Hacksu = item.SelectSingleNode("./td[3]").InnerText.Trim();
                    subject.Name = item.SelectSingleNode("./td[4]").InnerText.Trim();
                    subject.Score = item.SelectSingleNode("./td[5]").InnerText.Trim();
                    subject.Professor = item.SelectSingleNode("./td[6]").InnerText.Trim();
                    subject.Department = item.SelectSingleNode("./td[7]").InnerText.Trim();
                    subject.Remain = item.SelectSingleNode("./td[8]").InnerText.Trim();
                    result.Add(subject);
                }

                return result;
            }
        }
    }
}
