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
    const string FileName = "Kakuro10x9.csv";
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

        int rowCounter = 0;
        int columnCounter = 1;

        // Pārbauda faila pieejamību
        string textFile = new string(@"../../../" + FileName);
        if (!File.Exists(textFile))
        {
            Console.WriteLine("T-01a. Nevar atrast failu ar nosaukumu \"{0}\"", FileName);
            return;
        }
        Console.WriteLine("T-01b. Fails ar nosaukumu \"{0}\" ir atrasts un tiek apstrādāts\n", FileName);

        string[] lines = File.ReadAllLines(textFile);

        foreach (string line in lines)
        {
            Console.WriteLine(line);
            rowCounter++;
        }

        foreach (char c in lines[0])
            if (c == ListSeparator) columnCounter++; //svarīgi windows uzstādījumos (Control Panel - Region - Advanced - List Separator)
        Console.Write("T-02. Mīklas izmēri ir atrasti, un tie ir {0}x{1}\n", rowCounter, columnCounter);
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
        Console.WriteLine("T-03a. Horizontālās summas nolasītas.\n");
        for (int i = 0; i < rowCounter; i++)
        {
            for (int j = 0; j < columnCounter; j++)
            {
                Console.Write(verticalSumMatrix[i, j] + " ");
            }
            Console.WriteLine();
        }
        Console.WriteLine("T-03b. Vertikālās summas nolasītas.\n");
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
                if (stringMatrix[i][j] == "")
                {
                    int i_var = i;
                    int j_var = j;
                    while ((verticalSumMatrix[i_var, j_var] == 0) && (stringMatrix[i_var][j_var] == ""))
                    {
                        i_var--;
                    }
                    if (verticalSumMatrix[i_var, j_var] != 0)
                        CellVerticalSumLink[i, j] = verticalSumMatrix[i_var, j_var];
                    //tagad 2D masīvā CellVerticalSumLink katrās baltās rūtiņas koordinātās glabājās tās vertikālā summa.
                }
            }
        }
        //glabās summai piederošai rūtiņu skaitu sektorā (2 līdz 9). 
        //Saraksta izmērs vienāds ar summu saraksta izmēru.
        List<int> sectorCellCountList = new List<int>();
        //horizontālie sektori
        for (int i = 0; i < rowCounter; i++)
        {
            for (int j = 0; j < columnCounter; j++)
            {
                //atrod pirmo (virzienā pa labi) balto rūtiņu, kurai pieder noteiktā horizontālā summa 
                if (CellHorizontalSumLink[i, j] > 0)
                {
                    int cellCount = 0;
                    int sumLink = CellHorizontalSumLink[i, j];
                    bool doBreak = false;
                    //iziet cauri sektoram, kamēr nav sastapti sektoram nepiederoša rūtiņa vai matricas mala.
                    //skaita sektora garumu
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
        //vertikālie sektori
        for (int j = 0; j < columnCounter; j++)
        {
            for (int i = 0; i < rowCounter; i++)
            {
                //atrod pirmo (virzienā no augšas uz leju) balto rūtiņu, kurai pieder noteiktā vertikālā summa
                if (CellVerticalSumLink[i, j] > 0)
                {
                    int cellCount = 0;
                    int sumLink = CellVerticalSumLink[i, j];
                    bool doBreak = false;
                    //iziet cauri sektoram, kamēr nav sastapti sektoram nepiederoša rūtiņa vai matricas mala.
                    //skaita sektora garumu
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
        //vertikālās koordinātas
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
        Console.WriteLine("Failā ir {0} horizontālas un {1} vertikālas summas", horizontalSumCount, verticalSumCount);
        //Tā kā katr
        Console.WriteLine("T-05. Kopā ir {0} baltas rūtiņas*.\n", WhiteCellCoordinatesInOrder.Count / 2);
        //pārbauda balto rūtiņu piesaisti pie summas
        for (int i = 0; i < rowCounter; i++)
        {
            for (int j = 0; j < columnCounter; j++)
            {
                Console.Write(CellHorizontalSumLink[i, j] + " ");
            }
            Console.WriteLine();
        }
        Console.WriteLine("T-06a. Balto rūtiņu piesaiste pie horizontālās summas ir veiksmīga.\n");
        for (int i = 0; i < rowCounter; i++)
        {
            for (int j = 0; j < columnCounter; j++)
            {
                Console.Write(CellVerticalSumLink[i, j] + " ");
            }
            Console.WriteLine();
        }
        Console.WriteLine("T-06b. Balto rūtiņu piesaiste pie vertikālās summas ir veiksmīga.\n");

        foreach (int sector in sectorCellCountList)
        {
            Console.Write(sector + " ");
        }
        Console.WriteLine();
        Console.WriteLine("T-07. Sektoru garumi ir saskaitīti veiksmīgi.\n");

        int sumCount = sumList.Count;
        //datu formatēšana
        int[][] problem_autoFill = new int[sumCount][];
        int coordinatesWalker = 0; //izstaigās visas balto rūtiņu koordinātes (glabāts lineāri WhiteCellCoordinatesInOrder) un kopēs tos vajadzīgajā sektorā problēmā 
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
        Console.WriteLine("T-08a. Ievadītas {0} horizontālās summas ar sektora rūtiņu koordinātām\n", horizontalSumCount);
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
        Console.WriteLine("T-08b. Ievadītas {0} vertikālās summas ar sektora rūtiņu koordinātām\n", verticalSumCount);
        //visi tukšumi -> summas un melnās rūtiņas
        int blanksCount = 0;
        for (int i = 1; i < rowCounter; i++)
        {
            for (int j = 1; j < columnCounter; j++)
            {
                if ((stringMatrix[i][j] == BlankSymbol) || (stringMatrix[i][j].Contains(@"\")))
                    blanksCount++;
            }
        }
        int[,] blanksCoordinatesArray = new int[blanksCount, 2];
        int blankRows = 0;
        int blankCols = 0;
        for (int i = 1; i < rowCounter; i++)
        {
            for (int j = 1; j < columnCounter; j++)
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
        //pārbauda, vai tukšumi (melnās un summu rūtiņas) ir atrasti
        for (int i = 0; i < blanksCount; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                Console.Write(blanksCoordinatesArray[i, j] + " ");
            }
            Console.WriteLine();
        }
        Console.WriteLine("T-09. Tukšumi ir nolasīti\n");

        int count_sectors = problem_autoFill.GetLength(0);

        int count_blanks = blanksCoordinatesArray.GetLength(0);

        /************** RISINĀJUMA KODS: **************/
        
        /*
         * Risinājuma plāns ir ņemts no OR-Tools instrukcijas priekš ierobežojumu programmēšanas
         * https://developers.google.com/optimization/cp/cp_solver
         */

        /** Variables **/

        //matricai izņem 0-to rindu un 0-to kolonnu, jo tajās pēc Kakuro noteikumiem nevar būt baltas rūtiņas 
        IntVar[,] matrix = solver.MakeIntVarMatrix(rowCounter - 1, columnCounter - 1, 0, 9, "matrix");
        IntVar[] matrix_flat = matrix.Flatten();

        /**Constraints**/

        // ieliek tukšumos 0
        for (int i = 0; i < count_blanks; i++)
        {
            //solver.Add(matrix[blanks[i, 0] - 1, blanks[i, 1] - 1] == 0);
            solver.Add(matrix[blanksCoordinatesArray[i, 0] - 1, blanksCoordinatesArray[i, 1] - 1] == 0);
        }

        //Katram no sektoriem ir sekojošie nosacījumi
        for (int i = 0; i < count_sectors; i++)
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
        //ieraksta laiku, kurā atrod un izdrukā risinājumu(s)
        var watch = new System.Diagnostics.Stopwatch();
        watch.Start();

        /**Solver.Search**/

        DecisionBuilder db = solver.MakePhase(matrix_flat, Solver.CHOOSE_FIRST_UNBOUND, Solver.ASSIGN_MIN_VALUE);
        solver.NewSearch(db);

        /** Output **/

        while (solver.NextSolution())
        {
            for (int i = 0; i < rowCounter - 1; i++)
            {
                for (int j = 0; j < columnCounter - 1; j++)
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
                Console.WriteLine(); //pēc katra i, jaunā rinda matricā
            }
            Console.WriteLine(); //pēc katra Solution
        }
        Console.WriteLine();

        //Risinājuma statistika
        Console.WriteLine("Iespējamo risinājumu: {0}", solver.Solutions());
        watch.Stop();
        Console.WriteLine("Risinājuma meklēšanas un attēlošanas laiks: {0} ms", watch.ElapsedMilliseconds);
        Console.WriteLine("Kopējais koda laiks, ieskaitot vienībtestēšanu: {0} ms", solver.WallTime());
        Console.WriteLine("Strupceļu skaits risinājuma kokā: {0}", solver.Failures());
        Console.WriteLine("Zaru skaits risinājuma kokā: {0} ", solver.Branches());

        solver.EndSearch(); //svarīgi neaizmirst
        if (solver.Solutions() == 0)
            Console.WriteLine("\nDiemžēl nav atrasta neviena risinājuma!");
    }

    public static void Main(String[] args)
    {
        Console.WriteLine("Programmai ir ");
        Solve();
    }
}