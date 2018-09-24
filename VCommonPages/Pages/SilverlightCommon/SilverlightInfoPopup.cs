using System;
using System.Linq;
using Ranorex;
using VRanorexLib;

namespace VCommonPages.Pages.SilverlightCommon
{
    public class SilverlightInfoPopup : SilverlightPage
    {

        private string _prefix()
        {
            return KtoryPopup == -1
                       ? ".//form"
                       : String.Format(".//form[@ChildIndex='{0}']/form", KtoryPopup);
        }

        public int KtoryPopup
        {
            get;
            set;
        }

        public enum ButtonTypes
        {
            OK,
            TAK,
            NIE,
            ANULUJ
        }

        private string NazwaGuzika(ButtonTypes enButton)
        {
            switch (enButton)
            {
                case ButtonTypes.OK:
                    return "OK";

                case ButtonTypes.TAK:
                    return "TAK";

                case ButtonTypes.NIE:
                    return "NIE";

                case ButtonTypes.ANULUJ:
                    return "ANULUJ";
            }
            return string.Empty;

        }

        public SilverlightInfoPopup(WebDocument openWebsite, int? browserId)
            : base(openWebsite, browserId)
        {
            OpenWebsite = openWebsite;
            BrowserId = browserId;
            KtoryPopup = -1;
        }



        public void KliknijGuzikOK(bool waitForLoad = true)
        {
            if (waitForLoad)
                WaitForLoad();
            KliknijGuzik(ButtonTypes.OK,waitForLoad);
        }

        public SilverlightInfoPopup KliknijGuzikTak(bool waitForLoad = true)
        {
            KliknijGuzik(ButtonTypes.TAK, waitForLoad);
            return new SilverlightInfoPopup(OpenWebsite, BrowserId);
        }

        //dla popupów które zwracają okienko edytora
        //zrobi sie kiedy znajdzie sie więcej takich i zobaczy co mają ze sobą wspólnego - LK 06.02
        //public SilverlightHybridWindow KliknijGuzikTakHybridWindow(string naglowekOkna="")
        //{
        //    KliknijGuzik(ButtonTypes.TAK);
        //    WaitForLoad();
        //    return new SilverlightHybridWindow(OpenWebsite,BrowserId,naglowekOkna,fields???);
        //}


        protected void KliknijGuzik(ButtonTypes enumGuzik, bool waitForLoad = true)
        {
            Helper.DebugLog(string.Format("Kliknij guzik: {0}", NazwaGuzika(enumGuzik)));
            PobierzGuzik(enumGuzik).Click();
            if (waitForLoad)
                WaitForLoad();
        }

        public void ZamknijPopup()
        {
            Helper.DebugLog("Kliknij ikonę zamknięcia okna");
            OpenWebsite.FindSingle<Button>(_prefix() + "/button[@automationid='CloseButton']").Click();
        }

        public string PobierzTekstPopupa()
        {
            return PobierzTextBox().TextValue;
        }

        public string PobierzNumerDokumentuZPopupa()
        {
            return PobierzTekstPopupa().Split().Last();
        }

        private Button PobierzGuzik(ButtonTypes enumGuzik)
        {
            switch (enumGuzik)
            {
                case ButtonTypes.OK:
                    return OpenWebsite.FindSingle<Button>(_prefix() + "/button[@name='OK']", new Duration(30000));

                case ButtonTypes.TAK:
                    return OpenWebsite.FindSingle<Button>(_prefix() + "/button[@name='Tak']", new Duration(30000));
            }
            return null;


        }




        private Text PobierzTextBox()
        {
            Helper.DebugLog("Pobierz pole tekstowe okna dialogowego");
            return OpenWebsite.FindSingle<Text>(_prefix() + "/container[@automationid='scrollViewer']/container/text", 30000);
        }

        public bool JestWidoczne()
        {
            Form okno;
            bool found = OpenWebsite.TryFindSingle<Form>(".//form/form[@visible='true']", 500, out okno);
            return found;
        }

    }
}