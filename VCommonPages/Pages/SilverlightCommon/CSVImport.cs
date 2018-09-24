using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace VCommonPages.Pages.SilverlightCommon
{
    public class CSVImport
    {
        private static List<List<string>> ReadCSV(string filename) 
        {
 
            // Ustal ścieżkę do zasobów
            UriBuilder uri = new UriBuilder(Assembly.GetExecutingAssembly().CodeBase);
            var dllPath = Path.GetDirectoryName(Uri.UnescapeDataString(uri.Path));
            CultureInfo kulturalnieProsze = CultureInfo.GetCultureInfo("pl-PL");
            var wartosciTabelki = new List<List<string>>();
         
            var calaTabelkaNieoddzielona = File.ReadAllLines(dllPath+filename, Encoding.GetEncoding(kulturalnieProsze.TextInfo.ANSICodePage));
                for (int i = 0; i < calaTabelkaNieoddzielona.Length;i++)
                {
                    if(!Regex.IsMatch(calaTabelkaNieoddzielona[i],"^[;]*$"))
                    {
                        wartosciTabelki.Add(PrepareLine(calaTabelkaNieoddzielona[i]).Split(';').ToList());
                        for (int j = 0; j < wartosciTabelki.Last().Count;j++)
                        {
                            //tak to sie kończy dzieci jak wam każą w scenariuszu sprawdzić średnik
                            wartosciTabelki.Last()[j] = wartosciTabelki.Last()[j].Replace("[ŚREDNIK]", ";");
                        }
                    }
                }
            
            return wartosciTabelki;
        }

        private static string PrepareLine(string line) //zamienia stringi jak to konieczne
        {
            var dict = new Dictionary<string, string>();
            dict.Add("FAŁSZ", "Nie");
            dict.Add("PRAWDA", "Tak");
            dict.Add("DEFAULT", "");
            
            var output = new StringBuilder(line);

            foreach (var kvp in dict)
            {
                output.Replace(kvp.Key, kvp.Value);
            }

            return output.ToString();

        }

        public static List<List<string>> ImportujTabelkeZCSV(string filename) //zwraca koncowa liste, w pozadanej formie (internaltable dla silverlighttable)
        {
            var importedTab = ReadCSV(filename);

            for (int i = 0; i < importedTab.Count(); i++)
            {
                for (int j=0;j<importedTab[i].Count();j++)
                {
                    importedTab[i][j] = (importedTab[i][j] != null ? importedTab[i][j].Trim() : null);
                }
            }
            return importedTab;
        }
    }
}
