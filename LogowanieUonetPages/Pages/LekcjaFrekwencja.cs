using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ranorex;

namespace LogowanieUonetPages
{
    public class LekcjaFrekwencja :DziennikStronaGlowna
    {
        public LekcjaFrekwencja (WebDocument openWebsite, int? browserId)
            : base(openWebsite, browserId)
           
        {
        }
        public void KliknijWZmienFrekwencję()
        {
            GetSpanTagByText("Zmień frekwencję").Click();
        }
        private TdTag WybierzKomórkęFrekwencjiDlaUcznia(int tr, int td)
        {
            string divtag = "v-grid-body-container v-unselectable";
            string mainDiv = string.Format("/div[@class='{0}']/", divtag);
            return Find<TdTag>(string.Format(mainDiv + "/table/tbody/tr[{0}]/td[{1}]", tr, td));
        }
        public void SprawdzFrekwencje()
        {

            WybierzKomórkęFrekwencjiDlaUcznia(1, 2).Click();
            GetSpanTagByText("s").Click();
            WybierzKomórkęFrekwencjiDlaUcznia(3, 2).Click();
            GetSpanTagByText("u").Click();
            WybierzKomórkęFrekwencjiDlaUcznia(5, 2).Click();
            GetSpanTagByText("●").Click();
            WybierzKomórkęFrekwencjiDlaUcznia(7, 2).Click();
            GetSpanTagByText("▬").Click();
            WybierzKomórkęFrekwencjiDlaUcznia(9, 2).Click();
            GetSpanTagByText("z").Click();
            GetSpanTagByText("Anuluj").Click();
            GetSpanTagByText("Tak").Click();
        }
        public DziennikStronaGlowna DoStronyGlownejDziennika()
        {
            var przejdzDoDziennika = new DziennikStronaGlowna(OpenWebsite, BrowserId);
            return przejdzDoDziennika;

        }

    }
}
