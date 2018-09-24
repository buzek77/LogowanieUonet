using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Ranorex;

namespace VCommonPages.Pages
{

    public abstract class  PortalLoginPage : Page
    {
        private string _url;
        private string _urlPortal = ConfigurationManager.AppSettings.Get("urlPortal") ?? string.Empty; 

        public PortalLoginPage(string url)
        {
            _url = url;
        }

        public void OtworzStrone()
        {
            OpenBrowser(_url, _urlPortal);
        }
        

        public void ZalogujSieJako(string login, string password = "demo") 
        {
            DebugLog("Wprowadzanie loginu : " + login);
            Find<InputTag>("/input[@id~'LoginName']").PressKeys(login);
            DebugLog("Wprowadzanie hasla : " + password);
            Find<InputTag>("/input[@id~'Password']").PressKeys(password);
            Keyboard.Press(Keys.Enter);
            
            OpenWebsite.WaitForDocumentLoaded(30000);
        }

        public void KliknijWPrzyciskZalogujSofista() //nie usuwać i nie kopiować rozwiązania:D mareks - dawid pozwolił na taką fanaberię
        {
            DebugLog("Wybranie przycisku zaloguj się");
            OpenWebsite.WaitForDocumentLoaded(); //nie usuwać!
            Find<ATag>(@"/a[@Class='loginButton']").Click();
            OpenWebsite.WaitForDocumentLoaded();
        }

    }
}
