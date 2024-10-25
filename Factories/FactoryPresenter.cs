using Blogify.Repos;
using Blogify.Repos.Interfaces;

namespace Blogify.Factories
{
    public static class FactoryPresenter
    {
        public static ILanguagePresenter GetPresenter(string language)
        {
            switch (language)
            {
                case "French":
                    return new FrenchPresenter();
                case "English":
                    return new EnglishPresenter();
                default:
                    return new EnglishPresenter();
            }
        }
    }
}
