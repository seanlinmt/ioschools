using System.Web.Script.Serialization;

namespace ioschoolsWebsite.Library.Liquid.filters
{
    public static class InputFilter
    {
        public static string json(object input)
        {
            var serializer = new JavaScriptSerializer();
            return serializer.Serialize(input);
        }
    }
}