using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Ranorex;
using Ranorex.Core;

namespace VCommonPages.Pages.Common
{
    /// <summary>
    /// Klasa do obsługi edycji w oknach modalnych 
    /// </summary>
    public abstract class VEditorPage : VWindowPage
    {
        public enum FiledTypes
        {
            TEXTBOX,
            COMBOBOX,
            RADIOBOX,
            CHECKBOX,
            DATEPICKER,
            TEXTAREA,
            SPINBUTTON,
            COLORPICKER,
            TIMEPICKER
        }

        public enum ToolTipData
        {
            ERROR,
            INFO, 
        }

        public struct Field
        {
            public string Label;
            public FiledTypes Type;
            public string Value;
        }

        private readonly string _mainWidnowDiv = "/div[#'{0}']";
        private readonly DivTag _window;
        private readonly Field[] _fields;

        protected VEditorPage(WebDocument openWebsite, int? browserId, string idOkna, Field[] fields, bool bezNaglowka = false, bool visibleOnly=true)
            : base(openWebsite, browserId, idOkna,bezNaglowka,visibleOnly)
        {
            _fields = fields;
            _mainWidnowDiv = string.Format(_mainWidnowDiv, idOkna);
            WaitForLoad();
            _window = OknoDialogowe;
        }

        public InputTag GetComboById(string id)
        {
            return _window.FindSingle<InputTag>(string.Format(".//input[@id = '{0}']", id));
        }

        private WebElement GetTrForLabel(string name)
        {
            var label = _window.FindSingle<LabelTag>(string.Format(".//label[@InnerText = '{0}']", name));
            return label.Parent.Parent;
        }

        protected WebElement GetTdForValue(string nazwaPola)
        {
            var tr = GetTrForLabel(nazwaPola);
            var children = tr.FindChildren<TdTag>();
            var tdValueTag = children[1];
            return tdValueTag;
        }

        private Boolean CheckTrForLabel(string name)
        {
            Duration timeOut = new Duration(5000);
            Element lbSearched;
            var jest = _window.TryFindSingle(string.Format(".//label[@InnerText = '{0}']", name), timeOut, out lbSearched);
            return jest;
        }

        /// <summary>
        /// Metoda wpisuje do wskazanej kontrolki podana wartość
        /// </summary>
        /// <param name="nazwaPola">nazwa pola - label definiujący kontrolkę</param>
        /// <param name="wartosc">wartośc która zostanie wpisana/wybrana</param>
        /// <param name="klawiatura">jeśli =true w pole Innertext zostanie wprowadzona podana wartość bez względu na typ</param>
        /// <param name="kierunek">parametr własciwy tylko dla kontrolki typu SPINBUTTON, 
        ///                         kierunek="gora" oznacza kliknięcie w górną/dolna część kontrolki
        ///                         natomiast parametr "wartość" oznacza liczbe kliknięć</param>
        /// <param name="wybierz2razy">wymusza 2 razy wybranie z comba potrzebne w testach CS w strasznie dynamicznie ładowanych comboxach </param>
        public void WprowadzWartoscDoPola(string nazwaPola, string wartosc, bool klawiatura = false, string kierunek="gora",bool wybierz2razy = false)
        {
            var field = _fields.ToList().Find(s => s.Label == nazwaPola);
            var realType = field.Type;
            if (klawiatura) realType = FiledTypes.TEXTBOX;
            switch (realType)
            {
                case FiledTypes.COMBOBOX:
                    {
                        var tdValueTag = GetTdForValue(nazwaPola);
                        var combo = tdValueTag.FindDescendants<InputTag>();
                        if (combo.Count != 1) throw new Exception("To nie jest comboBox");
                        SelectFromCombo(combo[0], wartosc);
                        if (wybierz2razy) SelectFromCombo(combo[0], wartosc); // DDRZ to jest po jest prblem w CS
                        break;
                    }
                case FiledTypes.TEXTBOX:
                    {
                        var tdValueTag = GetTdForValue(nazwaPola);
                        var texBox = tdValueTag.FindDescendants<InputTag>();
                        if (texBox.Count != 1) throw new Exception("To nie jest textBox");
                        texBox[0].Click();
                        texBox[0].InnerText = wartosc;
                        break;
                    }
                case FiledTypes.TEXTAREA:
                    {
                        var tdValueTag = GetTdForValue(nazwaPola);
                        var texBox = tdValueTag.FindDescendants<TextAreaTag>();
                        if (texBox.Count != 1) throw new Exception("To nie jest textArea");
                        texBox[0].Click();
                        texBox[0].InnerText = wartosc;
                        break;
                    }
                case FiledTypes.SPINBUTTON: // K.O
                    {
                        // wartość oznacza liczbę kliknięć kontrolki
                        // kierunek oznacza kliknięcie górnej/golnej połówki
                        WybierzStrzalkaZeSpinButtona(nazwaPola, int.Parse(wartosc), kierunek); 
                        break;
                    }

                case FiledTypes.RADIOBOX:
                    {
                        WebElement label;
                        if (nazwaPola != "")
                        {
                            var tdValueTag = GetTdForValue(nazwaPola);
                            label = tdValueTag.FindSingle<LabelTag>(string.Format(".//label[@InnerText = '{0}']", wartosc));
                        }
                        else
                        {
                            label = _window.FindSingle<LabelTag>(string.Format(".//label[@InnerText = '{0}']", wartosc));
                        }

                        label = label.Parent;
                        var radioBox = label.FindDescendants<InputTag>();
                        if (radioBox.Count != 1) throw new Exception("To nie jest radiobox");
                        radioBox[0].Click();
                        break;
                    }
                case FiledTypes.COLORPICKER:
                    {
                        // kolory: niebieski=0, czerwony=1, zielony=2, czarny=3, fioletowy=4
                        Find<InputTag>(".//input[id~'colorpicker']").Click();
                        var x = OpenWebsite.Find<ATag>(".//div[id~'colorpicker']//a").ToList();
                        x[Int32.Parse(wartosc)].Click();
                        break;
                    }

                case FiledTypes.CHECKBOX:
                    {
                        var label = _window.FindSingle<LabelTag>(string.Format(".//label[@InnerText = '{0}']", nazwaPola));
                        var ziemniak = label.Parent;
                        var checkbox = ziemniak.FindDescendants<InputTag>();
                        if (checkbox.Count != 1) throw new Exception("To nie jest checkBox");
                        checkbox[0].Click();
                        break;
                    }
                case FiledTypes.DATEPICKER: // K.O
                    {
                        var tdValueTag = GetTdForValue(nazwaPola);
                        var combo = tdValueTag.FindDescendants<InputTag>();
                        if (combo.Count != 1) throw new Exception("To nie jest comboBox");
                        WybierzZDatePicera(combo[0].Id, wartosc);
                        break;
                    }
                case FiledTypes.TIMEPICKER:
                    {
                        var tdValueTag = GetTdForValue(nazwaPola);
                        var combo = tdValueTag.FindDescendants<InputTag>();
                        //SelectFromTimePicker(combo[0],wartosc);
                        WybierzZVtimepicker(combo[0], wartosc); // k.o - nowa kontrolka do wyboru czasu
                        break;
                    }

            }
        }

        #region Niepotrzebny nadmiar - czy ktoś poza Efebem używa tej f-cji? 
        public void WprowadzWartoscDoPola(Field fieldToEdit, string wartosc, int index = 0)
        {
            switch (fieldToEdit.Type)
            {
                case FiledTypes.COMBOBOX:
                    {
                        var tdValueTag = GetTdForValue(fieldToEdit.Label);
                        var combo = tdValueTag.FindDescendants<InputTag>();
                        if (combo.Count != 1) throw new Exception("To nie jest comboBox");
                        SelectFromCombo(combo[index], wartosc);
                        break;
                    }
                case FiledTypes.TEXTBOX:
                    {
                        var tdValueTag = GetTdForValue(fieldToEdit.Label);
                        var texBox = tdValueTag.FindDescendants<InputTag>();
                        if (texBox.Count != 1) throw new Exception("To nie jest textBox");
                        texBox[index].Click();
                        texBox[index].InnerText = wartosc;
                        break;
                    }
                case FiledTypes.TEXTAREA:
                    {
                        var tdValueTag = GetTdForValue(fieldToEdit.Label);
                        var texBox = tdValueTag.FindDescendants<TextAreaTag>();
                        if (texBox.Count != 1) throw new Exception("To nie jest textArea");
                        texBox[index].Click();
                        texBox[index].InnerText = wartosc;
                        break;
                    }
                case FiledTypes.SPINBUTTON:
                    {
                        break;
                    }
            }
        }
        #endregion

        public string PobierzWartoscPola(string nazwaPola)
        {
            var field = _fields.ToList().Find(s => s.Label == nazwaPola);
            string value = "";

            switch (field.Type)
            {
                case FiledTypes.COMBOBOX:
                    {
                        var tdValueTag = GetTdForValue(nazwaPola);
                        var combo = tdValueTag.FindDescendants<InputTag>();
                        if (combo.Count != 1) throw new Exception("To nie jest comboBox");
                        value = combo[0].Value;
                        break;
                    }
                case FiledTypes.TEXTBOX:
                    {
                        var tdValueTag = GetTdForValue(nazwaPola);
                        var texBox = tdValueTag.FindDescendants<InputTag>();
                        if (texBox.Count != 1) throw new Exception("To nie jest textBox");
                        value = texBox[0].Value;
                        break;
                    }
                case FiledTypes.TEXTAREA:
                    {
                        var tdValueTag = GetTdForValue(nazwaPola);
                        var texBox = tdValueTag.FindDescendants<TextAreaTag>();
                        if (texBox.Count != 1) throw new Exception("To nie jest textBox");
                        value = texBox[0].InnerText;
                        break;
                    }
                case FiledTypes.DATEPICKER: // K.O
                    {
                        var tdValueTag = GetTdForValue(nazwaPola);
                        var combo = tdValueTag.FindDescendants<InputTag>();
                        if (combo.Count != 1) throw new Exception("To nie jest comboBox");
                        value = combo[0].Value;
                        break;
                    }
                case FiledTypes.COLORPICKER: // K.O
                    {
                        // kolory: niebieski=0, czerwony=1, zielony=2, czarny=3, fioletowy=4
                        Find<InputTag>(".//input[id~'colorpicker']").Click();
                        var idx = OpenWebsite.FindSingle<ATag>(".//div[id~'colorpicker' and @visible='true']//a[@class~'x-color-picker-selected']").Element.ChildIndex;
                        value = idx.ToString();
                        break;
                    }
               

            }

            return value;
        }

        /// <summary>
        /// Procedura sprawdza czy pole poprzedzone etykietą o nazwie nazwaPola jest wyszarzone
        /// </summary>
        /// <param name="nazwaPola">nazwa labela poprzedzajaca pole</param>
        /// <returns>true/false</returns>
        public Boolean CzyPoleNieaktywne(string nazwaPola)
        {
            var field = _fields.ToList().Find(s => s.Label == nazwaPola);
            string value = "";

            switch (field.Type)
            {
                case FiledTypes.COMBOBOX:
                    {
                        var tdValueTag = GetTdForValue(nazwaPola);
                        var combo = tdValueTag.FindDescendants<InputTag>();
                        if (combo.Count != 1) throw new Exception("To nie jest comboBox");
                        value = combo[0].Disabled;
                        break;
                    }
                case FiledTypes.TEXTBOX:
                    {
                        var tdValueTag = GetTdForValue(nazwaPola);
                        var texBox = tdValueTag.FindDescendants<InputTag>();
                        if (texBox.Count != 1) throw new Exception("To nie jest textBox");
                        value = texBox[0].Disabled;
                        break;
                    }
                case FiledTypes.TEXTAREA:
                    {
                        var tdValueTag = GetTdForValue(nazwaPola);
                        var texBox = tdValueTag.FindDescendants<TextAreaTag>();
                        if (texBox.Count != 1) throw new Exception("To nie jest textBox");
                        value = texBox[0].Enabled ? "True" : "False"; // ??
                        break;
                    }
                case FiledTypes.DATEPICKER: // K.O
                    {
                        var tdValueTag = GetTdForValue(nazwaPola);
                        var combo = tdValueTag.FindDescendants<InputTag>();
                        if (combo.Count != 1) throw new Exception("To nie jest comboBox");
                        value = combo[0].Disabled;
                        break;
                    }
                case FiledTypes.COLORPICKER: // K.O
                    {
                        // kolory: niebieski=0, czerwony=1, zielony=2, czarny=3, fioletowy=4
                        var colPic = Find<InputTag>(".//input[id~'colorpicker']");
                        value = colPic.Disabled;
                        break;
                    }

            }

            return value.ToUpper() == true.ToString().ToUpper();
        }


        private void WybierzStrzalkaZeSpinButtona(string nazwaPola, int iloscRazy, string kierunek) //kierunek to ''gora'' albo ''dol'' -dg
        {
            if (iloscRazy > 20) throw new Exception("Za duzo klikania spinbuttonem, maks to 20");

            // 6.11.2014 K.O
            // Zmieniono sposób obsługi kontrolki, ponieważ jej wartość zależy od
            // czasu trwania zdarzenia Click. Niestety nie da się ustalić wartości tego parametru.

            var keyCode = (kierunek == "gora" ? Keys.Up : Keys.Down);
            var tdValueTag = GetTdForValue(nazwaPola);
            var spin = tdValueTag.FindDescendants<InputTag>();
            if (spin.Count != 1) throw new Exception("To nie jest spinbutton");

            tdValueTag.Click();
            Keyboard.Press(keyCode, new Duration(10), iloscRazy);

        }

        public List<String> PobierzTooltipaWarningOTekscie(String trescToolTipa)
        {
            DebugLog("Sprawdzenie czy istnieje tooltip o tekście "+ trescToolTipa);
            IList<LiTag> tooltipy = OpenWebsite.Find<LiTag>(String.Format(".//div[@id~'tooltip-*']//td//li[@innertext='{0}']", trescToolTipa));
            String tekstTooltipa=(tooltipy.Count==0) ? "" : tooltipy[0].InnerText;
            return GetTooltipFromHtml(tekstTooltipa);
        }

        
        public List<string> PobierzToolTipDlaPola(string nazwaPola, ToolTipData rodzaj = ToolTipData.INFO)
        {
            
            var field = _fields.ToList().Find(s => s.Label == nazwaPola);
            string value = "";
            string sATTR = (rodzaj == ToolTipData.ERROR ? "data-errorqtip" : "data-qtip");
            switch (field.Type)
            {
                case FiledTypes.COMBOBOX:
                    {
                        var tdValueTag = GetTdForValue(nazwaPola);
                        var combo = tdValueTag.FindDescendants<InputTag>();
                        if (combo.Count != 1) throw new Exception("To nie jest comboBox");
                        value = combo[0].Element[sATTR].ToString();
                        break;
                    }
                case FiledTypes.TEXTBOX:
                    {
                        var tdValueTag = GetTdForValue(nazwaPola);
                        var texBox = tdValueTag.FindDescendants<InputTag>();
                        if (texBox.Count != 1) throw new Exception("To nie jest textBox");
                        value = texBox[0].Element[sATTR].ToString();
                        break;
                    }
                case FiledTypes.DATEPICKER:
                    {
                        var tdValueTag = GetTdForValue(nazwaPola);
                        var datePick = tdValueTag.FindDescendants<InputTag>();
                        if (datePick.Count != 1) throw new Exception("To nie jest datePicker");
                        value = datePick[0].Element[sATTR].ToString();
                        break;
                    }
            }

            return GetTooltipFromHtml(value);
        }

        public Boolean CzyWskazanePoleIstnieje(string nazwaPola)
        {
            DebugLog(string.Format("Sprawdź czy istnieje pole: {0}", nazwaPola));
            return CheckTrForLabel(nazwaPola);
        }

        /// <summary>
        /// Do użycia tylko, gdy w scenariuszu jest napisane, że należy skasować wartość w polu
        /// </summary>
        /// <param name="pola">nazwy pól (bez dwukropków)</param>
        public void SkasujWartoscZeWskazanychPol(params String[] pola)
        {
            Adapter text;
            
            for(int i = 0; i < pola.Count(); i++)
            {
                var field = _fields.ToList().Find(s => s.Label == pola[i]);
                if (field.Type == FiledTypes.COLORPICKER || field.Type == FiledTypes.RADIOBOX || field.Type == FiledTypes.CHECKBOX || field.Type == FiledTypes.SPINBUTTON)
                    throw new Exception("Nie można skasować warotści z tego typu pola");
                
                DebugLog("Kasowanie wartości w polu " + pola[i]);

                if (field.Type == FiledTypes.TEXTAREA)
                {
                    text = GetTdForValue(pola[i]).FindDescendant<TextAreaTag>();
                    TextAreaTag textArea = (TextAreaTag)text;
                    while (textArea.InnerText != null)
                    {
                        text.Click();
                        text.PressKeys("{shift down}{home}{up}{shift up}{delete}");
                    }
                }
                else
                {
                    text = GetTdForValue(pola[i]).FindDescendant<InputTag>();
                    text.Click();
                    text.PressKeys("{shift down}{home}{shift up}{delete}");
                }
            }
        }
    }

}

