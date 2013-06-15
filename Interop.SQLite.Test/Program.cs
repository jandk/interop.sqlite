using System;
using System.Collections.Generic;
using System.IO;

namespace Interop.SQLite.Test
{
    internal class Person
    {
        public string Name { get; set; }
    }

    internal static class Program
    {
        private static void Main()
        {
            string filename = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "test.sqlite"
                );

            using (var db = new SQLite3(filename))
            {
                try
                {
                    using (SQLite3Transaction transaction = db.BeginTransaction())
                    {
                        db.Query("CREATE TABLE User ( ID INTEGER PRIMARY KEY, Name TEXT UNIQUE );");

                        db.Query("INSERT INTO User (Name) VALUES ('John')");
                        db.Query("INSERT INTO User (Name) VALUES ('Jeff')");
                        db.Query("INSERT INTO User (Name) VALUES ('Jesus')");

                        transaction.Commit();
                    }
                }
                catch (SQLite3Exception ex)
                {
                    Console.WriteLine("EXCEPTION: " + ex.Message);
                }

                using (IEnumerator<User> enumerator = db.QueryEnumerator<User>("SELECT ID, Name FROM User"))
                {
                    enumerator.MoveNext();
                    enumerator.MoveNext();
                    Console.WriteLine(enumerator.Current);

                    enumerator.Reset();
                    while (enumerator.MoveNext())
                        Console.WriteLine(enumerator.Current);
                }
            }
        }

        private class User
        {
            // ReSharper disable MemberCanBePrivate.Local
            public int ID { get; set; }
            public string Name { get; set; }
            // ReSharper restore MemberCanBePrivate.Local

            public override string ToString()
            {
                return String.Format("ID: {0}, Name: {1}", ID, Name);
            }
        }
    }
}