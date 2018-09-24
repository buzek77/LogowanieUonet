using Ranorex;
using VRanorexLib;

namespace VCommonPages.Pages
{
    /// <summary>
    /// Klasa do obsługi okienka modalnego Print
    /// </summary>
    public class VPrintDialogBox : Page
    {

        private Host _app;
        public enum ButtonTypes
        {
            DRUKUJ,
            ZASTOSUJ,
            ANULUJ
        }

        private string NazwaGuzika(ButtonTypes enButton)
        {
            switch (enButton)
            {
                case ButtonTypes.DRUKUJ:
                    return "Drukuj";
                case ButtonTypes.ZASTOSUJ:
                    return "Zastosuj";
                case ButtonTypes.ANULUJ:
                    return "Anuluj";
            }
            return string.Empty;

        }

        public VPrintDialogBox()
        {
            this._app = Host.Local;
        }



        public void KliknijGuzikDrukuj()
        {
            KliknijGuzik(ButtonTypes.DRUKUJ);
        }

        public void KliknijGuzikZastosuj()
        {
            KliknijGuzik(ButtonTypes.ZASTOSUJ);
        }

        public void KliknijGuzikAnuluj()
        {
            KliknijGuzik(ButtonTypes.ANULUJ);
        }

        public void KliknijGuzik(ButtonTypes enumGuzik)
        {
            Helper.DebugLog(string.Format("Kliknij guzik: {0}", NazwaGuzika(enumGuzik)));
            var ctrl = PobierzGuzik(enumGuzik);
            ctrl.Click();
        }

        public void ZamknijOknoKomunikatow()
        {
            Helper.DebugLog("Kliknij ikonę zamknięcia okna");
            PobierzGuzikZamknieciaOkna().Click();
        }

        private Ranorex.Button PobierzGuzik(ButtonTypes enumGuzik)
        {
            switch(enumGuzik)
            {
                case ButtonTypes.ZASTOSUJ:
                    return _app.FindSingle<Ranorex.Button>("form/button[@text~'.astosuj' or @text~'.aply']", new Duration(30000));
                case ButtonTypes.DRUKUJ:
                    return this._app.FindSingle<Ranorex.Button>("form/button[@text~'.rukuj' or @text~'.rint']", new Duration(30000));
                case ButtonTypes.ANULUJ:
                    return _app.FindSingle<Ranorex.Button>("form/button[@text~'.nuluj' or @text~'.ancel']", new Duration(30000));
            }
            return null;
        }

        private Ranorex.Button PobierzGuzikZamknieciaOkna()
        {
            return _app.FindSingle<Ranorex.Button>("form/titlebar/button[@accessiblename='Zamknij' or @accessiblename='Close']");
        }


        private Ranorex.Text PobierzTextBox()
        {
            Helper.DebugLog("Pobierz pole tekstowe okna dialogowego");
            return this._app.FindSingle<Text>("form/combobox/text");
        }

    }
}
