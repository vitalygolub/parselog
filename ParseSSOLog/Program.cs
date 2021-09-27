using System;
using System.Data.SqlClient;
using System.IO;
using System.Text.RegularExpressions;

namespace ParseSSOLog
{
    class Program
    {
        static void Main(string[] args)
        {
            string s;
            data d = new data();
            int rowcount = 0;
            bool flag=false;

            SqlConnection con = new SqlConnection("Data Source=(local)\\sqlexpress;Initial Catalog=test;Integrated Security=true");
            con.Open();
            var cmd = new SqlCommand();
            cmd.CommandText = @"if exists(select 1 from sys.tables where name = 'logdata')
    DROP TABLE logdata
    CREATE TABLE logdata([location] varchar(50)NULL,controller varchar(50),action varchar(50),session varchar(50),returnurl varchar(500)NULL,clientsid varchar(20)NULL,rownumber int primary key, timeofday time(0));";
            cmd.Connection = con;
            cmd.ExecuteNonQuery();


            using (StreamReader sr = new StreamReader(@".\is4-20210927.txt"))
            {
                
                while (!sr.EndOfStream) 
                {
                    s = sr.ReadLine();
                    rowcount++;
                    if (rowcount == 66787)
                        rowcount.ToString();
                    if (s.Length > 9 && s.IndexOf("[") == 9) //new log entry
                    {
                        //preserve existing
                        // save d;
                        if (d.session != null)
                        {
                            save(con,d);
                        }
                        d.init();
                        flag = false;
                    }

                    if (s.Contains("[RIMIis4.Quickstart.LogSessionFilter.LogSessionFilter]")) 
                    {
                        Regex rex=new Regex(@"(.*) (\w*) controller:(\w*) action:(\w*) sessionid=(.*)", RegexOptions.Singleline);
                        if (rex.Match(s).Success) 
                        {
                            flag = true;    //log entry we interested
                            
                            d.location=rex.Replace(s, "$2");
                            d.controller=rex.Replace(s, "$3");
                            d.action=rex.Replace(s, "$4");
                            d.session=rex.Replace(s, "$5");
                            d.rownumber = rowcount;
                            d.timeofday = DateTime.Parse(s.Substring(0,8));
                        }
                        continue;
                    }
                    else if (flag ) 
                    {
                        var key = "\tReturnUrl:";
                        if (s.StartsWith(key))
                        {
                            d.returnurl = s.Substring(key.Length);
                        }
                        else
                        {
                            key = "\tClientId:";
                            if (s.StartsWith(key))
                            {
                                d.clientsid = s.Substring(key.Length);
                            }

                        }
                    }
                }
            }

            if (d.session != null)
                save(con, d);

        }

        private static void save(SqlConnection con, data d) 
        {
            SqlCommand cmd = new SqlCommand(@"INSERT INTO[dbo].[logdata]  ([location] ,[controller] ,[action] ,[session] ,[returnurl] ,[clientsid],[rownumber],[timeofday])
                 VALUES(@location, @controller,@action, @session, @returnurl, @clientsid,@rownumber,@timeofday)");
            cmd.Connection = con;
            cmd.Parameters.AddWithValue("location", d.location);
            cmd.Parameters.AddWithValue("controller", d.controller);
            cmd.Parameters.AddWithValue("action", d.action);
            cmd.Parameters.AddWithValue("session", d.session);
            cmd.Parameters.AddWithValue("returnurl", d.returnurl??"");
            cmd.Parameters.AddWithValue("clientsid", d.clientsid??"");
            cmd.Parameters.AddWithValue("rownumber", d.rownumber);
            cmd.Parameters.AddWithValue("timeofday", d.timeofday);
            cmd.ExecuteNonQuery();
        }

    }

    class data 
    {
        public string location { get; set; }
        public string controller { get; set; }
        public string action { get; set; }
        public string session { get; set; }
        public string returnurl { get; set; }
        public string clientsid { get; set; }
        public int rownumber { get; set; }
        public DateTime timeofday { get; set; }

        public void init() 
        {
            session = null;returnurl = null;clientsid = null;
        }
    }
}
