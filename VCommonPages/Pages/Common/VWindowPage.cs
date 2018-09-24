using System;
using Ranorex;

namespace VCommonPages.Pages.Common
{
    /// <summary>
    /// Klasa do obsługi okien modalnych
    /// </summary>
    public abstract class VWindowPage : Page
    {
        public enum ButtonTypes
        {
            ZAPISZ,
            ANULUJ,
            ZMIEN,
            DALEJ,
            WSTECZ,
            USUN,
            OK
        }

        private string NazwaGuzika(ButtonTypes enButton)
        {
            switch(enButton)
            {
                case ButtonTypes.ANULUJ:
                    return "Anuluj";
                case ButtonTypes.ZAPISZ:
                    return "Zapisz";
                case ButtonTypes.ZMIEN:
                    return "Zmien";
                case ButtonTypes.DALEJ:
                    return "Dalej";
                case ButtonTypes.WSTECZ:
                    return "Wstecz";
                case ButtonTypes.USUN:
                    return "Usuń";
                case ButtonTypes.OK:
                    return "OK";
            }
            return string.Empty;

        }

        private DivTag _windowDialog;
        public DivTag OknoDialogowe
        {
            get { return _windowDialog; }
        }

        protected VWindowPage(WebDocument openWebsite, int? browserId, string idOkna, bool bezNaglowka=false,bool visibleOnly=false)
            : base(openWebsite, browserId)
        {
            WaitForLoad();
            SpanTag _headerWindow;
            String path = String.Empty;
            if(!bezNaglowka)
                path=string.Format(".//div[@class~'x-window']//span[@innertext='{0}' and @class~'x-window-header-text']", idOkna);
            else
            {
                path=string.Format(".//div[@class~'x-window']//span[@class~'x-window-header-text']"); //w paisie, okno edycyjne bez naglowka, bez innertextu - Tadzio, 11.03
            }
            if(visibleOnly)
            {
                path += "[@Visible='true']";
            }

            _headerWindow = OpenWebsite.FindSingle<SpanTag>(path);
            _windowDialog = _headerWindow.Parent.Parent.Parent.Parent.Parent.Parent.As<DivTag>(); 

        }

        public SpanTag PobierzGuzik(ButtonTypes enumGuzik)
        {

            string sciezkaDoWybranegoGuzika = string.Format(".//span[@class~'{0}' and @innertext='{1}']", "x-btn-inner", NazwaGuzika(enumGuzik));
            return _windowDialog.FindSingle<SpanTag>(sciezkaDoWybranegoGuzika);
            

            // Kod użyty do zdebugowania testów które wywalały sie tylko na buildzie nocnym. Zostawiam bo może komus się przyda.-Adrian A. 
            //DebugLog(string.Format(">>>Jestem w : {0}", _windowDialog.GetPath()));
            //DebugLog(string.Format(">>>Sciezka: : {0}", sciezkaDoWybranegoGuzika));
            //try
            //{
            //    var elem = _windowDialog.FindSingle<SpanTag>(sciezkaDoWybranegoGuzika);
            //    return elem;
            //}catch(ElementNotFoundException ex)
            //{
            //    DebugLog(string.Format(">>>Element Not found exception : {0}", ex));
            //    DebugLog("Próba znalezienia jakiegokolwiek //span z ww innertextem");
            //    try
            //    {
            //        try
            //        {
            //            var elem =
            //                _windowDialog.Find<SpanTag>(string.Format(".//span[@innertext='{0}']",
            //                                                          NazwaGuzika(enumGuzik)));
            //            DebugLog("Znaleziono : " + elem.Count);
            //            for(var i=0;i<elem.Count;i++){
            //                Element ele = elem.ToList()[i].Element;
            //                foreach (var atr in ele.Attributes)
            //                {
            //                    DebugLog("Attribute: :" + atr + "=> " + ele.GetAttributeValueText(atr.ToString()));
            //                }
            //            }
            //            if (elem.Count == 0 )
            //            {
            //                throw;
            //            }
            //        }
            //        catch (ElementNotFoundException)
            //        {
            //            DebugLog("Nie udało sie");
            //            DebugLog("Próba znalezienia jakiegokolwiek //span");
            //            var elem = _windowDialog.Find<SpanTag>(".//span");
            //            DebugLog("Znaleziono : " + elem.Count);
            //            for (var i = 0; i < elem.Count; i++)
            //            {
            //                Element ele = elem.ToList()[i].Element;
            //                DebugLog("Element nr "+i);
            //                foreach (var atr in ele.Attributes)
            //                {
            //                    DebugLog("Attribute :" + atr + "=> "  + ele.GetAttributeValueText(atr.ToString()));
            //                }
            //            }
            //            throw;
            //        }
            //        throw;
            //    }
            //    catch(ElementNotFoundException)
            //        {
            //            DebugLog("Drzewo:");
            //            traverse(_windowDialog.Element);
            //            throw;
            //        }
            //}
        }
        //private void traverse(Element e,int level=0)
        //{
        //    var off = new string('-', level*3);
        //    var elem = e.Children;
        //    foreach(var ele in elem)
        //    {
        //        foreach (var atr in ele.Attributes)
        //        {
        //            DebugLog(off + "Attribute" + ele.ChildIndex + " :" + atr + "=> " + ele.GetAttributeValueText(atr.ToString()));
        //        }
        //        for (var i = 0; i < ele.Children.Count;i++)
        //        {
        //            traverse(ele.Children[i],level++);
        //        }

        //    }
        //}

        public ImgTag PobierzGuzikZamknieciaOkna()
        {
            string sciezkaDoWybranegoGuzika = string.Format(".//img[@class~'{0}']", "x-tool-img x-tool-close");
            return _windowDialog.FindSingle<ImgTag>(sciezkaDoWybranegoGuzika);
        }

        public void KliknijGuzikZapisz(int waitingTime=10000)
        {
            KliknijGuzik(ButtonTypes.ZAPISZ, waitingTime);
        }

        public void KliknijGuzikAnuluj()
        {
            KliknijGuzik(ButtonTypes.ANULUJ);
        }

        public void KliknijGuzikDalej(int waitingTime=10000)
        {
            KliknijGuzik(ButtonTypes.DALEJ, waitingTime);
        }

        public void KliknijGuzikWstecz(int waitingTime = 10000)
        {
            KliknijGuzik(ButtonTypes.WSTECZ, waitingTime);
        }

        public void KliknijGuzikUsun()
        {
            KliknijGuzik(ButtonTypes.USUN);
        }

        public void KliknijGuzikOK()
        {
            KliknijGuzik(ButtonTypes.OK);
        }

        public void KliknijGuzikZamknieciaOkna()
        {
            PobierzGuzikZamknieciaOkna().Click();
        }

        protected void KliknijGuzik(ButtonTypes enumGuzik, int waitingTime=10000)
        {
            DebugLog(string.Format("Kliknij guzik: {0}", NazwaGuzika(enumGuzik)));
            PobierzGuzik(enumGuzik).Click();

            WaitForLoad(waitingTime);
        }
    }
}
