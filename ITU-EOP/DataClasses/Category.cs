using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITU_EOP
{
    /// <summary>
    /// Třída uchovávající data pro Kategorii
    /// </summary>
    public class Category
    {
        public string name { get; set; }                //Jméno kategorie
        public TimeSpan timeInCategory { get; set; }    //Celkový čas strávný v kategorii
        public TimeSpan target { get; set; }            //Momentální cíl pro dannou kategorii
        public string translatedName { get; set; }      //Jméno pro zobrazení v češtině

        public Category(string _name, TimeSpan _timeInCategory, TimeSpan _target, string _translatedName)
        {
            name = _name;
            timeInCategory = _timeInCategory;
            target = _target;
            translatedName = _translatedName;
        }
    }
}
