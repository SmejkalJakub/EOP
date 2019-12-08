/*
 * ITU projekt - TimeTracker
 * Modul s pomocnou třidou zpracovávající soubory, která se používa pro načítání dat z databáze.
 * Dominik Nejedlý, xnejed09
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace ITU_EOP
{
    class FileOperator
    {
        readonly string currentDirectory = Directory.GetCurrentDirectory();

        /// <summary>
        /// Načtení souboru do colekce, načítání celého souboru obsahující popis aplikací uložených z předchozích použití.
        /// </summary>

        public ObservableCollection<Application> loadFile(DateTime date, ref int wholeTime)
        {
            ObservableCollection<Application> returningCollection =
                Newtonsoft.Json.JsonConvert.DeserializeObject<ObservableCollection<Application>>(File.ReadAllText(makePath(date)));

            foreach (Application item in returningCollection)
            {
                wholeTime += (int)item.timeInApp.TotalSeconds;
            }

            return returningCollection;
        }
        /// <summary>
        ///Načtení souboru ze zvolené cesty.
        /// </summary>

        public Dictionary<string, string> loadFile(string path)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(path));
        }

        /// <summary>
        /// Zkontrolování zda soubor existuje.
        /// </summary>
        public bool checkFile(DateTime date)
        {
            return File.Exists(makePath(date));
        }

        /// <summary>
        /// Zkontrolování souboru ze zvolené cesty.
        /// </summary>
        public bool checkFile(string path)
        {
            return File.Exists(path);
        }

        /// <summary>
        /// Načtení celého měsíce z databáze.
        /// </summary>
        public ObservableCollection<Application> loadMonth(DateTime date, ref int wholeTime)
        {
            ObservableCollection<Application> loadedDay;
            ObservableCollection<Application> returningCollection = new ObservableCollection<Application>();

            int day = 1;

            date = new DateTime(date.Year, date.Month, 1);

            if (!Directory.Exists(Path.Combine(currentDirectory + "\\" + date.Year + "\\" + date.Month)))
            {
                return returningCollection;

            }

            while (day <= 31)
            {
                if (!File.Exists(Path.Combine(currentDirectory + "\\" + date.Year + "\\" + date.Month + "\\" + date.Day + ".json")))
                {
                    date = date.AddDays(1);
                    day++;
                    continue;
                }
                else
                {
                    loadedDay = loadFile(date, ref wholeTime);
                }

                day++;

                if (loadedDay != null)
                {
                    foreach (Application item in loadedDay)
                    {

                        if (returningCollection.Any(i => i.name == item.name))
                        {
                            var application = returningCollection.Single(i => i.name == item.name);
                            application.timeInApp = application.timeInApp.Add(item.timeInApp);
                        }
                        else
                        {
                            returningCollection.Add(item);
                        }
                    }
                }
                date = date.AddDays(1);
            }

            return returningCollection;



        }

        /// <summary>
        /// Načtení celého roku z databáze.
        /// </summary>
        public ObservableCollection<Application> loadYear(DateTime date, ref int wholeTime)
        {
            ObservableCollection<Application> loadedMonth;
            ObservableCollection<Application> returningCollection = new ObservableCollection<Application>();

            int month = 1;

            date = new DateTime(date.Year, 1, 1);

            if (!Directory.Exists(Path.Combine(currentDirectory + "\\" + date.Year)))
            {
                return returningCollection;

            }


            while (month <= 12)
            {
                if (!Directory.Exists(Path.Combine(currentDirectory + "\\" + date.Year + "\\" + date.Month)))
                {
                    date = date.AddMonths(1);
                    month++;
                    continue;
                }
                else
                {
                    loadedMonth = loadMonth(date, ref wholeTime);
                }
                month++;

                if (loadedMonth != null)
                {
                    foreach (Application item in loadedMonth)
                    {
                        if (returningCollection.Any(i => i.name == item.name))
                        {
                            var application = returningCollection.Single(i => i.name == item.name);
                            application.timeInApp = application.timeInApp.Add(item.timeInApp);
                        }
                        else
                        {
                            returningCollection.Add(item);
                        }
                    }
                }
                date = date.AddMonths(1);
            }

            return returningCollection;



        }

        public string makePath(DateTime date)
        {
            string year = date.Year.ToString();
            string month = date.Month.ToString();
            string day = date.Day.ToString();
            day += ".json";


            string currDirectory = Directory.GetCurrentDirectory();


            return Path.Combine(currDirectory + "\\" + year + "\\" + month + "\\" + day);
        }


        /// <summary>
        /// Vytvoření potřebných složek.
        /// </summary>
        public void createDirectories()
        {
            string currDirectory = Directory.GetCurrentDirectory();

            if (!Directory.Exists(Path.Combine(currDirectory + "\\" + DateTime.Today.Year)))
            {
                Directory.CreateDirectory(Path.Combine(currDirectory + "\\" + DateTime.Today.Year));
                if (!Directory.Exists(Path.Combine(currDirectory + "\\" + DateTime.Today.Year + "\\" + DateTime.Today.Month)))
                {
                    Directory.CreateDirectory(Path.Combine(currDirectory + "\\" + DateTime.Today.Year + "\\" + DateTime.Today.Month));
                }
            }
        }

    }
}
