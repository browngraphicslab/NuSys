using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace NusysServer
{
    public class ErrorLog
    {
        private static readonly string _filepath = Constants.FILE_FOLDER + "errorlog";

        public static void AddError(Exception e)
        {
            AddErrorString(e.Message + "  " + e.Source + "  " + e.StackTrace);
        }
        public static void AddErrorString(string error, bool secondAttempt = false)
        {
            try
            {
                string lines = "";
                string line;
                StreamReader file = new StreamReader(_filepath);
                while ((line = file.ReadLine()) != null)
                {
                    lines += line + "\n";
                }
                file.Close();
                lines += DateTime.UtcNow + "   " + error;
                using (StreamWriter outputFile = new StreamWriter(_filepath))
                {
                    outputFile.WriteLine(lines);
                }
            }
            catch (Exception e)
            {
                try
                {
                    Directory.CreateDirectory(Constants.FILE_FOLDER);
                    var s = File.Create(_filepath);
                    if (!secondAttempt)
                    {
                        AddErrorString(error, true);
                    }
                    s.Close();
                }
                catch (Exception f)
                {
                }
            }
        }
    }
}