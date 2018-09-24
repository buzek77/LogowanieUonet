using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LogowanieUonetPages.Pages;
using VCommonPages;
using Ranorex;
using System.Collections;
using System.Data.SqlClient;
using System.Configuration;
using LogowanieUonetPages;

namespace LogowanieUonetPages.Pages
{
    public class LekcjaOceny:DziennikStronaGlowna
    {
        public LekcjaOceny(WebDocument openWebsite, int? browserId)
            : base(openWebsite, browserId)
           
        {
        }
        public void KliknijWZmienOceny()
        {
            GetSpanTagByText("Zmień oceny i ich opis").Click();

        }
        private int PobierzIDWierszUcznia(string uczen, int indexKolumny, string divtag) //funkcja z dziennik oddziału page
        {
            string mainDiv = string.Format("/div[@class='{0}']/", divtag);

            var index = Find<TdTag>(string.Format(mainDiv + "/td[@innertext='{0}']", uczen)).Parent.Element.ChildIndex + 1;
            //var wynik=Find<TdTag>(string.Format(mainDiv + "/table/tbody/tr[{0}]/td[{1}]", index, indexKolumny));
            return index;

        }
        public TdTag WybierzKomórkęOcenDlaUcznia(int tr, int td ) //funkcja z dziennik oddziału page
        {
            string divtag = "v-grid-body-container v-unselectable";
            string mainDiv = string.Format("/div[@class='{0}']/", divtag);
            return  Find<TdTag>(string.Format(mainDiv + "/table/tbody/tr[{0}]/td[{1}]", tr, td));
            

        }

        public void wpiszOceny(Uczen[] uczen)
        {
            for (int i = 0; i <= 6; i++)
            {
                string divTag = "v-grid-list-container left v-unselectable";
                int x = PobierzIDWierszUcznia(uczen[i].DaneUcznia, 4, divTag);
                WybierzKomórkęOcenDlaUcznia(x, 1).Click();
                string y = uczen[i].Ocena;
                Keyboard.Press(y);
                Keyboard.Press(Keys.Enter); 
            }
            GetSpanTagByText("Anuluj").Click();
            
        }
        public DziennikStronaGlowna DoStronyGlownejDziennika()
        {
            var przejdzDoDziennika = new DziennikStronaGlowna(OpenWebsite, BrowserId);
             return przejdzDoDziennika;

        }
        
    }
}
