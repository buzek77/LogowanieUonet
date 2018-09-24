using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Ranorex;
using Ranorex.Core;
using VRanorexLib;
using System.Globalization;
using Button = Ranorex.Button;
using DateTime = System.DateTime;
using Form = Ranorex.Form;
using System.Drawing;

namespace VCommonPages.Pages.SilverlightCommon
{
    public class SilverlightPage : Page
    {

        public SilverlightPage()
        {
            OpenWebsite = null;
            BrowserId = null;
        }

        public SilverlightPage(WebDocument openWebsite,int? browserId)
        {
            OpenWebsite = openWebsite;
            BrowserId = browserId;
        }

        protected IList<T> FindAll<T>(string xPath, int miliseconds = 20000) where T : Adapter
        {
            string correctPath = "./" + xPath;
            return OpenWebsite.Find<T>(correctPath, new Duration(miliseconds));
        }

        /// <summary>
        /// Wybiera date w kalendarzu
        /// </summary>
        /// <param name="calendar">Obiekt kalendarza pojawia sie po kliknieciu na ikone kalendarza</param>
        /// <param name="date">Data w formacie DD-MM-YYYY, np. 28-01-2000</param>
        protected void SilverlightPickDateInCalendar(Ranorex.DateTime calendar, string date)
        {
            string[] months = { "styczeń", "luty", "marzec", "kwiecień", "maj", "czerwiec", "lipiec", "sierpień", "wrzesień", "październik", "listopad", "grudzień" };
            string[] parsedDate = date.Split('-');

            DebugLog("Wybieram date: "+date);
            //wybor roku
            WaitForLoadInWindow();
 
            var headerText = calendar.FindSingle<Button>("./button[@automationid='HeaderButton' and @visible='true']");
            headerText.Press(); //15.05 LK - normalnie zamiast tych dwóch pressów chyba bym wydzielił nowy typ DatePickerów albo jakiś parametr,
            headerText.Press(); //żeby każdy działał w dobry sposób, ale w tym momencie to juz chyba nie ma sensu...
            ClickByLocationDate(parsedDate[2]);

            string szukanybutton = months[int.Parse(parsedDate[1]) - 1] + " " + parsedDate[2];
            ClickByLocationDate(szukanybutton);
            ClickByLocationDate(date);
        }

        private void ClickByLocationDate(string name)
        {
            var el =
                OpenWebsite.FindSingle<Text>(string.Format(".//datetime/button[@name='{0}' and @visible='true']/text",
                                                             name));

            Mouse.MoveTo(el.ScreenRectangle.X, el.ScreenRectangle.Y);
            Mouse.Click(el.ScreenRectangle.X, el.ScreenRectangle.Y);
        }

        public void ClickOrPress(Element e, bool doubleClick = false, bool fakeclick = false) //sciagniete ze starego fionetu
        {
            if (e.Location.X > 0 && e.Location.Y > 0)
            {
                InfoLog("Próba kliknięcia przycisku " + e.GetAttributeValueText("Name"));
                var waitUntil = DateTime.Now.AddSeconds(120);// zwiekszony 30 jest zamałe dla niektorych testow na pro-hudsonie np generowanie pk z rozrachunkiem
                var waitNeeded = true;
                while (waitNeeded && DateTime.Now.CompareTo(waitUntil) < 0)
                {
                    Mouse.MoveTo(e.ScreenLocation.X + (e.ScreenRectangle.Width / 2) + 5,
                                 e.ScreenLocation.Y + (e.ScreenRectangle.Height / 2) + 5);
                    Delay.Milliseconds(150);
                    Mouse.MoveTo(e.ScreenLocation.X + e.ScreenRectangle.Width / 2,
                                 e.ScreenLocation.Y + e.ScreenRectangle.Height / 2);
                    Delay.Milliseconds(150);
                    //    InfoLog("Kursor podczas oczekiwania " + Mouse.CursorName);
                    waitNeeded = Mouse.CursorName.Equals("Default") || Mouse.CursorName.Equals("IBeam");
                    //     InfoLog("Oczekiwanie " + waitNeeded);
                    Delay.Milliseconds(150);
                }
                if (waitNeeded && DateTime.Now.CompareTo(waitUntil) > 0)
                    throw new RanorexException("Przekroczono czas oczekiwania");
                Helper.WaitUntilLoad(10, () => Adapter.Create<Unknown>(e).Element.Visible, "Przycisk nie jest visible");
                Helper.WaitUntilLoad(10, () => Adapter.Create<Unknown>(e).Element.Valid, "Przycisk nie jest valid");

                Mouse.Click(e);
                if (doubleClick) Mouse.Click(e);
            }
            else
            {
                InfoLog("Element ma ujemne współrzędne. Trzaba użyć funkcji Press");

                if (e.As<Button>() != null)
                {
                    e.As<Button>().Press();
                }

                else if (e.As<Ranorex.TabPage>() != null)
                {
                    e.As<Ranorex.TabPage>().Click();
                }
                else
                {
                    throw new Exception(string.Format("Element {0} nie jest określony", e.GetPath(PathBuildMode.Reduce)));
                }
            }
        }


        protected Button GetButtonByNameInWindow(string name)
        {
            return Find<Button>(string.Format("/form[@classname='Popup']//button[@name='{0}' and @visible='true']", name));
        }

        protected Button GetButtonByName(string name)
        {
            
            var everyButtonWithName = FindAll<Button>(string.Format("/button[@name~'{0}' and @visible='true']", name));
            if(everyButtonWithName.Count()>1)
                throw new Exception("Znaleziono wiecej niz jeden button o danej nazwie; Postaw sekcje i stamtad szukaj buttona - w kontekscie sekcji, a nie widoku.");
            if(everyButtonWithName.Count()==0)
                throw new Exception("Nie znaleziono buttona za pomoca funkcji GetButtonByName o parametrze "+name);
            return everyButtonWithName[0];
        }

        protected Text GetTextInPopup(string name)
        {
            return Find<Text>(string.Format("/form[@classname='Popup']//text[@name='{0}']", name));
        }

        //protected void KliknijButtonDalej() //eh, nie wiem co z buttonami...
        //{
        //    GetButtonByNameInWindow("Dalej").Click();
        //    WaitForLoad();
        //}

        //protected void KliknijButtonZapisz() //eh, nie wiem co z buttonami...
        //{
        //    GetButtonByNameInWindow("Zapisz").Click();
        //    WaitForLoad();
        //}

        protected Text GetTextByName(string name)
        {
            return Find<Text>(string.Format("/text[@name='{0}' and @visible='true']", name));
        }


        protected Ranorex.TabPage GetTabpageByName(string name)
        {
            return Find<Ranorex.TabPage>(string.Format("/tabpage[@name='{0}' and @visible='true']", name));
        }

        protected IList<Button> GetSectionsInTabpage(string tabpage)
        {
            WaitForLoad();
            var TabPage = GetTabpageByName(tabpage);
            var AllSections = TabPage.Find<Button>("./button[@automationid='ExpanderButton' and @visible='true']");

            return AllSections;
        }

        #region waitforloady
        protected void WaitForLoad()
        {
            int i = 0;
            while (Find<Text>("/text[@name='Proszę czekać...']", 10000).Visible) //13.05 LK - zmiana limitu timeouta z 7000 na 10000, specjalnie dla testu _096
            {
                System.Threading.Thread.Sleep(1000);
                i++;
                if (i > 30) throw new Exception("Proszę czekać jest za długo");
            }
        }

        protected void WaitForLoadInWindow()
        {
            int i = 0;
            while (Find<Text>("/form//text[@name='Proszę czekać...']", 7000).Visible)
            {
                System.Threading.Thread.Sleep(1000);
                i++;
                if (i > 30) throw new Exception("Proszę czekać jest za długo");
            }
        }

        protected void WaitForProgressBarInElement(Unknown cell)//todo 
        {
            int i = 0;
            while (cell.FindSingle<Ranorex.ProgressBar>("./progressbar[@classname='ProgressBar']", 7000).Visible)
            {
                System.Threading.Thread.Sleep(1000);
                i++;
                if (i > 30) throw new Exception("Proszę czekać jest za długo");
            }
        }

        protected void WaitForProgressBarInElement(Ranorex.ProgressBar progressBar)//todo 
        {
            int i = 0;
            Delay.Milliseconds(100);

            while (progressBar.Visible)
            {
                System.Threading.Thread.Sleep(1000);
                i++;
                if (i > 30) throw new Exception("Proszę czekać jest za długo");
            }
        }
        #endregion

        #region techniczne - lepiej nie ruszac
        protected List<string> GetLabelledTextboxContent(string label, string div="", int childIndexDifference=1)
        {
            List<string> PairedElements = new List<string>();
            
            var foundLabel = Find<Text>(div + string.Format("/text[@name='{0}' and @visible='true' and @controltype='Text']",label));
            int labelIndex = foundLabel.Element.ChildIndex;
            int textboxIndex = labelIndex + childIndexDifference;
            var foundTextbox =
                Find<Text>(string.Format(div + "/text[@childindex='{0}' and @visible='true' and @controltype='Edit']", textboxIndex));

            PairedElements.Add(foundLabel.TextValue.Replace((char)160, ' '));
            PairedElements.Add(foundTextbox.TextValue.Replace((char)160,' '));
            return PairedElements;
        }

        protected Form GetLabelledForm(string label, string div = "", int childIndexDifference = 1, bool hasToBeVisible=true)
        {
            string path = "";
            string elementPath = "";


            if(hasToBeVisible)
                path = string.Format("/text[@name='{0}' and @visible='true' and @controltype='Text']", label);
            else
            {
                path = string.Format("/text[@name='{0}' and @controltype='Text']", label);
            }
                
            var foundLabel = Find<Text>(div + path);
            int labelIndex = foundLabel.Element.ChildIndex;
            int textboxIndex = labelIndex + childIndexDifference;

            if (hasToBeVisible)
                elementPath = string.Format("/form[@childindex='{0}' and @visible='true']", textboxIndex);
            else
            {
                elementPath = string.Format("/form[@childindex='{0}']", textboxIndex);
            }

            var foundElement =
                Find<Form>(string.Format(div + elementPath));

            return foundElement;
        }

        protected Button GetLabelledButton(string label, string div = "", int childIndexDifference = 1)
        {
            var foundLabel = Find<Text>(div + string.Format("/text[@name='{0}' and @visible='true' and @controltype='Text']", label));
            int labelIndex = foundLabel.Element.ChildIndex;
            int textboxIndex = labelIndex + childIndexDifference;
            var foundElement =
                Find<Button>(string.Format(div + "/button[@childindex='{0}' and @visible='true']", textboxIndex));

            return foundElement;
        }

    }
#endregion
}
