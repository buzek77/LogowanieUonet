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
using VCommonPages.Pages.Common;

namespace LogowanieUonetPages
{
    public class DziennikStronaGlowna : StronaGlowna
    {
        public DziennikStronaGlowna(WebDocument openWebsite, int? browserId)
            : base(openWebsite, browserId)
           
        {
        }
        protected SpanTag GetTreeItem(string value, bool equal = true, string myPath = "", int id = 1)
        {
            if (equal)
                return Find<SpanTag>(string.Format("/div[#'main-tree-panel']{1}//span[@innertext='{0}'][{2}]", value, myPath, id));

            return Find<SpanTag>(string.Format("/div[#'main-tree-panel']{1}//span[@innertext~'{0}'][{2}]", value, myPath, id));
        }
        public void OtworzDzienTygodnia()
        {
            GetTreeItem("poniedziałek, 10 sierpnia 2015, Ferie letnie").DoubleClick();
        }
        
        public DziennikDodajLekcje UwtorzLekcje(string tekst)
        {
            InfoLog("otwieram lekcję");
            var element2 = GetTreeItem(tekst);
            element2.EnsureVisible();
            element2.MoveTo();
            element2.DoubleClick();
            GetSpanTagByText("Opis lekcji").Click();
            GetATagById("btn-utworz-lekcje").Click();
            var przejdzDoDodajLekcje = new DziennikDodajLekcje(OpenWebsite, BrowserId);
            return przejdzDoDodajLekcje;

            ;
        }
       
        public void UsunLekcje(string tekst)
        {
            InfoLog("usuwam lekcje");
            GetSpanTagByText(tekst).Click();
            GetSpanTagByText("Opis lekcji").Click();
            string mainDiv = string.Format("/div[#'vmodelSectionLekcja']/");
            Find<SpanTag>(string.Format(mainDiv + "/span[@innertext='Zmień']")).Click();
            GetSpanTagByText("Usuń").Click();
            GetSpanTagByText("Tak").Click();

        }
        public string SprawdzZapisanaLekcje(int i)
        {
            
            InfoLog("rozpoczynam sprawdzanie rozkład");
            string wynik = "";
            if (i==1)
            {
                wynik = GetSpanTagByText("2ag Wiedza o kulturze").InnerText.ToString();
                return wynik;
            }
            if (i == 2)
            {
                wynik = GetLabelTagByText("Chrzanowski Mariusz [MC] ").InnerText.ToString();
                return wynik;
            }
            if (i == 3)
            {
                wynik = GetLabelTagByText("Nie ").InnerText.ToString();
                return wynik;
            }
            if (i == 4)
            {
                wynik = GetLabelTagByText("brak ").InnerText.ToString();
                return wynik;
            }
            if (i == 5)
            {
                wynik = GetLabelTagByText("2ag ").InnerText.ToString();
                return wynik;
            }
            if (i == 6)
            {
                wynik = GetLabelTagByText("Wiedza o kulturze ").InnerText.ToString();
                return wynik;
            }
            if (i == 7)
            {
                wynik = GetLabelTagByText("NOWA ERA - Podręcznik do wiedzy o kulturze dla liceum i technikum ").InnerText.ToString();
                return wynik;
            }
            if (i == 8)
            {
                wynik = GetLabelTagByText("Człowiek w przestrzeni kultury ").InnerText.ToString();
                return wynik;
            }

            return wynik;


        }

        public void WylogujZDziennika()
        {
            GetSpanTagByText("Wyloguj się").Click();
        }
        public LekcjaOceny PrzejdzDoOcen()
        {
            GetSpanTagByText("Oceny").Click();
            var przejdzDoOcen = new LekcjaOceny(OpenWebsite, BrowserId);
            return przejdzDoOcen;
        }
        public LekcjaFrekwencja PrzejdzDoFrekwencji()
        {
            GetSpanTagByText("Frekwencja").Click();
            var przejdzDoFrekwencji = new LekcjaFrekwencja(OpenWebsite, BrowserId);
            return przejdzDoFrekwencji;
        }


    }

}
