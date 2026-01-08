using System;
using System.Collections.Generic;
using System.Windows.Forms;
using NeuralNetwork1;

namespace NeuralNetworkZodiac
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new NeuralNetworksStand(new Dictionary<string, Func<int[], BaseNetwork>>
            {
                {"Accord.Net", structure => new AccordNet(structure)},
                {"Student Network", structure => new StudentNetwork(structure)},
            }));
        }
    }
}