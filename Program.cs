using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessing
{
    class Program
    {
        static void Main(string[] args)
        {


            Console.WriteLine("Write path to image you want to process: ");
            var path = Console.ReadLine();
            var processors = new Processors(path);

            Console.WriteLine("*Succesfully loaded image*\n");
            Console.WriteLine("Choose processing method: ");
            Console.WriteLine("1. Regionprops (only monochrome)");
            Console.WriteLine("2. Kirsch filtration (RGB or mono)");
            Console.WriteLine("3. Opening with circular element (Mono and logical)");
            Console.WriteLine("4. Labeling (Logical)");
            Console.WriteLine("5. Exit\n");

            var choice = Convert.ToInt32(Console.ReadLine());
            switch (choice)
            {
                case 1:
                    processors.Regionprops();
                    break;
                case 2:
                    processors.KirschFiltration();
                    break;
                case 3:
                    processors.OpenWithCircle();
                    break;
                case 4:
                    processors.Labeling();
                    break;
            }
        }
    }
}
