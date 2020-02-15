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
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Sugang.INHA_API
{
    public class Subject
    {
        public string Hacksu;
        public string Group;
        public string Name;
        public string Class;
        public string Score;
        public string Type;
        public string Time;
        public string Professor;
        public string Department;
        public string Estimation;
        public string Remain;
        public string Bigo;
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

        public static List<Subject> LoadCurrentSeasonSubjects()
        {
            var url = "https://sugang.inha.ac.kr/sugang/SU_51001/Lec_Time_Search.aspx";

            var wc = new WebClient();
            var html = wc.DownloadString(url);

            var document = new HtmlDocument();
            document.LoadHtml(html);
            var root_node = document.DocumentNode;

            var dept = new List<(string, string, bool)>();

            foreach (var node in root_node.SelectNodes("//select[@name='ddlDept']/option"))
                dept.Add((node.GetAttributeValue("value", ""), node.InnerText.Trim(), true));
            foreach (var node in root_node.SelectNodes("//select[@name='ddlKita']/option"))
                dept.Add((node.GetAttributeValue("value", ""), node.InnerText.Trim(), false));

            var result = new List<Subject>();

            var form = root_node.SelectSingleNode("//form[@name='form1']");

            Parallel.ForEach(dept, dd =>
            {
                var request = (HttpWebRequest)WebRequest.Create(url);

                request.Method = "POST";
                request.Referer = url;
                request.ContentType = "application/x-www-form-urlencoded";
                request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0";
                request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate, br");
                request.Headers.Add(HttpRequestHeader.AcceptLanguage, "ko,en-US;q=0.7,en;q=0.3");
                request.Headers.Add("Upgrade-Insecure-Requests", "1");
                request.Host = "sugang.inha.ac.kr";

                var request_stream = new StreamWriter(request.GetRequestStream());
                request_stream.Write($@"itisWebCommonPath=%2FITISWebCommon&itisExternalLinkSite=http%3A%2F%2Fsugang.inha.ac.kr&reportRootPath=%2FITISWebCommon%2Freport&htxtExportType=EXCEL&winClosed=open&errorMessage=&informationMessage=&confirmMessage=&confirmMessageSecu=&informationLeft=&__EVENTTARGET={(dd.Item3 ? "ddlDept" : "ddlKita")}&__EVENTARGUMENT=&__LASTFOCUS=&__VIEWSTATE={Uri.EscapeDataString(form.SelectSingleNode("./input[@name='__VIEWSTATE']").GetAttributeValue("value", ""))}&_VIEWSTATEGENERATOR=C62F3353&__EVENTVALIDATION={Uri.EscapeDataString(form.SelectSingleNode("./input[@name='__EVENTVALIDATION']").GetAttributeValue("value", ""))}&ddlDept={(dd.Item3 ? dd.Item1 : "0194002")}&ddlKita={(dd.Item3 ? "4" : dd.Item1)}&ddlTime1=%BC%B1%C5%C3&ddlTime2=%BC%B1%C5%C3&ddlTime3=%BC%B1%C5%C3&rdoKwamokGubun=99&mb_search=&hhdGetval=E-Learning+%B0%AD%C0%C7%BD%C3%B0%A3%C7%A5+%B9%D7+%B0%AD%C0%C7%B0%E8%C8%B9%BC%AD%7C4%7C000&hhdSrchGubun=search2&hhdPopUpState=&hhdCallPage=&hhdCacheTime=&hidLang=KOR&hhdhaksubunban=");
                request_stream.Close();

                //
                //  Create Session
                //

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    var res = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(51949)).ReadToEnd();
                    var regex = new Regex(@"<td class=""Center"">.*?([A-Z]{3}[0-9]{4})\-([0-9]{3}).*?Center"">.*?</td>.*?Center"">(.*?)</td>.*?Center"">(.*?)</td>.*?Center"">(.*?)</td>.*?Center"">(.*?)</td>.*?Center"">(.*?)</td>.*?Center"">(.*?)</td>.*?Center"">(.*?)</td>.*?Center"">(.*?)</td>");
                    var match = regex.Match(Regex.Replace(res, "&nbsp;", " "));
                    var mresult = new List<Subject>();
                    while (match.Success)
                    {
                        var ss = new Subject();
                        ss.Department = dd.Item2;
                        ss.Hacksu = match.Groups[1].Value;
                        ss.Group = match.Groups[2].Value;
                        ss.Name = match.Groups[3].Value;
                        ss.Class = match.Groups[4].Value;
                        ss.Score = match.Groups[5].Value;
                        ss.Type = match.Groups[6].Value;
                        ss.Time = match.Groups[7].Value;
                        ss.Professor = match.Groups[8].Value;
                        ss.Estimation = match.Groups[9].Value;
                        ss.Bigo = match.Groups[10].Value;
                        mresult.Add(ss);
                        match = match.NextMatch();
                    }

                    lock(result)
                        result.AddRange(mresult);
                }
            });

            return result;
        }

        public static void SubscribeCourseByHacksu(this SugangSession ss, string hacksu)
        {
        }

        public static void UnsubscribeCourseByHacksu(this SugangSession ss, string hacksu)
        {
        }

        public static List<Subject> GetSubscribedCourses(this SugangSession ss)
        {
            var url = "https://sugang.inha.ac.kr/sugang/SU_51001/Lec_Time_Table.aspx";

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";

            request.CookieContainer = new CookieContainer();
            request.CookieContainer.Add(ss.Cookies[0]);

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
                    subject.Hacksu = item.SelectSingleNode("./td[2]").InnerText.Trim();
                    subject.Group = item.SelectSingleNode("./td[3]").InnerText.Trim();
                    subject.Name = item.SelectSingleNode("./td[4]").InnerText.Trim();
                    subject.Class = item.SelectSingleNode("./td[5]").InnerText.Trim();
                    subject.Score = item.SelectSingleNode("./td[6]").InnerText.Trim();
                    subject.Type = item.SelectSingleNode("./td[7]").InnerText.Trim();
                    subject.Time = item.SelectSingleNode("./td[8]").InnerText.Trim();
                    subject.Professor = item.SelectSingleNode("./td[9]").InnerText.Trim();
                    subject.Estimation = item.SelectSingleNode("./td[10]").InnerText.Trim();
                    subject.Bigo = item.SelectSingleNode("./td[11]").InnerText.Trim();
                    result.Add(subject);
                }

                return result;
            }
        }
    }
}
