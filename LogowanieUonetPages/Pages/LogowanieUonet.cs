using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LogowanieUonetPages.Pages;
using VCommonPages;
using Ranorex;
using System.Collections;
using System.Data.SqlClient;
using System.Configuration;

namespace LogowanieUonetPages
{
    public class LogowanieUonet : LogowanieStronaStartowa
    { 
        
        public  LogowanieUonet(WebDocument openWebsite,int? browserId):base(openWebsite,browserId)
        {
            
        }

        public void WpiszLogin(string tekst)
        {
            InfoLog("wpisuję login");
            GetInputTagByName("LoginName").Value = tekst;

        }

        public void WpiszPass(string tekst)
        {
            InfoLog("wpisuję hasło");
            GetInputTagByName("Password").Value = tekst;
        }

        public StronaGlowna Zaloguj2()
        {
           InfoLog("Klikam w guzik zaloguj");
            GetInputTagByType("submit").Click();
            
            var przejdzdoStronaGlowna = new StronaGlowna(OpenWebsite, BrowserId);
            return przejdzdoStronaGlowna;
        }

        
       
          
    }
    }

