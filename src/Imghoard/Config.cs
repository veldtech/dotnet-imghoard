namespace Imghoard
{
    public class Config
    {
        public string Tenancy { get; set; } = "production";
        public string Endpoint { get; set; } = "https://api.miki.ai/";

        public static Config Default()
        {
            return new Config();
        }
    }
}
