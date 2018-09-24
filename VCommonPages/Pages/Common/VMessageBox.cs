using Ranorex;

namespace VCommonPages.Pages.Common
{
    /// <summary>
    /// Klasa do obsługi okienek dialogowych monitów
    /// </summary>
    public class VMessageBox: Page
    {
        public enum ButtonTypes
        {
            OK,
            TAK,
            NIE,
            ANULUJ,
            ZATWIERDZ
        }

        private string NazwaGuzika(ButtonTypes enButton)
        {
            switch (enButton)
            {
                case ButtonTypes.OK:
                    return "OK";
                case ButtonTypes.TAK:
                    return "Tak";
                case ButtonTypes.NIE:
                    return "Nie";
                case ButtonTypes.ANULUJ:
                    return "Anuluj";
                case ButtonTypes.ZATWIERDZ:
                    return "Zatwierdź";
            }
            return string.Empty;

        }

        private DivTag _windowDialog;
        public DivTag OknoDialogowe
        {
            get { return _windowDialog; }
        }

        private WebDocument _webDoc;
        public WebDocument WebDoc
        {
            get { return _webDoc; }
        }

        private int? _browserId;
        public int? BrowserId
        {
            get { return _browserId; }
        }

        public string IdTabeli
        {
            get { return "messagebox"; }
        }


        public VMessageBox(WebDocument openWebsite, int? browserId) : base(openWebsite, browserId)
        {
            WaitForLoad();
            this._webDoc = openWebsite;
            this._browserId = browserId;

            DebugLog("Odszukaj okno komunikatów");
            this._windowDialog = this.WebDoc.FindSingle<DivTag>(".//div[@id~'^messagebox' and @class~'x-message-box']");
        }

        public bool CzyZawieraKomunikat(string tekst)
        {
            HtmlTable table = PobierzTabelke();
            return table.SprawdzCzyZawieraInformacje(tekst);
        }

        public string PobierzKomunikat()
        {
            HtmlTable table = PobierzTabelke();
            if (table != null && table.TabelkaInternal.Count > 0)
            {
                var txt = "";
                foreach (var l in table.TabelkaInternal)
                    txt += string.Join("", l);
                return txt;
            }
            else
                return "";
        }

        private HtmlTable PobierzTabelke()
        {
            DebugLog("Pobierz tabelkę: " + this.IdTabeli);
            return new HtmlTable(this.WebDoc, this.IdTabeli, BrowserId, true);
        }

        private SpanTag PobierzGuzik(ButtonTypes enumGuzik)
        {
            string sciezkaDoWybranegoGuzika = string.Format(".//span[@class~'{0}' and @innertext='{1}']", "x-btn-inner", NazwaGuzika(enumGuzik));
            return _windowDialog.FindSingle<SpanTag>(sciezkaDoWybranegoGuzika);
        }

        private ImgTag PobierzGuzikZamknieciaOkna()
        {
            string sciezkaDoWybranegoGuzika = string.Format("//div[@Id~'messagebox']//img[@class~'{0}']", "x-tool-img x-tool-close");
            return Find<ImgTag>(sciezkaDoWybranegoGuzika);
        }

        public void KliknijGuzikOK()
        {
            KliknijGuzik(ButtonTypes.OK);
        }

        public void KliknijGuzikNie()
        {
            KliknijGuzik(ButtonTypes.NIE);
        }

        public void KliknijGuzikTak()
        {
            KliknijGuzik(ButtonTypes.TAK);
        }

        public void KliknijGuzikAnuluj()
        {
            KliknijGuzik(ButtonTypes.ANULUJ);
        }

        public void KliknijGuzikZatwierdz()
        {
            KliknijGuzik(ButtonTypes.ZATWIERDZ);
        }

        public void KliknijGuzik(ButtonTypes enumGuzik)
        {
            DebugLog(string.Format("Kliknij guzik: {0}", NazwaGuzika(enumGuzik)));
            PobierzGuzik(enumGuzik).Click();

            WaitForLoad();
        }

        public void ZamknijOknoKomunikatow()
        {
            DebugLog("Kliknij ikonę zamknięcia okna");
            PobierzGuzikZamknieciaOkna().Click();

            WaitForLoad();
        }

    }
}
