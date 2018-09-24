using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Ranorex;
using Ranorex.Core;
using VRanorexLib;
using System.Globalization;

namespace VCommonPages.Pages
{
    public class Page
    {
        // sciezki (tu bedziemy eksperymetowac czy nie beda gdzie idziej
        private const string DomPath = "/dom[@domain='{0}']";

        public Helper Helper = new Helper();
        protected int? BrowserId = null;
        protected WebDocument OpenWebsite = null;

        public Page()
        {
            OpenWebsite = null;
            BrowserId = null;
        }
        private string pathToSwitchPanelLeft = "/div[@id~'vswitchpanel' and id~'-targetEl']/div[@id~'vgrid' and childindex=0]";
        public string PathToSwitchPanelLeft
        {
            get { return pathToSwitchPanelLeft; }
            set { pathToSwitchPanelLeft = value; }
        }

        /// <summary>
        /// Scieżka do prawego grida
        /// </summary>
        private string pathToSwitchPanelRight = "/div[@id~'vswitchpanel' and id~'-targetEl']/div[@id~'vgrid' and childindex=2]";
        public string PathToSwitchPanelRight
        {
            get { return pathToSwitchPanelRight; }
            set { pathToSwitchPanelRight = value; }
        }

        public bool IsBrowserOpen()
        {
            var stanBrowsera = !(BrowserId == null || OpenWebsite == null || OpenWebsite.PageUrl == null);
            DebugLog(string.Format("Browser open : {0}", stanBrowsera ? "tak" : "nie") ); 
            return stanBrowsera;
        }

        public Page(WebDocument openWebsite,int? browserId)
        {
            OpenWebsite = openWebsite;
            BrowserId = browserId;
        }

        // nie ma errologa w witrynach page jesli jest erro musi wyleciec do testu i tam byc zafielowany
        public static void InfoLog(string message)
        {
            Helper.InfoLog(message);
        }

        public static void DebugLog(string message)
        {
            Helper.DebugLog(message);
        }

        protected T Find<T>(string xPath,int miliseconds = 30000) where T : Adapter
        {
            string correctPath = "./" + xPath;
            return OpenWebsite.FindSingle<T>(correctPath, new Duration(miliseconds));
        }

        protected ATag GetATagByText(String text)
        {
            return Find<ATag>(String.Format("/a[@innertext='{0}']",text));
        }
        
        protected SelectTag GetSelectTagById(string id)
        {
            return Find<SelectTag>(string.Format("/select[#'{0}']", id));
        }

        protected SpanTag GetSpanTagByText(string tekst, bool equal = true)
        {
            if(equal)
                return Find<SpanTag>(string.Format("/span[@InnerText = '{0}']", tekst));
            else
                return Find<SpanTag>(string.Format("/span[@InnerText ~ '{0}']", tekst));
        }

        protected SpanTag GetSpanTagById(string id)
        {
            return Find<SpanTag>(string.Format("/span[@id~'{0}']", id));
        }

        protected SpanTag GetSpanTagByDivAndText(string div,string tekst)
        {
            return Find<SpanTag>(string.Format("/div[#'{0}']//span[@InnerText = '{1}']",div,tekst));
        }

        protected SpanTag GetSpanTagByDivAndText(string div, string divMiddle, string tekst)
        {
            return Find<SpanTag>(string.Format("/div[#'{0}']{1}//span[@InnerText = '{2}']", div, divMiddle, tekst));
        }


        protected SpanTag GetSpanTagByDivAndId(string div, string id)
        {
            return Find<SpanTag>(string.Format("/div[#'{0}']//span[#'{1}']", div, id));
        }

        protected TdTag GetTdTagByText(string text)
        {
            return Find<TdTag>(string.Format("/td[@innertext = '{0}']", text));
        }

        protected InputTag GetInputTagByText(string text)
        {
            return Find<InputTag>(string.Format("/input[@innertext = '{0}']", text));
        }

        protected InputTag GetInputTagById(string id)
        {
            return Find<InputTag>(string.Format("/input[@Id~'{0}']", id));
        }

        protected InputTag GetInputTagByName(string name)
        {
            return Find<InputTag>(string.Format("/input[@name = '{0}']", name));
        }

        protected LabelTag GetLabelTagByText(string text)
        {
            return Find<LabelTag>(string.Format("/label[@innertext = '{0}']", text));
        }

        protected DivTag GetDivTagByText(string text, string myPath="")
        {
            return Find<DivTag>(string.Format("{1}/div[@innertext = '{0}']", text, myPath));
        }

        protected DivTag GetDivTagById(string id)
        {
            return Find<DivTag>(string.Format("/div[@id = '{0}']", id));
        }

        protected ImgTag GetImgTagByText(string text)
        {
            return Find<ImgTag>(string.Format("/img[@id='{0}']", text));
        }

        protected List<InputTag> GetComboboxList(string id)
        {
            return OpenWebsite.Find<InputTag>(string.Format(".//input[Id~'{0}']", id)).ToList();
        }

        protected List<SpanTag> GetAllSpanTagById(string id,bool visible=true) //20.11_Pau - erato
        {
            if(visible)
                return OpenWebsite.Find<SpanTag>(string.Format(".//span[@id~'{0}'][@visible='True']", id)).ToList();
            return OpenWebsite.Find<SpanTag>(string.Format(".//span[@id~'{0}']", id)).ToList();
        }

        protected IList<SpanTag> GetAllSpanTagByText(string name, bool visible=true) //20.11_Pau - erato
        {
            if(visible)
            return OpenWebsite.Find<SpanTag>(string.Format(".//span[@InnerText='{0}' and @visible='true']", name));
            return OpenWebsite.Find<SpanTag>(string.Format(".//span[@InnerText='{0}']", name));
        }

        protected List<LabelTag> PobierzWszystkieLabelkiZDiva(string id)//dg
        {
            return OpenWebsite.Find<LabelTag>(string.Format(".//div[#'{0}']//label[Id~'label']", id)).ToList();
        }

        protected List<TdTag> PobierzWszystkieTdTagiZDiva(string id)//dg
        {
            return OpenWebsite.Find<TdTag>(string.Format(".//div[#'{0}']//td", id)).ToList();
        }

        protected void SelectFromCombo(InputTag combo, string value, Boolean onlyVisible=true)
        {
            WaitForLoad();
            combo.Click();
            WaitForLoad();
            Keyboard.Press(Keys.Down);
            WaitForLoad();
            //Find<LiTag>(string.Format("/div[@Id~'boundlist' and @visible='true']//li[@InnerText='{0}']", value)).Click();
            Find<LiTag>(string.Format("/div[@Id~'boundlist' {1}]//li[@InnerText='{0}']", value, onlyVisible ? "and @visible='true'" : "")).Click();
            WaitForLoad();
        }

        protected void SelectFromCombo(string comboId, string value, Boolean onlyVisible = true)
        {
            WaitForLoad();
            SelectFromCombo(Find<InputTag>(string.Format("/input[@Id~'{0}']", comboId)),value, onlyVisible);
        }


        protected void SelectFromTimePicker(InputTag timepicker, string value)
        {
            WaitForLoad();
            timepicker.Click();
            WaitForLoad();
            Keyboard.Press(Keys.Down);
            WaitForLoad();
            Find<LiTag>(string.Format("/div[@Id~'timepicker' and @visible='true']//li[@InnerText='{0}']", value)).Click();
            WaitForLoad();
        }


        protected List<LiTag> SelectFromComboListItems(string comboInputTag)
        {
            var inputTagCombo = Find<InputTag>(string.Format("/input[@Id~'{0}']", comboInputTag));
            WaitForLoad();

            DebugLog("Rozwiń listę elementów combo");
            inputTagCombo.Click();
            Keyboard.Press(Keys.Down);
            WaitForLoad();

            DebugLog("Pobierz elementy listy combo");
            List<LiTag> lst = OpenWebsite.Find<LiTag>("./body/div[@Id~'boundlist' and @visible='true']//li").ToList();

            return lst;
        }


        protected void WaitForLoad(int waitingTime=10000)
        {
            int i = 0;
            while (Find<DivTag>("/div[@class~'x-mask-loading']", waitingTime).Visible)
            {
                System.Threading.Thread.Sleep(1000);
                i++;
                if(i>30) throw new Exception("Proszę czekać jest za długo");
            }

        }

        protected void OpenBrowser(string url,string urlToFind = "")
        {
            if (BrowserId != null) CloseBrowser();
            
            InfoLog("Otwarcie okna przegladarki");
            InfoLog("url: "+ (string.IsNullOrEmpty(url) ? "brak url" : url));
            BrowserId = Host.Local.OpenBrowser(url, "ie", true, true);
            string pageUrl = GetUrlWithoutHttp(url);
            
            Host.Local.GetApplicationForm((int)BrowserId, 15000).Maximize();
            
            if (urlToFind != "") pageUrl = GetUrlWithoutHttp(urlToFind);

            bool found = Host.Local.TryFindSingle(string.Format(DomPath, pageUrl), new TimeSpan(0, 0, 90), out OpenWebsite);
            if (!found)
            {
                throw new Exception("Nie znaleziono paga z aplikacja");
            }
            OpenWebsite.WaitForDocumentLoaded(new Duration(60000));
        }

        public static string GetUrlWithoutHttp(string url)
        {
            string[] paths = url.Replace("http://", "").Replace("https://", "").Split('/');
            string pageUrl = paths[0];
            return pageUrl;
        }

        public void Reload()
        {
            DebugLog("RELOAD");
            if(OpenWebsite==null)
            {
                DebugLog("OpenWebsite null");
            }  
            else if (OpenWebsite.PageUrl==null)
            {
                DebugLog("OpenWebsite.PageUrl null");
            }
            OpenWebsite.Navigate(OpenWebsite.PageUrl);
            DebugLog("AFTER NAVIGATE");
            OpenWebsite.WaitForDocumentLoaded(new Duration(60000));
        }

        public void CloseBrowser()
        {
            if (BrowserId == null) return;
            Host.Local.CloseApplication((int)BrowserId);
            BrowserId = null;
        }

        public void GoToSite(string url)
        {
            OpenWebsite = string.Format(DomPath, url); // TODO
            OpenWebsite.Navigate(url);
            OpenWebsite.WaitForDocumentLoaded();
        }

        protected void WybierzZDatePicera(string id, string date, int firstYear=2011, int lastYear=2020)
        {
            DebugLog("Szukam inputTaga o id:"+id);
            var inputTag = Find<InputTag>(string.Format("/input[#'{0}']", id));
            WybierzZDatePicera(inputTag,date,firstYear,lastYear);
        }


        /// <summary>
        /// Funkcja aktywuje kontrolkę datepicker'a, a następnie wybiera wskazaną datę
        /// </summary>
        /// <param name="id">identyfikator pola typu input kontrolki kalendarza</param>
        /// <param name="date">data którą chcemy wybrać</param>
        protected void WybierzZDatePicera(InputTag inputTag, string date, int firstYear=2011, int lastYear=2020)
        {

            DebugLog("Wybieram date : " + date);

            // Sprawdz jakiego rodzaju kontrolka kalenarza
            // Jeśli pole Input (id) nie aktywuje kontrolki kalendarza
            // trzeba ją znaleźć
            if(!inputTag.Class.Contains("x-trigger-noedit"))
            {
                var trTag = inputTag.Parent.Parent;
                var divTag = trTag.FindSingle<DivTag>(".//td[2]/div");
                divTag.Click();
            }
            else
            {
                inputTag.Click();
            }

            string[] months = { "Sty", "Lut", "Mar", "Kwi", "Maj", "Cze", "Lip", "Sie", "Wrz", "Paź", "Lis", "Gru" };
            string[] parseDate = date.Split('.');

            // Odszukaj id kontrolki datepicker-*
            var idDatePicker = Find<DivTag>("body/div[@id~'datepicker-' and @Visible='True']").Id;

            // kliknij w selektor miesiąca
            Find<SpanTag>(string.Format("/div[#'{0}']//span[@id~'splitbutton-*' and Visible='True']", idDatePicker)).Click();

            System.Threading.Thread.Sleep(1000); // K.O - żeby się nie pogubił

            // wybierz rok
            // Uwaga, na liście lat kontrolki domyślnie są lata 2011-2020
            ShiftListYears(idDatePicker, parseDate[2], firstYear, lastYear);
            Find<ATag>(string.Format("/div[#'{0}']//div[@Id~'yearEl' and Visible='True']//a[@innertext='{1}']", idDatePicker, parseDate[2])).Click();

            // wybierz miesiąc
            Find<ATag>(string.Format("/div[#'{0}']//a[@innertext='{1}']", idDatePicker, months[int.Parse(parseDate[1]) - 1])).Click();

            // kliknij guzik OK
            Find<SpanTag>(string.Format("/div[#'{0}']//span[@InnerText ~'OK']", idDatePicker)).Click();

            System.Threading.Thread.Sleep(2000); // K.O - żeby się nie pogubił

            // wybierz dzien
            Find<ATag>(string.Format("/div[#'{0}']//table[Id~'datepicker']//td[@class~'-active' and Visible='True']/a[@innertext='{1}']", idDatePicker, int.Parse(parseDate[0]).ToString())).Click();

            WaitForLoad();
        }

        /// <summary>
        /// f-cja klika kontrolki strzałek zmieniających zakres lat
        /// </summary>
        /// <param name="idDatePicker">identyfikator kontrolki z lista lat</param>
        /// <param name="yearToFind">rok który chcemy kliknac</param>
        /// <param name="firstYear">pierwszy rok na liście</param>
        /// <param name="lastYear">ostatni rok na liście</param>
        private void ShiftListYears(string idDatePicker, string yearToFind, int firstYear, int lastYear) // K.O
        {
            var year = int.Parse(yearToFind);
            if(firstYear>year || lastYear<year)
            {
                Boolean shiftLeft = (firstYear > year);
                var aTagPath = ".//div[#'{0}']//div[@Id~'yearEl' and Visible='True']//a[@id~'{1}']";
                // Ustal liczbe przesunięć
                var cnt = (shiftLeft ? firstYear - year : year - lastYear);
                var cntClicks = ((cnt - (cnt % 10)) / 10) + ((cnt % 10) > 0 ? 1 : 0);
                
                var aTag = Find<ATag>(string.Format(aTagPath, idDatePicker, (shiftLeft ? "prevEl" : "nextEl")));
                aTag.Click(cntClicks, new Duration(1000));

                System.Threading.Thread.Sleep(1000); // K.O - żeby się nie pogubił

            }

        }

        /// <summary>
        /// Funkcja aktywuje kontrolkę vtimepicker'a, a następnie wybiera wskazaną datę
        /// Uwaga! brak wartości parametru czas, spowoduje wybór guzika "TERAZ"
        /// </summary>
        /// <param name="id">identyfikator pola typu input kontrolki zegara</param>
        /// <param name="czas">data którą chcemy wybrać w formacie [hh:mm]</param>
        protected void WybierzZVtimepicker(InputTag inputTag, string czas="")
        {

            DebugLog(string.Format("Wybieram wskazany czas [hh:mm] : {0}", (czas == string.Empty ? "TERAZ" : czas)));

            var minuty = "";
            var godziny = "";
            
            if (!string.IsNullOrEmpty(czas))
            {
                int min = 0;
                int godz = 0;
                var godzinyMinuty = czas.Split(':');
                if (int.TryParse(godzinyMinuty[0], out godz) == false || godz > 24)
                {
                    DebugLog(string.Format("Podano niepoprawną wartość dla godzin: <{0}>", godzinyMinuty[0]));
                    return;
                }

                if (int.TryParse(godzinyMinuty[1], out min) == false || min > 59)
                {
                    DebugLog(string.Format("Podano niepoprawną wartość dla minut: <{0}>", godzinyMinuty[1]));
                    return;
                }

                // Minuty modulo 5!
                minuty = (min > 5 ? min - (min % 5) : (min % 5)).ToString().PadLeft(2, '0');
                godziny = godz.ToString().PadLeft(2, '0');

            }

            // Sprawdz jakiego rodzaju kontrolka kalenarza
            // Jeśli pole Input (id) nie aktywuje kontrolki kalendarza
            // trzeba ją znaleźć
            if (!inputTag.Class.Contains("x-trigger-noedit"))
            {
                var trTag = inputTag.Parent.Parent;
                var divTag = trTag.FindSingle<DivTag>(".//td[2]/div");
                divTag.Click();
            }
            else
            {
                inputTag.Click();
            }
                
            // Odszukaj id kontrolki vtimepicker-*
            var idTimePicker = Find<DivTag>("body/div[@id~'vtimepicker-' and @Visible='True']").Id;

            System.Threading.Thread.Sleep(1000); // K.O - żeby się nie pogubił

            if (!string.IsNullOrEmpty(czas))
            {
                // wybierz godzinę
                Find<ATag>(string.Format("/div[#'{0}']//td[@class~'hours']/a[@innertext='{1}']", idTimePicker, godziny)).Click();

                // wybierz minutę - kończy działanie kontrolki
                Find<ATag>(string.Format("/div[#'{0}']//td[@class~'minutes']/a[@innertext='{1}']", idTimePicker, minuty)).Click();

                // kliknij guzik OK
                //Find<SpanTag>(string.Format("/div[#'{0}']//span[@InnerText ~'OK']", idTimePicker)).Click();
            }
            else
            {
                // kliknij guzik TERAZ
                Find<SpanTag>(string.Format("/div[#'{0}']//span[@InnerText ~'TERAZ']", idTimePicker)).Click();
            }

            System.Threading.Thread.Sleep(2000); // K.O - żeby się nie pogubił

            WaitForLoad();
        }


        #region Funkcje dla ToolTip'a

        protected List<string> GetTooltipFromHtml(string html)
        {
            string[] rows = html.Split(new[] { "<br/>", "</br>", "<li>", "</li>" }, StringSplitOptions.None);

            List<string> lists = new List<string>();
            foreach (var row in rows)
            {
                lists.Add(row.Trim());
            }
            return lists;
        }

        /// <summary>
        /// F-cja sprawdza poprawność toolTipa zawierającego datę
        /// Sprawdza:
        /// 1. Czy zawiera we wskazanym wierszu datę
        /// 2. Czy data jest w ustalonym formacie "dd.MM.yyyy HH:mm"
        /// 3. opcjonalnie - tylko dla elementów edytowalnych, czy data w toooltipe jest późniejsza od wskazanej w parametrze dataPrzedZmiana
        /// 4. Czy pozostałe elementy są zgodne ze wzorcowym tooltipem
        /// </summary>
        /// <param name="toolTip">zawartośc ToolTip'a</param>
        /// <param name="labelWierszDaty">napis poprzedzający datę w tooltipie</param>
        /// <param name="toolTipPrzed">zawartość ToolTipa do porównania - domyśnie = null</param>
        /// <param name="toolTipWzorcowy">zawartość ToolTipa wzorcowego do porównania</param>
        /// <returns></returns>
        protected Boolean CheckDataInTooltipFromHtml(List<string> toolTip, List<string> toolTipWzorcowy, List<string> toolTipPrzed = null, string labelWierszDaty = "Data:")
        {
            var fmtDaty = "dd.MM.yyyy HH:mm";
            var data = System.DateTime.Now;
            var wiersz = toolTip.FindIndex(s => s.Contains(labelWierszDaty));
            if (wiersz >= 0)
            {
                // Sprawdź format daty z tooltipa : dd.MM.yyyy HH:mm
                if (System.DateTime.TryParseExact(toolTip[wiersz].Replace(labelWierszDaty, "").Trim(), fmtDaty, null, DateTimeStyles.None, out data))
                {
                    if (toolTipPrzed != null)
                    {
                        var dataPrzedZmiana = System.DateTime.Now;
                        var wiersz2 = toolTipPrzed.FindIndex(s => s.Contains(labelWierszDaty));
                        if (wiersz2 >= 0 && System.DateTime.TryParseExact(toolTipPrzed[wiersz2].Replace(labelWierszDaty, "").Trim(), fmtDaty, null,
                                                      DateTimeStyles.None, out dataPrzedZmiana))
                        {
                            // Sprawdź aktualność dat
                            if (data > dataPrzedZmiana)
                                return true;
                            else
                            {
                                DebugLog(string.Format("Data z wierszy drugiego toolTipa: [{0}] < = od daty z pierwszego: [{1}]", toolTip[wiersz], dataPrzedZmiana.ToString(fmtDaty)));
                                return false;
                            }
                        }
                        else
                        {
                            DebugLog(string.Format("ToolTip nie zawiera napisu [{0}] poprzedzającego datę", labelWierszDaty));
                            return false;
                        }
                    }
                    else
                    {
                        if (toolTipWzorcowy.Count != toolTip.Count)
                        {
                            DebugLog("Liczba wierszy ToolTipa wzorcowego i aktualnego jest różna!");
                            return false;
                        }
                        else
                        {
                            // Sprawdz pozostałe wiersze tooltipów poza wierszem z datą
                            for (int i = 0; i < toolTipWzorcowy.Count; i++)
                            {
                                if (toolTip[i].TrimEnd() != toolTipWzorcowy[i] &&
                                    !toolTip[i].Contains(labelWierszDaty))
                                {
                                    DebugLog(string.Format("Wiersz toltipa wzorcowego: {0} różny od aktualnego: {1}", toolTipWzorcowy[i], toolTip[i]));
                                    return false;
                                }
                            }
                            return true;
                        }
                    }
                }
                else
                {
                    DebugLog(string.Format("Wiersz toolTipa: [{0}], nie zawiera daty w poprawnym formacie [{1}]", toolTip[wiersz], fmtDaty));
                    return false;
                }

            }
            else
            {
                DebugLog(string.Format("ToolTip nie zawiera napisu [{0}] poprzedzającego datę", labelWierszDaty));
                return false;
            }

        }


        public Boolean SprawdzZawartoscToolTipa(WebElement tdTag, string oczekiwanyToolTip) //16.12 - zmieniono InputTag na Webelement - do testów
        {
            // sprawdź czy istnieje tooltip!
            if(CzyIstniejeToolTip(tdTag))
            {
                var tltp = tdTag.Element["data-errorqtip"].ToString(); 
                {
                    var lst = GetTooltipFromHtml(tltp);
                    DebugLog("Sprawdz zawartosc tooltipa");
                    return lst.FindIndex(o => o.Contains(oczekiwanyToolTip)) >= 0;
                }
            }
            else
            {
                return false;
            }
        }

        public List<string> PobierzTooltipaDlaWskazanegoElementu(string nazwaElementu, bool prawyGrid = true, bool identyczny = true)
        {
            DebugLog(string.Format("Pobierz divTag ucznia {0}", nazwaElementu));
            var operat = identyczny ? "=" : "~";
            var pathToGrid = (prawyGrid ? PathToSwitchPanelRight : PathToSwitchPanelLeft);
            var divTag = Find<DivTag>(string.Format("/{0}//div[@innertext{1}'{2}']", pathToGrid, operat, nazwaElementu));
            var y = GetTooltipFromHtml(divTag.Parent.Element["data-errorqtip"].ToString());
            return y;
        }

        public Boolean CzyIstniejeToolTip(WebElement tdTag) //16.12 - zmieniono InputTag na Webelement - do testów
        {
            DebugLog("Ustaw cursor na polu");
            tdTag.MoveTo();
            tdTag.Click();
            Keyboard.Press(Keys.Tab); // żeby zadziałał validator

            DebugLog("Pobierz zawartość tooltipa");
            var tltp = tdTag.Element["data-errorqtip"]; // sprawdź czy istnieje tooltip!
            if (tltp == null)
            {
                DebugLog("Nie znaleziono tooltipa");
                return false;
            }
            else
                return true;
        }

      
        #endregion

        #region Obsługa panelu wydruków
        protected Boolean WydrukGotowyDoPobrania(string tytulWydruku="", int waitingTime = 30000)
        {
            // Sprawdź czy panel pobierania wydruków jest rozwinięty
            if (PobierzPanelPobierania().Visible == false)
            {
                DebugLog("Panel pobierania wydruków zwinięty");
                return false;
            }

            DebugLog("Sprawdz czy jest kontener aktualnych wydruków w panelu pobierania wydruków");
            var divContainer = PobierzKontenerAktualnychWydrukowPaneluPobieraniaWydrukow();

            if(divContainer!=null)
            {
                // pobierz aktualny wydruk
                var pierwszyWydruk = PobierzAktualnyWydrukPaneluPobieraniaWydrukow(divContainer);

                DebugLog("Sprawdz tytul aktualnego wydruku w panelu pobierania wydruków");
                string tytul = PobierzTytulAktualnegoWydrukuPaneluPobieraniaWydrukow(pierwszyWydruk);

                if (tytul.Contains(tytulWydruku))
                {
                    string pth="/div[@id='master-container']/div[@id='info-panel']//div[@class~'x-progress-bar'][1]";
                    DebugLog("Pobierz kontrolkę progressbar aktualnego wydruku w panelu pobierania wydruków");
                    var pbar = Find<DivTag>(pth);

                    if(pbar.Visible)
                    {
                        DebugLog("Progressbar aktywny, Wydruk w trakcie tworzenia..");
                        int i = 0;
                        while (Find<DivTag>(pth, waitingTime).Visible)
                        {
                            System.Threading.Thread.Sleep(1000);
                            i++;
                            if (i > 30) throw new Exception("Proszę czekać jest za długo");
                        }
                    }

                    return SprawdzCzyIkonyAktywneAktualnegoWydrukuPaneluPobieraniaWydrukow(pierwszyWydruk);
                }
                else
                {
                    DebugLog(string.Format("Tytuł aktualnego wydruku: {0} inny niż oczekiwano: {1}", tytul, tytulWydruku));
                    return false;
                }

            }

            return false;
        }

        private DivTag PobierzPanelPobierania()
        {
            DebugLog("Pobierz panel pobierania wydruków");
            return Find<DivTag>("/div[#master-container]/div[#info-panel]");
        }

        private IList<DivTag> PobierzListeKontenerowPaneluPobieraniaWydrukow()
        {
            DebugLog("Pobierz listę kontenerów wydruków z panelu pobierania wydruków");
            var divPanelContainers = Find<DivTag>("/div[#master-container]//div[#info-panel-innerCt]");
            return divPanelContainers.FindChildren<DivTag>();
        }

        private DivTag PobierzKontenerAktualnychWydrukowPaneluPobieraniaWydrukow()
        {
            var listaKontenerow = PobierzListeKontenerowPaneluPobieraniaWydrukow();

            DebugLog("Sprawdz czy jest kontener aktualnych wydruków w panelu pobierania wydruków");
            foreach (var divContainer in listaKontenerow)
            {
                if (divContainer.Id.StartsWith("container-"))
                    return divContainer;
            }
            return null;
        }

        private DivTag PobierzAktualnyWydrukPaneluPobieraniaWydrukow(DivTag divContainer)
        {
            DebugLog("Pobierz aktualny wydruk");
            return divContainer.FindDescendant<SpanTag>()
                                            .FindDescendant<DivTag>()
                                            .FindDescendants<DivTag>()[0]
                                            .FindDescendant<DivTag>()
                                            .FindDescendant<DivTag>();
        }

        private string PobierzTytulAktualnegoWydrukuPaneluPobieraniaWydrukow(DivTag pierwszyWydruk)
        {
            return pierwszyWydruk.FindDescendant<LabelTag>().InnerText;
        }

        private IList<ATag> PobierzListeIkonAktualnegoWydrukuPaneluPobieraniaWydrukow(DivTag pierwszyWydruk)
        {
            return pierwszyWydruk
                            .FindDescendants<DivTag>()[1] // jeśli progressbar widoczny to [2]
                            .FindDescendant<DivTag>()
                            .FindDescendant<DivTag>()
                            .FindDescendants<ATag>();
        }

        private Boolean SprawdzCzyIkonyAktywneAktualnegoWydrukuPaneluPobieraniaWydrukow(DivTag pierwszyWydruk)
        {
            var listaIkon = PobierzListeIkonAktualnegoWydrukuPaneluPobieraniaWydrukow(pierwszyWydruk);

            Boolean ikonyAktywne = true;
            foreach (var ikona in listaIkon)
            {
                if (ikona.Visible == false)
                {
                    DebugLog("Ikona wydruku/pobierania nieaktywna");
                    ikonyAktywne = false;
                }
                else
                {
                    ikona.MoveTo();
                }
            }
            return ikonyAktywne;
        }

        #endregion

        #region Funkcje dat i formater dat
        /// <summary>
        /// Funkcja zwaraca podaną datę w dwóch formatach:
        /// "D" - '8 września 2013'
        /// "" - 8.09.2013
        /// </summary>
        /// <param name="dt">data</param>
        /// <param name="format">"D" - '8 września 2013', 
        ///                     "dz" - środa, 8 września 2013,
        ///                     "dzz" - Środa 08.09.2013,
        ///                     "ddz2" - Środa, 08.09.2013,
        ///                     "rmd" - '2013-12-09',
        ///                     "dd2" - '09.12.15',
        ///                     "hm" - '08.09.2013 12:12',
        ///                     "dd" - '08.09.2013',
        ///                     "" '8.09.2013'</param>
        /// <returns></returns>
        public string SformatujPodanaDate(System.DateTime dt, string format = "")
        {
            CultureInfo ci = CultureInfo.CreateSpecificCulture("pl-PL");
            DateTimeFormatInfo dtfi = ci.DateTimeFormat;
            dtfi.DateSeparator = ".";

            if(format=="D")
                return dt.ToString("D", ci);    // '8 września 2013'
            if (format == "dz")                 // środa, 8 września 2013
                return ci.DateTimeFormat.GetDayName(dt.DayOfWeek)+", "+ SformatujPodanaDate(dt, "D"); 
            if (format == "ddz")                // Środa 08.09.2013
            {
                var dzien = ci.DateTimeFormat.GetDayName(dt.DayOfWeek);
                return string.Format("{0}{1} {2}", dzien.Substring(0, 1).ToUpper(), dzien.Substring(1), SformatujPodanaDate(dt, "dd"));
            }
            if (format == "ddz2")                // Środa, 08.09.2013
            {
                var dzien = ci.DateTimeFormat.GetDayName(dt.DayOfWeek);
                return string.Format("{0}{1}, {2}", dzien.Substring(0, 1).ToUpper(), dzien.Substring(1), SformatujPodanaDate(dt, "dd"));
            }

            if(format == "rmd")
            {
                return dt.ToString("yyyy-MM-dd", ci); // '2013-12-09'
            }

            if (format == "dd2")
            {
                return dt.ToString("dd.MM.yy", ci); // '09.12.15'
            }

            if(format == "hm")
            {
                return dt.ToString("dd.MM.yyyy HH:mm", ci); // '08.09.2013 12:12'
            }

            if (format == "dd")
                return dt.ToString("dd.MM.yyyy", ci); // '08.09.2013'
            else
                return dt.ToString("d.MM.yyyy", ci); // '8.09.2013'

        }

        /// <summary>
        /// Funkcja zwraca datę najbliższego dnia tygodnia od wskazanej daty
        /// Uwaga!
        /// Przyjęto że tydzień zaczyna się od niedzieli
        ///  </summary>
        /// <param name="data">data</param>
        /// <param name="dzienTygodnia">enumeracja dnia tygodnia</param>
        /// <returns>data w formacie d.MM.yyyy</returns>
        public string UstalDateDlaWskazanegoDniaTygodnia(System.DateTime data, DayOfWeek dzienTygodnia, string fmt="")
        {
           
            if (data.DayOfWeek == dzienTygodnia)
                return SformatujPodanaDate(data, fmt);
            else
            {
                if ((int)data.DayOfWeek < (int)dzienTygodnia)
                {
                    return SformatujPodanaDate(data.AddDays((int)dzienTygodnia - (int)data.DayOfWeek), fmt);
                }
                else
                {
                    return SformatujPodanaDate(data.AddDays(-1 * ((int)data.DayOfWeek - (int)dzienTygodnia)), fmt);
                }
            }
            return string.Empty;
        }

        public string UstalTydzienDlaWskazanejDaty(System.DateTime data)
        {
            var poczatekTygodnia = UstalDateDlaWskazanegoDniaTygodnia(data, DayOfWeek.Monday);
            string tydzien = "";
            System.DateTime dateTime;
            if (System.DateTime.TryParseExact(poczatekTygodnia, "d.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime))
            {
                var poczatek = SformatujPodanaDate(dateTime, "dd");
                var koniec = SformatujPodanaDate(dateTime.AddDays(6), "dd");
                tydzien = string.Format("{0} - {1}", poczatek.Substring(0, 5), koniec);
            }
            return tydzien;
        }

        #endregion
    }
}
