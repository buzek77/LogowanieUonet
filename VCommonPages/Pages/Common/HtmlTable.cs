using System;
using System.Collections.Generic;
using System.Linq;
using Ranorex;
using System.Diagnostics;

namespace VCommonPages.Pages.Common
{
    /// <summary>
    /// Klasa do pobierania zbioru informacji z tabelek
    /// </summary>
    public class HtmlTable : Page , IEquatable<HtmlTable> // aby dało sie porownywac przez nunita
    {
        internal readonly List<List<string>> TabelkaInternal = new List<List<string>>();

        public HtmlTable(WebDocument openWebsite, string id, int? browserId, bool ignoreHeader = false, string myPath = "", int wierszPoczatek=0, int liczbaWierszyDoPobrania=0, int kolumnaPoczatek = 0, int liczbaKolumnDoPobrania = 0, int numerTabelki=0)
            : base(openWebsite, browserId)
        {
            TabelkaInternal = PobierzTabelkeJakoListe(id, ignoreHeader, myPath, wierszPoczatek, liczbaWierszyDoPobrania, kolumnaPoczatek, liczbaKolumnDoPobrania, numerTabelki);
              
        }


        public HtmlTable(IEnumerable<string[]> dane)
        {
            List<List<string>> _tablela = new List<List<string>>();
            foreach (var row in dane)
            {
                List<string> columns = new List<string>();
                foreach (var column  in row)
                {
                    columns.Add(column);
                }
                _tablela.Add(columns);
            }
            TabelkaInternal = _tablela;
        }// konstruktor dla wersji z kodu

        public override string ToString() // nunit uzywa tostring o pobierania wartosci expectedu but was
        {
            string toReturn = "";
            foreach (var row in TabelkaInternal)
            {
                toReturn += "{";
                foreach (var columns in row)
                {
                    toReturn += columns +"|";
                }
                toReturn += "} \n";
            }
            return toReturn;
        }

        public List<List<string>> ToList()
        {
            return TabelkaInternal;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tableId">Identyfikator tabelki</param>
        /// <param name="ignoreHeader">true = pomiń nagłówek tabeli</param>
        /// <param name="myPath"> ścieżka doprecyzująca pozycje tabelki opisanej identyfikatorem tableId</param>
        /// <param name="wierszPoczatek">numer wiersza od którego ma pobrać dane, domyślnie 0</param>
        /// <param name="liczbaWierszyDoPobrania">liczba wierszy do pobrania, domyślnie 0</param>
        /// <param name="kolumnaPoczatek">numer kolumny od której ma pobrać dane, domyślnie 0</param>
        /// <param name="liczbaKolumnDoPobrania">liczba kolumn do pobrania, domyślnie 0</param>
        /// <param name="numerTabelki">numer tabelki do pobrania, domyślnie 1</param>
        /// <returns></returns>
        protected List<List<string>> PobierzTabelkeJakoListe(string tableId, bool ignoreHeader = false, string myPath = "", int wierszPoczatek = 0, int liczbaWierszyDoPobrania = 0, int kolumnaPoczatek = 0, int liczbaKolumnDoPobrania = 0, int numerTabelki=0)
        {
            DebugLog("Pobieramy wartosci z tabelki");
            
            // Ustal ścieżkę pomocniczą do obiektu TBodyTable
            string divPath = string.Format("{0}/div[@Id~'{1}'][{2}]", myPath, tableId, (numerTabelki==0 ? "" : numerTabelki.ToString()));

            List<List<string>> wartosci = new List<List<string>>();
            
            // Pobierz nagłowek tabeli
            if (!ignoreHeader)
            {
                // Sprawdź czy tabelka ma nagłówek w sekcji theadTag
                var cntChildren = Find<Unknown>(divPath).FindDescendants<TableTag>()[0].FindDescendants<THeadTag>().Count;
                if (cntChildren > 0)
                {
                    var head = Find<TrTag>(divPath + "//table/thead/tr");
                    wartosci.Add(head.FindChildren<ThTag>().Select(row => row.InnerText ?? "").ToList());
                }
                else
                {
                    // lub w sekcji ThTag
                    cntChildren = Find<Unknown>(divPath).FindDescendants<TableTag>()[0].FindDescendants<ThTag>().Count;
                    if (cntChildren > 0)
                    {
                        var head = Find<TrTag>(divPath + "//table/tbody/tr[1]");
                        wartosci.Add(head.FindChildren<ThTag>().Select(row => row.InnerText ?? "").ToList());
                        wierszPoczatek++; // K.O !!!
                    }
                }
            }

            // Pobierz obiekt TBodyTable
            var table = Find<TBodyTag>(divPath + "//table[@visible='True']/tbody");
            
            // Ustal liczbę wierszy i kolumn
            var cntRows = (liczbaWierszyDoPobrania == 0 ? table.FindChildren<TrTag>().Count : liczbaWierszyDoPobrania);
            
            // K.O
            // Uwaga, w przypadku gdy mamy do czynienia z tabelami których wiersze mają zmienną liczbe kolumn
            // zawężanie liczby kolumn poprzez ustawianie wartości parametru liczbaKolumnDoPobrania, nie bedzie działało poprawnie (!)
            // W sytuacji tabel regularnych powinno działać.
            var cntCols = (liczbaKolumnDoPobrania == 0 ? (table.FindChildren<TrTag>().Count > 0 ?
                                                                table.FindChildren<TrTag>().
                                                                    Select(tr => tr.FindChildren<WebElement>().Count).Max() : 0) : // wiersz może zawierać ThTag, może także mieć zmienna liczbę kolumn, stąd ta zabawa
                                                            liczbaKolumnDoPobrania);

            DebugLog(string.Format("Ustalono do pobrania - wierszy:{0}, column:{1}", cntRows, cntCols));

            // Pobierz zawartość wskazanej tabelki
            wartosci.AddRange(table.FindChildren<TrTag>().
                Skip(wierszPoczatek).               // pomiń początkową liczbę wierszy
                Take(cntRows).                      // pobierz wskazaną liczbę wierszy
                Select(tr => tr.FindChildren<TdTag>().
                    Skip(kolumnaPoczatek).          // pomiń początkową liczbę kolumn
                    Take(cntCols).                  // pobierz wskazaną liczbę kolumn, uwaga w przypadku tabel z wierszmi zawierającymi zmienna liczbe kolumn ta konstrukcja może źle działać
                        Select(td => td.FindChildren<WebElement>().Count > 0 ?
                                            (td.FindDescendants<WebElement>().Where(w => w.InnerText != null).Count() > 0 ? // sprawdz czy są obiekty z ustawionymu atrybutem InnerText
                                                string.Join("", td.FindDescendants<WebElement>().                           // ta konstrukcja jest niebędna żeby zbudować napis z wielu elementów, które mają ustawiony atrybut InnerText
                                                                    Where(w => w.InnerText != null).
                                                                    Select(o => o.InnerText.Trim()).ToList()) : "") :
                                            (td.InnerText != null ? td.InnerText.Trim() : "")).                             // jeśli brak obiektów a nadtzędny ma ustawiony atrybut InnerText
                        ToList()));


            #region __OLD__
            /*
            if (liczbaKolumnDoPobrania != 0)
            {
                // Uwaga! kolumny moga mieć ustawione atrybut Colspan!
                wartosci.AddRange(table.FindChildren<TrTag>().
                    Skip(wierszPoczatek).
                    Take(cntRows).
                    Select(tr => tr.FindChildren<TdTag>().
                        Select(td => td.InnerText != null ? td.InnerText.Trim() : GetInnerText(td)).
                        Skip(kolumnaPoczatek).Take(cntCols).
                        ToList()));

                //wartosci.AddRange(table.FindChildren<TrTag>().
                //    Skip(wierszPoczatek).
                //    Take(cntRows).
                //    Select(tr => tr.FindChildren<TdTag>().
                //        Skip(kolumnaPoczatek).
                //        Take(cntCols).
                //        Select(td => td.FindChildren<WebElement>().Count > 0 ?
                //                            (td.FindDescendants<WebElement>().Where(w => w.InnerText != null).Count() > 0 ?
                //                                string.Join("", td.FindDescendants<WebElement>().
                //                                                    Where(w => w.InnerText != null).
                //                                                    Select(o => o.InnerText.Trim()).ToList()) : "") :
                //                            (td.InnerText != null ? td.InnerText.Trim() : "")).
                //        ToList()));


            }
            else
            {

                wartosci.AddRange(table.FindChildren<TrTag>().
                    Skip(wierszPoczatek).
                    Take(cntRows).
                    Select(tr => tr.FindChildren<TdTag>().
                        Select(td => td.InnerText != null ? td.InnerText.Trim() : GetInnerText(td)).
                        ToList()));

                //wartosci.AddRange(table.FindChildren<TrTag>().
                //    Skip(wierszPoczatek).
                //    Take(cntRows).
                //    Select(tr => tr.FindChildren<TdTag>().
                //        Select(td => td.FindChildren<WebElement>().Count > 0 ?
                //                            (td.FindDescendants<WebElement>().Where(w => w.InnerText != null).Count() > 0 ? // sprawdz czy są obiekty z ustawionymu atrybutem InnerText
                //                                string.Join("", td.FindDescendants<WebElement>().                           // ta konstrukcja jest niebędna żeby zbudować napis z wielu elementów, które mają ustawiony atrybut InnerText
                //                                                    Where(w => w.InnerText != null).
                //                                                    Select(o => o.InnerText.Trim()).ToList()) : "") :
                //                            (td.InnerText != null ? td.InnerText.Trim() : "")).                             // jeśli brak obiektów a nadtzędny ma ustawiony atrybut InnerText
                //        ToList()));

            }
            */
            #endregion

            return wartosci;
        }


        private string GetInnerText(WebElement parent)
        {
            List<string> value = new List<string>();
            foreach (var element in parent.FindChildren<WebElement>())
            {
                if (element.InnerText != null)
                    value.Add(element.InnerText.Trim());
                else
                    value.Add(GetInnerText(element));
            }
            return String.Join("", value.ToArray());
        }

        protected string[][] PobierzTabelke(string tableId, bool ignoreHeader = false, string myPath = "")
        {
            var wartosci = PobierzTabelkeJakoListe(tableId, ignoreHeader);
            string[][] array = new string[wartosci.Count][];
            for (int index = 0; index < wartosci.Count; index++)
            {
                var row = wartosci[index];
                array[index] = row.ToArray();
            }
            return array;
        }

        public bool SprawdzCzyZawieraInformacje(string nazwa)
        {
            DebugLog("Sprawdź czy w Tabeli istnieje wpis " + nazwa);
            if (TabelkaInternal.Count == 1)
            {
                foreach (var columns in TabelkaInternal[0])
                {
                    if (columns.Contains(nazwa))
                        return true;
                }
            }
            else
            {
                foreach (var columns in TabelkaInternal)
                {
                    if (columns.Contains(nazwa))
                        return true;
                }
            }
            return false;
        }


        public bool Equals(HtmlTable other)
        {
            var otherTable = other.TabelkaInternal;
            if (otherTable.Count != TabelkaInternal.Count)
            {
                DebugLog(string.Format("Różna ilość wierszy jest {0} ma być {1}",otherTable.Count,TabelkaInternal.Count));
                return false;
            }
            for (int i = 0; i < TabelkaInternal.Count; i++)
            {
                if (otherTable[i].Count != TabelkaInternal[i].Count)
                {
                    DebugLog(string.Format("Rozna ilość kolumn w wierszu {2} jest {0} ma być {1}", otherTable[i].Count, TabelkaInternal[i].Count,i));
                    return false;
                }
                for (int j = 0; j < TabelkaInternal[i].Count; j++)
                {
                    if (TabelkaInternal[i][j] != otherTable[i][j])
                    {
                        DebugLog(string.Format("Różna wartość w elemencie [{0}][{1}] jest : {2} a ma być {3}", i, j, otherTable[i][j], TabelkaInternal[i][j]));
                        return false;
                    }
                }
            }
            return true;
        }

        public string PobierzWartosc(int wiersz,int kolumna)
        {
            return TabelkaInternal[wiersz][kolumna];
        }

        //UWAGA MÓWI PAU!
        //Można Pobrać wartości kolumny wywołując konstruktor klasy z odpowiednimi parametrami, który wywołuje PobierzTabelkeJakoListe, ale on zwraca nam zmienną TabelkaInternal, której mamy w Pageach nie używać. Ta funkcja zwraca mi rzeczywistą listę elementów
        //Można pokusić się o przebudowe tej klasy, ale to zajmie sporo czasu, a Przedszkola czekają
        public List<string> PobierWartosciZKolumny(int numerKolumny)
        {
            List<string> lista = new List<string>();
            foreach (List<string> list in TabelkaInternal)
            {
                lista.Add(list[numerKolumny]);
            }
            return lista;
        }
        /// <summary>
        /// Funkcja do wyboru ikony edycji - ołówka
        /// </summary>
        /// <param name="name"></param>
        /// 
        public void KliknijWIkoneEdycji(string name)
        {
            var numer = PobierzNumerWiersza(name);
            Find<ImgTag>(string.Format("//tr[{0}]//imgtag[@Class~'icon-edit']", numer+1)).Click(); 
        }

        /// <summary>
        /// Funkcja do wyboru ikony przejścia do jakiegoś widoku, taka z żółtą strzałką
        /// </summary>
        /// <param name="name"></param>
        /// 
        public void WybierzIkonePrzejsciaDoWidoku(string name, string myPath="") //01.12_Pau - Erato
        {
            var row = PobierzNumerWiersza(name);
            var ikona = Find<ImgTag>(string.Format("{1}//tr[{0}]//img[@class~'icon-step_into']", row + 1,myPath));
            ikona.EnsureVisible();
            ikona.Click();
        }

        public List<string> PobierzWierszZTabeli(string nazwa, int numerKolumny = 0, Boolean identyczny = true) // 27.11 K.O
        {
            if(identyczny)
                return TabelkaInternal.Find(s => s[numerKolumny].Equals(nazwa));
            else
                return TabelkaInternal.Find(s => s[numerKolumny].Contains(nazwa));
        }

        public List<string> PobierzWiersz(int numerWiersza) 
        {
            return TabelkaInternal[numerWiersza];
        } 
 
        /// <summary>
        /// F-cja sprawdza czy wiersz opisany przez parametr dane
        /// istnieje w tabeli. Wiersz który sprawdzamy może zawierać powtórzenia.
        /// </summary>
        /// <param name="dane">poszukiwany wiersz</param>
        /// <returns></returns>
        public bool SprawdzCzyTabelaZawieraWiersz(List<string> dane)
        {
            if (UstalNumerWierszaTabeliZawierajacejWiersz(dane) >= 0)
                return true;
            else
                return false;
        }

        /// <summary>
        /// F-cja sprawdza czy wiersz opisany przez parametr dane
        /// istnieje w tabeli. Wiersz który sprawdzamy może zawierać powtórzenia.
        /// Jeśli wiersz zostanie znaleziony, zwraca jego numer,
        /// jeśli nie zwraca wartość -1
        /// </summary>
        /// <param name="dane">poszukiwany wiersz</param>
        /// <returns></returns>
        public int UstalNumerWierszaTabeliZawierajacejWiersz(List<string> dane)
        {
            // K.O tak jest szybciej
            #region INFO
            //Exists in list a but not in b
            //var c = (from i
            //            in a
            //         let found = b.Any(j => j == i)
            //         where !found
            //         select i)
            //         .ToList();

            //Exists in both lists
            //var d = (from i
            //            in a
            //         let found = b.Any(j => j == i)
            //         where found
            //         select i)
            //         .ToList();

            //var foo4 = !a.Except(d).Any();
            #endregion

            if (dane.Count != 0)
            {
                for (int nr = 0; nr < TabelkaInternal.Count; nr++)
                {
                    var wiersz = TabelkaInternal[nr];
                    //Exists in list wiersz but not in dane
                    var roznice = (from i in dane
                                   let found = wiersz.Any(j => j == i)
                                   where !found
                                   select i)
                                    .ToList();
                    if (roznice.Count == 0)
                        return nr;
                }
            }
            return -1;
        }


        public int PobierzNumerWiersza(string parametr)
        {
            return TabelkaInternal.FindIndex(o => o.Contains(parametr));
        }

        public int PobierzNumerWiersza(string parametr,int numerKolumny) //20.11_Pau - Erato
        {
            return TabelkaInternal.FindIndex(o => o[numerKolumny].Contains(parametr));
        }
       
        //podałam parametr numer wiersza, pomnieważ czasami się zdarza tak, że wiersze mają różna ilość kolumn
        public int PobierzNumerKolumny(string parametr, int numerWiersza=0) 
        {
            return TabelkaInternal[numerWiersza].FindIndex(x => x.Contains(parametr));
        }

        public int PoliczIloscKolumnWWierszu(int numerWiersza)
        {
            return TabelkaInternal[numerWiersza].Count;
        }

        public int PoliczLiczbeWierszyWTabeli()
        {
            return TabelkaInternal.Count;
        }

        public string GetToAssertHelper()
        {

            string begin = @"string[][] values = new[]
                           {";
            List<string> rowsString = new List<string>();
            foreach (var row in TabelkaInternal)
            {
                List<string> column = row.Select(vfield => vfield.Replace("\"", "\\\"")).ToList();
                string wynik = "new [] {" + String.Join(",", column.Select(s => "\"" + s + "\"")) + "}";
                rowsString.Add(wynik);
            }

            begin += String.Join(",", rowsString) + " };";
            return begin;
        }

        public void ZaznaczOdznaczCheckboxWTabeli(string[] dane)
        {
            for (int i = 0; i < dane.Count(); i++)
            {
                var row = PobierzNumerWiersza(dane[i]);
                var checkbox = Find<DivTag>(string.Format("/tr[{0}]//div[@class~'-row-checker']", row + 1));
                checkbox.Click();
            }
        }

        //Uwaga! Funkcja stanowi podstawę działania testów również dla innych aplikacj, np. Erato!
        public static HtmlTable PobierzTabelePodNaglowkiem(string gridId, WebDocument OpenWebsite, string hdName, int? browserId, string tableId , int iloscKolumn = 0, Boolean enforceVisible=false)
        {
            HtmlTable table = null;
            var divHeader = OpenWebsite.Find<DivTag>(string.Format(".//div[id='{0}']//div[id~'header$']", gridId));
            for (int i = 0; i < divHeader.Count(); i++)
            {
                var headerName = divHeader[i].FindDescendant<SpanTag>().InnerText;
                if (headerName.Contains(hdName))
                {
                    divHeader[i].EnsureVisible();
                    var id = divHeader[i].Parent.Id;
                    var myPath = "/div[@id='" + id + "']/";

                    // k.o - sekcje mogą zawierać kilka teabelek, które mogą być niewidoczne mimo widocznej nazwy sekcji
                    // dlatego trzeba zapewnić jakiś mechanizm do uwidocznenia takiej tabelki, dlatego że f-cja klasy HtmlTable
                    // pobierająca tabelki pobiera tylko tabele widoczne
                    if (enforceVisible)
                    {
                        var pth = string.Format("./{0}/div[@id~'{1}']", myPath, tableId);
                        var divTag = OpenWebsite.Find<DivTag>(pth);
                        if (divTag.Count > 0)
                        {
                            var div = divTag.First();
                            div.EnsureVisible();
                        }
                    }

                    table = new HtmlTable(OpenWebsite, tableId, browserId, true, myPath, 0, 0, 0, iloscKolumn);
                    break;
                }
            }
            return table;
        }
    }
}
