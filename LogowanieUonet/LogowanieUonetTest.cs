using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VCommonPages;
using NUnit.Framework;
using VCommonPages.Pages;
using System.Reflection;
using System.Text.RegularExpressions;
using LogowanieUonetPages;
using System.Data.SqlClient;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using VRanorexLib;
using LogowanieUonet;





namespace LogowanieUonet
{
    public class LogowanieUonetTest : POPBaseTest
    {
        public string uzytkownik = ConfigurationManager.AppSettings["uzytkownik"];
        protected override void OpenBrowser()
        {
            MainPage = new LogowanieStronaStartowa(Url);
         }
        private LogowanieStronaStartowa UonetLogin
        {
            get
            {
                return (LogowanieStronaStartowa)MainPage;
            }
        }
       
       

        [Test, TestName("Sprawdzanie Rozkładu mateirału, tworzenie lekcji")]
        public void UonetLoginTest()
        {
            Try(() =>
                    {
                        var login = UonetLogin;
                        new FunkcjeNaSQL().ZrobBackup();
                        var zaloguj = login.Zaloguj1();
                        zaloguj.WpiszLogin(uzytkownik);
                        zaloguj.WpiszPass(new FunkcjeNaSQL().PobierzHaslo());
                        var zalogowany = zaloguj.Zaloguj2();
                        var wdziennku = zalogowany.WybierzDziennikSzkoly();
                        wdziennku.OtworzDzienTygodnia();
                        var utworzlekcje = wdziennku.UwtorzLekcje("1. ");
                        utworzlekcje.DodajGrupe("2ag");
                        Assert.That(utworzlekcje.SprawdzGrupe(), Is.EqualTo("2ag"));
                        utworzlekcje.DodajPrzedmiot(new FunkcjeNaSQL().PobierzPrzedmiot());
                        utworzlekcje.LekacjaDalej();
                        utworzlekcje.WybierzRozdział();
                        Assert.That(utworzlekcje.SprawdzRozklad(1), Is.EqualTo("NOWA ERA - Podręcznik do wiedzy o kulturze dla liceum i technikum"));
                        Assert.That(utworzlekcje.SprawdzRozklad(2), Is.EqualTo("Człowiek w przestrzeni kultury 1 godz"));
                        Assert.That(utworzlekcje.SprawdzRozklad(3), Is.EqualTo("Człowiek w przestrzeni kultury "));
                        Assert.That(utworzlekcje.SprawdzRozklad(4), Is.EqualTo("1"));
                        var zapisanalekcja = utworzlekcje.ZapiszLekcje();
                        Assert.That(zapisanalekcja.SprawdzZapisanaLekcje(1), Is.EqualTo("2ag Wiedza o kulturze"));
                        zapisanalekcja.UsunLekcje("2ag Wiedza o kulturze");
                        zapisanalekcja.WylogujZDziennika();
                        Wyloguj();
                        RestoreDb();
                        
                     }
            );
        }
        [Test, TestName("Wprowadzanie Ocen")]
        public void UonetWprowadzanieOcen()
        {
            Try(() =>
            {
                var login = UonetLogin;
                new FunkcjeNaSQL().ZrobBackup();
                var zaloguj = login.Zaloguj1();
                zaloguj.WpiszLogin(uzytkownik);
                zaloguj.WpiszPass(new FunkcjeNaSQL().PobierzHaslo());
                var zalogowany = zaloguj.Zaloguj2();
                var wdzienniku = zalogowany.WybierzDziennikSzkoly();
                wdzienniku.OtworzDzienTygodnia();
                var utworzlekcje = wdzienniku.UwtorzLekcje("1. ");
                utworzlekcje.DodajGrupe("2ag");
                utworzlekcje.DodajPrzedmiot(new FunkcjeNaSQL().PobierzPrzedmiot());
                utworzlekcje.LekacjaDalej();
                var zapisanalekcja = utworzlekcje.ZapiszLekcje();
                var lekcjaoceny = zapisanalekcja.PrzejdzDoOcen();
                lekcjaoceny.KliknijWZmienOceny();
                lekcjaoceny.wpiszOceny(new FunkcjeNaSQL().PobierzDaneUcznia());
                wdzienniku = lekcjaoceny.DoStronyGlownejDziennika();
                utworzlekcje = wdzienniku.UwtorzLekcje("2. ");
                utworzlekcje.DodajGrupe("2a");
                utworzlekcje.DodajPrzedmiot(new FunkcjeNaSQL().PobierzPrzedmiot());
                utworzlekcje.LekacjaDalej();
                zapisanalekcja = utworzlekcje.ZapiszLekcje();
                var lekcjaFrekwencja = zapisanalekcja.PrzejdzDoFrekwencji();
                lekcjaFrekwencja.KliknijWZmienFrekwencję();
                lekcjaFrekwencja.SprawdzFrekwencje();
                zapisanalekcja= lekcjaFrekwencja.DoStronyGlownejDziennika();
                zapisanalekcja.UsunLekcje("2ag Wiedza o kulturze");
                zapisanalekcja.UsunLekcje("2a Wiedza o kulturze");
                zapisanalekcja.WylogujZDziennika();
                Wyloguj();
                RestoreDb();
            }
            );
        }

    }
}
