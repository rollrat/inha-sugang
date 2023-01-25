/***

   Copyright (C) 2020-2023. rollrat. All Rights Reserved.
   
   Author: Jeong HyunJun

***/

using HtmlAgilityPack;
using Sugang.INHA_API;
using Sugang.INHA_API.Crpyto;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Sugang.EVERYTIME_API
{
    public class TimeTableSemester
    {
        public string Year;
        public string Semester;
        public string StartDate;
        public string EndDate;
        public string IsFormal;
        public string IsSupported;
        public string HasSyllabus;
        public string HasSubjectDatabase;
        public string UpdatedAt;
    }

    public class TimeTableTable
    {
        public string Id;
        public string Name;
        public string Priv;
        public string IsPrimary;
        public string CreatedAt;
        public string UpdatedAt;
    }

    public static class EverytimeUtils
    {
        public static List<TimeTableSemester> ListingSemesters(this EverytimeSession session)
        {
            var request = session.CreatePostRequest("https://api.everytime.kr/find/timetable/subject/semester/list");
            var res = (HttpWebResponse)request.GetResponse();
            var gz = new BinaryReader(new GZipStream(res.GetResponseStream(), CompressionMode.Decompress)).ReadBytes(65535);
            var xml = Encoding.UTF8.GetString(gz);
            res.Close();

            var document = new HtmlDocument();
            document.LoadHtml(xml);
            var root_node = document.DocumentNode;

            var result = new List<TimeTableSemester>();

            foreach (var semester in root_node.SelectNodes("//semester"))
            {
                result.Add(new TimeTableSemester
                {
                    Year = semester.GetAttributeValue("year", ""),
                    Semester = semester.GetAttributeValue("semester", ""),
                    StartDate = semester.GetAttributeValue("start_date", ""),
                    EndDate = semester.GetAttributeValue("end_date", ""),
                    IsFormal = semester.GetAttributeValue("is_formal", ""),
                    IsSupported = semester.GetAttributeValue("is_supported", ""),
                    HasSyllabus = semester.GetAttributeValue("has_syllabus", ""),
                    HasSubjectDatabase = semester.GetAttributeValue("has_subject_database", ""),
                    UpdatedAt = semester.GetAttributeValue("updated_at", ""),
                });
            }

            return result;
        }
        
        public static List<TimeTableTable> GetTableListFromSemester(this EverytimeSession session, TimeTableSemester semester)
        {
            var request = session.CreatePostRequest("https://api.everytime.kr/find/timetable/table/list/semester");
            var rs = new StreamWriter(request.GetRequestStream());
            rs.Write($"year={semester.Year}&semester={Uri.EscapeDataString(semester.Semester)}");
            rs.Close();
            var res = (HttpWebResponse)request.GetResponse();
            var gz = new BinaryReader(new GZipStream(res.GetResponseStream(), CompressionMode.Decompress)).ReadBytes(65535);
            var xml = Encoding.UTF8.GetString(gz);
            res.Close();

            var document = new HtmlDocument();
            document.LoadHtml(xml);
            var root_node = document.DocumentNode;

            var result = new List<TimeTableTable>();

            if (root_node.SelectNodes("//table") != null)
            {
                foreach (var table in root_node.SelectNodes("//table"))
                {
                    result.Add(new TimeTableTable
                    {
                        Id = table.GetAttributeValue("id", ""),
                        Name = table.GetAttributeValue("name", ""),
                        Priv = table.GetAttributeValue("priv", ""),
                        IsPrimary = table.GetAttributeValue("is_primary", ""),
                        CreatedAt = table.GetAttributeValue("created_at", ""),
                        UpdatedAt = table.GetAttributeValue("updated_at", ""),
                    });
                }
            }

            return result;
        }

        public static List<string> GetHacksuFromTableInfo(this EverytimeSession session, TimeTableTable table)
        {
            var request = session.CreatePostRequest("https://api.everytime.kr/find/timetable/table");
            var rs = new StreamWriter(request.GetRequestStream());
            rs.Write($"id={table.Id}");
            rs.Close();
            var res = (HttpWebResponse)request.GetResponse();
            var gz = new BinaryReader(new GZipStream(res.GetResponseStream(), CompressionMode.Decompress)).ReadBytes(65535 * 128);
            var xml = Encoding.UTF8.GetString(gz);
            res.Close();

            var document = new HtmlDocument();
            document.LoadHtml(xml);
            var root_node = document.DocumentNode;

            var result = new List<string>();

            foreach (var subject in root_node.SelectNodes("//internal"))
            {
                result.Add(subject.GetAttributeValue("value", ""));
            }

            return result;
        }
    }
}
