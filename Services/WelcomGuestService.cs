using Blogify.Repos.Interfaces;

namespace Blogify.Services
{
    public class WelcomGuestService
    {
        readonly ILanguagePresenter _languagePresenter;
        public WelcomGuestService(ILanguagePresenter languagePresenter)
        {
            _languagePresenter = languagePresenter;
        }
        public string Welcome()
        {
            return _languagePresenter.Present();
        }
    }
}
