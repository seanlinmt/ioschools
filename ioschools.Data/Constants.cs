namespace ioschools.Data
{
    public static class Constants
    {
        public const int LUCENE_SEARCH_RESULTS_MAX = 50;
        public const int DURATION_1DAY_SECS = 86400;
        public const int DURATION_1HOUR_SECS = 3600;
        public const string DATEFORMAT_DATEPICKER = "ddd, d MMM yyyy";
        public const string DATEFORMAT_DATEPICKER_SHORT = "d MMM yyyy";
        public const string DATETIME_STANDARD = "dd MMMM yyyy";
        public const string DATETIME_SHORT_DATE = "dd MMM yyyy";
        public const string DATETIME_STANDARD_TIME = "h:mm tt";
        public const string DATETIME_FULL = "h:mm tt, dd MMMM yyyy";
        public const string DATETIME_SHORTTIME = "hh\\:mm";
        public const long MAX_DISK_SIZE = 100000000; // 100MB
#if DEBUG
        public const string GOOGLEMAP_APIKEY =
            "GOOGLEMAP_APIKEY";
#else
        // lodge actual
        //public const string GOOGLEMAP_APIKEY =
        //    "GOOGLEMAP_APIKEY";
        // lodge1
        public const string GOOGLEMAP_APIKEY =
            "GOOGLEMAP_APIKEY";
        // lodge2
        //public const string GOOGLEMAP_APIKEY =
       //     "GOOGLEMAP_APIKEY";
#endif

        public const string UPLOAD_PATH = "Uploads/";
        public const string UPLOAD_PATH_BLOG = "Uploads/Blog/";
        public const string UPLOAD_PATH_HOMEWORK = "Uploads/Homework/";
        public const string UPLOAD_PATH_REGISTRATION = "Uploads/Registration/";

        public const string SESSION_ID = "id";
        public const string SESSION_USERGROUP = "usergroup";
        public const string SESSION_SCHOOL = "school";
        public const string SESSION_NAME = "name";

        // attendance
        public const int ATTENDANCE_TRIGGER_LEVEL = 3;

#if DEBUG
        public const string HTTP_HOST = "http://localhost:33224";
#else
        public const string HTTP_HOST = "http://ioschools.edu.my";
#endif
    }
}