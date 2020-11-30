namespace ioschools.Library.Imaging
{
    public static class UtilImage
    {
        /// <summary>
        /// wraps <img></img> tag around imageurl
        /// </summary>
        /// <param name="imgpath"></param>
        /// <returns></returns>
        public static string ToHtmlImage(this string imgpath)
        {
            return string.Concat("<img src='", imgpath, "' alt='' />");
        }

        /// <summary>
        /// wraps <img></img> tag around imageurl
        /// </summary>
        /// <param name="imgpath"></param>
        /// <returns></returns>
        public static string ToHtmlImage(this string imgpath, string imageclass)
        {
            return string.Concat("<img class='", imageclass, "' src='", imgpath, "' alt='' />");
        }
    }
}
