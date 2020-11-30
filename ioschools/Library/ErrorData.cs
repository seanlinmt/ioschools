namespace ioschools.Library
{
    public class ErrorData
    {
        public bool success { get; set; }
        public string message { get; set; }

        public ErrorData()
        {
            success = true;
        }
    }
}