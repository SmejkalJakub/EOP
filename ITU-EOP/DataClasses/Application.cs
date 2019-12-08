using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ITU_EOP
{
    /// <summary>
    /// Třída uchovávající data o Aplikaci
    /// </summary>
    public class Application
    {
        public TimeSpan timeInApp { get; set; }         //Čas strávený v aplikaci
        public int percenTimeInApp { get; set; }        //Čas strávený v aplikaci (%)
        public string name { get; set; }                //Jméno aplikace
        private Category category;
        public Category Category
        {
            get { return category; }
            set
            {
                if (value != category)
                {
                    category = value;
                }
            }
        }

        public ImageSource workIcon { get; set; }
        public ImageSource funIcon { get; set; }        //Icony kategorií
        public ImageSource otherIcon { get; set; }
    }
}
