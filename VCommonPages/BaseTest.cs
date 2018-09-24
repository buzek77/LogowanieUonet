using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Ranorex;
using VCommonPages.Pages;
using VRanorexLib;

namespace VCommonPages
{
    public class TestName : System.Attribute
    {
        public string Name;

        public TestName(string name)
        {
            this.Name = name;

        }
    }

    public abstract class POPBaseTest
    {
        public ControlSearcher Repo = ControlSearcher.GetInstance(); // do wywalenia ale bez tego nie dzialaja niektorw funkcje vranoreliba trzeba porpawicw vranorexlibies

        protected Page MainPage; 
        
        //nie ruszać żeby kogoś rączki nie swędziały usunąć te klucze! mareks
        public string Url = ConfigurationManager.AppSettings["url"];
        public string UrlKomunikacja = ConfigurationManager.AppSettings["urlKomunikacja"];
        public string UrlSofista = ConfigurationManager.AppSettings["urlSofista"];


        public string assemblyName = "EfebRanorex";
        protected bool TestSuccess = true;
        protected bool AfterReload = true;
        protected bool CloseBrowserAfterTest = false;
        protected static int _testCounter;
        protected static int _allTestsCounter = -1;


        protected abstract void OpenBrowser();

        public void RestoreDb()
        {
            if (Convert.ToBoolean(ConfigurationManager.AppSettings["restoreDatabase"]))
            {
                Helper.DebugLog("Przywracanie bazy danych");
                try
                {
                    var snapshotRestore = ConfigurationManager.AppSettings["restoreFromSnapshot"];
                    if(snapshotRestore != null)
                    {
                        DBBackup.Setup(Convert.ToBoolean(snapshotRestore));
                    }
                    else
                    {
                        DBBackup.Setup();   
                    }          
                }
                catch (Exception e)
                {
                    Helper.ReportLog(ReportLevel.Error, "Przywracanie bazy danych nie powiodlo sie " + e);
                    Helper.StopRecord(false);
                    Assert.Fail();
                }
            }
        }


        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            RestoreDb();
            
            Helper.ReportLog(ReportLevel.Info, string.Format("assemblyname: {0}, _allTestCounter: {1}", assemblyName, _allTestsCounter.ToString()));

            if (_allTestsCounter == -1)  
            {
                _allTestsCounter = UnignoredTestsTotalCount();
            }
            Helper.StartRecord();
            Try(OpenBrowser);
        }

        [SetUp]
        public void SetUp()
        {
            Try(() =>
                    {
                        Helper.StartRecord();
                        Helper.ReportLog(ReportLevel.Info, String.Format("<---------------Rozpoczecie testu {0} z {1} -------------->", ++_testCounter, _allTestsCounter));
                        if (!MainPage.IsBrowserOpen())
                        {
                            Try(OpenBrowser);
                            AfterReload = true;
                            TestSuccess = true;
                        }
                        else
                        {
                            if (!TestSuccess) // jak zmienimy nunita na nowego do wywalenia nunit 2.6 ma taka flage
                            {
                                MainPage.Reload();
                                
                                AfterReload = true;
                                TestSuccess = true;
                            }
                        }
                        
                    });
        }

        [TearDown]
        public void TearDown()
        {
            Helper.StopRecord();
            if (CloseBrowserAfterTest)
            {
                MainPage.CloseBrowser();
            }
        }


        [TestFixtureTearDown]
        public void CleanupFixture()
        {
           //MainPage.CloseBrowser();
        }

        public void SuccesLog(string message)
        {
            Helper.ReportLog(ReportLevel.Success, message + " zakończony powodzeniem");
        }

        public void Try(Action testCode)
        {
            StackTrace st = new StackTrace();
            StackFrame sf = st.GetFrame(1);
            bool setup = true;
            string testName = "SetUp";
            var method = sf.GetMethod();
            var test = method.GetCustomAttributes(typeof(TestAttribute), true);
            if (test.Length == 1)
            {
                setup = false;
                TestName attr = (TestName)method.GetCustomAttributes(typeof(TestName), true)[0];
                testName = attr.Name;
            }

            if (MainPage != null && !setup)
            {
                Page.InfoLog(testName);
            }

            try
            {
                testCode();
                if (!setup)
                {
                    SuccesLog(testName);
                }
            }
            catch (Exception ex)
            {
                TestFail(ex, method);
            }
        }

        public void TestFail(Exception e, MethodBase testName)
        {
            TestSuccess = false;
            Helper.ReportLog(ReportLevel.Error, Regex.Replace(e.ToString(), "<[^>]+>", string.Empty), testName);
            Helper.FailLog("Test " + testName + " zakończony niepowodzeniem");
            Helper.StopRecord(false);
            Assert.Fail();
        }

        public void Wyloguj()
        {
            CloseBrowserAfterTest = true;
            MainPage.CloseBrowser();
            
        }

        /// <summary>
        /// Metoda liczy nieignorowane testy zawarte w AppDomain w którym NUnit uruchamia testy
        /// </summary>
        /// <param name="saveTestsNames">Po ustawieniu na 'true' loguje nazwy testów przeznaczonych do wykonania w pliku zadanym parametrem fileName</param>
        /// <param name="fileName">Nazwa pliku do którego zapisywane są nazwy testów</param>
        /// <returns>Liczba nieignorowanych testów w tym assembly</returns>       
        public int UnignoredTestsTotalCount(bool saveTestsNames = false, string fileName = null)
        /* Patrz: http://www.nunit.org/index.php?p=assemblyIsolation&r=2.6.2 */
        {
            int testCounter = 0;
            fileName = fileName ?? Path.GetTempFileName();

            var testNames = new List<string>();
            
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Where(a => a.FullName.StartsWith(assemblyName)))
                foreach (var type in assembly.GetTypes().Where(x => x.GetCustomAttributes(typeof(TestFixtureAttribute), false).Any()))
                    foreach (var method in type.GetMethods())
                    {
                        var attributes = method.GetCustomAttributes(false);
                        if (attributes.OfType<TestAttribute>().Any() && !attributes.OfType<IgnoreAttribute>().Any())
                        {
                            testCounter++;
                            if (saveTestsNames) testNames.Add(method.Name + " w klasie " + type.Name);
                        }
                    }

            if (saveTestsNames)
            {
                System.IO.File.WriteAllLines(fileName, testNames);
                Helper.ReportLog(ReportLevel.Info, "Zapisano log w " + fileName);
            }
            return testCounter;
        }

    }
}
