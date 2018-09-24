using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ranorex;
using VRanorexLib;
using VCommonPages.Pages;

namespace VCommonPages.Pages.SilverlightCommon
{

    public class SilverlightLabelTextboxSection : SilverlightPage,IEquatable<SilverlightLabelTextboxSection>
    {
        public struct LabelTextboxFields
        {
            public string Label;
            public string Value;
        }

        public readonly List<LabelTextboxFields> _fields = new List<LabelTextboxFields>();
        private readonly IList<Button> _buttons;

        public SilverlightLabelTextboxSection(TabPage zakladka, int CIStart, int CIStop, bool ignoreLabels = false, int CIDiff=1) //WERSJA SMUTNA I PELNA CIERPIENIA - do starych tabpage'y
        {
            if (ignoreLabels) //tylko i wylacznie do klikania, bez labelek
            {
                _buttons = GetButtonByNameAndSection(CIStart, CIStop, zakladka);
            }


            if (!ignoreLabels) //labelki i klikanie
            {
                _buttons = GetButtonByNameAndSection(CIStart, CIStop, zakladka);
                MagicalPairingOfTextboxesAndLabels(zakladka, CIStart, CIStop, CIDiff);
            }

        }

        public SilverlightLabelTextboxSection(TabPage zakladka, string sekcja,bool ignoreLabels=false, int CIDiff=1)//PODSTAWOWA WERSJA - do 90% tabpage'ow
        {

            List<int> IndexList = GetSectionIndexes(zakladka, sekcja);
            int CIStart = IndexList[0];
            int CIEnd = IndexList[1];
            
            if (ignoreLabels) //tylko i wylacznie do klikania, bez labelek
            {
                _buttons = GetButtonByNameAndSection(CIStart,CIEnd,zakladka);
            }

            
            if (!ignoreLabels) //labelki i klikanie
            {
                _buttons = GetButtonByNameAndSection(CIStart, CIEnd, zakladka);
                MagicalPairingOfTextboxesAndLabels(zakladka,CIStart,CIEnd, CIDiff);
            }

            #region POMOCY WYSLIJCIE POSILKI
            //var wszystkieSekcje =
            //    zakladka.Find<Button>(string.Format("./button[@automationid='ExpanderButton']"));

            //List<int> indeksyWszystkichSekcji = new List<int>();
            //foreach (Button sekcjabutt in wszystkieSekcje) //ukrylem tutaj subtelny zart slowny, jest tak subtelny ze zwracam na niego uwage
            //{
            //    indeksyWszystkichSekcji.Add(sekcjabutt.Element.ChildIndex);
            //}
            //if(ignoreLabels)
            //{
            //    _button = GetButtonByNameAndSection(nameOfButton, sekcja, zakladka);
            //}

            //bool czySaDwieSekcje = indeksyWszystkichSekcji.Count > 1;
            //bool czySekcjaWKtorejSzukamyJestOstatnia=false;

            //Button sekcjaWKtorejSzukamy = wszystkieSekcje.First(x => x.Text == sekcja);
            //var indeksWLiscieSekcjiWKtorejSzukamy = wszystkieSekcje.IndexOf(sekcjaWKtorejSzukamy);
            //var indeksSekcjiWKtorejSzukamy = sekcjaWKtorejSzukamy.Element.ChildIndex;
            //int indeksKolejnejSekcji=0;

            //if (czySaDwieSekcje)
            //{
            //    if (indeksWLiscieSekcjiWKtorejSzukamy == (wszystkieSekcje.Count-1))
            //        czySekcjaWKtorejSzukamyJestOstatnia = true;
            //    if(indeksWLiscieSekcjiWKtorejSzukamy<(wszystkieSekcje.Count-1))
            //        indeksKolejnejSekcji = wszystkieSekcje[indeksWLiscieSekcjiWKtorejSzukamy+1].Element.ChildIndex;
            //}

            //if (!ignoreLabels)
            //{
            //    var labelki = GetTextboxLabels(zakladka);
            //    var tekstboksy = GetEditableTextboxes(zakladka);

            //    for (int i = 0; i < labelki.Count; i++)
            //    {
            //        if (czySaDwieSekcje && czySekcjaWKtorejSzukamyJestOstatnia)
            //            if (labelki[i].Element.ChildIndex > indeksSekcjiWKtorejSzukamy)
            //            {
            //                {
            //                    var fields = new LabelTextboxFields
            //                                     {Label = labelki[i].TextValue, Value = tekstboksy[i].TextValue};
            //                    _fields.Add(fields);
            //                }
            //            }

            //        if (czySaDwieSekcje)
            //        {
            //            if (labelki[i].Element.ChildIndex > indeksSekcjiWKtorejSzukamy &&
            //                labelki[i].Element.ChildIndex < indeksKolejnejSekcji)
            //            {
            //                var fields = new LabelTextboxFields
            //                                 {Label = labelki[i].TextValue, Value = tekstboksy[i].TextValue};
            //                _fields.Add(fields);
            //            }
            //        }
            //        if (!czySaDwieSekcje)
            //        {
            //            var fields = new LabelTextboxFields
            //                             {Label = labelki[i].TextValue, Value = tekstboksy[i].TextValue};
            //            _fields.Add(fields);
            //        }
            //    }
            //}
            #endregion
        }

        public SilverlightLabelTextboxSection(string[][] dane)
        {
            List<LabelTextboxFields> _tabela = new List<LabelTextboxFields>();
            foreach (var row in dane)
            {
                if (row.Length != 2) throw new Exception("Niepoprawne dane");
                LabelTextboxFields vfield = new LabelTextboxFields { Label = row[0], Value = row[1] };
                _tabela.Add(vfield);
            }
            _fields = _tabela;
        }

        public bool SprawdzCzySekcjaZawiera(string[][] tablica)
        {
            
                for (int i = 0; i < tablica.GetLength(0); i++)
                {
                    if (_fields[i].Label == tablica[i][0])
                    {
                        string first = (_fields[i].Value ?? "").Replace((char)160, ' ');
                        string second = (tablica[i][1] ?? "").Replace((char)160, ' ');

                        if (first != second)
                        {
                            throw new Exception(
                                string.Format(
                                    "Niezgodne wartosci. Labele: '{0}', było '{1}'. Wartosci: '{2}', było '{3}'",
                                    tablica[i][0], _fields[i].Label, tablica[i][1], _fields[i].Value));
                        }
                        }
                }

            return true;
        }

        public enum LewaCzyPrawa
        {
            Lewa, Prawa
        }

        public bool SprawdzCzySekcjaZawiera(string[][] tablica, LewaCzyPrawa ktoraSekcja)
        {
            int numerLabela = ktoraSekcja == LewaCzyPrawa.Lewa ? 1 : 2;
            for (int i = 0; i < tablica.GetLength(0); i++)
            {
                string first = (PobierzWartosc(tablica[i][0],numerLabela) ?? "").Replace((char)160, ' ');
                string second = (tablica[i][1] ?? "").Replace((char)160, ' ');
                if (first != second)
                    return false;
            }
            return true;
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

        public string PobierzWartosc(string labelName,int ktoraWartosc=1)
        {
            return _fields.Where(x => x.Label == labelName).ElementAt(ktoraWartosc-1).Value;
        }

        public void KliknijWButton(string buttonName)
        {
            //getButtonByName(buttonName).Press();
            ClickOrPress(getButtonByName(buttonName));
        }

        public bool SprawdzAktywnoscButtona(string buttonName)
        {
            return !getButtonByName(buttonName).Enabled; //!GetButtonByName(buttonName).Enabled;
        }

        private Button getButtonByName(string buttonName)
        {
            return _buttons.Single(s => s.Text == buttonName);
        }

        #region techniczne do buttonow
        private IList<Button> GetButtonByNameAndSection(int CIStart, int CIEnd, TabPage tabpage) //funkcja do szukania buttonow w sekcji, przy czym sekcja zawiera sie miedzy jakimis childindexami
        {
            var AllButtons = tabpage.Find<Button>(string.Format("./button[@visible='true']"));

            return GetButtonBetweenIndexes(AllButtons, CIStart, CIEnd);

        }

        private IList<Button> GetButtonBetweenIndexes(IList<Button> buttonList, int CIStart, int CIEnd)
        {
            IList<Button> filteredButtonList = new List<Button>();
            foreach (Button singleButton in buttonList)
            {

                if (singleButton.Element.ChildIndex >= CIStart && singleButton.Element.ChildIndex <= CIEnd)
                    filteredButtonList.Add(singleButton);
            }

            return filteredButtonList;
        }


        private List<int> GetSectionIndexes(TabPage zakladka, string sekcja)
        {
            List<int> IndexList = new List<int>();
            var AllSections = zakladka.Find<Button>("./button[@automationid='ExpanderButton' and @visible='true']");

            foreach (Button singleSection in AllSections)
            {
                if (singleSection.Text == sekcja)
                {
                    int desiredSectionIndex = singleSection.Element.ChildIndex;
                    int desiredSectionListIndex = AllSections.IndexOf(singleSection);
                    IndexList.Add(desiredSectionIndex);

                    if(AllSections.Count()==1 || desiredSectionListIndex==AllSections.Count()-1) //jest tylko jedna sekcja lub kiedy szukana sekcja jest ostatnia
                    {
                        IndexList.Add(1000); //przepraszam, ale tak bedzie wygodniej i szybciej, niz ladowac wszystkie elementy na zakladce i szukac indeksu ostatniego
                        break;
                    }

                    if (AllSections.Count() > 1) //jest wiecej niz 1 sekcja
                    {
                        int nextSectionIndex = AllSections[desiredSectionListIndex + 1].Element.ChildIndex;
                        IndexList.Add(nextSectionIndex);
                    }
                }
            }

            return IndexList;


        }
        #endregion

        #region pobieranie labelek i ich parowanie - nie polecam ruszac
        private void MagicalPairingOfTextboxesAndLabels(TabPage zakladka, int CIStart, int CIStop, int CIDifference=1)
        {
            var labelki = GetTextboxLabels(zakladka,CIStart,CIStop);
            var tekstboksy = GetEditableTextboxes(zakladka, CIStart, CIStop);
            if (labelki.Count() != tekstboksy.Count())
            {
                int j = 0;
                for (int i = 0; i < labelki.Count(); i++)
                {
                    if (labelki[i].Element.ChildIndex + CIDifference == tekstboksy[j].Element.ChildIndex)
                    {
                        var fields = new LabelTextboxFields
                                         {Label = labelki[i].TextValue, Value = tekstboksy[j].TextValue};
                        _fields.Add(fields);
                        j++;
                    }
                    else
                    {
                        var fields = new LabelTextboxFields {Label = labelki[i].TextValue, Value = ""};
                        _fields.Add(fields);
                    }
                }
            }

            if(labelki.Count()==tekstboksy.Count())
            for (int i = 0; i < labelki.Count(); i++)
            {
                    var fields = new LabelTextboxFields { Label = labelki[i].TextValue, Value = tekstboksy[i].TextValue };
                    _fields.Add(fields);
            }
        }


        private List<Text> GetEditableTextboxes(TabPage zakladka, int CIStart, int CIStop) //zbiera wszystkie edytowalne tekstboksy z zakladki
        {
            var wszystkieTekstboksy = zakladka.Find<Text>("./text[@controltype='Edit' and @visible='true']");

            List<Text> tekstboksy = new List<Text>();
            foreach (Text tekstboks in wszystkieTekstboksy)
            {
                if (tekstboks.Element.ChildIndex > CIStart &&
                    tekstboks.Element.ChildIndex < CIStop)
                tekstboksy.Add(tekstboks);
            }

            return tekstboksy;
        }

        private List<Text> GetTextboxLabels(TabPage zakladka, int CIStart, int CIStop) //to nie sa labelki, tylko texty, ale labelki brzmi logiczniej/jasniej
        {
            var wszystkieLabelki = zakladka.Find<Text>("./text[@controltype='Text' and @visible='true']");
            wszystkieLabelki.RemoveAt(0); //usuwa pierwszy element, czyli tekstboks opisujacy nazwe zakladki

            List<Text> labelki = new List<Text>();
            foreach (Text labelka in wszystkieLabelki)
            {
                if (labelka.Element.ChildIndex > CIStart &&
                    labelka.Element.ChildIndex < CIStop)
                labelki.Add(labelka);
            }

            return labelki;
        }
        #endregion

        #region techniczne do nunita
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

        public bool Equals(SilverlightLabelTextboxSection other)
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
        #endregion
    }

}
