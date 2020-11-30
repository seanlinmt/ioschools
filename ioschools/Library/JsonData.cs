namespace ioschools.Library
{
    public class JsonData : ErrorData
    {
        public object data { get; set; }
        public string title { get; set; }
    }

    public static class JsonDataHelper
    {
        public static JsonData ToJsonOKMessage(this string msg)
        {
            return new JsonData
                       {
                           message = msg, 
                           success = true, 
                           data = null
                       };
        }

        public static JsonData ToJsonOKData(this object data, string dialogTitle = "")
        {
            return new JsonData
                       {
                           message = "", 
                           success = true, 
                           data = data,
                           title = dialogTitle
                       };
        }

        public static JsonData ToJsonFailData(this object data)
        {
            return new JsonData
                       {
                           message = "", 
                           success = false, 
                           data = data
                       };
        }

        public static JsonData ToJsonFail(this string msg)
        {
            return new JsonData
                       {
                           message = msg, 
                           success = false
                       };
        }
    }
}
