using System;
using System.Collections.Generic;
using System.Linq;
using Ranorex;

namespace VCommonPages.Pages.Common
{
    /// <summary>
    /// Klasa do pobierania informacji z sekcji danych
    /// </summary>
    public class VModelSection : IEquatable<VModelSection>
    {
        public struct VModelFileds
        {
            public string Label;
            public string Value;
        }

        public readonly List<VModelFileds> _fields = new List<VModelFileds>();
        private SpanTag zmienButton;

        public VModelSection(WebElement vmodelsection, bool parseLinks = false)
        {
            var rows = vmodelsection.FindSingle<TBodyTag>("./table/tbody/");
            var row = rows.FindChildren<TrTag>();
            int licznik = row.Count;
            if (row.Last().FindChildren<TdTag>().Last().FindDescendants<SpanTag>().Count > 0)
            {
                licznik = licznik - 1;
                zmienButton = rows.FindSingle<SpanTag>(".//span[@innertext='Zmień']");
            }

            for (int i = 0; i < licznik; i++)
            {
                var columns = row[i].FindChildren<TdTag>();
                var labelcount = row[i].FindDescendants<LabelTag>().Count;
                if (labelcount != 0)
                {
                    if (columns.Count != 2) throw new Exception("To nie jest VMODELSECTION");
                    var label = columns[0].FindChild<LabelTag>();
                    var value = columns[1].FindChild<LabelTag>();
                    if (label != null && value != null)
                    {
                        var valueFromLabel = value.InnerText.Trim();
                        if (parseLinks)
                        {
                            var aTags = value.FindDescendants<ATag>();
                            if (aTags.Count > 0) valueFromLabel = aTags[0].InnerText;
                        }
                        var vfields = new VModelFileds { Label = label.InnerText.Trim(), Value = valueFromLabel };
                        _fields.Add(vfields);
                    }
                }
            }
        }

        public VModelSection(string[][] dane)
        {
            List<VModelFileds> _tablela = new List<VModelFileds>();
            foreach (var row in dane)
            {
                if (row.Length != 2) throw new Exception("Niepoprawne dane");
                VModelFileds vfield = new VModelFileds { Label = row[0], Value = row[1] };
                _tablela.Add(vfield);
            }
            _fields = _tablela;
        }

        public void KliknijPrzyciskZmien()
        {
            zmienButton.Click();
        }

        private static List<DivTag> PobierzWszystkieVModeleZDiva(WebDocument openWebsite, string id, bool onlyVisible, string sectionName = "vmodelsection")
        {
            string visiblePath = "[@Visible='true']";
            if (!onlyVisible) visiblePath = "";
            return openWebsite.Find<DivTag>(string.Format(".//div[#'{0}']//div[Id~'{1}' or @id~'vmodelSection']{2}", id, sectionName, visiblePath)).ToList();
        }


        public override string ToString() // nunit uzywa tostring o pobierania wartosci expectedu but was
        {
            string toReturn = "";
            foreach (var row in _fields)
            {
                toReturn += "{";

                toReturn += row.Label + "|" + row.Value;

                toReturn += "} \n";
            }
            return toReturn;
        }

        /// <summary>
        /// Funkcja zwaraca listę sekcji danych
        /// </summary>
        /// <param name="openWebsite"></param>
        /// <param name="id"></param>
        /// <param name="zmien"></param>
        /// <param name="mySectionName">inna niż domyślna nazwa sekcji</param>
        /// <returns></returns>
        public static List<VModelSection> PobierzWszystkieSekcje(WebDocument openWebsite, string id, bool zmien = false, string mySectionName = "", bool onlyVisible = true, bool parseLinks = false)
        {
            List<DivTag> vmodele = string.IsNullOrEmpty(mySectionName) ? PobierzWszystkieVModeleZDiva(openWebsite, id, onlyVisible) : PobierzWszystkieVModeleZDiva(openWebsite, id, onlyVisible, mySectionName);

            return vmodele.Select(divTag => new VModelSection(divTag, parseLinks)).ToList();
        }

        /// <summary>
        /// Funkcja zwraca obiekt typy VModelSection która zawiera podany label i wartość
        /// </summary>
        /// <param name="openWebsite"></param>
        /// <param name="id"></param>
        /// <param name="label">napis</param>
        /// <param name="value">wartość pola napis</param>
        /// <param name="mySectionName">nazwa sekcji - domyślnie vmodelsection</param>
        /// <returns></returns>
        public static VModelSection PobierzSekcjeOLabelku(WebDocument openWebsite, string id, string label, string value = "", string mySectionName = "", bool onlyVisible = true, bool parseLinks = false)
        {
            var allSection = PobierzWszystkieSekcje(openWebsite, id, true, mySectionName, onlyVisible, parseLinks);
            for (int i = 0; i < allSection.Count; i++)
            {
                var wartosc = allSection[i].PobierzWartosc(label);
                if (wartosc != null && (wartosc == value || value == ""))
                {
                    return allSection[i];
                }
            }
            throw new Exception("Nie ma sekcji o podanych parametrach : " + label + ' ' + value);
        }

        public bool SprawdzLabel(string labelName)
        {
            bool flag = false;
            foreach (var model in _fields)
            {
                if (model.Label == labelName)
                    flag = true;
            }
            return flag;
        }

        public string PobierzWartosc(string labelName)
        {
            foreach (var model in _fields)
            {
                if (model.Label == labelName)
                    return model.Value;
            }
            return null;
        }

        public static bool CzyIstniejaTabele(WebDocument openWebsite, string nazwaTabeli, string mySectionName = "")
        {
            bool czyTabelaIstnieje;
            var tabelka = PobierzWszystkieSekcje(openWebsite, nazwaTabeli, false, mySectionName);

            czyTabelaIstnieje = tabelka.Count != 0;

            return czyTabelaIstnieje;
        }

        public string GetToAssertHelper()
        {

            string begin = @"string[][] values = new[]
                           {";
            List<string> rowsString = new List<string>();
            foreach (var row in _fields)
            {
                List<string> column = new List<string>();
                column.Add(row.Label);
                column.Add(row.Value);
                string wynik = "new [] {" + String.Join(",", column.Select(s => "\"" + s + "\"")) + "}";
                rowsString.Add(wynik);
            }

            begin += String.Join(",", rowsString) + " };";
            return begin;
        }

        public bool Equals(VModelSection other)
        {
            var otherTable = other._fields;
            if (otherTable.Count != _fields.Count)
            {
                return false;
            }
            for (int i = 0; i < _fields.Count; i++)
            {
                if (other._fields[i].Label != _fields[i].Label || other._fields[i].Value != _fields[i].Value)
                {
                    return false;
                }
            }
            return true;
        }
    }

}
