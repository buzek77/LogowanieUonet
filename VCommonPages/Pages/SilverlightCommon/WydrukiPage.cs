using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Ranorex;
using Ranorex.Core;
using Text = Ranorex.Text;


namespace VCommonPages.Pages.SilverlightCommon
{
    public class WydrukiPage : SilverlightPage
    {
        private string tabpage;

        public WydrukiPage(WebDocument openWebsite, int? browserId, string tabpage = "Wydruk")
            : base(openWebsite, browserId)
        {
            this.tabpage = tabpage;
            WaitForLoad();
        }
        public void ZmienRodzajWydruku(string nowyRodzaj)
        {
            DebugLog("Zmieniam rodzaj wydruku na: "+nowyRodzaj);
            var sekcja = GetTabpageByName(tabpage);
            var button = sekcja.FindSingle<Button>(".//button[@automationid='ButtonOpen']");
            button.Click();
            var item = sekcja.FindSingle<Text>(String.Format(".//text[@name='{0}']", nowyRodzaj));
            item.Click();
            WaitForLoad();
        }
        public bool SprawdzCzyPoleDataDuplikatu()
        {
            string nazwa = "Data duplikatu";
            var sekcja = GetTabpageByName(tabpage);
            
            var poleZLabelka = sekcja.FindSingle<Text>(string.Format(".//text[@name='{0}' and @visible='true']",nazwa));
            int indeksPolaZLabelka = poleZLabelka.Element.ChildIndex;
            int indeksPolaZTekstboksem = indeksPolaZLabelka + 1;
            var kalendarzCombo = sekcja.FindSingle<ComboBox>(string.Format(".//combobox[@childindex='{0}' and @visible='true']", indeksPolaZTekstboksem));
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
        public Bitmap PobierzBMP()
        {
            DebugLog("Pobieram zdjecie dokumentu");
            var sekcja = GetTabpageByName(tabpage);
            Element pdf = sekcja.FindSingle(".//container[@automationid='documentPreviewScrollViewer']");
            return Imaging.CaptureImage(pdf);
        }

        public void zmienRozmiar()
        {
            DebugLog("Zmieniam rozmiar");
            var sekcja = GetTabpageByName(tabpage);
            var button = sekcja.FindSingle<Button>(".//button[@automationid='zoom']//button[@classname='Button']",new Duration(30000));
            button.Click();
            var item = sekcja.FindSingle<Text>(String.Format(".//button[@automationid='zoom']//text[@name='{0}']", "Whole Page"));
            item.Click();
            WaitForLoad();
        }

        new private void WaitForLoad()
        {
            var sekcja = GetTabpageByName(tabpage);
            int i = 0;
            Delay.Milliseconds(2000);
            var loading = sekcja.FindSingle<Text>("./container/text[@name='Loading...']");

            while(loading.Visible)
            {
                System.Threading.Thread.Sleep(1000);
                i++;
                if (i > 30) throw new Exception("Loading jest za długo");
            }
        }

        public void KliknijDalej()
        {
            var sekcja = GetTabpageByName(tabpage);
            var button = sekcja.FindSingle<Button>(".//button[@automationid='nextPage']", new Duration(3000));
            button.Click();
        }

        public void PorownajBitmapy(Bitmap oczekiwany, Bitmap rzeczywisty,double similarity)
        {
            DebugLog(String.Format("Oczekiwany obrazek ma wymiary: {0}",oczekiwany.Size));
            DebugLog(String.Format("Rzeczywisty obrazek ma wymiary: {0}", rzeczywisty.Size));

            if(rzeczywisty.Height>oczekiwany.Height)
            {
                DebugLog("Skalowanie oczekiwanego do rzeczywistego");
                var tmp = rzeczywisty;
                rzeczywisty = oczekiwany;
                oczekiwany = tmp;
            }else
            {
                DebugLog("Skalowanie rzeczywistego do oczekiwanego");
            }
            var newImage = new Bitmap(oczekiwany.Width, oczekiwany.Height);
            using (Graphics gr = Graphics.FromImage(newImage))
            {
                gr.SmoothingMode = SmoothingMode.HighQuality;
                gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
                gr.DrawImage(rzeczywisty, new Rectangle(0, 0, oczekiwany.Width, oczekiwany.Height));
            }

            var realSimilarity = Ranorex.Imaging.Compare(oczekiwany, newImage);
            DebugLog(String.Format("Zgodność wynosi: {0}",realSimilarity));
            Assert.IsTrue(realSimilarity > similarity);
        }
    }
}
