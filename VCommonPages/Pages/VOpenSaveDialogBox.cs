using Ranorex;
using VRanorexLib;

namespace VCommonPages.Pages
{
    /// <summary>
    /// Klasa odo obsługi okna moalnego Open/Save
    /// </summary>
    public class VOpenSaveDialogBox : Page
    {

        private Host _app;
        public enum ButtonTypes
        {
            OTWORZ,
            ZAPISZ,
            ANULUJ
        }

        private string NazwaGuzika(ButtonTypes enButton)
        {
            switch (enButton)
            {
                case ButtonTypes.OTWORZ:
                    return "Otworz";
                case ButtonTypes.ZAPISZ:
                    return "Zapisz";
                case ButtonTypes.ANULUJ:
                    return "Anuluj";
            }
            return string.Empty;

        }

        public VOpenSaveDialogBox()
        {
            this._app = Host.Local;
        }



        public void KliknijGuzikOtworz()
        {
            KliknijGuzik(ButtonTypes.OTWORZ);
        }

        public void KliknijGuzikZapisz()
        {
            KliknijGuzik(ButtonTypes.ZAPISZ);
        }

        public void KliknijGuzikAnuluj()
        {
            KliknijGuzik(ButtonTypes.ANULUJ);
        }

        public void KliknijGuzik(ButtonTypes enumGuzik)
        {
            Helper.DebugLog(string.Format("Kliknij guzik: {0}", NazwaGuzika(enumGuzik)));
            PobierzGuzik(enumGuzik).Click();

            //WaitForLoad();
        }

        public void ZamknijOknoKomunikatow()
        {
            Helper.DebugLog("Kliknij ikonę zamknięcia okna");
            PobierzGuzikZamknieciaOkna().Click();
        }

        public void OtworzPlik(string nazwaPliku)
        {
            DebugLog("Otwórz plik");
            var txtb = PobierzTextBox();
            Helper.DebugLog(string.Format("Wpisz nazwę pliku do pobrania: {0}", nazwaPliku));
            txtb.PressKeys(nazwaPliku);
            Helper.DebugLog("Kliknij guzik Otwórz");
            PobierzGuzik(ButtonTypes.OTWORZ).Click();
        }

        private Ranorex.Button PobierzGuzik(ButtonTypes enumGuzik)
        {
            switch(enumGuzik)
            {
                case ButtonTypes.ZAPISZ:
                    return this._app.FindSingle<Ranorex.Button>("form/button[@text~'.apisz' or @text~'.ave']", new Duration(30000));
                case ButtonTypes.OTWORZ:
                    return this._app.FindSingle<Ranorex.Button>("form/button[@text~'.twórz' or @text~'.pen']", new Duration(30000));
                case ButtonTypes.ANULUJ:
                    return this._app.FindSingle<Ranorex.Button>("form/button[@text~'.nuluj' or @text~'.ancel']", new Duration(30000));
            }
            return null;
        }

        private Ranorex.Button PobierzGuzikZamknieciaOkna()
        {
            return this._app.FindSingle<Ranorex.Button>("form/titlebar/button[@accessiblename='Zamknij' or @accessiblename='Close']");
        }


        private Ranorex.Text PobierzTextBox()
        {
            Helper.DebugLog("Pobierz pole tekstowe okna dialogowego");
            return this._app.FindSingle<Text>("form/combobox/text", new Duration(8000));
        }

    }
}
