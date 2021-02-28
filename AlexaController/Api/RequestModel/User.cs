namespace AlexaController.Api.RequestData
{
    public class User
    {
        public string userId { get; set; }
        public string accessToken { get; set; }
        public Permissions permissions { get; set; }
    }
}
