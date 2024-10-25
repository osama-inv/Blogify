using System.Security.Claims;

namespace Blogify.Factories
{
    public  class ClaimsFactory
    {
        static public Claim? GetLanguageClaim(string claim)
        {
            return
                    claim switch
                    {
                        "French" => new Claim("Language", "French"),
                        "English" => new Claim("Language", "English"),
                        _ => null
                    };

        }
    }
}
