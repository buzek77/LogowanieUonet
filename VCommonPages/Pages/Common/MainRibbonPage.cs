using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Ranorex;

namespace VCommonPages.Pages.Common
{
    /// <summary>
    /// Klasa do obsługi wyboru kontrolek na wstążce
    /// </summary>
    public class MainRibbonPage : Page
    {
        public MainRibbonPage(WebDocument openWebsite, int? browserId)
            : base(openWebsite, browserId)
        {

        }

        public string PobierzNazweZalogowanegoUzytkownika() //20.11_PAU - Erato
        {
            DebugLog("Pobieranie nazwy zalogowanego użytkownika");
            WaitForLoad(30000);
            var zalogowanyUzytkownik = Find<SpanTag>("/div[#ribbon-view-targetEl]//div[@id~'tabpanel']/div[@id~'tabbar']//span[@id~'btnInnerEl']");
            zalogowanyUzytkownik.MoveTo();

            return zalogowanyUzytkownik.InnerText.TrimEnd();
        }

        protected void WybierzZakladkeNaRibbonie(string tekst) //20.11_PAU - Erato
        {
            WaitForLoad(30000);
            Keyboard.Press(Keys.Escape);
            DebugLog("Wybieram zakładke : " + tekst);
            GetSpangTagOnRibbon(tekst).Click();
            WaitForLoad(30000); // K.O
        }

        protected void WybierzPrzyciskNaRibbonie(string tekst) //20.11_PAU - Erato
        {
            string divMiddle = "//div[#'ribbon-view-tab-body']";
            Keyboard.Press(Keys.Escape);
            DebugLog("Wybieram przycisk : " + tekst);
            GetSpangTagOnRibbon(tekst, divMiddle).Click();
            WaitForLoad(30000); // K.O
        }

        public bool SprawdzCzyZakladkaPodswietlona(string nazwaZakladki) //mareks
        {
            var nazwa = GetSpanTagByText(string.Format("{0}", nazwaZakladki));
            nazwa.MoveTo();
            return nazwa.Parent.Parent.Parent.Class.Contains("x-active x-tab-active");
        }
        
        private SpanTag GetSpangTagOnRibbon(string tekst, string divMiddle = "")
        {
            // Niestety, niektóre z guzików nazywają się tak samo jak zakładki,
            // dlatego należy bardziej precyzyjnie adresować elementy K.O
            return GetSpanTagByDivAndText("ribbon-view", divMiddle, tekst);
        }


        public void WybierzPrzyciskStartLubWylogujNaRibbonie(string tekst)
        {
            string divMiddle = "//div[#'ribbon-user-tab-body']";
            Keyboard.Press(Keys.Escape);
            DebugLog("Wybieram przycisk : " + tekst);
            GetSpangTagOnRibbon(tekst, divMiddle).Click();
            //WaitForLoad();
        }

        protected bool SprawdzCzyJestBrakInfomacji(string id)
        {
            DebugLog("Sprawdzamy element od id " + id);
            HtmlTable table = new HtmlTable(OpenWebsite,id,BrowserId);
            return table.SprawdzCzyZawieraInformacje("Brak informacji do wyświetlenia");
        }

        protected SpanTag GetTreeItem(string value, bool equal = true, string myPath="", int id=1)
        {
            if(equal)
                return Find<SpanTag>(string.Format("/div[#'main-tree-panel']{1}//span[@innertext='{0}'][{2}]", value, myPath, id));
           
            return Find<SpanTag>(string.Format("/div[#'main-tree-panel']{1}//span[@innertext~'{0}'][{2}]", value, myPath, id));
        }

        protected Boolean SprawdzCzyDrzewkoZawiera(string value, bool equal = true, string myPath = "", bool visible=true)
        {
            var elements = OpenWebsite.Find<SpanTag>(string.Format(".//div[#'main-tree-panel']{0}//span{1}", myPath, visible ? "[@visible='True']" : String.Empty));
            return (equal
                 ? from el in elements where el.InnerText.Equals(value) select el
                 : from el in elements where el.InnerText.Contains(value) select el).Any();
        }

        protected void WybierzNaDrzewku(string value, bool equal = true, bool kliknijIkone = false, int index = 1) //20.11_PAU - Erato
        {
            if(kliknijIkone==false)
                GetTreeItem(value, equal, "", index).Click();
            else
            {
                // Jeśli pozycja na drzewie jest zbyt długa kliknięcie nie działa, trzeba kliknąć ikonę
                GetTreeItem(value, equal, "[@Visible='True']", index).Parent.FindSingle<ImgTag>("./img[@Class~'x-tree-icon-leaf']").Click();
            }
            WaitForLoad();
        }

        protected void RozwinNaDrzewku(string value, bool equal = true, string myPath = "", int index = 1) //20.11_PAU - Erato
        {
            var element = GetTreeItem(value, equal, myPath, index);
            if(!element.Parent.Parent.Parent.Class.Contains("expanded")) //19.01_Pau - modyfikacja na potrzeby sekretariatu
                element.Parent.FindSingle<ImgTag>("./img[@Class~'x-tree-elbow-plus' or @Class~'x-tree-elbow-end-plus']").Click();
            WaitForLoad();
        }

        protected HtmlTable PobierzZawartoscDrzewka(string tableId, int wierszPoczatkowy = 0, int iloscWierszy = 0) //20.11_PAU - Erato
        {
            DebugLog("Pobierz zawartość drzewka");
            return new HtmlTable(OpenWebsite, tableId, BrowserId, false, "", wierszPoczatkowy, iloscWierszy);
        }

        protected bool SprawdzCzyElementNaDrzewkuPodswietlony(string text)
        {
            var element = GetSpanTagByText(text);
            element.MoveTo();
            return element.Parent.Parent.Parent.Class.Contains("-row-focused");
        }

        protected void WybierzPrzyciskNaVaccpanel(string nazwa, string id="vaccpanel-view")
        {
            DebugLog("Wybieram przycisk : " + nazwa);
            Find<TdTag>(string.Format("/div[#'{1}']//td[@innertext='{0}']", nazwa,id)).Click();
            WaitForLoad();
        }
       
    }
}
