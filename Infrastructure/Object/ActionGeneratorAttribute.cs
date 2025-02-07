namespace Infrastructure.Object
{
    public class ActionGeneratorAttribute : Attribute
    {
        public string Description { get; set; }
        public ActionGeneratorAttribute(string description)
        {
            Description = description;
        }
    }
    public class ActionGeneratorMethodAttribute : Attribute
    {
        public RequestType Type { get; set; }
        public string RouteName { get; set; }
        public string Description { get; set; }
        public bool AuthorizeCheck { get; set; }
        public ActionGeneratorMethodAttribute(RequestType type = RequestType.GET, string description = "", string routename = "", bool authorizeCheck = false)
        {
            this.Type = type;
            this.RouteName = routename.ToLower();
            this.AuthorizeCheck = authorizeCheck;
            Description = description;
        }
    }
    public enum RequestType
    {
        GET, Post
    }
}
