using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Ranorex;

namespace VCommonPages.Pages.Common
{
    /// <summary>
    /// Klasa do obsługi kontrolki - "dwie listy"
    /// </summary>
    public class VSwitchPanel :VWindowPage
    {

        private DivTag _windowDialog;
        public DivTag OknoDialogowe
        {
            get { return _windowDialog; }
        }

        /// <summary>
        /// Scieżka do lewego grida
        /// </summary>
        //private string pathToSwitchPanelLeft = "/div[@id~'vswitchpanel' and id~'-targetEl']/div[@id~'vgrid' and childindex=0]";
        //public string PathToSwitchPanelLeft
        //{
        //    get { return pathToSwitchPanelLeft; }
        //    set { pathToSwitchPanelLeft = value; }
        //}

        ///// <summary>
        ///// Scieżka do prawego grida
        ///// </summary>
        //private string pathToSwitchPanelRight = "/div[@id~'vswitchpanel' and id~'-targetEl']/div[@id~'vgrid' and childindex=2]";
        //public string PathToSwitchPanelRight
        //{
        //    get { return pathToSwitchPanelRight; }
        //    set { pathToSwitchPanelRight = value; }
        //}

        public VSwitchPanel(WebDocument openWebsite, int? browserId, string idOkna)
            : base(openWebsite, browserId, idOkna)
        {
            WaitForLoad();
            var _headerWidow = OpenWebsite.FindSingle<SpanTag>(string.Format(".//div[@class~'x-window']//span[@innertext='{0}' and @class~'x-window-header-text']", idOkna));
            this._windowDialog = _headerWidow.Parent.Parent.Parent.Parent.Parent.Parent.As<DivTag>(); 
        }

        private HtmlTable PobierzLewegoLubPrawegoGrida(string lewaCzyPrawa)
        {
            var myPath = string.Format("{0}/div[@id~'-body']", (lewaCzyPrawa == "lewa" ? PathToSwitchPanelLeft : PathToSwitchPanelRight));
            HtmlTable tabelka = new HtmlTable(OpenWebsite, "gridview", BrowserId, true, myPath);
            return tabelka;
        }

        public HtmlTable PobierzLewegoGrida() 
        {
            return PobierzLewegoLubPrawegoGrida("lewa");
        }

        public HtmlTable PobierzPrawegoGrida() 
        {
            return PobierzLewegoLubPrawegoGrida("prawa");
        }

        //domyślnie 0 "+" jeżeli 1 to "-" mareks
        public void KliknijGuzikPlusaMinusa(int plusCzyMinus=0) 
        {
            DebugLog("Kliknij plus lub minus");
            var plusMinus = Find<ATag>(string.Format("//a[@class='x-btn x-unselectable x-box-item x-btn-default-small x-noicon x-btn-noicon x-btn-default-small-noicon' and childindex={0}]", plusCzyMinus));
            plusMinus.MoveTo();
            plusMinus.Click();
        }

        public void KliknijStrzalke()
        {
            DebugLog("Kliknij strzałkę");
            var strzalka = Find<SpanTag>(string.Format("//div[@id~'container']/div/a/span/span/span"));
            strzalka.Click();
        }

        
        #region klikanie w strzałki porządkujące na liście
        /// <summary>
        /// Klikanie po strzałkach sortujących listę elementów
        ///  </summary>
        /// <param name="ilosc"> ilość kliknięć jaka ma zostać wykonana </param>
        public void KliknijGuzikStrzalkiWGore(int ilosc)
        {
            var guzik = Find<SpanTag>(string.Format("//span[@class~'x-btn-icon-el x-form-itemselector-up']"));
            int cnt = 0;

            while (cnt < ilosc)
            {
                DebugLog("kliknięcie w strzałkę w górę");
                guzik.Click();
                cnt++;
            }
        }

        public void KliknijGuzikStrzalkiWDol(int ilosc)
        {
            int cnt =0;
            var guzik = Find<SpanTag>(string.Format("//span[@class~'x-btn-icon-el x-form-itemselector-down']"));
            while (cnt < ilosc)
            {
                DebugLog("kliknięcie w strzałkę w dół");
                guzik.Click();
                cnt++;
            }
        }

        public void KliknijGuzikPrzesunNaSamaGore()
        {
            var guzik = Find<SpanTag>(string.Format("//span[@class~'x-btn-icon-el x-form-itemselector-top']"));
            DebugLog("kliknięcie w strzałkę przesuń na pierwszą pozycję");
            guzik.Click();
        }

        public void KliknijGuzikPrzesunNaOstatniaPozycje()
        {
            var guzik = Find<SpanTag>(string.Format("//span[@class~'x-btn-icon-el x-form-itemselector-bottom']"));
            DebugLog("kliknięcie w strzałkę przesuń na ostatnią pozycję");
            guzik.Click();
        }

        #endregion

        public void KliknijWelementPoLewejStronieRaz(string nazwaElementu)
        {
            var elementPoLewej = Find<DivTag>(string.Format("/{0}//div[@innertext='{1}']", PathToSwitchPanelLeft, nazwaElementu));
            elementPoLewej.Click();
        }
        
        public bool SprawdzCzyPoLewejStronieJestElement(string nazwaElementu)
        {
            var x = PobierzLewegoLubPrawegoGrida("lewa");
            if (x.SprawdzCzyZawieraInformacje(nazwaElementu))
                return true;

            return false;
        }

        public bool SprawdzCzyPoPrawejStronieJestElement(string nazwaElementu)
        {
            var x = PobierzLewegoLubPrawegoGrida("prawa");
            if (x.SprawdzCzyZawieraInformacje(nazwaElementu))
                return true;

            return false;
        }

        public void KliknijElementPoLewejStronieDwaRazy(string nazwaElementu, bool identyczny = true)
        {
            var operat = identyczny ? "=" : "~";
            var elementPoLewej = Find<DivTag>(string.Format("/{0}//div[@innertext{1}'{2}']", PathToSwitchPanelLeft, operat , nazwaElementu));
            elementPoLewej.DoubleClick();
        }

        public void KliknijWElementPoLewejStronieRaz(string nazwaElementu, bool identyczny = true)
        {
            var operat = identyczny ? "=" : "~";
            var elementPoLewej = Find<DivTag>(string.Format("/{0}//div[@innertext{1}'{2}']", PathToSwitchPanelLeft, operat, nazwaElementu));
            elementPoLewej.Click();
        }

        public void KliknijWElementPoPrawejStronieDwaRazy(string nazwaElementu, bool identyczny = true)
        {
            var operat = identyczny ? "=" : "~";
            var elementPoPrawej = Find<DivTag>(string.Format("/{0}//div[@innertext{1}'{2}']", PathToSwitchPanelRight, operat, nazwaElementu));
            elementPoPrawej.DoubleClick();
        }

        public void KliknijWElementPoPrawejStronieRaz(string nazwaElementu, bool identyczny = true)
        {
            var operat = identyczny ? "=" : "~";
            var elementPoPrawej = Find<DivTag>(string.Format("/{0}//div[@innertext{1}'{2}']", PathToSwitchPanelRight, operat, nazwaElementu));
            elementPoPrawej.Click();
        }

        public void KliknijWpoleWyszukaj(string wartosc)
        {
            DebugLog("Kliknij pole Wyszukaj");
            var wyszukaj = Find<InputTag>(string.Format("//input[@id='txtSearch-inputEl']"));
            wyszukaj.Click();
            Keyboard.Press(wartosc);
        }

        public void KliknijWGuzikPokaz()
        {
            DebugLog("Kliknij guzik Pokaż");
            var pokaz = Find<SpanTag>(string.Format("//span[@id='uczniowie-search-button-btnIconEl']"));
            pokaz.Click();
        }

        public void KliknijWKolumneSkreslonyPrzyUczniu(string uczen, string czySkreslony, string zmiana)
        {
            DebugLog("Kliknij w kolumne skreślony");
            var skreslony = Find<DivTag>(string.Format("//div[@innertext='{0}']", uczen));
            skreslony.MoveTo();
            var kolumnaSkreslony = skreslony.Parent.Parent.FindDescendants<TdTag>()[1].FindSingle<DivTag>(string.Format("./div[@innertext='{0}']" ,czySkreslony));
            kolumnaSkreslony.Click();

            DebugLog(string.Format("Zmiana statusu skreślony na: {0}", zmiana));
            var z = Find<InputTag>(".//input[Name~'Skreslony']");
            
            SelectFromCombo(z, zmiana);
        }

       
    }
}
