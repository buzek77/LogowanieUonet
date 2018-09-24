using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Ranorex;
using System.Windows.Forms;
using Ranorex.Core;
using Form = Ranorex.Form;
using Button = Ranorex.Button;
using CheckBox = Ranorex.CheckBox;
using TabPage = Ranorex.TabPage;
using Text = Ranorex.Text;

namespace VCommonPages.Pages.SilverlightCommon
{
    public class SilverlightTableEditorWindow : SilverlightPage
    {
        public enum FieldTypes
        {
            TEXTBOX,
            COMBOBOX,
            VALDICT
        }

        public struct TableEditorFields
        {
            public string Label;
            public FieldTypes Type;
            public string Value;
        }

        private static readonly TableEditorFields[] Fields =
                {
                    new TableEditorFields {Label = "Lp", Type = FieldTypes.TEXTBOX},
                    new TableEditorFields {Label = "Nazwa towaru / Usługi", Type = FieldTypes.TEXTBOX}, //to ma miec spacje.
                    new TableEditorFields {Label = "Ilość", Type = FieldTypes.TEXTBOX},
                    new TableEditorFields {Label = "J.m.", Type = FieldTypes.COMBOBOX},
                    new TableEditorFields {Label = "Cena brutto", Type = FieldTypes.TEXTBOX},
                    new TableEditorFields {Label = "Wartość brutto", Type = FieldTypes.TEXTBOX},
                    new TableEditorFields {Label = "Rodzaj planu", Type = FieldTypes.VALDICT},
                    new TableEditorFields {Label = "Klasyfikacja budżetowa", Type = FieldTypes.VALDICT},
                    new TableEditorFields {Label = "Zadanie", Type = FieldTypes.VALDICT},
                    new TableEditorFields {Label = "Finansowanie", Type = FieldTypes.VALDICT},
                    new TableEditorFields {Label = "Opis", Type = FieldTypes.TEXTBOX},
                    new TableEditorFields {Label = "Środek trwały", Type = FieldTypes.COMBOBOX},
                    new TableEditorFields {Label = "Typ księgowania", Type = FieldTypes.VALDICT},
                    //DS
                    new TableEditorFields {Label = "Stawka VAT", Type = FieldTypes.COMBOBOX},
                    new TableEditorFields {Label = "Cena netto", Type = FieldTypes.TEXTBOX},
                    new TableEditorFields {Label = "Kwota VAT", Type = FieldTypes.TEXTBOX},
                    new TableEditorFields {Label = "PKWiU", Type = FieldTypes.VALDICT},
                    new TableEditorFields {Label = "CPV", Type = FieldTypes.VALDICT},
                    
                    new TableEditorFields {Label = "VAT-7", Type = FieldTypes.COMBOBOX},

                    //wzorce numeracji
                    new TableEditorFields {Label = "Numer", Type = FieldTypes.TEXTBOX},
                    new TableEditorFields {Label = "Rok obrachunkowy", Type = FieldTypes.TEXTBOX}
                };



        protected List<List<Text>> InternalTable = new List<List<Text>>();
        private readonly Form _okno;
        private readonly Container _container;
        private Table _table;
        private readonly bool _znajdzWKontenerze;

        private bool paramDetailsPresenter;
        private bool paramIgnoreHeader;
        private int paramKtoraTabelka;
        //private int paramPierwszeRuchomePole;

        public SilverlightTableEditorWindow(WebDocument openWebsite, int? browserId, string idOkna = "", bool ignoreHeader = false, bool detailsPresenter = false, int ktoraTabelka = 0, bool emptyWindow = false, bool znajdzWKontenerze = true)
            : base(openWebsite, browserId)
        {
            _znajdzWKontenerze = znajdzWKontenerze;
            WaitForLoadInWindow();
            if (idOkna != "")
                _okno = OpenWebsite.FindSingle<Form>(string.Format(".//form/form[@name='{0}' and @visible='true']", idOkna));
            if (idOkna == "")
                _okno = OpenWebsite.FindSingle<Form>(string.Format(".//form/form[@visible='true']"));
            if (_znajdzWKontenerze)
                _container = _okno.FindSingle<Container>(".//container[@automationid='scrollViewer' and @visible='true']");
            if (!emptyWindow)
                InternalTable = PobierzTabelkeDoEdycji(detailsPresenter, ignoreHeader, ktoraTabelka);

            paramDetailsPresenter=detailsPresenter;
            paramIgnoreHeader=ignoreHeader;
            paramKtoraTabelka=ktoraTabelka;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="detailsPresenter">czy szukac detailspresenter </param>
        /// 
        /// <param name="ignoreHeader">ciekawe co to znaczy</param>
        /// <param name="ktoraTabelka">zaskakuajce - ktora tabelke ladujemy</param>
        /// <param name="pierwszeRuchomePole">to jest fajne - indeks pierwszego pola ktore sie rusza gdy przesuwamy suwakiem tabelke</param>
        /// <returns></returns>
        public List<List<Text>> PobierzTabelkeDoEdycji(bool detailsPresenter = false, bool ignoreHeader = false, int ktoraTabelka = 0, int pierwszeRuchomePole = 0)
        {
            List<List<Text>> teksty = new List<List<Text>>();

            IList<Table> wszystkieTabelki =_znajdzWKontenerze? _container.Find<Table>(".//table[@visible='true']"):
                _okno.Find<Table>(".//table[@visible='true']");
            var szukanaTabelka = wszystkieTabelki[ktoraTabelka];
            _table = szukanaTabelka;

            var szukanaTabelkaPath = szukanaTabelka.GetPath();

            // Odczytaj naglowek - w zasadzie nie wyobrazam sobie tabelki we fionecie bez naglowkow
            if (!ignoreHeader)
            {
                var naglowek =
                    szukanaTabelka.FindSingle<Row>(szukanaTabelkaPath +
                                                   "/row[@automationid='ColumnHeadersPresenter' and @visible='true']");

                var daneZNaglowka = (List<Text>)naglowek.Find<Text>("./cell/text");
                List<Text> przefiltrowanaLista = new List<Text>();

                foreach (Text text in daneZNaglowka)
                {
                    if (!text.Visible)
                        text.EnsureVisible();

                    if (text.Visible && text.TextValue != null)
                        przefiltrowanaLista.Add(text);
                }

                teksty.Add(przefiltrowanaLista);
            }

            // Odczytaj wiersze tabeli
            DebugLog(string.Format("Czytam tabelke w celu jej edycji"));
            var rowsPresenter = Find<Unknown>(szukanaTabelkaPath + "/element[@automationid='RowsPresenter' and @visible='true']");

            List<Unknown> wierszeTabelki = new List<Unknown>(rowsPresenter.FindChildren<Unknown>());

            #region render
            if (wierszeTabelki.Count > 0)
            {
                // Przejedz na koniec tabelki, zeby wyrenderowac wszystkie cudowne, pelne radosci i zycia, komorki
                int szerokoscTabelki = wierszeTabelki[0].FindChildren<Unknown>().Count - 2;
                    //-2 bo jest detailspresenter
                List<Unknown> pierwszyWiersz = new List<Unknown>(wierszeTabelki[0].FindChildren<Unknown>());
                pierwszyWiersz[szerokoscTabelki].EnsureVisible();
                // Przejechac na poczatek tez wypada, zaufaj mi, jestem elektrykiem, znam sie na tym -dg
                pierwszyWiersz[pierwszeRuchomePole].EnsureVisible();
            }

             #endregion

            if (!detailsPresenter)
                foreach (Unknown row in wierszeTabelki)
                {
                    if (row.Visible)
                    {
                        List<Text> jedenRow = new List<Text>();
                        jedenRow.Clear();
                        var lista = row.Find<Text>("./element[@classname='DataGridCell']/text");
                        foreach (Text komorka in lista)
                        {
                            //if (komorka.TextValue != null)

                            komorka.EnsureVisible();
                            if (komorka.Visible){
                                //13.04 LK - robimy najpierw EnsureVisible, a potem jeśli jest visible to sczytujemy
                                //to powinno pokryć różne rodzaje tabelek - alternatywą jest zrobienie parametrów, no ale bez przesady
                                //troche niebezpieczna zmiana, więc jeżeli coś w okolicy się sypnie to na 95% jest to wina tej otoczki
                                jedenRow.Add(komorka);
                            }

                            //if (komorka.TextValue == null)
                            //    stringi.Add(komorka);
                        }

                        teksty.Add(jedenRow);
                    }
                }

            return teksty;
        }

        public void AktualizujTabelke()
        {
            InternalTable = PobierzTabelkeDoEdycji(paramDetailsPresenter,paramIgnoreHeader,paramKtoraTabelka);
        }

        public SilverlightTable PobierzTabelkeDoPorownania()
        {
            return new SilverlightTable(InternalTable);
        }

        public string PobierzWartoscZTabelki(string kolumna, int lp)
        {
            return KomorkaWTabelce(kolumna, lp).TextValue;
        }

        public void KliknijWKomorkeTabelki(string kolumna, int lp)
        {
            //zmieniam bo było niedobrze a chyba nikt tego nie uzywa i tak - LK
            //tzn był od razu wewnętrzny tekst zamiast jego parenta, ale ranorex coś go nie widział - sam nie wiem co z tym nie tak
            ((Unknown)KomorkaWTabelce(kolumna,lp).Element.Parent).Click();
        }

        private Text KomorkaWTabelce(string kolumna, int lp)
        {
            int td = -1;
            for (var i = 0; i < InternalTable[0].Count && td==-1; i++)
            {
                if(InternalTable[0][i].TextValue==kolumna)
                {
                    td = i;
                }
            }
            return InternalTable[lp][td]; // lp = tr
        }

        public SilverlightTable PobierzValdict(string kolumna, int wiersz, string opcjalnyFiltrDoValdicta = "")
        {
            var field = Fields.ToList().Find(s => s.Label == kolumna);
            if (field.Type != FieldTypes.VALDICT) throw new Exception("Wybrana kolumna nie jest valdictem.");

            var x = KomorkaWTabelce(kolumna, wiersz); //text
            var element = x.Element.Parent; //element

            //nie moja magia jeszcze nie wnikałem AA
            #region przesuwanie o 1 pole do przodu zeby uniknac problemow z niewidocznym buttonem pomimo ze jest visible

            int ci = x.Element.Parent.ChildIndex;
            int ilosckolumn = InternalTable[0].Count(); //z headera
            string path = "";
            if (ci < ilosckolumn)
                path = string.Format(".//element[@childindex='{0}']", ci + 1);
            ZnajdzWKontenerzeLubOknie<Unknown>(path).EnsureVisible();

            //scenariusz 02_08_087 jest w poniższy sposób naprawiony, ale te child indexy wyżej dalej są źle
            //bo nie uwzględniają niewidzialnych kolumn. Trzeba by bylo je jakoś zgodnie numerować żeby znać
            //PRAWDZIWĄ ilość, jeśli pojawi się kiedyś jakiś problem. To tak na przyszłość - LK
            //updejt o 14:28 - O, już wiem, może po prostu pod ilosckolumn podstawić childIndex ostatniej kolumny (czyli maksymalny)?
            if (InternalTable[0].Last().TextValue == kolumna)
            {
                Keyboard.Down(Keys.Shift);
                Keyboard.Press(Keys.End);
                Keyboard.Up(Keys.Shift);
            }

            #endregion

            element.EnsureVisible();
            element.As<Unknown>().Click();
            //klikamy w button, valdict sie rozwija, wybieramy dane z listy

            Button button = button = element.FindSingle("./button[@automationid='ButtonOpen']").As<Button>();
            if (opcjalnyFiltrDoValdicta == "")
            {
                button.EnsureVisible();
                button.Click();
                //czarna strzalka pojawia sie po nacisnieciu na komorke
            }else //klikamy na valdict, wpisujemy wartosc ktora JEST FILTREM, valdict sie rozwija, klikamy na wartosc tekstowa w rozwinietym menu
            {
                Keyboard.Press(opcjalnyFiltrDoValdicta);
                Keyboard.Press(Keys.Enter);
            }
            WaitForProgressBarInElement(element.As<Unknown>());
            //progressbar tam jest i czyha - atakuje gdy valdict ładuje dane

            var list = element.Find("./form/list/listitem/text");
            var ileKolumnWValdict = (from li in list select li.ChildIndex).Max();
            var valdict = new List<List<string>>();
            for (var i = 0; i < list.Count(); )
            {
                var row = new List<string>();
                while (i < list.Count() && list[i].ChildIndex < ileKolumnWValdict)//dodaje mniejsze od max child indexa
                {
                    row.Add(list[i++].As<Text>().TextValue);
                }
                row.Add(list[i++].As<Text>().TextValue);//i na koniec maksymalnego child indexa
                valdict.Add(row);
            }
            button.Click();//zamykanie valdicta

            for (int i = 0; i < valdict.Count; i++)
            {
                for (int j = 0; j < valdict[i].Count; j++)
                {
                    DebugLog(String.Format("[{0},{1}]: {2}",i,j,valdict[i][j]));
                }
            }

            return new SilverlightTable(valdict);
        }

        public void WprowadzWartoscDoPola(string kolumna, int wiersz, string wartosc, string VALDICTzMenuCzyZReki = "z menu", string opcjalnyFiltrDoValdicta = "")
        {
            var field = Fields.ToList().Find(s => s.Label == kolumna);
            var x = KomorkaWTabelce(kolumna, wiersz); //text
            var element = x.Element.Parent; //element
            switch (field.Type)
            {
                case FieldTypes.TEXTBOX:
                    {

                        element.EnsureVisible();
                        element.As<Unknown>().Click();
                        Keyboard.Press(wartosc);

                        break;
                    }

                case FieldTypes.VALDICT:
                    {
                        #region przesuwanie o 1 pole do przodu zeby uniknac problemow z niewidocznym buttonem pomimo ze jest visible

                        int ci = x.Element.Parent.ChildIndex;
                        int ilosckolumn = InternalTable[0].Count(); //z headera
                        string path = "";
                        if (ci < ilosckolumn)
                            path = string.Format(".//element[@childindex='{0}']", ci + 1);
                        ZnajdzWKontenerzeLubOknie<Unknown>(path).EnsureVisible();
                        
                        //scenariusz 02_08_087 jest w poniższy sposób naprawiony, ale te child indexy wyżej dalej są źle
                        //bo nie uwzględniają niewidzialnych kolumn. Trzeba by bylo je jakoś zgodnie numerować żeby znać
                        //PRAWDZIWĄ ilość, jeśli pojawi się kiedyś jakiś problem. To tak na przyszłość - LK
                        //updejt o 14:28 - O, już wiem, może po prostu pod ilosckolumn podstawić childIndex ostatniej kolumny (czyli maksymalny)?
                        if(InternalTable[0].Last().TextValue==kolumna)
                        {
                            Keyboard.Down(Keys.Shift);
                            Keyboard.Press(Keys.End);
                            Keyboard.Up(Keys.Shift);                           
                        }

                        #endregion

                        element.EnsureVisible();
                        element.As<Unknown>().Click();

                        if (VALDICTzMenuCzyZReki == "z menu") //klikamy w button, valdict sie rozwija, wybieramy dane z listy
                        {
                            var button = element.FindSingle("./button[@automationid='ButtonOpen']",5000); //13.05 LK - timeout dla guziorów, a co sobie będziemy żałować
                            button.EnsureVisible();
                            button.As<Button>().Click();
                            //czarna strzalka pojawia sie po nacisnieciu na komorke
                            WaitForProgressBarInElement(element.As<Unknown>());
                            //progressbar tam jest i czyha - atakuje gdy valdict ładuje dane
                            element.FindSingle(string.Format(".//text[@name='{0}']", wartosc), 30000).As<Text>().Click();
                            //wybieramy dane
                        }

                        if (VALDICTzMenuCzyZReki == "z reki" && opcjalnyFiltrDoValdicta == "") //klikamy na valdict, wpisujemy wartosc ktora NIE JEST FILTREM tylko wartoscia ostateczna
                        {
                            Keyboard.Press(wartosc);
                        }

                        if (VALDICTzMenuCzyZReki == "z reki" && opcjalnyFiltrDoValdicta != "")  //klikamy na valdict, wpisujemy wartosc ktora JEST FILTREM, valdict sie rozwija, klikamy na wartosc tekstowa w rozwinietym menu
                        {
                            Keyboard.Press(opcjalnyFiltrDoValdicta);
                            Keyboard.Press(Keys.Enter);

                            WaitForProgressBarInElement(element.As<Unknown>());
                            element.FindSingle(string.Format(".//text[@name='{0}']", wartosc), 30000).As<Text>().Click();
                        }
                        break;
                    }

                case FieldTypes.COMBOBOX:
                    {
                        element.EnsureVisible();

                        element.As<Unknown>().Click();
                        var szczauka = element.FindSingle("./button");
                        szczauka.As<Button>().Click();

                        //element.As<Unknown>().DoubleClick();
                        //13.04 LK - eksperymentalna wersja klikania w comboboxy
                        //stara wersja robiła doubleClick, ale jeżeli w comboboxie jest warning to tak to nie działa
                        //dlatego teraz będziemy klikać w strzałeczke, po bożemu

                        WaitForLoad();
                        element.FindSingle(string.Format(".//text[@name='{0}']", wartosc)).As<Text>().Click();
                        break;
                    }
                default:
                    throw new Exception(String.Format("{0} o typie {1} nie jest obsługiwana",kolumna, field.Type));
            }
        }

        public void WprowadzWartosciDoPolWWieluWierszach(string[] nazwyKolumn, int[] wiersze, string[] wartosci)
        {
            if (nazwyKolumn.Length != wiersze.Length || nazwyKolumn.Length != wartosci.Length)
                throw new ArgumentException(string.Format("kolumn: {0}, wierszy: {1}, wartosci: {2}", nazwyKolumn.Length,
                    wiersze.Length, wartosci.Length));
            for (int i = 0; i < nazwyKolumn.Length; i++)
                WprowadzWartoscDoPola(nazwyKolumn[i], wiersze[i], wartosci[i]);
        }

        public void WprowadzWieleWartosciDoPolWJednymWierszu(int wiersz, string[] pola, string[] wartosci)
        {
            if (pola.Length != wartosci.Length)
                throw new ArgumentException("pola.Length!=wartosci.Length");
            for (int i = 0; i < pola.Length; i++)
            {
                WprowadzWartoscDoPola(pola[i], wiersz, wartosci[i]);
                WaitForLoadInWindow(); //29.06 LK - dodajemy bo za dużo incydentów z przymulaniem na serwerze ostatnio
            }
        }

        public void WprowadzWartoscDoFiltru(string kolumna, string filtr)
        {
            var headers=_table.FindSingle<Row>("./row[@automationid='ColumnHeadersPresenter' and @visible='true']");
            var ci=headers.Element.ChildIndex + 1;

            var row=_table.FindSingle<Row>(String.Format("./row[@childindex={0} and @visible='true']",ci));

            List<string> naglowki = new List<string>();
            foreach (Text naglowek in InternalTable[0])
            {
                naglowki.Add(naglowek.TextValue);
            }
            var columnIndex = InternalTable[0][naglowki.IndexOf(kolumna)].Element.Parent.ChildIndex;

            var komorka = row.FindSingle<Cell>(String.Format("./cell[@classname='DataGridCell' and @columnindex={0}]",columnIndex));
            komorka.EnsureVisible();
            komorka.Click();
            Keyboard.Press(filtr);

            AktualizujTabelke();
        }


        public bool SprawdzAktywnoscPola(string kolumna, int wiersz)
        {
            var field = Fields.ToList().Find(s => s.Label == kolumna);
            var pole = KomorkaWTabelce(kolumna, wiersz);
            var element = pole.Element.Parent;
            switch (field.Type)
            {
                case FieldTypes.TEXTBOX:
                    {
                        element.As<Unknown>().Click();
                        var valueBeforeEditing = pole.TextValue;

                        double result;
                        Keyboard.Press(Double.TryParse(valueBeforeEditing, out result) ? "42,42" : "TEST");
                        var aktywne = pole.TextValue != valueBeforeEditing;
                        if (aktywne)
                        {//pole aktywne, powracamy do starej tresci
                            int len = pole.TextValue.Length;
                            while (len-->0)
                            {
                                Keyboard.Press("\b");
                            }
                            Keyboard.Press(valueBeforeEditing ?? String.Empty);
                        }
                        return aktywne; 
                    }

                case FieldTypes.VALDICT:
                    {
                        element.As<Unknown>().Click();

                        //debug dla testu D_02_06_079
                        DebugLog("Sprawdzam aktywnośc pola typu VALDICT");
                        DebugLog(String.Format("Ilośc dzieci valdicta: {0}",element.Children.Count()));
                        foreach (var dziecie in element.Children)
                        {
                            DebugLog(String.Format("Rola dziecka nr {0} : {1}",dziecie.ChildIndex,dziecie.Role));
                        }

                        //17.04 LK - zależność między testami, jeśli wcześniej wyrenderowały się obrazki dla errorów
                        //to nie możemy sprawdzać aktywności na podstawie ilości dzieci. Sprawdzimy po przycisku

                        Element niekochanyButton;
                        return element.TryFindSingle("./button[@automationid='ButtonOpen']", out niekochanyButton);

                        //return element.Children.Count() > 1 &&
                        //       element.FindSingle("./button[@automationid='ButtonOpen']").Enabled;
                    }

                case FieldTypes.COMBOBOX:
                    {
                        element.As<Unknown>().DoubleClick();// jakas roznica czy click czy double click? //AA
                        return element.Children.Count() > 1 &&
                               element.FindSingle("./form[@automationid='Popup']").Visible; //nie powinno tu byc Enabled? //AA
                    }
                default:
                    throw new Exception(String.Format("{0} ma typ {1}, który nie jest obsługiwany",kolumna,field.Type));
            }
        }

        public void SprawdzCzyPolaSaNieaktywne(string[] kolumny, int[] wiersze)
        {
            if (kolumny.Count() != wiersze.Count())
                throw new Exception("Ilosc elementow w tabeli kolumny i tabeli wiersze nie jest rowna, a powinna byc");

            for (int i = 0; i < kolumny.Count(); i++)
            {
                bool rzeczywistyStan = SprawdzAktywnoscPola(kolumny[i], wiersze[i]);
                if (rzeczywistyStan)
                    throw new Exception(string.Format("Aktywnosc pol sie nie zgadza - pole: [{0}][{1}], oczekiwany stan: false, rzeczywisty stan: true", kolumny[i], wiersze[i]));
            }
        }

        public void SprawdzCzyPolaSaNieaktywneWWierszu(string[] kolumny, int wiersz)
        {
            SprawdzAktywnoscWieluPol(kolumny, new[] {wiersz}, false);
            //13.04 LK - poprawka tej linijki, było tu jakieś dziwne enumerable które wstawiało tablice na szerokość kolumn
            //ta funkcja sprawdza dla danej kolumny wszystkie podane wiersze, a nie sparowane 1:1.
        }

        //public void SprawdzAktywnoscWieluPol(string[] kolumny, int[] wiersze, bool[] oczekiwanaAktywnosc)
        //{
        //    for (int i = 0; i < kolumny.Count(); i++)
        //    {
        //        bool rzeczywistyStan = SprawdzAktywnoscPola(kolumny[i], wiersze[i]);
        //        if (rzeczywistyStan != oczekiwanaAktywnosc[i])
        //            throw new Exception(string.Format("Aktywnosc pol sie nie zgadza - pole: [{0}][{1}], oczekiwany stan: {2}, rzeczywisty stan: {3}", kolumny[i], wiersze[i], oczekiwanaAktywnosc[i], rzeczywistyStan));
        //    }
        //}

        public void SprawdzAktywnoscWieluPol(string[] kolumny, int[] wiersze, bool oczekiwanaAktywnosc)
        {
            for (int i = 0; i < kolumny.Count(); i++)
            {
                for (int j = 0; j < wiersze.Count(); j++)
                {
                    bool rzeczywistyStan = SprawdzAktywnoscPola(kolumny[i], wiersze[j]);
                    if (rzeczywistyStan != oczekiwanaAktywnosc)
                        throw new Exception(
                            string.Format(
                                "Aktywnosc pol sie nie zgadza - pole: [{0}][{1}], oczekiwany stan: {2}, rzeczywisty stan: {3}",
                                kolumny[i], wiersze[j], oczekiwanaAktywnosc, rzeczywistyStan));
                }
            }

        }

        public bool SprawdzZawartoscWieluPol(string[] kolumny, string[] wartosci, int numerWiersza)
        {
            if (kolumny.Length != wartosci.Length)
                throw new Exception("Niezgodna ilość pól i wartości");

            for (int i = 0; i < kolumny.Length; i++)
            {
                var s1 = wartosci[i];
                var s2 = PobierzWartoscZTabelki(kolumny[i], numerWiersza);

                if (!String.Equals(s1 ?? "", s2 ?? ""))
                {
                    InfoLog(String.Format("Dla {2} spodziewane: {0}, było: {1}", s1 ?? "", s2 ?? "", kolumny[i]));
                    return false;
                }
                InfoLog(String.Format("Porównano kolumnę {0}", kolumny[i]));
            }

            return true;
        }

        public bool SprawdzZawartoscTooltipow(string[][] dane)
        {
            //tablica z danymi powinna zawierać tablice 4 elementowe, po jednej dla każdego tooltipa. Elementy to kolejno:
            //1 - Nazwa kolumny w której mamy tooltip
            //2 - Spodziewana treść wewnątrz tooltipa
            //3 - typ tooltipa. Zamieniamy go na odpowiednią wartość child index wewnątrz komórki
            //4 - numer wiersza który sprawdzamy
            //typy to "error" i "warning"

            foreach (var tooltip in dane)
            {
                InfoLog(String.Format("Szukam tooltipa {0}", tooltip[0]));
                string wartoscOczekiwana = tooltip[1];
                int ci;
                switch (tooltip[2]) //jest jeszcze jeden obrazek z przesunieciem 0, ale nie wiem jaki...
                {
                    case "error":
                        ci = 2;
                        break;

                    case "warning":
                        ci = 1;
                        break;

                    default:
                        InfoLog("Nie podano typu tooltipa do odnalezienia.");
                        return false;
                }

                var komorka = KomorkaWTabelce(tooltip[0], Int32.Parse(tooltip[3])).Element.Parent;

                Picture obrazek = komorka.FindSingle(String.Format("./picture[@ChildIndex={0} and @visible='true']", ci));

                obrazek.MoveTo();
                Delay.Milliseconds(600); //TUTAJ JEST ANIMACJA LADOWANIA TOOLTIPA - MUSI ZOSTAC

                string wartoscAktualna = OpenWebsite.FindSingle<Text>(".//tooltip[@classname='ToolTip' and @visible='true']/text[@visible='true']").TextValue; //todo losowo znajduje tooltipy w D_02_02_130

                if (wartoscOczekiwana != wartoscAktualna)
                {
                    InfoLog(String.Format("Wartość niezgodna, ma być: {0}, jest: {1}", wartoscOczekiwana, wartoscAktualna));
                    return false;
                }
            }

            return true;
        }

        public void KliknijWIkoneUsunWWierszu(int numerWiersza)
        {
            //jeżeli istnieją tabele gdzie ikona usuń jest w innej kolumnie to do poprawki
            var wiersz = _znajdzWKontenerze?_container.FindSingle(String.Format(".//element[@automationid='RowsPresenter']/element[@childindex={0}]", numerWiersza - 1)):
                _okno.FindSingle(String.Format(".//element[@automationid='RowsPresenter']/element[@childindex={0}]", numerWiersza - 1));
            Button przycisk = wiersz.FindSingle("./element[1]/button[@visible='true']");
            przycisk.Click();
        }

        public void KliknijWCheckbox(uint wiersz)
        {
            var rowsPresenter = _table.FindSingle<Unknown>("./element[@automationid='RowsPresenter']");
            var checkBox = rowsPresenter.FindSingle<CheckBox>(string.Format("./element[{0}]/element[1]/checkbox[@classname='CheckBox']", wiersz));
            checkBox.MoveTo();
            checkBox.Check();
        }

        public T ZnajdzWKontenerzeLubOknie<T>(RxPath path) where T : Adapter
        {
            if (_znajdzWKontenerze)
                return _container.FindSingle<T>(path);
            return _okno.FindSingle<T>(path);
        }

        #region buttony i pola tekstowe w tabelce/tabelkach

        public bool SprawdzCzyDodajNowyWierszJestAktywne()
        {
            return GetButtonByNameInWindow("Dodaj nowy wiersz").Enabled;
            

        }

        public bool SprawdzCzyPrzeliczenieVATJestAktywne()
        {
            string windowDiv = "/container[@automationid='scrollViewer']";

            var button = GetLabelledButton("Przeliczenie VAT", windowDiv, 2);
            button.Click();
            var form = GetLabelledForm("Przeliczenie VAT", windowDiv, 3, false);
            if (form.Visible)
            {
                button.Click();
                return true;
            }
            return false;
        }

        public void ZmienWartoscPrzeliczanieVAT(string nowaWartosc)
        {
            string windowDiv = "/container[@automationid='scrollViewer']";

            var button = GetLabelledButton("Przeliczenie VAT", windowDiv, 2);
            button.Click();
            var form = GetLabelledForm("Przeliczenie VAT", windowDiv, 3);
            form.FindSingle<Text>(string.Format(".//text[@name='{0}']", nowaWartosc)).Click();

        }

        public List<List<string>> PobierzSumaNettoVATBrutto()
        {
            string windowDiv = "/container[@automationid='scrollViewer']";

            List<List<string>> lista = new List<List<string>>();
            lista.Add(GetLabelledTextboxContent("Suma netto", windowDiv));
            lista.Add(GetLabelledTextboxContent("Suma VAT", windowDiv));
            lista.Add(GetLabelledTextboxContent("Suma brutto", windowDiv));
            return lista;
        }

        public void KliknijDodajNowyWiersz(string nazwa = "Dodaj nowy wiersz", int pierwszeRuchomePole = 0,bool aktualizujTabelke=true)
        {
            WaitForLoadInWindow();
            GetButtonByNameInWindow(nazwa).Click();

            WaitForLoadInWindow();
            if(aktualizujTabelke)
                InternalTable = PobierzTabelkeDoEdycji(pierwszeRuchomePole: pierwszeRuchomePole);     //dobre programowanie, pozdrawiam, Tadzio
        }

        public SilverlightInfoPopup KliknijZapisz()
        {
            GetButtonByNameInWindow("Zapisz").Click();
            WaitForLoad();
            return new SilverlightInfoPopup(OpenWebsite, BrowserId);
        }

        public SilverlightInfoPopup KliknijZapiszIZakoncz(bool waitForLoad=true)
        {
            GetButtonByNameInWindow("Zapisz i zakończ").Click();
            if(waitForLoad)
                 WaitForLoad();
            return new SilverlightInfoPopup(OpenWebsite, BrowserId);
        }

        public SilverlightInfoPopup KliknijUsun()
        {
            GetButtonByNameInWindow("Usuń").Click();
            WaitForLoad();
            return new SilverlightInfoPopup(OpenWebsite, BrowserId);
        }

        public SilverlightInfoPopup KliknijAnuluj()
        {
            GetButtonByNameInWindow("Anuluj").Click();
            WaitForLoad();
            return new SilverlightInfoPopup(OpenWebsite, BrowserId);
        }

        public void KliknijUsunZaznaczone()
        {
            GetButtonByNameInWindow("Usuń zaznaczone").Click();    
            WaitForLoad();
            InternalTable = PobierzTabelkeDoEdycji(); //niedobrze, bo nie mamy argumentów które normalnie dostaje konstruktor... może sie wysypać w specyficznych przypadkach- LK
        }


        public int IloscWierszy
        {
            get { return InternalTable.Count; }
        }

        public bool SprawdzAktywnoscDodaiIReguluj()
        {
            return GetButtonByNameInWindow("Dodaj i reguluj").Enabled;
        }

        public bool SprawdzAktywnoscUsunZaznaczone()
        {
            return GetButtonByNameInWindow("Usuń zaznaczone").Enabled;
        }

        public bool SprawdzAktywnoscDodaj()
        {
            return GetButtonByNameInWindow("Dodaj").Enabled;
        }

        public bool SprawdzNaglowekTabeli(string[] nazwyKolumn)
        {
            var naglowek = InternalTable[0];
            if (naglowek.Count != nazwyKolumn.Length)
                return false;
            for (int i = 0; i < naglowek.Count; i++)
                if (naglowek[i].TextValue != nazwyKolumn[i])
                    return false;
            return true;
        }

        #endregion
    }
}
