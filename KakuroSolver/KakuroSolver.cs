/*
 * Kakuro risinātājs ar OR-Tools palīdzību.
 * Autors: Timurs Sņetkovs ts18028
 * 
 * Versija v1.0. Minimālais produkts. Uzrakstīts risinātājs ar OR-Tools metožu palīdzību vienai dotai mīklai. Pielietota instrukcija vietnēs saistībā ar OR-Tools
 * Datums: 10.11.2020.
 * 
 * Versija v1.1. .csv faila integrēšana - mīklas matrica glabājās programmā, summas glabājās atsevišķi, tās ir sakartotas secībā visas horizontālās -> visas vertikālās.
 * Datums: 02.12.2020.
 * 
 * Par Kakuro (angliski): https://en.wikipedia.org/wiki/Kakuro
 * 
 * Par OR-Tools: 
 * OR-Tools ir kompānijas Google izstrādāts atvērtā pirmkoda programmatūras komplekts, paredzēts optimizēšanai. 
 * Tas ir noregulēts, lai risinātu pasaules sarežģītākas problēmas transportlīdzekļu maršrutēšanā (vehicle routing), 
 * plānošanā (scheduling), sakārtošana konteinerā (bin packing), plūsmās (flows), grafos (graphs), 
 * lineārajā optimizācija (linear and mixed-integer programming) un ierobežojumu optimizācijā (constraint optimization).
 * 
 * Piedāvātais kakuro mīklas risinājums izmanto Ierobežojumu Optimizāciju - mīklu risinot, ir noteikumi, kas ierobežo 
 * iespējamo risinājumu kopu gan pēc vesela saprata, gan pēc loģiskiem secinājumiem. Pēdējo var salīdzināt ar cilvēcisko
 * stratēģiju, risinot mīklu.
 * 
 * Projekts ir izveidots iekš OR-Tools instalētas programmatūras direktorijas, lai vienkāršāka pieeja funkcijām
*/
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Google.OrTools.ConstraintSolver;


public class Kakuro
{
    /*
     * Funkcija izveido dažus constraint'us,
     */
    public static void calc(Solver solver, //programma OR-Tools ar iepriekš definētiem metodiem
                             int[] cellCoordinateArray, //masīvs ar to rūtiņu koordinātēm, kas pieder vienam segmentam (2+ rūtiņas horizontāli vai vertikāli)
                             IntVar[,] matrix, //OR-Tools šablona matrica
                             int result) //mīklā dota summa
    {
        // izveido constraint, lai visas vērtības ir pozitīvas
        int len = cellCoordinateArray.Length / 2;
        for (int i = 0; i < len; i++)
        {
            solver.Add(matrix[cellCoordinateArray[i * 2] - 1, cellCoordinateArray[i * 2 + 1] - 1] >= 1);
        }

        // izveido constraint, lai skaitļu summa ir vienāda ar doto mīklā
        solver.Add((from i in Enumerable.Range(0, len)
                    select matrix[cellCoordinateArray[i * 2] - 1, cellCoordinateArray[i * 2 + 1] - 1])
                    .ToArray().Sum() == result);

        // izveido constraint, lai visi cipari ir dažādi
        solver.Add((from j in Enumerable.Range(0, len)
                    select matrix[cellCoordinateArray[j * 2] - 1, cellCoordinateArray[j * 2 + 1] - 1])
                    .ToArray().AllDifferent());
    }

    private static void Solve()
    {
        const char ListSeparator = ',';

        Solver solver = new Solver("Kakuro"); //Deklarē klasi, kurā ietilpst OR-Tools metodes
        /************* .CSV INTEGRĀCIJA *************/

        //Console.WriteLine("Enter file name:");
        // string fileName = Console.ReadLine();

        int rowCounter = 0;
        int columnCounter = 1;

        // Read the file and display it line by line.  
        string textFile =
            new string(@"../../../csvFile.csv");// + fileName);
        string[] lines = File.ReadAllLines(textFile);
        //if (File.Exists(textFile))
        //{
        foreach (string line in lines)
        {
            Console.WriteLine(line);
            rowCounter++;
        }

        foreach (char c in lines[0])
            if (c == ListSeparator) columnCounter++; //svarīgi windows uzstādījumos (Control Panel - Region - Advanced - List Separator)
        //}
        //izveido teksta matricu, kur glabāsies .csv dati
        string[][] stringMatrix = new string[rowCounter][];
        for (int i = 0; i < stringMatrix.GetLength(0); i++)
        {
            stringMatrix[i] = new string[columnCounter];
        }
        Console.WriteLine();

        //ievada .csv iekš matricas
        for (int i = 0; i < rowCounter; i++)
        {
            string[] stringLine = lines[i].Split(",");
            for (int j = 0; j < columnCounter; j++)
            {
                stringMatrix[i][j] = stringLine[j];
            }
        }

        //horizontālo summu matrica, lai vēlāk kārtot summas H-V secībā, pa ceļam uzzinot koordinātas
        int[,] horizontalSumMatrix = new int[rowCounter, columnCounter];
        int[,] verticalSumMatrix = new int[rowCounter, columnCounter];
        for (int i = 0; i < rowCounter; i++)
        {
            for (int j = 0; j < columnCounter; j++)
            {
                int position = stringMatrix[i][j].IndexOf(@"\");
                if (position >= 0)
                {
                    string HorSubStr_ij = stringMatrix[i][j].Substring(0, position);
                    if (HorSubStr_ij.Length != 0)
                        horizontalSumMatrix[i, j] = int.Parse(HorSubStr_ij);
                    string VerSubStr_ij = stringMatrix[i][j].Substring(position+1);
                    if (VerSubStr_ij.Length != 0)
                        verticalSumMatrix[i, j] = int.Parse(VerSubStr_ij);
                }
            }

        }
        
        //izraksta visas horizontālas summas un tad visas vertikālas
        for (int i = 0; i < rowCounter; i++)
        {
            for (int j = 0; j < columnCounter; j++)
            {
                Console.Write(horizontalSumMatrix[i, j] + " ");
            }
            Console.WriteLine();//matricas nākamā rinda
        }
        Console.WriteLine();
        for (int i = 0; i < rowCounter; i++)
        {
            for (int j = 0; j < columnCounter; j++)
            {
                Console.Write(verticalSumMatrix[i,j] + " ");
            }
            Console.WriteLine();//matricas nākamā rinda
        }
        //ieraksta summas vienā sarakstā, secībā H,V
        List<int> sumList= new List<int>();
        for (int i = 0; i < rowCounter; i++)
        {
            for (int j = 0; j < columnCounter; j++)
            {
                if(horizontalSumMatrix[i, j]>0)
                    sumList.Add(horizontalSumMatrix[i, j]);
            }
        }
        for (int i = 0; i < rowCounter; i++)
        {
            for (int j = 0; j < columnCounter; j++)
            {
                if (verticalSumMatrix[i, j] > 0)
                    sumList.Add(verticalSumMatrix[i, j]);
            }
        }
        //pārbauda, vai saraksts ir pareizs
        Console.WriteLine("Summas sarakstā: ");
        foreach (int sum in sumList)
        {
            Console.Write(sum+" ");
        }



        Console.WriteLine();
        Console.WriteLine("Failā {0} rindas un {1} kolonnas.", rowCounter, columnCounter);
        Console.WriteLine();
        /*
        for (int i = 0; i < rowCounter; i++)
        {

            for (int j = 0; j < columnCounter; j++)
            {
                Console.Write(stringMatrix[i][j]);
            }
        }
        Console.WriteLine();
        */


        /************* DOTS LAUKS: *************/

        /*
         * Programma risina problēmu, kas ir aprakstīta
         * Wikipēdijas lapā. https://en.wikipedia.org/wiki/Kakuro
         * Šajā koda sadaļā kakuro piemērs - lauks - ievadīts manuāli
        */

        // matricas izmērs, neskaitot summas kreisajā kolonnā un augšajā rindā
        int n = 7; //rindas
        int m = 7; //kolonnas

        // segmenti:
        // formāts: {summa, segmentu koordinātas (rinda,kolonna)}
        // Sākot ar 1.
        int[][] problem =
        {
            new int[] {16,  1,1, 1,2},
            new int[] {24,  1,5, 1,6, 1,7},
            new int[] {17,  2,1, 2,2},
            new int[] {29,  2,4, 2,5, 2,6, 2,7},
            new int[] {35,  3,1, 3,2, 3,3, 3,4, 3,5},
            new int[] { 7,  4,2, 4,3},
            new int[] { 8,  4,5, 4,6},
            new int[] {16,  5,3, 5,4, 5,5, 5,6, 5,7},
            new int[] {21,  6,1, 6,2, 6,3, 6,4},
            new int[] { 5,  6,6, 6,7},
            new int[] { 6,  7,1, 7,2, 7,3},
            new int[] { 3,  7,6, 7,7},

            new int[] {23,  1,1, 2,1, 3,1},
            new int[] {30,  1,2, 2,2, 3,2, 4,2},
            new int[] {27,  1,5, 2,5, 3,5, 4,5, 5,5},
            new int[] {12,  1,6, 2,6},
            new int[] {16,  1,7, 2,7},
            new int[] {17,  2,4, 3,4},
            new int[] {15,  3,3, 4,3, 5,3, 6,3, 7,3},
            new int[] {12,  4,6, 5,6, 6,6, 7,6},
            new int[] { 7,  5,4, 6,4},
            new int[] { 7,  5,7, 6,7, 7,7},
            new int[] {11,  6,1, 7,1},
            new int[] {10,  6,2, 7,2}
        };


        int count_segm = problem.GetLength(0); // Segmentu skaits, jeb summu skaits

        // Tukšumi
        // Sākas ar 1
        int[,] blanks = {
            {1,3}, {1,4},
            {2,3},
            {3,6}, {3,7},
            {4,1}, {4,4}, {4,7},
            {5,1}, {5,2},
            {6,5},
            {7,4}, {7,5}
        };

        int count_blanks = blanks.GetLength(0);

        /************** RISINĀJUMA KODS: **************/
        /*
         * Decision variables
         */
        IntVar[,] matrix = solver.MakeIntVarMatrix(n, m, 0, 9, "matrix");
        IntVar[] matrix_flat = matrix.Flatten();

        /*
         * Constraints
         */

        // ieliek tukšumos 0
        for (int i = 0; i < count_blanks; i++)
        {
            solver.Add(matrix[blanks[i, 0] - 1, blanks[i, 1] - 1] == 0);
        }

        //Katram no segmentiem ir sekojošie nosacījumi
        for (int i = 0; i < count_segm; i++)
        {
            // Katrs segments ir viena daļa no problēmas 2D masīva
            int[] segment = problem[i];

            // s2 glābā segmenta garumu rūtiņās x2, jo katrs elements ir koordināte -> katrai rūtiņai ir 2 skaitļi
            // katrs 2n-1 elements ir rindas numurs, katrs 2n elements ir kolonnas numurs
            int[] s2 = new int[segment.Length - 1];
            for (int j = 1; j < segment.Length; j++)
            {
                s2[j - 1] = segment[j];
            }
            // s2 ir 'segment' bez summas

            // funkcija izveido kakuro constraint'us
            calc(solver, s2, matrix, segment[0]);
        }

        /*
         * Solver.Search
         */
        DecisionBuilder db = solver.MakePhase(matrix_flat,
                                              Solver.CHOOSE_FIRST_UNBOUND,
                                              Solver.ASSIGN_MIN_VALUE);

        solver.NewSearch(db);

        /*
         * Output
         */
        while (solver.NextSolution())
        {
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    int value = (int)matrix[i, j].Value();
                    if (value > 0)
                    {
                        Console.Write(value + " ");
                    }
                    else
                    {
                        Console.Write("  ");
                    }
                }
                Console.WriteLine(); //pēc katra i
            }
            Console.WriteLine(); //pēc katra Solution
        }
        Console.WriteLine();

        //Risinājuma statistika
        Console.WriteLine("Iespējamo risinājumu: {0}", solver.Solutions());
        Console.WriteLine("WallTime: {0}ms", solver.WallTime());
        Console.WriteLine("Failures: {0}", solver.Failures());
        Console.WriteLine("Branches: {0} ", solver.Branches());

        solver.EndSearch(); //svarīgi neaizmirst
    }

    public static void Main(String[] args)
    {
        Solve();
    }
}