using System;
using System.Collections.Generic;
using System.Text;
using Office365Service.Models;
using System.Linq;
using MySqlConnector;
using MySql.Data.MySqlClient;

namespace Office365Service
{
/// <summary>
/// Service for connecting with the Master DB which handles the MUUID's. 
/// </summary>
    public class MasterDBServices
    {
        //Get MUUID
        public Guid GetMUUID()
        {
            Guid MUUID = new Guid();
            using(var conn = new MySqlConnection(Constant.DBConnectionString))
            {
                conn.Open();
                var query = "Select UUID()";
                var command = new MySqlCommand(query, conn);
                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    MUUID = new Guid(reader["UUID()"].ToString());
                }
                conn.Close();
                Console.WriteLine("Select UUID():" + MUUID);
            }
            return MUUID;
        }

        public void CreateEntity(Guid muuid, string id, string entityType)
        {
            using (var db = new MasterDbContext())
            {
                ASCIIEncoding enc = new ASCIIEncoding();
                Console.WriteLine(muuid.ToString());
                Master master = new Master();
                master.Uuid = MySQLByteArray(muuid);
                master.SourceEntityId = id;
                master.EntityType = entityType.ToString();
                master.EntityVersion = 1;
                master.Source = "PLANNING";
                
                db.Master.Add(master);
                db.SaveChanges();
            }
        }

        public Master GetGraphIdFromMUUID(Guid muuid)
        {
            Master master = new Master();
            using(var db = new MasterDbContext())
            {
                master = (from m in db.Master
                 where m.Uuid == MySQLByteArray(muuid) && m.Source == XMLSource.PLANNING.ToString()
                 select m).FirstOrDefault();
                
                Console.WriteLine(master.Id);
                Console.WriteLine(new Guid(master.Uuid).ToString());
                Console.WriteLine(master.SourceEntityId);
                Console.WriteLine(master.EntityType);
                Console.WriteLine(master.EntityVersion);
                Console.WriteLine(master.Source);
            }
            return master;
        }

        public void ChangeEntityVersion(Guid muuid)
        {
            using(var db = new MasterDbContext())
            {
                Master master = (from m in db.Master
                                 where m.Uuid == MySQLByteArray(muuid) && m.Source == XMLSource.PLANNING.ToString()
                                 select m).FirstOrDefault();
                master.EntityVersion++;
                Console.WriteLine("Entity Version: " + master.EntityVersion);
                db.SaveChanges();
            }
        }

        public bool CheckSourceEntityVersionIsHigher(Guid muuid, XMLSource source)
        {
            bool result = false;
            using(var db = new MasterDbContext())
            {
                Master masterSource = (from  m in db.Master
                                       where m.Uuid == MySQLByteArray(muuid) && m.Source == source.ToString()
                                       select m).FirstOrDefault();
                Master masterPlanning = (from m in db.Master
                                         where m.Uuid == MySQLByteArray(muuid) && m.Source == XMLSource.PLANNING.ToString()
                                         select m).FirstOrDefault();
                if (masterSource != null && masterPlanning != null)
                    result = masterSource.EntityVersion > masterPlanning.EntityVersion;
            }
            return result;
        }

        //Deze functie is nodig om Guid.ToByteArray gelijk te stellen met UUID_TO_BIN van MySQL
        //Guid.ToByteArray flipt de eerste 3 groepen van een UUID op een of andere manier 
        public byte[] MySQLByteArray(Guid guid)
        {
            byte[] c = guid.ToByteArray();
            return new byte[] { c[3], c[2], c[1], c[0], c[5], c[4], c[7], c[6], c[8], c[9], c[10], c[11], c[12], c[13], c[14], c[15] };
        }
    }
}
