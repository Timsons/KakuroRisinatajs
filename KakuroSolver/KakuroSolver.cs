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
 * Versija v1.2. Pilnveidota strādājoša saite ar .csv failu. paliek savienot ar esošo algoritmu-piemēru.
 * Datums: 22.12.2020.
 * 
 * Versija v2.0. Pilnībā strādājošs minimālais produkts ar jebkuru Kakuro piemēru. Gatavs aizstāvēšanai
 * Datums: 31.12.2020.
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
 * Projekts ir izveidots iekš OR-Tools instalētas programmatūras direktorijas, lai vienkāršāka pieeja funkcijām.
 * 
 * Palaist projektu var, nospiežot uz zaļo trijstūru augšā. Programma nolasīs un atrisinās zemāk minētu failu .csv formātā
*/
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Google.OrTools.ConstraintSolver;


public class Kakuro
{

    const char ListSeparator = ',';
    const string FileName = "Kakuro29x29.csv"; //29x29 neatrisināja pa 839 minūtēm, 14x14Mul vairāki risinājumi
    const string BlankSymbol = "x";

    
    /*
     * Funkcija izveido ierobežojumus
     */
    public static void calc(Solver solver, //programma OR-Tools ar iepriekš definētiem metodiem
                             int[] cellCoordinateArray, //masīvs ar to rūtiņu koordinātēm, kas pieder vienam segmentam (2+ rūtiņas horizontāli vai vertikāli)
                             IntVar[,] matrix, //OR-Tools šablona matrica
                             int result) //mīklā dota summa
    {
        // izveido ierobežojumu, lai visas vērtības ir pozitīvas
        int len = cellCoordinateArray.Length / 2;
        for (int i = 0; i < len; i++)
        {
            solver.Add(matrix[cellCoordinateArray[i * 2] - 1, cellCoordinateArray[i * 2 + 1] - 1] >= 1);
        }

        // izveido ierobežojumu, lai skaitļu summa ir vienāda ar doto mīklā
        solver.Add((from i in Enumerable.Range(0, len)
                    select matrix[cellCoordinateArray[i * 2] - 1, cellCoordinateArray[i * 2 + 1] - 1])
                    .ToArray().Sum() == result);

        // izveido ierobežojumu, lai visi cipari ir dažādi
        solver.Add((from j in Enumerable.Range(0, len)
                    select matrix[cellCoordinateArray[j * 2] - 1, cellCoordinateArray[j * 2 + 1] - 1])
                    .ToArray().AllDifferent());
    }

    private static void Solve()
    {
        

        Solver solver = new Solver("Kakuro"); //Deklarē klasi, kurā ietilpst OR-Tools metodes
        /************* .CSV INTEGRĀCIJA *************/

        //Console.WriteLine("Enter file name:");
        // string fileName = Console.ReadLine();

        int rowCounter = 0;
        int columnCounter = 1;

        // Read the file and display it line by line.  
        string textFile = new string(@"../../../" + FileName);
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
        int horizontalSumCount = 0;
        int verticalSumCount = 0;
        for (int i = 0; i < rowCounter; i++)
        {
            for (int j = 0; j < columnCounter; j++)
            {
                int position = stringMatrix[i][j].IndexOf(@"\");
                if (position >= 0)
                {
                    string HorSubStr_ij = stringMatrix[i][j].Substring(position + 1);
                    if (HorSubStr_ij.Length != 0)
                        horizontalSumMatrix[i, j] = int.Parse(HorSubStr_ij);
                    string VerSubStr_ij = stringMatrix[i][j].Substring(0, position);
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
        Console.WriteLine("Horizontālās summas OK\n");
        for (int i = 0; i < rowCounter; i++)
        {
            for (int j = 0; j < columnCounter; j++)
            {
                Console.Write(verticalSumMatrix[i, j] + " ");
            }
            Console.WriteLine();
        }
        Console.WriteLine("Vertikālās summas OK\n");
        //ieraksta summas vienā sarakstā, secībā H,V
        List<int> sumList = new List<int>();
        for (int i = 0; i < rowCounter; i++)
        {
            for (int j = 0; j < columnCounter; j++)
            {
                if (horizontalSumMatrix[i, j] > 0)
                {
                    sumList.Add(horizontalSumMatrix[i, j]);
                    horizontalSumCount++;
                }
            }
        }
        for (int j = 0; j < columnCounter; j++) 
        {
            for (int i = 0; i < rowCounter; i++)
            {
                if (verticalSumMatrix[i, j] > 0)
                {
                    sumList.Add(verticalSumMatrix[i, j]);
                    verticalSumCount++;
                }
            }
        }
        //pārbauda, vai saraksts ir pareizs
        Console.WriteLine("Summas sarakstā: ");
        foreach (int sum in sumList)
        {
            Console.Write(sum + " ");
        }
        Console.WriteLine();

        //izņem visas balto rūtiņu koordinātas
        int[,] CellHorizontalSumLink = new int[rowCounter, columnCounter];
        int[,] CellVerticalSumLink = new int[rowCounter, columnCounter];
        for (int i = 0; i < rowCounter; i++)
        {
            for (int j = 0; j < columnCounter; j++)
            {
                if (stringMatrix[i][j] == "") //ja ir balta
                {
                    //mainīgie, kurus varēs mainīt
                    int i_var = i;
                    int j_var = j;
                    while ((horizontalSumMatrix[i_var, j_var] == 0) && (stringMatrix[i_var][j_var] == ""))
                    {
                        j_var--;
                    }
                    if (horizontalSumMatrix[i_var, j_var] != 0)
                        CellHorizontalSumLink[i, j] = horizontalSumMatrix[i_var, j_var];
                    //tagad 2D masīvā CellHorizontalSumLink katrās baltās rūtiņas koordinātās glabājās tās horizontālā summa.
                }
            }
        }
        for (int i = 0; i < rowCounter; i++)
        {
            for (int j = 0; j < columnCounter; j++)
            {
                if (stringMatrix[i][j] == "") //ja ir balta
                {
                    //mainīgie, kurus varēs mainīt
                    int i_var = i;
                    int j_var = j;
                    while ((verticalSumMatrix[i_var, j_var] == 0) && (stringMatrix[i_var][j_var] == ""))
                    {
                        i_var--;
                    }
                    if (verticalSumMatrix[i_var, j_var] != 0)
                        CellVerticalSumLink[i, j] = verticalSumMatrix[i_var, j_var];
                    //tagad 2D masīvā CellVerticalSumLink katrās baltās rūtiņas koordinātās glabājās tās horizontālā summa.
                }
            }
        }
        //glabās summai piederošai rūtiņu skaitu sektorā (2 līdz 9)
        List<int> sectorCellCountList = new List<int>();
        for (int i = 0; i < rowCounter; i++)
        {
            for (int j = 0; j < columnCounter; j++)
            {
                if (CellHorizontalSumLink[i, j] > 0)
                {
                    //mainīgie, kurus varēs mainīt
                    int cellCount = 0;
                    int sumLink = CellHorizontalSumLink[i, j];
                    
                    bool doBreak = false;
                    while (!doBreak)
                    {
                        if (sumLink == CellHorizontalSumLink[i, j])
                        {
                            cellCount++;
                            if (j + 1 < columnCounter)
                            {
                                j++;
                            }
                            else doBreak = true;
                        }
                        else 
                        { 
                            doBreak = true;
                            j--;
                        }
                    }
                    sectorCellCountList.Add(cellCount);
                }
            }
        }

        
        for (int j = 0; j < columnCounter; j++)
        {
            for (int i = 0; i < rowCounter; i++)
            {
                if (CellVerticalSumLink[i, j] > 0)
                {
                    //mainīgie, kurus varēs mainīt
                    //Console.WriteLine("We are at {0} column and found first white cell on {1} row", j, i);
                    int cellCount = 0;
                    int sumLink = CellVerticalSumLink[i, j];

                    bool doBreak = false;
                    while (!doBreak)
                    {
                        if (sumLink == CellVerticalSumLink[i, j])
                        {
                            cellCount++;
                            if (i + 1 < rowCounter)
                            {
                                i++;
                            }
                            else doBreak = true;
                        }
                        else
                        {
                            doBreak = true;
                            i--;
                        }
                    }
                    //Console.WriteLine("We are at {0} column and end on {1} row", j, i);
                    sectorCellCountList.Add(cellCount);
                }
            }
        }
        List<int> WhiteCellCoordinatesInOrder = new List<int>();
        //pievieno sarakstam tikai visas baltas koordinātas secībā. Šeit tikai horizontālās, pēc tam vertikālās
        for (int i = 0; i < rowCounter; i++)
        {
            for (int j = 0; j < columnCounter; j++)
            {
                if (CellHorizontalSumLink[i, j] != 0)
                {
                    WhiteCellCoordinatesInOrder.Add(i);
                    WhiteCellCoordinatesInOrder.Add(j);
                }
            }
        }
        for (int j = 0; j < columnCounter; j++)
        {
            for (int i = 0; i < rowCounter; i++)
            {
                if (CellVerticalSumLink[i, j] != 0)
                {
                    WhiteCellCoordinatesInOrder.Add(i);
                    WhiteCellCoordinatesInOrder.Add(j);
                }

            }
        }
        Console.WriteLine();
        Console.WriteLine("Failā {0} rindas un {1} kolonnas, {2} horizontālas un {3} vertikālas summas", rowCounter, columnCounter, horizontalSumCount, verticalSumCount);
        Console.WriteLine("Kopā ir {0} baltas koordinātas\n", WhiteCellCoordinatesInOrder.Count);
        //pārbauda balto rūtiņu piesaisti pie summas
        for (int i = 0; i < rowCounter; i++)
        {
            for (int j = 0; j < columnCounter; j++)
            {
                Console.Write(CellHorizontalSumLink[i, j] + " ");
            }
            Console.WriteLine();
        }
        Console.WriteLine("horizontalSumLink OK\n");
        for (int i = 0; i < rowCounter; i++)
        {
            for (int j = 0; j < columnCounter; j++)
            {
                Console.Write(CellVerticalSumLink[i, j] + " ");
            }
            Console.WriteLine();
        }
        Console.WriteLine("verticalSumLink OK\n");

        foreach (int sector in sectorCellCountList)
        {
            Console.Write(sector + " ");
        }
        Console.WriteLine();
        Console.WriteLine("sectors OK\n"); 

        int sumCount = sumList.Count;
        int[][] problem_autoFill = new int[sumCount][];
        int coordinatesWalker = 0;
        Console.WriteLine("inicializēts masīvs ar problēmu OK\n");
        //visas horizontālās rindas problēmā. šajā sintaksē ir vienkāršākais veids pielagoties OR-Tools metodēm, nododot datus. 
        for (int i = 0; i < horizontalSumCount; i++)
        {
            problem_autoFill[i] = new int[2 * sectorCellCountList[i] + 1];
            problem_autoFill[i][0] = sumList[i];
            for (int j = 1; j < problem_autoFill[i].Length; j++)
            {
                problem_autoFill[i][j] = WhiteCellCoordinatesInOrder[coordinatesWalker];
                coordinatesWalker++;
            }
        }
        for (int i = 0; i < horizontalSumCount; i++)
        {
            for (int j = 0; j < problem_autoFill[i].GetLength(0); j++)
            {
                Console.Write("{0} ", problem_autoFill[i][j]);
            }
            Console.WriteLine();
        }
        Console.WriteLine("ievadītas {0} horizontālas summas ar koordinātēm OK\n", horizontalSumCount);
        //visas vertikālas rindas problēmā
        for (int i = horizontalSumCount; i < sumCount; i++)
        {
            problem_autoFill[i] = new int[2 * sectorCellCountList[i] + 1];
            problem_autoFill[i][0] = sumList[i];
            for (int j = 1; j < problem_autoFill[i].Length; j++)
            {
                problem_autoFill[i][j] = WhiteCellCoordinatesInOrder[coordinatesWalker];
                coordinatesWalker++;
            }
        }
        for (int i = horizontalSumCount; i < sumCount; i++)
        {
            for (int j = 0; j < problem_autoFill[i].GetLength(0); j++)
            {
                Console.Write("{0} ", problem_autoFill[i][j]);
            }
            Console.WriteLine();
        }
        Console.WriteLine("ievadītas {0} vertikālās summas ar koordinātēm OK\n", verticalSumCount);
        Console.WriteLine("Aizpildīts masīvs ar problēmu OK\n");
        //visi tukšumi
        int blanksCount = 0;
        for (int i = 1; i < rowCounter; i++)
        {
            for (int j = 1; j < columnCounter; j++)
            {
                if ((stringMatrix[i][j] == BlankSymbol)||(stringMatrix[i][j].Contains(@"\")))
                    blanksCount++;
            }
        }
        int[,] blanksCoordinatesArray = new int[blanksCount,2];
        int blankRows = 0;
        int blankCols = 0;
        for (int i = 1; i < rowCounter; i++)
        {
            for (int j =1 ; j < columnCounter; j++)
            {
                if ((stringMatrix[i][j] == BlankSymbol) || (stringMatrix[i][j].Contains(@"\")))
                {
                    blanksCoordinatesArray[blankRows, blankCols] = i;
                    blankCols++;
                    blanksCoordinatesArray[blankRows, blankCols] = j;
                    blankCols--;
                    blankRows++;
                }
            }
        }
        Console.WriteLine("Blanks:");
        for (int i = 0; i < blanksCount; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                Console.Write(blanksCoordinatesArray[i, j]+" ");
            }
            Console.WriteLine();
        }
        Console.WriteLine("Blanks OK\n");
        
        for (int i =0;i< problem_autoFill.GetLength(0);i++)
        {
            for (int j = 0; j < problem_autoFill[i].GetLength(0); j++)
            {
                Console.Write("{0} ",problem_autoFill[i][j]);
            }
            Console.WriteLine();
        }

        Console.WriteLine("ProblemRead OK\n");
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


        //int count_segm = problem.GetLength(0); // Segmentu skaits, jeb summu skaits
        int count_segm = problem_autoFill.GetLength(0);

        // Tukšumi, tai skaitā arī summas rūtiņās.
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

        //int count_blanks = blanks.GetLength(0);
        int count_blanks = blanksCoordinatesArray.GetLength(0);

        /************** RISINĀJUMA KODS: **************/
        /*
         * Decision variables
         */
        IntVar[,] matrix = solver.MakeIntVarMatrix(rowCounter-1, columnCounter-1, 0, 9, "matrix");
        IntVar[] matrix_flat = matrix.Flatten();

        /*
         * Constraints
         */

        // ieliek tukšumos 0
        for (int i = 0; i < count_blanks; i++)
        {
            //solver.Add(matrix[blanks[i, 0] - 1, blanks[i, 1] - 1] == 0);
            solver.Add(matrix[blanksCoordinatesArray[i, 0] - 1, blanksCoordinatesArray[i, 1] - 1] == 0);
        }

        //Katram no sektoriem ir sekojošie nosacījumi
        for (int i = 0; i < count_segm; i++)
        {
            // Katrs sektors ir viena daļa no problēmas 2D masīva
            int[] sector = problem_autoFill[i];

            // s2 glābā sektora garumu rūtiņās x2, jo katrs elements ir koordināte -> katrai rūtiņai ir 2 skaitļi
            // katrs 2n-1 elements ir rindas numurs, katrs 2n elements ir kolonnas numurs
            int[] s2 = new int[sector.Length - 1];
            for (int j = 1; j < sector.Length; j++)
            {
                s2[j - 1] = sector[j];
            }
            // s2 ir 'sector' bez summas

            // funkcija izveido kakuro constraint'us
            calc(solver, s2, matrix, sector[0]);
        }
        var watch = new System.Diagnostics.Stopwatch();
        watch.Start();
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
            for (int i = 0; i < rowCounter-1; i++)
            {
                for (int j = 0; j < columnCounter-1; j++)
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
        watch.Stop();
        Console.WriteLine("Risinājuma meklēšanas un attēlošanas laiks: {0} ms", watch.ElapsedMilliseconds);
        Console.WriteLine("Kopējais koda laiks, ieskaitot vienībtestēšanu: {0}ms", solver.WallTime());
        Console.WriteLine("Kļūdas risinājuma meklēšanā: {0}", solver.Failures());
        Console.WriteLine("Zaru skaits risinājuma kokā: {0} ", solver.Branches());
        
        solver.EndSearch(); //svarīgi neaizmirst
        
    }

    public static void Main(String[] args)
    {
        Solve();
    }
}