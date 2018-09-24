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

namespace LogowanieUonetPages.Pages
{
    public class StronaGlowna:LogowanieUonet
    {
         public  StronaGlowna(WebDocument openWebsite,int? browserId):base(openWebsite,browserId)
        {
            // WaitForLoad();
         }
         public DziennikStronaGlowna WybierzDziennikSzkoly()
         {
             InfoLog("Wybieram dziennik szkoly");
             string divButton = string.Format("/div[#'content']//div[@innertext='Dziennik']");
             var element =
                    Find<DivTag>(divButton + "/../../", new Duration(120000)).NextSibling.FindSingle<SpanTag>(string.Format(".//span[@innertext='ZS50']"));
             element.EnsureVisible();
             element.MoveTo();
             element.Click();

             var przejdzDoDziennika = new DziennikStronaGlowna(OpenWebsite, BrowserId);
             return przejdzDoDziennika;

         }
         
    }
}
