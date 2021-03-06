﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace MonteCarlo
{
    class Program
    {
        static string TablicaDoString<T>(T[][] tab)
        {
            string wynik = "";
            for (int i = 0; i < tab.Length; i++)
            {
                for (int j = 0; j < tab[i].Length; j++)
                {
                    wynik += tab[i][j].ToString() + " ";
                }
                wynik = wynik.Trim() + Environment.NewLine;
            }

            return wynik;
        }

        static double StringToDouble(string liczba)
        {
            double wynik; liczba = liczba.Trim();
            if (!double.TryParse(liczba.Replace(',', '.'), out wynik) && !double.TryParse(liczba.Replace('.', ','), out wynik))
                throw new Exception("Conversion from string to double failed.");

            return wynik;
        }

        static int StringToInt(string liczba)
        {
            int wynik;
            if (!int.TryParse(liczba.Trim(), out wynik))
                throw new Exception("Conversion from string to int failed.");

            return wynik;
        }

        static bool HasValue(double value)
        {
            return !Double.IsNaN(value) && !Double.IsInfinity(value);
        }

        static string[][] StringToTablica(string sciezkaDoPliku)
        {
            string trescPliku = File.ReadAllText(sciezkaDoPliku);
            string[] wiersze = trescPliku.Trim().Split(new char[] { '\n' });
            string[][] wczytaneDane = new string[wiersze.Length][];

            for (int i = 0; i < wiersze.Length; i++)
            {
                string wiersz = wiersze[i].Trim();
                string[] cyfry = wiersz.Split(new char[] { ' ' });
                wczytaneDane[i] = new string[cyfry.Length];
                for (int j = 0; j < cyfry.Length; j++)
                {
                    string cyfra = cyfry[j].Trim();
                    wczytaneDane[i][j] = cyfra;
                }
            }
            return wczytaneDane;
        }

        static void Main(string[] args)
        {
            // i.e.
            // australian.txt
            // diabetes.txt
            // hepatitis-filled.txt
            // heartdisease.txt
            string plik_dane = @"australian.txt";

            string testowy = @"testowyPRE.txt";

            // WARNING !!!
            // testowy needs to be TST dataset used in bootstrap program to operate on 
            // the same data in order to obtain comparable results

            string[][] wczytane_dane = StringToTablica(plik_dane);

            /*******************************************************************************************/

            Console.WriteLine("\n \n Specify percentage amount of objects that were randomized into TST dataset before Bootstrap test (e.g. 20, 25) \n \n " +
                              "Please, keep also in mind that Monte Carlo splits based test requires to pass testing set that you received as\n" +
                              " testowy.txt after running Bootstrap based test. It should be named as testowyPRE.txt to be correctly read by \n MonteCarlo program. \n \n " +
                              "If you do not own copy of testowy.txt file, make sure to run Bootstrap based test first. \n \n ");
            int procentObiektow = Convert.ToInt32(Console.ReadLine());

            decimal liczbaTestowych = (procentObiektow / (decimal)100) * wczytane_dane.Length;

            decimal pozostalo = wczytane_dane.Length - liczbaTestowych;

            string[][] wczytany_Testowy = new string[(int)liczbaTestowych][];

            wczytany_Testowy = StringToTablica(testowy);

            // przechowuje informacje niezbędne do uzyskania komitetu klasyfikatorów
            string[][] daneKlasyfikatora = new string[(int)liczbaTestowych][];

            for (int i = 0; i < daneKlasyfikatora.Length; i++)
            {
                daneKlasyfikatora[i] = new string[50];
            }

            // i.e.
            // australian = 14
            // diabetes = 8
            // hepatitis-filled = 19
            // heartdisease = 13
            int liczbaKolumn = 14;
            int indexDecyzji = 14;

            // i.e.
            // australian klasa0 => 0, klasa1 => 1
            // diabetes klasa0 => 1, klasa1 => 0
            // hepatitis-filled klasa0 => 2, klasa1 = 1
            // heartdisease klasa0 => 2, klasa1 => 1
            string klasa0 = "0";
            string klasa1 = "1";

            const double epsilon = 0.00000001;

            decimal eff_global_acc = 0;
            decimal eff_balanced_acc = 0;
            decimal eff_global_cov = 0;
            decimal eff_balanced_cov = 0;
            decimal eff_tpr0 = 0;
            decimal eff_tpr1 = 0;
            decimal eff_youdenIndex = 0;
            decimal eff_matthewCorelation = 0;

            int liczbaObiektowTreningowych = 0;

            Random wx = new Random();

            int liczbaDecyzji0 = 0;
            int liczbaDecyzji1 = 0;

            List<int> listaWylosowanych = new List<int>();
            int indexerTestowy = 0;

            using (StreamWriter writeToFile = new StreamWriter("testowy.txt"))
            {
                writeToFile.WriteLine(TablicaDoString(wczytany_Testowy));
            }

            for (int p = 10; p <= 100; p += 10) // 10 20 .. 100
            {
                Console.WriteLine("----------------------------------------------");
                Console.WriteLine($"{p}% TRN dataset test about to start...");
                Console.WriteLine("----------------------------------------------");
                Thread.Sleep(1500);

                decimal liczbaTreningowych = (p / (decimal)100) * pozostalo;

                string[][] wczytany_Treningowy = new string[(int)liczbaTreningowych][];

                using (StreamWriter writetofile_globalAcc = new StreamWriter($"global_acc{p}%.txt"))
                {
                    using (StreamWriter writetofile_comitee = new StreamWriter($"komitet{p}%.txt"))
                    {
                        for (int g = 0; g < 50; g++)
                        {
                            // *************************************************
                            // setting TRN dataset for current iteration..
                            // *************************************************
                            int indexerTreningowego = 0;

                            List<int> juzWylosowane = new List<int>(); // przetrzymuje informacje o wylosowanych do TRN

                            while (indexerTreningowego != wczytany_Treningowy.Length)
                            {
                                for (int i = 0; i < wczytane_dane.Length; i++)
                                {
                                    bool jestTestowa = false;

                                    int wylosowanyIndeks = wx.Next(0, wczytane_dane.Length);

                                    // sprawdz czy wylosowany indeks juz nie zostal uzyty w losowaniu do testowego
                                    foreach (var id in listaWylosowanych)
                                    {
                                        if (id == wylosowanyIndeks)
                                        {
                                            jestTestowa = true;
                                            break;
                                        }
                                    }

                                    // sprawdz czy taki indeks nie zostal juz wylosowany w TRN
                                    foreach (var id in juzWylosowane)
                                    {
                                        if (id == wylosowanyIndeks)
                                        {
                                            jestTestowa = true;
                                            break;
                                        }
                                    }

                                    juzWylosowane.Add(wylosowanyIndeks);

                                    if (indexerTreningowego == wczytany_Treningowy.Length)
                                    {
                                        break;
                                    }

                                    if (!jestTestowa)
                                    {
                                        wczytany_Treningowy[indexerTreningowego] = wczytane_dane[wylosowanyIndeks];

                                        indexerTreningowego++;
                                    }
                                }
                            }

                            using (StreamWriter writetext = new StreamWriter($"treningowy{p}%_i{g + 1}.txt"))
                            {
                                writetext.WriteLine(TablicaDoString(wczytany_Treningowy));
                            }

                            int liczbaObiektowKlasy0 = 0;
                            int liczbaObiektowKlasy1 = 0;

                            // **********************************
                            // Naiwny Klasyfikator Bayesa
                            // **********************************

                            for (int i = 0; i < wczytany_Treningowy.Length; i++)
                            {
                                if (wczytany_Treningowy[i][indexDecyzji] == klasa0)
                                {
                                    liczbaObiektowKlasy0++;
                                }
                                else if (wczytany_Treningowy[i][indexDecyzji] == klasa1)
                                {
                                    liczbaObiektowKlasy1++;
                                }
                            }

                            liczbaObiektowTreningowych = liczbaObiektowKlasy0 + liczbaObiektowKlasy1;


                            List<double> param0 = new List<double>();
                            List<double> param1 = new List<double>();
                            List<int> decyzjeKlasyfikatora = new List<int>();

                            double P0 = liczbaObiektowKlasy0 / (double)liczbaObiektowTreningowych;
                            double P1 = liczbaObiektowKlasy1 / (double)liczbaObiektowTreningowych;

                            int liczbaObiektowPoprawnieSklasyfikowanych = 0;
                            int liczbaObiektowSklasyfikowanych = 0;
                            int liczbaKlas = 2;

                            int liczbaObiektowPoprawnieSklasyfikowanych_klasa0 = 0;
                            int liczbaObiektowPoprawnieSklasyfikowanych_klasa1 = 0;
                            int liczbaObiektowSklasyfikowanych_klasa0 = 0;
                            int liczbaObiektowSklasyfikowanych_klasa1 = 0;
                            int liczbaObiektowBlednieSklasyfikowanych_klasa0 = 0;
                            int liczbaObiektowBlednieSklasyfikowanych_klasa1 = 0;

                            // ustawienie na obiekcie z testowego
                            for (int i = 0; i < wczytany_Testowy.Length; i++)
                            {
                                // ustawiamy sie na kolumnie
                                for (int j = 0; j < liczbaKolumn; j++)
                                {
                                    double testowa = StringToDouble(wczytany_Testowy[i][j]);
                                    int licznikTrafien0 = 0;
                                    int licznikTrafien1 = 0;

                                    // przechodzimy po wartosciach danej kolumny
                                    for (int w = 0; w < wczytany_Treningowy.Length; w++)
                                    {
                                        double treningowa = StringToDouble(wczytany_Treningowy[w][j]);

                                        // jezeli decyzja 0 treningowego
                                        if (wczytany_Treningowy[w][indexDecyzji] == klasa0
                                            && Math.Abs(treningowa - testowa) < epsilon)
                                        {
                                            licznikTrafien0++;
                                        }

                                        if (wczytany_Treningowy[w][indexDecyzji] == klasa1
                                            && Math.Abs(treningowa - testowa) < epsilon)
                                        {
                                            licznikTrafien1++;
                                        }

                                        // pod koniec danej kolumny zapisz liczby trafien
                                        if (w == wczytany_Treningowy.Length - 1)
                                        {
                                            if (liczbaObiektowKlasy0 <= 0)
                                            {
                                                param0.Add(0);
                                            }
                                            else if (liczbaObiektowKlasy0 > 0)
                                            {
                                                param0.Add(licznikTrafien0 / (double)liczbaObiektowKlasy0);
                                            }

                                            if (liczbaObiektowKlasy1 <= 0)
                                            {
                                                param1.Add(0);
                                            }
                                            else if (liczbaObiektowKlasy1 > 0)
                                            {
                                                param1.Add(licznikTrafien1 / (double)liczbaObiektowKlasy1);
                                            }
                                        }
                                    }
                                }

                                double wynik_param0 = 0;
                                double wynik_param1 = 0;

                                // zliczamy Param'y dla poszczególnych klas
                                foreach (var wartosc in param0)
                                {
                                    wynik_param0 += wartosc;
                                }

                                foreach (var wartosc in param1)
                                {
                                    wynik_param1 += wartosc;
                                }

                                wynik_param0 *= P0;
                                wynik_param1 *= P1;

                                if (double.IsNaN(wynik_param0) || double.IsNaN(wynik_param1))
                                {
                                    // nie klasyfikujemy obiektu
                                    Console.WriteLine("sad :[");
                                    Console.ReadLine();
                                }

                                // porównujemy
                                if (wynik_param0 > wynik_param1)
                                {
                                    Console.Write("TEST nr " + (g + 1) + " Object x" + (i + 1) + " gets decision 0");

                                    decyzjeKlasyfikatora.Add(0);

                                    liczbaDecyzji0++;

                                    // i - obiekt
                                    // g - iteracja
                                    daneKlasyfikatora[i][g] = klasa0;

                                    if (wczytany_Testowy[i][indexDecyzji] == klasa0)
                                    {
                                        Console.Write(
                                            " which is correct by TST expert data(object correctly classified)");

                                        liczbaObiektowPoprawnieSklasyfikowanych_klasa0++;
                                    }
                                    else if (wczytany_Testowy[i][indexDecyzji] == klasa1)
                                    {
                                        Console.Write(
                                            " which is incorrect by TST expert data(object not classified correctly)");

                                        liczbaObiektowBlednieSklasyfikowanych_klasa0++;
                                        Console.WriteLine("\n Te" + liczbaObiektowBlednieSklasyfikowanych_klasa0);
                                    }

                                    liczbaObiektowSklasyfikowanych_klasa0++;
                                }
                                else if (wynik_param1 > wynik_param0)
                                {
                                    Console.Write("TEST nr " + (g + 1) + " Object x" + (i + 1) + " gets decision 1");

                                    decyzjeKlasyfikatora.Add(1);

                                    // i - obiekt
                                    // g - iteracja
                                    daneKlasyfikatora[i][g] = klasa1;

                                    liczbaDecyzji1++;

                                    if (wczytany_Testowy[i][indexDecyzji] == klasa1)
                                    {
                                        Console.Write(
                                            " which is correct by TST expert data(object correctly classified)");

                                        liczbaObiektowPoprawnieSklasyfikowanych_klasa1++;
                                    }
                                    else if (wczytany_Testowy[i][indexDecyzji] == klasa0)
                                    {
                                        Console.Write(
                                            " which is incorrect by TST expert data(object not classified correctly)");

                                        liczbaObiektowBlednieSklasyfikowanych_klasa1++;
                                        Console.WriteLine("\n Ta" + liczbaObiektowBlednieSklasyfikowanych_klasa1);
                                    }

                                    liczbaObiektowSklasyfikowanych_klasa1++;
                                }
                                else if (Math.Abs(wynik_param1 - wynik_param0) < epsilon)
                                {
                                    // otrzymuje losowa decyzje
                                    Random r = new Random();
                                    var values = new[] { 0, 1 };
                                    int result = values[r.Next(values.Length)];

                                    Console.Write("TEST nr " + (g + 1) + " Object x" + (i + 1) + " gets decision " +
                                                  result);

                                    string wynik = "";

                                    if (result == 0)
                                    {
                                        wynik = klasa0;
                                    }
                                    else if (result == 1)
                                    {
                                        wynik = klasa1;
                                    }

                                    // decyzjeKlasyfikatora.Add(result);

                                    // i - obiekt
                                    // g - iteracja
                                    daneKlasyfikatora[i][g] = wynik;

                                    if (wynik == klasa0)
                                    {
                                        liczbaDecyzji0++;
                                    }
                                    else
                                    {
                                        liczbaDecyzji1++;
                                    }

                                    if (wczytany_Testowy[i][indexDecyzji] == wynik)
                                    {
                                        Console.Write(
                                            " which is correct by TST expert data(object correctly classified)");

                                        if (wynik == klasa0)
                                        {
                                            liczbaObiektowPoprawnieSklasyfikowanych_klasa0++;
                                        }
                                        else if (wynik == klasa1)
                                        {
                                            liczbaObiektowPoprawnieSklasyfikowanych_klasa1++;
                                        }
                                    }
                                    else
                                    {
                                        Console.Write(
                                            " which is incorrect by TST expert data(object not classified correctly)");

                                        if (wynik == klasa0)
                                        {
                                            liczbaObiektowBlednieSklasyfikowanych_klasa0++;
                                        }
                                        else if (wynik == klasa1)
                                        {
                                            liczbaObiektowBlednieSklasyfikowanych_klasa1++;
                                        }
                                    }

                                    if (wynik == klasa0)
                                    {
                                        liczbaObiektowSklasyfikowanych_klasa0++;
                                    }
                                    else if (wynik == klasa1)
                                    {
                                        liczbaObiektowSklasyfikowanych_klasa1++;
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Tromba");
                                    Console.ReadLine();
                                }

                                Console.WriteLine();

                                // Reset
                                param0.Clear();
                                param1.Clear();
                            }

                            liczbaObiektowSklasyfikowanych =
                                liczbaObiektowSklasyfikowanych_klasa1 + liczbaObiektowSklasyfikowanych_klasa0;

                            liczbaObiektowPoprawnieSklasyfikowanych =
                                liczbaObiektowPoprawnieSklasyfikowanych_klasa0 +
                                liczbaObiektowPoprawnieSklasyfikowanych_klasa1;

                            int poprawnieSklasyfikowane = 0;
                            int sklasyfikowane = 0;

                            // jezeli 3 iteracja to mamy juz 3 kolumny 
                            if (g > 1)
                            {
                                //using (StreamWriter writetext = new StreamWriter("komitetTabela.txt"))
                                //{
                                //    writetext.WriteLine(TablicaDoString(daneKlasyfikatora));
                                //}

                                // ustaw sie na obiekcie
                                for (int i = 0; i < daneKlasyfikatora.Length; i++)
                                {
                                    int liczba0 = 0;
                                    int liczba1 = 0;

                                    // Console.WriteLine("\nObiekt " + (i + 1));

                                    // zlicz liczbe wystapien
                                    for (int j = 0; j <= g; j++)
                                    {
                                        // Console.WriteLine("Kolumna = " + j);
                                        // Console.WriteLine("tera => " + daneKlasyfikatora[i][j]);

                                        if (daneKlasyfikatora[i][j] == klasa0)
                                        {
                                            liczba0++;
                                        }
                                        else if (daneKlasyfikatora[i][j] == klasa1)
                                        {
                                            liczba1++;
                                        }
                                    }

                                    // Console.WriteLine("zer jest => " + liczba0);
                                    // Console.WriteLine("jedynek jest => " + liczba1);
                                    // Console.ReadLine();

                                    // werdykt
                                    if (liczba0 > liczba1)
                                    {
                                        // Console.WriteLine("Obiekt " + (i + 1) + " ma decyzje => " + wczytany_Testowy[i][indexDecyzji]);

                                        if (wczytany_Testowy[i][indexDecyzji] == klasa0)
                                        {
                                            // Console.WriteLine("Decyzja 0 jest poprawnie przypisana");
                                            poprawnieSklasyfikowane++;
                                        }
                                        else
                                        {
                                            // Console.WriteLine("Decyzja 0 jest zle przypisana");
                                        }

                                        sklasyfikowane++;
                                    }
                                    else if (liczba1 > liczba0)
                                    {
                                        // Console.WriteLine("Obiekt " + (i + 1) + " ma decyzje => " + wczytany_Testowy[i][indexDecyzji]);

                                        if (wczytany_Testowy[i][indexDecyzji] == klasa1)
                                        {
                                            // Console.WriteLine("Decyzja 1 jest poprawnie przypisana");
                                            poprawnieSklasyfikowane++;
                                        }
                                        else
                                        {
                                            // Console.WriteLine("Decyzja 0 jest zle przypisana");
                                        }

                                        sklasyfikowane++;
                                    }
                                    else
                                    {
                                        Console.WriteLine(":(");

                                        // brak decyzji - nie klasyfikujemy

                                        // Console.ReadLine();
                                    }
                                }
                            }

                            Console.WriteLine();

                            // Policz Global Accuracy
                            decimal global_acc = liczbaObiektowPoprawnieSklasyfikowanych /
                                                 (decimal)liczbaObiektowSklasyfikowanych;

                            decimal komitet_global = 0;

                            if (g > 1)
                            {
                                komitet_global = poprawnieSklasyfikowane / (decimal)sklasyfikowane;
                            }

                            if (g > 1)
                            {
                                writetofile_globalAcc.WriteLine(String.Format("{0:0.0000}", global_acc));
                                writetofile_comitee.WriteLine(String.Format("{0:0.0000}", komitet_global));
                            }
                            else
                            {
                                writetofile_globalAcc.WriteLine(String.Format("{0:0.0000}", global_acc));
                            }

                            eff_global_acc += global_acc;

                            decimal poprawnieSklasyfikowaneKlasa0 = 0;

                            // Policz Balanced Accuracy
                            if (liczbaObiektowSklasyfikowanych_klasa0 != 0)
                            {
                                poprawnieSklasyfikowaneKlasa0 =
                                    liczbaObiektowPoprawnieSklasyfikowanych_klasa0 /
                                    (decimal)liczbaObiektowSklasyfikowanych_klasa0;
                            }

                            decimal poprawnieSklasyfikowaneKlasa1 = 0;

                            if (liczbaObiektowSklasyfikowanych_klasa1 != 0)
                            {
                                poprawnieSklasyfikowaneKlasa1 =
                                    liczbaObiektowPoprawnieSklasyfikowanych_klasa1 /
                                    (decimal)liczbaObiektowSklasyfikowanych_klasa1;
                            }

                            decimal balanced_acc = (poprawnieSklasyfikowaneKlasa0 + poprawnieSklasyfikowaneKlasa1) /
                                                   liczbaKlas;

                            eff_balanced_acc += balanced_acc;

                            decimal globalCoverage = liczbaObiektowSklasyfikowanych / (decimal)wczytany_Testowy.Length;

                            eff_global_cov += globalCoverage;

                            decimal TPR_klasy0 = 0;
                            decimal TPR_klasy1 = 0;

                            if (liczbaObiektowPoprawnieSklasyfikowanych_klasa0 +
                                liczbaObiektowBlednieSklasyfikowanych_klasa0 != 0)
                            {
                                TPR_klasy0 =
                                    liczbaObiektowPoprawnieSklasyfikowanych_klasa0 /
                                    (decimal)(liczbaObiektowPoprawnieSklasyfikowanych_klasa0 +
                                               liczbaObiektowBlednieSklasyfikowanych_klasa0);
                            }

                            if (liczbaObiektowPoprawnieSklasyfikowanych_klasa1 +
                                liczbaObiektowBlednieSklasyfikowanych_klasa1 != 0)
                            {
                                TPR_klasy1 =
                                    liczbaObiektowPoprawnieSklasyfikowanych_klasa1 /
                                    (decimal)(liczbaObiektowPoprawnieSklasyfikowanych_klasa1 +
                                               liczbaObiektowBlednieSklasyfikowanych_klasa1);
                            }

                            eff_tpr0 += TPR_klasy0;
                            eff_tpr1 += TPR_klasy1;

                            int TP = liczbaObiektowPoprawnieSklasyfikowanych_klasa0;
                            int FP = liczbaObiektowBlednieSklasyfikowanych_klasa1;
                            int FN = liczbaObiektowBlednieSklasyfikowanych_klasa0;
                            int TN = liczbaObiektowPoprawnieSklasyfikowanych_klasa1;

                            decimal Sensitivity = 0;

                            if (TP != 0 || FN != 0)
                            {
                                Sensitivity = TP / (decimal)(TP + FN);
                            }

                            decimal Specificity = 0;

                            if (TN != 0 || FP != 0)
                            {
                                Specificity = TN / (decimal)(TN + FP);
                            }

                            decimal youdenIndex = Sensitivity + Specificity - 1;

                            eff_youdenIndex += youdenIndex;
                        }
                    }
                }

                using (StreamWriter writetofile = new StreamWriter($"average{p}%.txt"))
                {
                    writetofile.WriteLine("Average Effectiveness");
                    writetofile.WriteLine("Global Accuracy => " + String.Format("{0:0.00}", eff_global_acc / 50));
                    writetofile.WriteLine("Global Coverage => " + String.Format("{0:0.00}", eff_global_cov / 50));
                    writetofile.WriteLine($"TPR{klasa0} => " + String.Format("{0:0.00}", eff_tpr0 / 50));
                    writetofile.WriteLine($"TPR{klasa1} => " + String.Format("{0:0.00}", eff_tpr1 / 50));
                    writetofile.WriteLine("Youden index => " + String.Format("{0:0.00}", eff_youdenIndex / 50));
                }

                Console.WriteLine("\n\n\nAverage Effectiveness \n");
                Console.WriteLine("Global Accuracy = " + String.Format("{0:0.00}", eff_global_acc / 50));
                Console.WriteLine("Global Coverage = " + String.Format("{0:0.00}", eff_global_cov / 50));
                Console.WriteLine("TPR0 = " + String.Format("{0:0.00}", eff_tpr0 / 50));
                Console.WriteLine("TPR1 = " + String.Format("{0:0.00}", eff_tpr1 / 50));
                Console.WriteLine("Youden index = " + String.Format("{0:0.00}", eff_youdenIndex / 50));

                // RESET
                eff_global_acc = 0;
                eff_global_cov = 0;
                eff_tpr0 = 0;
                eff_tpr1 = 0;
                eff_youdenIndex = 0;
            }

            Console.WriteLine("Finished.. Output data is available at /bin/Debug dir.");

            Console.ReadKey();
        }
    }
}
