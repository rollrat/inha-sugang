/***

   Copyright (C) 2020-2023. rollrat. All Rights Reserved.
   
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
using System.Web;

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

        // Query Parameters
        public string OpenPop;
    }

    public static class SugangUtils
    {
        public static List<Subject> QureyStatusByHaksu(this SugangSession ss, string haksu)
        {
            var request = ss.CreateGetRequest($"https://sugang.inha.ac.kr/sugang/SU_53005/Remain_Search.aspx?gb=direct&gubun=1&haksu={haksu}&objList=txtHaksu");

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

        public static List<Subject> LoadCurrentSeasonSubjects(this SugangSession ss)
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
                var request = ss.CreateGetRequest(url);

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
                    var regex = new Regex(@"<td class=""Center"">.*?([A-Z]{3}[0-9]{4})\-([0-9]{3}).*?Center"">.*?</td>.*?Center"">(.*?)</td>.*?Center"">(.*?)</td>.*?Center"">(.*?)</td>.*?Center"">(.*?)</td>.*?Center"">(.*?)</td>.*?Center"">(.*?)</td>.*?Center"">(.*?)</td>.*?Center"">(.*?)</td>.*?openPop\(""(.*?)""");
                    var match = regex.Match(Regex.Replace(res, " & nbsp;", " "));
                    var mresult = new List<Subject>();
                    while (match.Success)
                    {
                        var subject = new Subject();
                        subject.Department = dd.Item2;
                        subject.Hacksu = match.Groups[1].Value;
                        subject.Group = match.Groups[2].Value;
                        subject.Name = match.Groups[3].Value;
                        subject.Class = match.Groups[4].Value;
                        subject.Score = match.Groups[5].Value;
                        subject.Type = match.Groups[6].Value;
                        subject.Time = match.Groups[7].Value;
                        subject.Professor = match.Groups[8].Value;
                        subject.Estimation = match.Groups[9].Value;
                        subject.Bigo = match.Groups[10].Value;
                        subject.OpenPop = match.Groups[11].Value;
                        mresult.Add(subject);
                        match = match.NextMatch();
                    }

                    lock(result)
                        result.AddRange(mresult);
                }
            });

            return result;
        }

        public static void SubscribeCourseBySubject(this SugangSession ss, Subject subject)
        {
            var url = "https://sugang.inha.ac.kr/sugang/SU_51001/Lec_Time_Search.aspx";
            string html;
            using (var client = new WebClient())
                html = client.DownloadString(url);

            var document = new HtmlDocument();
            document.LoadHtml(html);
            var root_node = document.DocumentNode;
            var form = root_node.SelectSingleNode("//form[@name='form1']");

            var param = new Dictionary<string, string>();
            foreach (var input in form.SelectNodes(".//input"))
                if (!param.ContainsKey(input.GetAttributeValue("name", "")) && input.GetAttributeValue("type", "") == "hidden")
                    param.Add(input.GetAttributeValue("name", ""), input.GetAttributeValue("value", ""));

            param["hhdhaksubunban"] = subject.OpenPop;

            param.Add("ddlDept", "0194002");
            param.Add("ddlKita", "4");
            param.Add("ddlTime1", "선택");
            param.Add("ddlTime2", "선택");
            param.Add("ddlTime3", "선택");

            param.Add("__LASTFOCUS", "");
            param.Add("__EVENTTARGET", "ibtnReSave");
            param.Add("__EVENTARGUMENT", "");

            var reg = new Regex(@"%[a-f0-9]{2}");
            foreach (var key in param.Keys.ToArray())
                param[key] = reg.Replace(HttpUtility.UrlEncode(param[key], Encoding.GetEncoding(51949)), m => m.Value.ToUpperInvariant());

            var query = string.Join("&", param.ToList().Select(x => $"{x.Key}={x.Value}"));

            var request = ss.CreateGetRequest("https://sugang.inha.ac.kr/sugang/SU_51001/Lec_Time_Search.aspx");
            request.Method = "POST";
            request.Referer = "https://sugang.inha.ac.kr/sugang/SU_51001/Lec_Time_Search.aspx";
            request.ContentType = "application/x-www-form-urlencoded";
            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0";
            request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate, br");
            request.Headers.Add(HttpRequestHeader.AcceptLanguage, "ko,en-US;q=0.7,en;q=0.3");
            request.Headers.Add("Upgrade-Insecure-Requests", "1");
            request.Host = "sugang.inha.ac.kr";

            var request_stream = new StreamWriter(request.GetRequestStream());
            request_stream.Write(query);
            request_stream.Close();

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                var res = new StreamReader(response.GetResponseStream()).ReadToEnd();
            }
        }

        public static void UnsubscribeCourseBySubject(this SugangSession ss, Subject subject) 
        {

        }

        public static List<Subject> GetSubscribedCourses(this SugangSession ss)
        {
            var request = ss.CreateGetRequest("https://sugang.inha.ac.kr/sugang/SU_51001/Lec_Time_Table.aspx");

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
