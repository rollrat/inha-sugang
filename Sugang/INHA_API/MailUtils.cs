/***

   Copyright (C) 2020. rollrat. All Rights Reserved.
   
   Author: Jeong HyunJun

***/

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Sugang.INHA_API
{
    public class SearchResult
    {
        [JsonProperty(PropertyName = "id")]
        public string Id;
        [JsonProperty(PropertyName = "name")]
        public string Name;
        [JsonProperty(PropertyName = "email")]
        public string Email;
        [JsonProperty(PropertyName = "position")]
        public string Position;
        [JsonProperty(PropertyName = "dutyName")]
        public string[] DutyName;
        [JsonProperty(PropertyName = "nodeType")]
        public string NodeType;
        [JsonProperty(PropertyName = "departments")]
        public string[] Departments;
        [JsonProperty(PropertyName = "departmentIds")]
        public string[] DepartmentsIds;
    }

    public static class MailUtils
    {
        public static List<SearchResult> QueryAddress(this MailSession session, string keyword, string nodeType = "user", int page = 0, int offset = 20)
        {
            var requests = $"http://mail.inha.ac.kr/api/user/sort/list?keyword={HttpUtility.UrlEncode(keyword)}&nodeType={HttpUtility.UrlEncode(nodeType)}&page={page}&offset={offset}";

            var wc = (HttpWebRequest)WebRequest.Create(requests);
            wc.CookieContainer = new CookieContainer();
            wc.CookieContainer.Add(session.Cookies[0]);

            using (var response = (HttpWebResponse)wc.GetResponse())
            {
                var res = new StreamReader(response.GetResponseStream()).ReadToEnd();

                return JsonConvert.DeserializeObject<List<SearchResult>>(JValue.Parse(res)["data"].ToString());
            }
        }
    }
}
