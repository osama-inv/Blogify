using Blogify.Repos.Interfaces;

namespace Blogify.Repos
{
    public class FrenchPresenter : ILanguagePresenter
    {
        public string Present()
        {
            return "Bonjour mon ami, comment ça va?";
        }
    }
}
