using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VCommonPages;
using Ranorex;
using System.Collections;
using System.Data.SqlClient;
using System.Configuration;
using VCommonPages.Pages;

namespace LogowanieUonetPages
{
    public class LogowanieStronaStartowa : Page
    {
        public LogowanieStronaStartowa(string url)
        {
            OpenBrowser(url); //czeka na załadowanie dokumentu 
            OpenWebsite.WaitForDocumentLoaded();
        }
        public LogowanieStronaStartowa(WebDocument openWebsite, int? browserId)
            : base(openWebsite, browserId)
    {
        
    }
         public WebDocument openWeb()
        {
            return OpenWebsite;
        }
        public int? browser()
        {
            return BrowserId;
        }
           
        protected T Znajdz<T>(string xPath, int miliseconds = 30000) where T : Adapter
        {
            string correctPath = "./" + xPath;
            return OpenWebsite.FindSingle<T>(correctPath, new Duration(miliseconds));
        }

        protected DivTag GetTableTagById(String text)
        {
            return Znajdz<DivTag>(String.Format("/table[#'{0}", text));
        }


        protected InputTag GetInputTagByType(String text)
        {
            return Znajdz<InputTag>(String.Format("/input[@type='{0}']", text));
        }

        protected ATag GetATagByHref(String text)
        {
            return Znajdz<ATag>(String.Format("/a[@href='{0}']", text));
        }
        protected ATag GetATagById(String text)
        {
            return Znajdz<ATag>(String.Format("/a[#'{0}']", text));
        }
        protected LiTag GetLiTagByText(String text)
        {
            return Znajdz<LiTag>(String.Format("/li[@innertext='{0}']", text));
        }
       
        public LogowanieUonet Zaloguj1()
        {
            InfoLog("Klikam w guzik zaloguj");
            GetATagByText("Zaloguj się").Click();
            var przejdzDoLogowanieUonet2 = new LogowanieUonet(OpenWebsite, BrowserId);
            return przejdzDoLogowanieUonet2;

        }

        
    }
}
