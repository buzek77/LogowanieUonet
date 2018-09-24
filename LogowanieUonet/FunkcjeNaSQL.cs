using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using LogowanieUonetPages;
using VRanorexLib;
using LogowanieUonet;

namespace LogowanieUonet
{
   public class FunkcjeNaSQL 
   {
        public string uzytkownik = ConfigurationManager.AppSettings["uzytkownik"];
        public string connetionString = ConfigurationManager.AppSettings["connetionString"];
        public string backupFolder = ConfigurationManager.AppSettings["backupFolder"];
        public string czyRobicBackup = ConfigurationManager.AppSettings["czyRobicBackup"];
        public string przedmiotId = ConfigurationManager.AppSettings["przedmiotId"];
        
       public void PolaczZBaza()
        {
           SqlConnection cnn;
           cnn = new SqlConnection(connetionString);
        }
       
       public string PobierzHaslo()
       {
           string sql = "SELECT password FROM uzytkownicy WHERE username='" + uzytkownik + "'";
           using (SqlConnection con = new SqlConnection(connetionString))
           {
               con.Open();
               using (SqlCommand cmd = new SqlCommand(sql, con))
               {
                   using (SqlDataReader reader = cmd.ExecuteReader())
                   {
                       while (reader.Read())
                       {
                           string haslo = reader.GetValue(0).ToString();
                           return haslo;
                       }
                       return null;

                   }
               }
           }

       }
       
       public string PobierzPrzedmiot()
       {
           string sql = "SELECT przedmiot FROM Przedmioty WHERE id='" + przedmiotId + "'";
           using (SqlConnection con = new SqlConnection(connetionString))
           {
               con.Open();
               using (SqlCommand cmd = new SqlCommand(sql, con))
               {
                   using (SqlDataReader reader = cmd.ExecuteReader())
                   {
                       while (reader.Read())
                       {
                           string przedmiot = reader.GetValue(0).ToString();
                           return przedmiot;
                       }
                       return null;

                   }
               }
           }
        }

       public Uczen[] PobierzDaneUcznia()
       {
               string sql = "SELECT Nazwisko, Ocena FROM uczniowie";
               using (SqlConnection con = new SqlConnection(connetionString))
               {
                   con.Open();
                   using (SqlCommand cmd = new SqlCommand(sql, con))
                   {
                       using (SqlDataReader reader = cmd.ExecuteReader())
                       {
                           var lst = new List<Uczen>();
                           while (reader.Read())
                           {
                               var row = new Uczen();
                               row.DaneUcznia = reader.GetValue(0).ToString();
                               row.Ocena = reader.GetValue(1).ToString();
                               lst.Add(row);
                           }
                            Uczen[] tabela=lst.ToArray();
                            return tabela;
                       }
                   }
                }
           }



       public void ZrobBackup()
       {
           if (Convert.ToBoolean(czyRobicBackup))
           {
               Helper.InfoLog("Robie Backup");
               string sql = "BACKUP DATABASE [LoginUonet] TO  DISK = '" + backupFolder +
                            "' WITH  INIT ,  NOUNLOAD ,  NAME = N'MyDatabase backup',  NOSKIP , STATS = 10, NOFORMAT";
               using (SqlConnection con = new SqlConnection(connetionString))
               {
                   con.Open();
                   using (SqlCommand cmd = new SqlCommand(sql, con))
                   {
                       cmd.ExecuteNonQuery();
                   }
                   con.Close();
               }
           }
       }
       public void RestoreBackup()
       {

           if (Convert.ToBoolean(czyRobicBackup))
           {

               Helper.InfoLog("Przywracam backup");
               string sql = "USE MASTER RESTORE DATABASE [LoginUonet] FROM  DISK = '" + backupFolder + "' WITH REPLACE";
               using (SqlConnection con = new SqlConnection(connetionString))
               {
                   con.Open();
                   using (SqlCommand cmd = new SqlCommand(sql, con))
                   {
                       cmd.ExecuteNonQuery();
                   }
                   con.Close();
               }
           }
       }
    
    }
}
