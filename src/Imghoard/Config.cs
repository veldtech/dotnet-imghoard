using System.Reflection;

namespace Imghoard
{
    public class Config
    {
        public string Tenancy { get; set; } = "production";
        public string Endpoint { get; set; } = "https://api.miki.ai/";
        public string UserAgent { get; set; } = $"Mozilla/5.0 (Imghoard.Net/{Assembly.GetExecutingAssembly().GetName().Version.ToString().Substring(0, 3)}; +https://github.com/Mikibot/dotnet-imghoard)";

        /// <summary>
        /// Allows the client to use unstable, experimental features
        /// </summary>
        public bool Experimental { get; set; } = false;

        public static Config Default()
        {
            return new Config();
        }
    }
}
