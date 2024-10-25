using Blogify.Repos.Interfaces;

namespace Blogify.Repos
{
    public class EnglishPresenter : ILanguagePresenter
    {
        public string Present()
        {
            return "Hello out friend, how are you?";
        }
    }

}
