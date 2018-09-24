using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ranorex;
using Ranorex.Core;
using Form = Ranorex.Form;
using Button = Ranorex.Button;
using ComboBox = Ranorex.ComboBox;
using ToolTip = Ranorex.ToolTip;

namespace VCommonPages.Pages.SilverlightCommon
{
    public class SilverlightSimpleEditorWindow : SilverlightPage
    {
        //"simple" to taki subtelny zart.

        public enum FieldTypes
        {
            TEXTBOX,
            COMBOBOX,
            DATEPICKER,
            KEYBOARDDATEPICKER, //do tych kalendarzy, ktore dziwnie sie zachowuja
            VALDICT,

            //do okienka hybrydowego
            TABLEEDITOR,

            //stare kontrolki - do DZ chociazby
            OLDCOMBOBOX,
            COMBOBOXTIME    
        }

        public struct SimpleEditorFields
        {
            public string Label;
            public FieldTypes Type;
            public string Value;
        }

        private readonly SimpleEditorFields[] _fields;
        protected readonly Unknown _okno;
        private bool znajdzWKontenerze;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="openWebsite"></param>
        /// <param name="browserId"></param>
        /// <param name="idOkna"></param>
        /// <param name="fields"></param>
        /// <param name="znajdzWKontenerze"></param>
        /// <param name="lpWierszaDolnego"></param>
        public SilverlightSimpleEditorWindow(WebDocument openWebsite, int? browserId, string idOkna, SimpleEditorFields[] fields, bool znajdzWKontenerze = true)
            : base(openWebsite, browserId)
        {
            this.znajdzWKontenerze = znajdzWKontenerze;
            const int waitTime = 30000;
            WaitForLoad();
            if (idOkna != "")
                _okno = OpenWebsite.FindSingle<Unknown>(string.Format(".//form/form[@name='{0}' and @visible='true']", idOkna), waitTime);
            if (idOkna == "")
                _okno = OpenWebsite.FindSingle<Unknown>(string.Format(".//form/form[@visible='true']"), waitTime);
            if (znajdzWKontenerze)
                _okno = _okno.FindSingle<Unknown>(".//container[@classname='ScrollViewer']", waitTime);
            _fields = fields;
        }

        public SilverlightSimpleEditorWindow(WebDocument openWebsite, int? browserId, SimpleEditorFields[] fields, string idZakladki)
            : base(openWebsite, browserId)
        {
            //konstruktor dla wersji wewnątrz zakładki
            const int waitTime = 30000;
            WaitForLoad();
            if (idZakladki != "")
                _okno = OpenWebsite.FindSingle<Unknown>(string.Format(".//tabpagelist/tabpage[@name='{0}' and @visible='true']", idZakladki), waitTime);
            if (idZakladki == "")
                _okno = OpenWebsite.FindSingle<Unknown>(string.Format(".//tabpagelist/tabpage[@visible='true']"), waitTime);

            _fields = fields;
        }

        protected Text GetLabel(string label)
        {
            return ZnajdzWKontenerzeLubOknie<Text>(string.Format(".//text[@name='{0}' and @visible='true']", label));
        }

        public T ZnajdzWKontenerzeLubOknie<T>(RxPath path) where T : Adapter
        {
            return _okno.FindSingle<T>(path);
        }

        public IList<T> ZnajdzWKontenerzeLubOknieWszystkie<T>(RxPath path) where T : Adapter
        {
            return _okno.Find<T>(path);
        }

        public SilverlightTable PobierzValdict(string nazwaPola, string opcjalnyFiltrDoValdicta = "", int childIndexDifference = 1)
        {
            DebugLog("Wprowadzam wartosc do pola " + nazwaPola);
            var field = _fields.ToList().Find(s => s.Label == nazwaPola);
            if (field.Type != FieldTypes.VALDICT) throw new Exception("Wybrana kolumna nie jest valdictem.");

            const int iloscPolValdicta = 4;
            int indeksPola = GetIndexNextToLabel(nazwaPola);
            indeksPola += (childIndexDifference - 1) * iloscPolValdicta;
            var button =ZnajdzWKontenerzeLubOknie<Button>(string.Format(".//button[@childindex='{0}' and @visible='true' and @automationid='ButtonOpen']", indeksPola + 1));
            if (opcjalnyFiltrDoValdicta != "") //klikniecie na valdict i wpisanie filtru, po enterze otwiera sie valdict - nie otwieramy go buttonem
            {
                var tekstboksDoWpisaniaWartosci = ZnajdzWKontenerzeLubOknie<Text>(string.Format(".//text[@childindex='{0}' and @visible='true']", indeksPola));
                tekstboksDoWpisaniaWartosci.Click();
                Keyboard.Press(opcjalnyFiltrDoValdicta);
                Keyboard.Press(Keys.Enter);
            }
            else //otwieranie valdicta buttonem
            {
                button.Click();
            }
            int indeksProgressBara = indeksPola + 2;
            var progressBar = ZnajdzWKontenerzeLubOknie<Ranorex.ProgressBar>(string.Format(".//progressbar[@childindex='{0}']", indeksProgressBara));
            int indeksValdicta = indeksProgressBara + 1;
            //var valdictlista =ZnajdzWKontenerzeLubOknie<Form>(string.Format(".//form[@childindex='{0}']", indeksValdicta));

            var list = ZnajdzWKontenerzeLubOknieWszystkie<Text>(string.Format(".//form[@childindex='{0}']/list/listitem/text", indeksValdicta));

            WaitForProgressBarInElement(progressBar); //progressbar tam jest i czyha - atakuje gdy valdict ładuje dane

            var ileKolumnWValdict = (from li in list select li.Element.ChildIndex).Max();
            var valdict = new List<List<string>>();
            for (var i = 0; i < list.Count(); )
            {
                var row = new List<string>();
                while (i < list.Count() && list[i].Element.ChildIndex < ileKolumnWValdict)//dodaje mniejsze od max child indexa
                {
                    row.Add(list[i++].As<Text>().TextValue);
                }
                row.Add(list[i++].As<Text>().TextValue);//i na koniec maksymalnego child indexa
                valdict.Add(row);
            }
            button.Click();//zamykanie valdicta
            return new SilverlightTable(valdict);
        }

        public void WprowadzWartoscDoPola(string nazwaPola, string wartosc, string opcjalnyFiltrDoValdicta = "", int childIndexDifference=1)
        {
            DebugLog("Wprowadzam wartosc do pola " + nazwaPola);
            var field = _fields.ToList().Find(s => s.Label == nazwaPola);
            int indeksPola = GetIndexNextToLabel(nazwaPola, childIndexDifference);
            switch (field.Type)
            {
                case FieldTypes.TEXTBOX:
                    {
                        ZnajdzWKontenerzeLubOknie<Text>(string.Format(".//text[@childindex='{0}' and @visible='true']", indeksPola)).Click();
                        Keyboard.Press(wartosc);
                        break;
                    }

                case FieldTypes.VALDICT:
                    {
                        const int iloscPolValdicta = 4;
                        indeksPola = GetIndexNextToLabel(nazwaPola);
                        indeksPola += (childIndexDifference - 1)*iloscPolValdicta;
                        if (opcjalnyFiltrDoValdicta != "") //klikniecie na valdict i wpisanie filtru, po enterze otwiera sie valdict - nie otwieramy go buttonem
                        {
                            var tekstboksDoWpisaniaWartosci =
                                ZnajdzWKontenerzeLubOknie<Text>(string.Format(".//text[@childindex='{0}' and @visible='true']", indeksPola));
                            tekstboksDoWpisaniaWartosci.Click();
                            Keyboard.Press(opcjalnyFiltrDoValdicta);
                            Keyboard.Press(Keys.Enter);
                        }
                        else //otwieranie valdicta buttonem
                        {
                            int indeksButtona = indeksPola + 1;
                            var button =
                                ZnajdzWKontenerzeLubOknie<Button>(
                                    string.Format(".//button[@childindex='{0}' and @visible='true' and @automationid='ButtonOpen']", indeksButtona));
                            button.Click();
                        }
                        int indeksProgressBara = indeksPola + 2;
                        var progressBar = ZnajdzWKontenerzeLubOknie<Ranorex.ProgressBar>(string.Format(".//progressbar[@childindex='{0}']", indeksProgressBara));
                        int indeksValdicta = indeksProgressBara + 1;
                        //var valdictlista =ZnajdzWKontenerzeLubOknie<Form>(string.Format(".//form[@childindex='{0}']", indeksValdicta));

                        //wartosc
                        var valdict =ZnajdzWKontenerzeLubOknie<Form>(string.Format(".//form[@childindex='{0}']", indeksValdicta));
                        WaitForProgressBarInElement(progressBar);


                        var lista = (from item in valdict.Find<Text>(".//list/listitem/text") select item.TextValue).ToList();
                        while (!lista.Contains(wartosc) && lista.Count>0)
                       {
                           var button=valdict.FindSingle<Button>(".//button[@AutomationId='ButtonNext']");
                           if(!button.Enabled)
                           {
                               throw new Exception(String.Format("Nie znaleziono wartości {0} w valdictie {1}",wartosc,nazwaPola));
                           }
                           button.Click();
                           WaitForProgressBarInElement(progressBar);
                           lista = (from item in valdict.Find<Text>(".//list/listitem/text") select item.TextValue).ToList();
                       }


                        //WaitForProgressBarInElement(progressBar); //progressbar tam jest i czyha - atakuje gdy valdict ładuje dane
                        valdict.FindSingle<Text>(string.Format(".//text[@name='{0}']", wartosc)).Click(); //wybieramy dane
                        break;
                    }

                case FieldTypes.COMBOBOX:
                    {

                        int indeksButtona = indeksPola + 1;
                        var button =
                            ZnajdzWKontenerzeLubOknie<Button>(string.Format(".//button[@childindex='{0}' and @visible='true' and @automationid='ButtonOpen']", indeksButtona));
                        button.Click();

                        int indeksCombo = indeksPola + 2;
                        var combolista =
                            ZnajdzWKontenerzeLubOknie<Form>(string.Format(".//form[@childindex='{0}']", indeksCombo));

                        //combolista.FindSingle<ListItem>(string.Format(".//listitem[@name='{0}']", wartosc)).Click();
                        combolista.FindSingle<Text>(string.Format(".//listitem/text[@name='{0}']", wartosc)).Click();
                        break;
                    }

                case FieldTypes.DATEPICKER:
                    {
                        var combo = ZnajdzWKontenerzeLubOknie<ComboBox>(string.Format(".//combobox[@childindex='{0}' and @visible='true' and @classname='NDatePicker']", indeksPola));
                        var button = combo.FindSingle<Button>(".//button[@text='Show Calendar']");
                        button.Click();
                        var kalendarz = combo.FindSingle<Ranorex.DateTime>(".//form[@classname='Popup']/datetime");
                        SilverlightPickDateInCalendar(kalendarz, wartosc);

                        break;
                    }

                case FieldTypes.KEYBOARDDATEPICKER:
                    {
                        var combo = ZnajdzWKontenerzeLubOknie<ComboBox>(string.Format(".//combobox[@childindex='{0}' and @visible='true' and @classname='NDatePicker']", indeksPola));
                        var tekstboks = combo.FindSingle<Text>("./text");
                        tekstboks.TextValue = wartosc;
                        break;
                    }

                case FieldTypes.OLDCOMBOBOX:
                    {
                        var combo = ZnajdzWKontenerzeLubOknie<ComboBox>(string.Format(".//combobox[@childindex='{0}' and @visible='true']", indeksPola));
                        combo.Click();
                        combo.FindSingle<ListItem>(string.Format(@".//listitem[@name='{0}']", wartosc)).Click();
                        break;
                    }
                case FieldTypes.COMBOBOXTIME:
                    {
                        var data =
                            _okno.FindSingle<Text>(string.Format(
                                ".//combobox[@classname='TimePicker' and @childindex='{0}']/container/text[@automationid='Text']", indeksPola));
                        data.TextValue = wartosc;
                        break;
                    }
            }
        }

        public void WprowadzWartosciDoPol(string[] pola, string[] wartosci)
        {
            if (pola.Length != wartosci.Length)
                throw new ArgumentException("pola.Length!=wartosci.Length");
            for (int i = 0; i < pola.Length; i++)
            {
                WprowadzWartoscDoPola(pola[i], wartosci[i]);
                WaitForLoadInWindow(); //13.04 LK - gruba zmiana, specjalnie dla D_02_01_062
                //gdyby to komuś zaszkodzilo to wyrzucić tego Waita i wprowadzić go w tamtym miejscu indywidualnie
            }
        }

        public string PobierzWartoscPola(string nazwaPola, int childIndexDifference = 1)
        {
            DebugLog("Pobieram wartosc z pola " + nazwaPola);
            var field = _fields.ToList().Find(s => s.Label == nazwaPola);
            string value = "";
            var indeksPola = GetIndexNextToLabel(nazwaPola, childIndexDifference);
            switch (field.Type)
            {
                case FieldTypes.VALDICT:
                    const int iloscSkladowychValdicta=4;
                    indeksPola = GetIndexNextToLabel(nazwaPola);//valdicty wymagaja specjalnego traktowania
                    indeksPola += (childIndexDifference-1)*iloscSkladowychValdicta; // -1 bo juz jedno zostało dodane w getIndexNextToLabel. Ten magiczny zapis przesuwa nie o index a o tyle indeksów ile ma valdict
                    value = ZnajdzWKontenerzeLubOknie<Text>(string.Format(".//text[@childindex='{0}' and @visible='true']", indeksPola)).TextValue;
                    break;

                case FieldTypes.TEXTBOX:                
                case FieldTypes.COMBOBOX:
                    value = ZnajdzWKontenerzeLubOknie<Text>(string.Format(".//text[@childindex='{0}' and @visible='true']", indeksPola)).TextValue;
                    break;
                case FieldTypes.DATEPICKER:
                case FieldTypes.KEYBOARDDATEPICKER:
                    value = ZnajdzWKontenerzeLubOknie<Text>(string.Format(".//combobox[@childindex='{0}']/text[@visible='true']", indeksPola)).TextValue;
                    break;
                case FieldTypes.OLDCOMBOBOX:
                    value = ZnajdzWKontenerzeLubOknie<ComboBox>(string.Format(".//combobox[@childindex='{0}' and @visible='true']", indeksPola)).SelectedItemText;
                    break;
                case FieldTypes.COMBOBOXTIME:
                    var data = _okno.FindSingle<Text>(string.Format(".//combobox[@classname='TimePicker' and @childindex='{0}']/container/text[@automationid='Text']", indeksPola));
                    value = data.TextValue;
                    break;
                default:
                    throw new Exception(field.Type + " nie jest obsługiwany");
            }

            return value;
        }

        public bool SprawdzCzyPoleJestAktywneOrazZawartosc(string nazwaPola, string zawartosc, bool aktywne = true)
        {
            if (aktywne && !SprawdzCzyPoleJestAktywne(nazwaPola))
                    throw new Exception("pole "+nazwaPola+"nie jest aktywne, a powinno byc");
            if (!aktywne && SprawdzCzyPoleJestAktywne(nazwaPola))
                    throw new Exception("pole "+nazwaPola+"jest aktywne, a nie powinno");
            var wartosc = PobierzWartoscPola(nazwaPola) ?? "";
            if (zawartosc == null)
                zawartosc = "";
            if (wartosc != zawartosc)
                throw new Exception(String.Format("zawartosc niezgodna z pobrana: w polu {2} jest: \"{0}\" a powinno być \"{1}\"", wartosc,zawartosc,nazwaPola));

            return true;

        }


        public bool SprawdzCzyPoleJestAktywne(string nazwaPola, int childIndexDifference=1)
        {
            DebugLog("Sprawdzam aktywnosc pola: " + nazwaPola);
            var field = _fields.ToList().Find(s => s.Label == nazwaPola);
            int indeksPolaZTekstboksem = GetIndexNextToLabel(nazwaPola, childIndexDifference);
            switch (field.Type)
            {
                case FieldTypes.TEXTBOX:
                    {
                        var tekstboks = ZnajdzWKontenerzeLubOknie<Text>(string.Format(".//text[@childindex='{0}' and @visible='true']", indeksPolaZTekstboksem));
                        //string valueBeforeEditing = tekstboks.TextValue;

                        tekstboks.Focus();
                        tekstboks.Click();

                        //Keyboard.Press("TEST");
                        var valueBeforeEditing = tekstboks.TextValue;
                        double result;
                        Keyboard.Press(Double.TryParse(valueBeforeEditing, out result) ? "42,42" : "TEST");
                        var aktywne = tekstboks.TextValue != valueBeforeEditing;
                        if (aktywne)
                        {//pole aktywne, powracamy do starej tresci
                            int len = tekstboks.TextValue.Length;
                            while (len-- > 0)
                            {
                                Keyboard.Press("\b");
                            }
                            Keyboard.Press(valueBeforeEditing ?? String.Empty);
                            tekstboks.Click();
                        }
                        return aktywne; 

                        //string valueAfterEditing = tekstboks.TextValue;
                        //if (tekstboks.TextValue == valueBeforeEditing)
                        //    return false; //pole nieaktywne, zostala stara zawartosc
                        //if (tekstboks.TextValue == valueAfterEditing)
                        //    {
                        //    tekstboks.Click();
                        //    tekstboks.TextValue = valueBeforeEditing;
                        //    return true; //pole aktywne, powracamy do starej tresci
                        //}

                    }
                case FieldTypes.VALDICT:
                    {
                        const int iloscSkladowychValdicta = 4;
                        indeksPolaZTekstboksem = GetIndexNextToLabel(nazwaPola); //valdict specjalnej troski
                        indeksPolaZTekstboksem += (childIndexDifference - 1)*iloscSkladowychValdicta;
                        int indeksButtona = indeksPolaZTekstboksem + 1;
                        var button =
                            ZnajdzWKontenerzeLubOknie<Button>(string.Format(".//button[@childindex='{0}' and @visible='true' and @automationid='ButtonOpen']",indeksButtona));
                        button.Click();

                        int indeksValdicta = indeksButtona + 2;
                        var valdictlista =
                            ZnajdzWKontenerzeLubOknie<Form>(string.Format(".//form[@childindex='{0}']", indeksValdicta));
                        int indeksProgressBara = indeksButtona + 1;
                        var progressBar =
                            ZnajdzWKontenerzeLubOknie<Ranorex.ProgressBar>(string.Format(".//progressbar[@childindex='{0}']", indeksProgressBara));
                        WaitForProgressBarInElement(progressBar);
                        if (valdictlista.Visible)
                        {
                            button.Click();
                            return true;
                        }
                        return false;
                    }
                case FieldTypes.COMBOBOX:
                    {
                        const int ileSkladowychCombobox=3;
                        indeksPolaZTekstboksem = GetIndexNextToLabel(nazwaPola); //valdict specjalnej troski
                        indeksPolaZTekstboksem += (childIndexDifference - 1)*ileSkladowychCombobox;
                        int indeksButtona = indeksPolaZTekstboksem + 1;
                        var button =ZnajdzWKontenerzeLubOknie<Button>(string.Format(".//button[@childindex='{0}' and @visible='true' and @automationid='ButtonOpen']",indeksButtona));
                        button.Click();


                        int indeksCombo = indeksButtona + 1;
                        var combolista =ZnajdzWKontenerzeLubOknie<Form>(string.Format(".//form[@childindex='{0}']", indeksCombo));
                        if (combolista.Visible)
                        {
                            button.Click();
                            return true;
                        }
                        if (!combolista.Visible)
                            return false;

                        break;
                    }

                case FieldTypes.DATEPICKER:
                    {
                        var kalendarzCombo = ZnajdzWKontenerzeLubOknie<ComboBox>(string.Format(".//combobox[@childindex='{0}' and @visible='true']", indeksPolaZTekstboksem));
                        var buttonStrzalka = kalendarzCombo.FindSingle<Button>(".//button[@automationid='Button']");
                        buttonStrzalka.Click();

                        var combolista =
                            kalendarzCombo.FindSingle<Form>(".//form[@automationid='Popup']");

                        if (combolista.Visible)
                        {
                            buttonStrzalka.Click();
                            return true;
                        }
                        return false;
                    }
                case FieldTypes.KEYBOARDDATEPICKER:
                    {
                        
                        var combo =
                            _okno.FindSingle<ComboBox>(
                                string.Format(".//combobox[@childindex='{0}' and @visible='true']", indeksPolaZTekstboksem));
                        combo.Click();
                        return combo.Enabled; // nie wiem czy tyle wystarczy //TODO sprawdzic?
                    }
                case FieldTypes.OLDCOMBOBOX:
                    {
                        //to chyba jedyna rzecz ze starych testow jaka sie do czegokolwiek przydala.
                        DebugLog("Sprawdzam combobox starego typu. Zapiąć pasy, to będzie wyboista podróż.");

                        var combo = ZnajdzWKontenerzeLubOknie<ComboBox>(string.Format(".//combobox[@childindex='{0}' and @visible='true']", indeksPolaZTekstboksem));
                        combo.Click();

                        string oldValue = combo.Text;

                        foreach (var item in combo.Items)
                        {
                            if (!item.Text.Contains(oldValue))
                            {
                                item.EnsureVisible(); //probujemy zmienic wartosc w kombosie, myszka bedzie sie dziwnie zachowywac bo kontrolka jest uposledzona
                                item.Click();
                                break;
                            }
                        }
                        string newValue = combo.Text;

                        if (newValue == oldValue)
                            return false;
                        if (newValue != oldValue)
                        {
                            foreach (var item in combo.Items)
                            {
                                if (item.Text.Contains(oldValue))
                                {
                                    item.EnsureVisible();
                                    item.Click();
                                    break;
                                }
                            }
                            return true;
                        }

                        break;
                    }
                default:
                    throw new Exception(String.Format("Pole o nazwie {0} ma typ {1}, który nie jest obsługiwany", nazwaPola,field.Type));
            }

            return false;
        }


        public bool SprawdzIloscElementowNaCombo(int oczekiwanaIloscElementow, string labelkaCombo,int childIndexDifference=1)
        {
            var field = _fields.ToList().Find(s => s.Label == labelkaCombo);
            if (field.Type != FieldTypes.COMBOBOX) throw new Exception("Wybrane pole nie jest typu COMBOBOX");
            int indeksPolaZTekstboksem = GetIndexNextToLabel(labelkaCombo,childIndexDifference);
            int indeksButtona = indeksPolaZTekstboksem + 1;
            var button = ZnajdzWKontenerzeLubOknie<Button>(string.Format(".//button[@childindex='{0}' and @visible='true' and @automationid='ButtonOpen']", indeksButtona));
            button.Click();

            int indeksCombo = indeksButtona + 1;
            var combolista = ZnajdzWKontenerzeLubOknie<Form>(string.Format(".//form[@childindex='{0}']", indeksCombo));

            int rzeczywistaIloscElementow = combolista.Children.Count();
            button.Click();
            return oczekiwanaIloscElementow == rzeczywistaIloscElementow;
        }


        public bool SprawdzWieleWartosci(string[] pola, string[] wartosci)
        {
            for (int i = 0; i < pola.Count(); i++)
            {
                if (PobierzWartoscPola(pola[i]) != wartosci[i])
                    return false;
            }
            return true;
        }

        public void SprawdzCzyPolaSaNieaktywne(string[] pola)
        {
            for (int i = 0; i < pola.Count(); i++)
            {
                bool rzeczywistyStan = SprawdzCzyPoleJestAktywne(pola[i]);
                if (rzeczywistyStan)
                    throw new Exception(string.Format("Aktywnosc pol sie nie zgadza - pole: {0}, oczekiwany stan: false, rzeczywisty stan: true", pola[i]));
            }
        }

        public void SprawdzAktywnoscWieluPol(string[] pola, bool[] oczekiwanaAktywnosc)
        {
            for (int i = 0; i < pola.Count(); i++)
            {
                bool rzeczywistyStan = SprawdzCzyPoleJestAktywne(pola[i]);
                if (rzeczywistyStan != oczekiwanaAktywnosc[i])
                    throw new Exception(string.Format("Aktywnosc pol sie nie zgadza - pole: {0}, oczekiwany stan: {1}, rzeczywisty stan: {2}", pola[i], oczekiwanaAktywnosc[i], rzeczywistyStan));
            }

        }


        public bool SprawdzCzyPoleJestUkryte(string nazwaPola)
        {
            DebugLog("Sprawdzam czy pole " + nazwaPola + " jest ukryte");
            WaitForLoadInWindow();
            var poleZLabelka = ZnajdzWKontenerzeLubOknie<Text>(string.Format(".//text[@name='{0}']", nazwaPola));
            return !poleZLabelka.Visible; //ukryte=true, nieukryte=false
        }

        public bool SprawdzCzyPolaSaUkryte(string[] pola)
        {
            foreach (var p in pola)
                if (!SprawdzCzyPoleJestUkryte(p))
                    return false;
            return true;
        }

        public void KliknijDalej()
        {
            GetButtonByNameInWindow("Dalej").Click();
            WaitForLoad();
        }

        public SilverlightInfoPopup KliknijZapisz()
        {
            GetButtonByNameInWindow("Zapisz").Click();
            WaitForLoad();
            return new SilverlightInfoPopup(OpenWebsite, BrowserId);
        }

        public SilverlightInfoPopup KliknijZapiszIZakoncz(bool waitForLoad = true)
        {
            GetButtonByNameInWindow("Zapisz i zakończ").Click();
            if(waitForLoad)
                WaitForLoad();
            return new SilverlightInfoPopup(OpenWebsite, BrowserId);
        }

        public void KliknijAnuluj()
        {
            GetButtonByNameInWindow("Anuluj").Click();
            WaitForLoad();
        }

        public SilverlightInfoPopup KliknijUsun(bool waitForLoad = true)
        {
            GetButtonByNameInWindow("Usuń").Click();
            if (waitForLoad)
                WaitForLoad();
            return new SilverlightInfoPopup(OpenWebsite, BrowserId);
        }

        public SilverlightInfoPopup KliknijSprawdzDokument()
        {
            GetButtonByNameInWindow("Sprawdź dokument").Click();
            return new SilverlightInfoPopup(OpenWebsite,BrowserId);
        }

        public bool SprawdzCzyAktywnyAnuluj()
        {
            return GetButtonByNameInWindow("Anuluj").Enabled;
        }

        public bool SprawdzCzyAktywnyZapisz()
        {
            return GetButtonByNameInWindow("Zapisz").Enabled;
        }

        public bool SprawdzCzyAktywnyZapiszIZakoncz()
        {
            return GetButtonByNameInWindow("Zapisz i zakończ").Enabled;
        }

        public bool SprawdzCzyAktywnyUsun()
        {
            return GetButtonByNameInWindow("Usuń").Enabled;
        }

        virtual protected int GetIndexNextToLabel(string nazwaPola, int childIndexDifference = 1)
        {
            var poleZLabelka = GetLabel(nazwaPola);
            int indeksPolaZLabelka = poleZLabelka.Element.ChildIndex;
            int indeksPolaZTekstboksem = indeksPolaZLabelka + childIndexDifference;

            return indeksPolaZTekstboksem;
        }

        public string[][] PobierzTooltipyWRogu(int iloscbledow, string komunikatOBledach = "Błędów: ")
        {
            InfoLog("Najeżdzam na element z tooltipem");
            List<string[]> result = new List<string[]>();
            Text hoverObject;
            if(znajdzWKontenerze)
            {
                hoverObject = _okno.FindSingle<Text>(String.Format("..//text[@name='{0}']", komunikatOBledach));
                _okno.FindSingle<Text>(String.Format("..//text[@name='{0}']", iloscbledow));
            }
            else
            {
                hoverObject = _okno.FindSingle<Text>(String.Format(".//text[@name='{0}']", komunikatOBledach));
                _okno.FindSingle<Text>(String.Format(".//text[@name='{0}']", iloscbledow));                
            }


            hoverObject.MoveTo();
            Delay.Milliseconds(1000);  //TUTAJ JEST ANIMACJA LADOWANIA TOOLTIPA - MUSI ZOSTAC
            InfoLog("Szukam tooltipa");
            var tooltip = OpenWebsite.FindSingle<ToolTip>(string.Format(".//tooltip[@classname='ToolTip' and @visible='true']"));
            var automationID = (string)tooltip.Element.GetAttributeValue("AutomationId");

            if (automationID == "VSToolTip")
            {
                var tooltipList = tooltip.FindDescendants<ListItem>();
                foreach (var listItem in tooltipList)
                {
                    var textList = listItem.FindDescendants<Text>();
                    string[] row = new string[textList.Count];

                    int i = 0;
                    foreach (var text in textList)
                    {
                        row[i] = text.TextValue;
                        i++;
                    }
                    result.Add(row);
                }
            }

            return result.ToArray();
        }

        public bool SprawdzZawartoscTooltipow(string[][] dane)
        {
            //tablica z danymi powinna zawierać tablice 3 elementowe, po jednej dla każdego tooltipa. Elementy to kolejno:
            //1 - Nazwa labelki przy której mamy tooltip
            //2 - Spodziewana treść wewnątrz tooltipa
            //3 - typ tooltipa. Zamieniamy go na odpowiednią wartość przesunięcia w child indexie od labelki
            //typy to "error" i "warning"

            foreach (var tooltip in dane)
            {
                InfoLog(String.Format("Szukam tooltipa {0}", tooltip[0]));
                string wartoscOczekiwana = tooltip[1];
                var field = _fields.ToList().Find(s => s.Label == tooltip[0]);
                int przesuniecie;
                switch (tooltip[2]) //jest jeszcze jeden obrazek z przesunieciem 1, ale nie wiem jaki...
                {
                    case "error":
                        przesuniecie = 3;
                        break;

                    case "warning":
                        przesuniecie = 2;
                        break;

                    default:
                        InfoLog("Nie podano typu tooltipa do odnalezienia.");
                        return false;
                }

                Text label = ZnajdzWKontenerzeLubOknie<Text>(String.Format("./text[@name='{0}']", tooltip[0]));
                var ci = label.Element.ChildIndex;
                Picture obrazek;
                switch (field.Type)
                {
                    case FieldTypes.TEXTBOX:
                        {
                            ci++;
                            var textbox = ZnajdzWKontenerzeLubOknie<Text>(String.Format("./text[@ChildIndex={0} and @visible='true']", ci));
                            //uwaga na ewentualne zmiany kolejnosci elementow w textboxach w przyszlosci
                            obrazek = textbox.FindSingle<Picture>(String.Format("./picture[@ChildIndex={0} and @visible='true']", przesuniecie - 1));
                            break;
                        }

                    case FieldTypes.VALDICT:
                    case FieldTypes.COMBOBOX:
                        {
                            ci += przesuniecie;
                            obrazek = ZnajdzWKontenerzeLubOknie<Picture>(String.Format("./picture[@ChildIndex={0} and @visible='true']", ci));
                            break;
                        }

                    default:
                        return false;
                }

                obrazek.MoveTo();
                Delay.Milliseconds(1000); //TUTAJ JEST ANIMACJA LADOWANIA TOOLTIPA - MUSI ZOSTAC

                string wartoscAktualna = OpenWebsite.FindSingle<Text>(".//tooltip[@classname='ToolTip' and @visible='true']/text[@visible='true']").TextValue;

                if (wartoscOczekiwana != wartoscAktualna)
                {
                    InfoLog(String.Format("Wartość niezgodna, ma być: {0}, jest: {1}", wartoscOczekiwana, wartoscAktualna));
                    return false;
                }
            }

            return true;
        }

        protected void WaitForLoadInContainer()
        {
            //eksperymentalny WaitForLoad, sprawdzamy proszę czekać w konkretnym okienku
            //potrzebny do testu 05_08_031, bo za szybko kilka w guziora (prawdopodobnie)
            int i = 0;
            while (_okno.Element.Parent.FindSingle(".//text[@name='Proszę czekać...']", 7000).Visible)
            {
                System.Threading.Thread.Sleep(1000);
                i++;
                if (i > 30) throw new Exception("Proszę czekać jest za długo");
            }
        }
    }
}
