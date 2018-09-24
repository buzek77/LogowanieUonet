using System;
using Ranorex;
using Ranorex.Core;
using VCommonPages.Pages.SilverlightCommon;
using VRanorexLib;
using System.Collections.Generic;

namespace VCommonPages.Pages.SilverlightCommon
{
    public class SilverlightTable : SilverlightPage, IEquatable<SilverlightTable>
    {

        private List<List<string>> TabelkaInternal = new List<List<string>>();
        private Table _table;

        public List<SilverlightLabelTextboxSection> detailsData = new List<SilverlightLabelTextboxSection>();

        public int iloscKolumn
        {
            get { return TabelkaInternal[0].Count; }
        }

        public int iloscWierszy
        {
            get { return TabelkaInternal.Count; }
        }

        public SilverlightTable(WebDocument openWebsite, int? browserId, string tabpage, int ktoraTabelka = 0, bool ignoreHeader = false, bool detailsPresenter = false, bool forceVisibleCells = false)
            : base(openWebsite, browserId)
        {
            TabelkaInternal = PobierzTabelkeJakoListe(tabpage, ktoraTabelka, detailsPresenter, ignoreHeader,forceVisibleCells);
        }

        public SilverlightTable(string filename)
        {
            TabelkaInternal = CSVImport.ImportujTabelkeZCSV(filename);
        }

        internal SilverlightTable(List<List<string>> tabelka)
        {
            TabelkaInternal = tabelka;
        }

        public SilverlightTable(List<List<Text>> tabelka)
        {
            List<List<string>> result = new List<List<string>>();
            foreach (var row in tabelka)
            {
                var tempRow = new List<string>();
                foreach (var text in row)
                {
                    tempRow.Add(text.TextValue ?? "");
                }
                result.Add(tempRow);
            }
            TabelkaInternal = result;
        }

        public void WypiszZawartoscTabelkiDebug()
        {
            //15.04 LK - tymczasowa funkcja do debugowania z serwera, do wywalenia potem czy coś

            for (int i = 0; i < TabelkaInternal.Count;i++)
            {
                for (int j = 0; j < TabelkaInternal[i].Count;j++)
                {
                    DebugLog(String.Format("[{0}][{1}] : {2}",i,j,TabelkaInternal[i][j]));
                }

            }
        }

        public List<List<string>> PobierzTabelkaInternal()
        {
            return TabelkaInternal;
        } 

        public List<List<string>> PobierzTabelkeJakoListe(string tabpage, int ktoraTabelka = 0, bool detailsPresenter = false, bool ignoreHeader = false, bool forceVisibleCells = false)
        {
            List<List<string>> wartosci = new List<List<string>>();
            string tabPath = string.Format("/tabpage[@name='{0}']", tabpage);
            var tabpageWOknie = Find<TabPage>(tabPath);
            var wszystkieTabelkiWOknie = tabpageWOknie.FindChildren<Table>();
            Table szukanaTabelka = wszystkieTabelkiWOknie[ktoraTabelka];
            _table = szukanaTabelka;
            var szukanaTabelkaPath = szukanaTabelka.GetPath();

            // Odczytaj naglowek
            if (!ignoreHeader)
            {
                var naglowek =
                    szukanaTabelka.FindSingle<Row>(szukanaTabelkaPath +
                                                   "/row[@automationid='ColumnHeadersPresenter']");

                var daneZNaglowka = naglowek.Find<Cell>("./cell");

                List<string> stringiZNaglowka = new List<string>();
                foreach (Cell komorka in daneZNaglowka)
                {
                    if (!komorka.Visible)
                        komorka.EnsureVisible();

                    if (komorka.Visible && komorka.Text != null)
                        stringiZNaglowka.Add(komorka.Text);
                }

                if (stringiZNaglowka.Count > 0)
                    wartosci.Add(stringiZNaglowka);
            }

            // Odczytaj wiersze tabeli
            DebugLog(string.Format("Czytam tabelke, tabpage: {0}, tabelka nr: {1}", tabpage, ktoraTabelka));
            var wierszeTabelki = Find<Unknown>(szukanaTabelkaPath + "/element[@automationid='RowsPresenter' and @visible='true']");

            List<Unknown> listaWierszy = new List<Unknown>(wierszeTabelki.FindChildren<Unknown>());

            #region renderowanie
            // Przejedz na koniec tabelki, zeby wyrenderowac wszystkie cudowne, pelne radosci i zycia, komorki
            int szerokoscTabelki = listaWierszy[0].FindChildren<Unknown>().Count - 2; //-2 bo jest detailspresenter
            List<Unknown> pierwszyWiersz = new List<Unknown>(listaWierszy[0].FindChildren<Unknown>());
            pierwszyWiersz[szerokoscTabelki].EnsureVisible();
            // Przejechac na poczatek tez wypada, zaufaj mi, jestem elektrykiem, znam sie na tym -dg
            pierwszyWiersz[0].EnsureVisible();
            #endregion

            if (detailsPresenter)
            {
                foreach (Unknown row in listaWierszy)
                {
                    row.MoveTo();
                    //List<SilverlightLabelTextboxSection.LabelTextboxFields> presenter = new List<SilverlightLabelTextboxSection.LabelTextboxFields>();

                    var lista = row.Find<Text>("./element[@automationid='DetailsPresenter']/text[@visible='true']");
                    if (lista.Count % 2 != 0) throw new Exception("COS jest nie tak powina być parzysta ilosc lablek i wartosci");

                    var dane = new string[lista.Count / 2][];
                    for (int i = 0; i < lista.Count; i = i + 2)
                    {
                        //presenter.Add(new SilverlightLabelTextboxSection.LabelTextboxFields
                        //                  {Label = lista[i].TextValue, Value = lista[i + 1].TextValue});
                        dane[i / 2] = new[] { lista[i].TextValue, lista[i + 1].TextValue };
                    }
                    detailsData.Add(new SilverlightLabelTextboxSection(dane));
                }
            }


            foreach (Unknown row in listaWierszy)
            {
                List<string> stringi = new List<string>();
                stringi.Clear();
                if (detailsPresenter)
                    row.MoveTo();

                //17.04 LK - wiosenne porządki poniżej
                //dla forceVisibleCells==true pobieramy teraz całą listę, za to sprawdzamy komórki pojedynczo
                //czy są visible, i poruszamy się po nich w pętli.
                IList<Text> lista;

                lista = row.Find<Text>("./element[@classname='DataGridCell']/text");
                //else
                //    lista = row.Find<Text>("./element[@classname='DataGridCell']/text[@visible='true']");

                foreach (Text komorka in lista)
                {
                    if (forceVisibleCells)
                    {
                        komorka.EnsureVisible();
                        if(!komorka.Visible)
                            continue;
                    }

                    if (komorka.TextValue != null)
                        stringi.Add(komorka.TextValue);
                    if (komorka.TextValue == null)
                        stringi.Add("");
                    
                }

                wartosci.Add(stringi);
            }

            return wartosci;
        }


        //public string PobierzWartoscSzczegolowe(int wiersz,bool prawa,string label)
        //{
        //    var labelki = detailsData[wiersz].FindAll(s => s.Label.Trim() == label.Trim());
        //    if (labelki.Count != 2) throw new Exception("Cos jest nie tak powino byc count 2 ");
        //    return prawa ? labelki[1].Value : labelki[0].Value;
        //}



        public void KliknijIkoneEdycjiWWierszu(int numerWiersza, string tabpage, int ktoraTabelka = 0, bool automationID = true,bool szukajPoLp = true)
        {
            var presenter = _table.FindSingle<Unknown>("./element[@automationid='RowsPresenter']");

            //04.05 LK - grubsza zmiana. Numer wiersza LP nie zawsze odpowiada childindexowi, bo mogą być przeskoki w Lp bo np. inne wiersze zostały usunięte.
            //dlatego zamiast po childindexie szukamy konkretnego textu w kolumnie o childindexie 2 i potem cofamy się dwa parenty w tył żeby otrzymać wiersz.
            //06.05 LK - no i paczcie, niektóre tabelki nie mają LP więc sie sypie. Dodajemy parametr szukajPoLp, domyślnie na true, w kontrahencie na false.
            Unknown wiersz;
            if (szukajPoLp)
            {
                var text =
                    presenter.FindSingle<Unknown>(string.Format(".//element[@childindex='2']/text[@text='{0}']",
                                                                numerWiersza));
                wiersz = text.Element.Parent.Parent.As<Unknown>();
            }
            else
            {
                wiersz = presenter.FindSingle<Unknown>(string.Format("./element[{0}]", numerWiersza));
            }

            Button przycisk;
            if (automationID)
                przycisk = wiersz.FindSingle<Button>(".//button[@automationid='ChangeRegulationsElement']");
            else
                przycisk = wiersz.FindSingle<Button>("./element/button[@classname='Button']");

            przycisk.Click();
        }

        public bool SprawdzCzyOlowekJestWyszarzony(int numerWiersza, string tabpage, int ktoraTabelka = 0)
        {
            var presenter = _table.FindSingle<Unknown>("./element[@automationid='RowsPresenter']");
            var wiersz = presenter.FindSingle<Unknown>(string.Format("./element[{0}]", numerWiersza));
            var przycisk = wiersz.FindSingle<Button>(".//button[@automationid='ChangeRegulationsElement']");
            return !przycisk.Enabled;
        }

        public SilverlightTable PobierzWybraneWierszeZTabelki(int[] wiersze)
        {
            List<List<string>> nowaTabelka = new List<List<string>>();
            List<List<string>> staraTabelka = TabelkaInternal;

            foreach (List<string> row in staraTabelka)
            {
                foreach (int indeks in wiersze)
                    if (staraTabelka.IndexOf(row) == indeks)
                        nowaTabelka.Add(row);
            }
            return new SilverlightTable(nowaTabelka);
        }


        public SilverlightTable PobierzWybraneKolumnyZTabelki(int[] kolumny)
        {
            List<List<string>> nowaTabelka = new List<List<string>>();
            List<List<string>> staraTabelka = TabelkaInternal;

            foreach (List<string> row in staraTabelka)
            {
                List<string> nowyWiersz = new List<string>();
                foreach (int numerkolumny in kolumny)
                    nowyWiersz.Add(row[numerkolumny - 1]);
                nowaTabelka.Add(nowyWiersz);
            }

            return new SilverlightTable(nowaTabelka);
        }

        public string PobierzNazweWybranejKolumny(int kolumna)
        {
            return TabelkaInternal[0][kolumna - 1];
        }

        #region Porównanie
        public bool Equals(SilverlightTable other)
        {
            var wynik = true;
            var otherTable = other.TabelkaInternal;
            if (otherTable.Count != TabelkaInternal.Count)
            {
                InfoLog(string.Format("Różna ilość wierszy jest {0} ma być {1}", otherTable.Count, TabelkaInternal.Count));
                return false;
            }
            for (int i = 0; i < TabelkaInternal.Count; i++)
            {
                if (otherTable[i].Count != TabelkaInternal[i].Count)
                {
                    InfoLog(string.Format("Rozna ilość kolumn w wierszu {2} jest {0} ma być {1}", otherTable[i].Count, TabelkaInternal[i].Count, i));
                    return false;
                }
                for (int j = 0; j < TabelkaInternal[i].Count; j++)
                {
                    TabelkaInternal[i][j] = TabelkaInternal[i][j].Replace((char)160, ' ');
                    otherTable[i][j] = otherTable[i][j].Replace((char)160, ' ');
                    if (TabelkaInternal[i][j] != otherTable[i][j])
                    {
                        InfoLog(string.Format("Różna wartość w elemencie [{0}][{1}] jest : \"{2}\" a ma być \"{3}\"", i, j, otherTable[i][j], TabelkaInternal[i][j]));
                        wynik = false;
                    }
                }
            }
            return wynik;
        }

        #endregion

    }
}
