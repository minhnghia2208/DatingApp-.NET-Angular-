namespace API.Errors
{
    public class APIExceptions
    {
        public APIExceptions(int statusCode, string message, string details){
            StatusCode = statusCode;
            Message = message;
            Details = details;
        }
        public int StatusCode{set; get;}
        public string Message { get; set; }
        public string Details { get; set; }
    }
}