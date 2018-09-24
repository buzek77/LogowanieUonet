using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VCommonPages;
using Ranorex;
using System.Collections;
using System.Data.SqlClient;
using System.Configuration;


namespace LogowanieUonetPages.Pages
{
    public class DziennikDodajLekcje:DziennikStronaGlowna
    {
        public  DziennikDodajLekcje(WebDocument openWebsite,int? browserId):base(openWebsite,browserId)
        {
         }
  
    
    public void DodajGrupe(string tekst)
    {
        InfoLog("dodaję grupę");
        GetInputTagById("cbOddzial-inputEl").Click();
        GetLiTagByText(tekst).Click();
    }

    public string SprawdzGrupe()
    {
        InfoLog("sprawdza grupe");
        var sprawdzGrupe = GetInputTagByName("DefinicjaGrupy").Value;
        return sprawdzGrupe;
    }
    public void DodajPrzedmiot(string tekst)
    {
        InfoLog("dodaję przedmiot");
        GetTableTagById("cbPrzedmiot-triggerWrap']//div[@class='x-trigger-index-0 x-form-trigger x-form-arrow-trigger x-form-trigger-first']").Click();
        GetLiTagByText(tekst).Click();
       
    }

    public void LekacjaDalej()
    {
        InfoLog("klikam dalej");
        GetSpanTagByText("Dalej").Click();

    }
    public void WybierzRozdział()
    {
        InfoLog("Wybieram rozdział materialu");
        GetTableTagById("cbRozklad-triggerWrap']//div[@visible='true']").Click();
        GetLiTagByText("NOWA ERA - Podręcznik do wiedzy o kulturze dla liceum i technikum").Click();
        GetTableTagById("cbPozycja-triggerWrap']//div[@class='x-trigger-index-0 x-form-trigger x-form-arrow-trigger x-form-trigger-first']").Click();
        GetLiTagByText("Człowiek w przestrzeni kultury 1 godz").Click();

    }
    public string SprawdzRozklad(int i)
    {
        InfoLog("rozpoczynam sprawdzanie rozkład");

        string wynik = "";


        if (i == 1)
        {
            InfoLog("sprawdzam rozkłąd rozkład");
            wynik = GetInputTagByName("IdRozkladuMaterialu").Value.ToString();
            return wynik;
        }
        if (i == 2)
        {
            InfoLog("sprawdzam pozycję");
            wynik = GetInputTagByName("IdPozRozkladuMaterialu").Value.ToString();
            return wynik;
        }
        if (i == 3)
        {
            InfoLog("sprawdzam temat");
            wynik = GetInputTagById("tbTemat-inputEl").Value.ToString();
            return wynik;
        }
        if (i == 4)
        {
            InfoLog("sprawdzam numer lekcji");
            wynik = GetInputTagByName("NrLekcji").Value.ToString();
            return wynik;
        }

        return wynik;
    }
        public DziennikStronaGlowna ZapiszLekcje()
        {
            InfoLog("zapisuje lekcję");
            GetSpanTagByText("Zapisz").Click();
            var przejdzDoDziennika = new DziennikStronaGlowna(OpenWebsite, BrowserId);
            return przejdzDoDziennika;
        }

        public void AnulujLekcje()
        {
            InfoLog("anuluję lekcje");
            GetSpanTagByText("Anuluj").Click();
        }
        
    }
}
