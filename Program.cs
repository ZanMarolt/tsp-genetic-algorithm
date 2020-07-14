using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project3
{
    public class Point
    {
        public int Id { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public Point(int id, int x, int y)
        {
            this.Id = id;
            this.X = x;
            this.Y = y;
        }
    }
    public class Tour
    {
        public int[] PathPoints;

        public Tour(int numberOfPoints)
        {
            PathPoints = new int[numberOfPoints];
        }

        public void GenerateRandomPath() //at start application needs first random path - it has to be correct, so it's basically random without repeatations
        {
            bool[] isDrawn = new bool[PathPoints.Length];
            int randomPoint;

            for (int n = 0; n < PathPoints.Length;)
            {
                randomPoint = GlobalRandom.r.Next(0, PathPoints.Length);

                if (isDrawn[randomPoint] == false)
                {
                    isDrawn[randomPoint] = true;
                    PathPoints[n] = randomPoint;
                    n++;
                }
            }
        }

        public int CalculateFitness() //calculate path lenght - uses distances matrix (also includes back to hometown)
        {
            int sumDistancePath = 0;

            for (int n = 0; n < PathPoints.Length - 1; n++)
            {
                sumDistancePath += Distances.distancesArray[PathPoints[n], PathPoints[n + 1]];
            }
            sumDistancePath += Distances.distancesArray[PathPoints[PathPoints.Length - 1], PathPoints[0]];

            return sumDistancePath;
        }

        public void PrintPath() //method to print path when needed ex. when new, better path is found - written for console version of algorithm
        {
            foreach (int point in PathPoints)
            {
                Console.Write(point + "-");
            }
            Console.Write(PathPoints[0]);
            Console.Write(" Total length: " + CalculateFitness());
        }

        public void MutatePath() //inversion mutation - select 2 random genes - genes between them are inverted 
        {
            int gen1 = GlobalRandom.r.Next(0, PathPoints.Length);
            int gen2 = GlobalRandom.r.Next(0, PathPoints.Length);

            if (gen1 > gen2) //to be sure that gen1 <= gen2
            {
                int foo = gen2;
                gen2 = gen1;
                gen1 = foo;
            }

            int[] arr1 = new int[gen2 - gen1];

            for (int p = gen1, x = 0; p < gen2; p++, x++)
            {
                arr1[x] = PathPoints[p];
            }

            Array.Reverse(arr1); //inversion

            for (int p = gen1, x = 0; p < gen2; p++, x++)
            {
                PathPoints[p] = arr1[x];
            }
        }
    }
    static class FileReader
    {
        public static int[,] CreateDistanceMatrix(List<Point> points) //method that creates matrix with distances - good for optimalization, faster calculating during checking path length
        {
            int size = points.Count;
            int[,] distancesArray = new int[size, size];

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    distancesArray[i, j] = CalculateDistance(points[i], points[j]);
                }
            }
            return distancesArray;
        }

        public static int CalculateDistance(Point pointA, Point pointB) //calculates distances beetwen points to create distance matrix
        {
            double Distance = Math.Pow((pointA.X - pointB.X), 2) + Math.Pow((pointA.Y - pointB.Y), 2); //takes coordinations of 2 points (2D - X,Y)
            return (int)Math.Round(Math.Sqrt(Distance));
        }

        public static List<Point> ReadFile(string filePath) //read file
        {
            List<Point> PointList = new List<Point>();

            string[] lines = File.ReadAllLines(filePath);

            bool NODE_COORD_TYPE = false; //in next line after that phrase are coordinations (this is what is needed)

            foreach (string line in lines)
            {
                if (NODE_COORD_TYPE && line != "EOF")
                {
                    string[] row = line.Trim().Split(' ');
                    Point point = new Point(int.Parse(row[0]), int.Parse(row[1]), int.Parse(row[2])); //dont care about 1 integer in row - it's just ID which start from 0. Dont needed.
                    PointList.Add(point);
                }

                if (line == "NODE_COORD_SECTION")
                {
                    NODE_COORD_TYPE = true;
                }
            }
            return PointList;
        }
    }
        class Population
    {
        public Tour[] PopulationArray;
        int numberOfPaths;
        public int numberOfPoints;
        public static Random r = new Random();

        public Tour bestPathInPopulation;
        public int lengthofBestPath = int.MaxValue;

        public Population(int numberOfPaths, int numberOfPoints) //constructor of population - it creates population with random paths
        {
            PopulationArray = new Tour[numberOfPaths];
            for (int i = 0; i < numberOfPaths; ++i)
            {
                PopulationArray[i] = new Tour(numberOfPoints);
                PopulationArray[i].GenerateRandomPath();
            }

            this.numberOfPaths = numberOfPaths;
            this.numberOfPoints = numberOfPoints;
            bestPathInPopulation = new Tour(numberOfPoints);
        }


        public void PrintPopulation() //print whole population to console - for debugging purposes
        {
            foreach (Tour path in PopulationArray)
            {
                path.PrintPath();
                Console.WriteLine();
            }
            Console.WriteLine();
        }


        public void CrossoverPopulation(int crossoverChance) //crossover - user specify crossover chance - paths are crossovered or just copied to next generation
        {
            Tour[] newPopulationArray = new Tour[numberOfPaths];
            for (int i = 0; i < numberOfPaths; ++i) //new empty generation
            {
                newPopulationArray[i] = new Tour(numberOfPoints);
            }

            for (int k = 0; k < PopulationArray.Length; k++) //loop all paths
            {
                int individual1 = k;
                int individual2 = GlobalRandom.r.Next(0, numberOfPaths);

                if (GlobalRandom.r.Next(0, 100) < crossoverChance && !PopulationArray[individual1].PathPoints.SequenceEqual(PopulationArray[individual2].PathPoints))
                {

                    Array.Copy(CrossoverPaths(PopulationArray[individual1].PathPoints, PopulationArray[individual2].PathPoints), newPopulationArray[k].PathPoints, newPopulationArray[k].PathPoints.Length);
                }

                else
                {
                    Array.Copy(PopulationArray[k].PathPoints, newPopulationArray[k].PathPoints, PopulationArray[k].PathPoints.Length);
                }
            }

            Array.Copy(newPopulationArray, PopulationArray, newPopulationArray.Length); //copy new generation to main population array 
        }

        int[] CrossoverPaths(int[] path1, int[] path2) //crossover 2 paths
        {
            int pos1 = GlobalRandom.r.Next(0, path1.Length);
            int pos2 = GlobalRandom.r.Next(0, path1.Length);

            if (pos1 > pos2)
            {
                int foo1 = pos2;
                pos2 = pos1;
                pos1 = foo1;
            }

            int[] tab1 = new int[pos2 - pos1];
            int[] newPath = new int[path1.Length];

            for (int p = pos1, x = 0; p < pos2; p++, x++)
            {
                tab1[x] = path1[p];
            }

            for (int p = 0; p < path1.Length; p++)
            {
                newPath[p] = -1;
            }

            for (int p = pos1; p < pos2; p++)
            {
                newPath[p] = path1[p];
            }

            int foo = 0;
            for (int p = 0; p < path1.Length;)
            {
                if (newPath[p] == -1)
                {
                    if (!tab1.Contains(path2[foo]))
                    {
                        newPath[p] = path2[foo];
                        p++;
                    }
                    foo++;
                }
                else p++;
            }
            return newPath;
        }


        public void Mutation(int mutationChance) //mutate whole population
        {
            for (int k = 0; k < PopulationArray.Length; k++)
            {
                if (GlobalRandom.r.Next(0, 100) < mutationChance) //user specify mutation chance - path mutates or dont change
                {
                    PopulationArray[k].MutatePath();
                }
            }
        }


        public void TournamentSelection(int startPrinting) //get the best paths in population. Application make tournaments between 3 random paths - the best of them is in new population.
        {
            Tour[] newPopulationArray = new Tour[numberOfPaths];
            for (int i = 0; i < numberOfPaths; ++i)
            {
                newPopulationArray[i] = new Tour(numberOfPoints);
            }

            int[] fitnessArray = new int[numberOfPaths];

            for (int k = 0; k < numberOfPaths; k++)
            {
                fitnessArray[k] = PopulationArray[k].CalculateFitness();
                if (fitnessArray[k] < lengthofBestPath)
                {
                    if (startPrinting > 5000)
                    {
                        PopulationArray[k].PrintPath();
                    }
                    Array.Copy(PopulationArray[k].PathPoints, bestPathInPopulation.PathPoints, PopulationArray[k].PathPoints.Length);
                    lengthofBestPath = fitnessArray[k];
                    Console.WriteLine();
                }
            }

            for (int k = 0; k < numberOfPaths; k++)
            {
                int tournamentWinner;

                int individual1 = GlobalRandom.r.Next(0, numberOfPaths);
                int individual2 = GlobalRandom.r.Next(0, numberOfPaths);
                int individual3 = GlobalRandom.r.Next(0, numberOfPaths);

                int sumDistancePath1 = fitnessArray[individual1];
                int sumDistancePath2 = fitnessArray[individual2];
                int sumDistancePath3 = fitnessArray[individual3];

                if (sumDistancePath1 <= sumDistancePath2 && sumDistancePath1 <= sumDistancePath3) tournamentWinner = individual1;
                else if (sumDistancePath2 <= sumDistancePath1 && sumDistancePath2 <= sumDistancePath3) tournamentWinner = individual2;
                else tournamentWinner = individual3;

                Array.Copy(PopulationArray[tournamentWinner].PathPoints, newPopulationArray[k].PathPoints, PopulationArray[k].PathPoints.Length);
            }

            Array.Copy(newPopulationArray, PopulationArray, newPopulationArray.Length);
        }
    }
    static class GlobalRandom
    {
        public static Random r = new Random();
    }
    
    static class Distances
    {
        public static int[,] distancesArray;
    }
    class Program
    {
        static void Main(string[] args)
        {
            string filePath = "/Users/zanmarolt/src/my/tsp-algo/extract/att48.txt";

            List<Point> PointList = FileReader.ReadFile(filePath); //Generate List with Points (XY coords)
            Distances.distancesArray = FileReader.CreateDistanceMatrix(PointList); //Create Distances Matrix between points (Faster calculations during main loop)

            int individualsInGeneration = 90;
            int mutationChance = 50;
            int crossoverChance = 40;
            int numberOfLoops = 200000; //number of generations
            int numberOfPoints = PointList.Count;
            Population population = new Population(individualsInGeneration, numberOfPoints); //first random population

            for (int i = 0; i <= numberOfLoops; i++)
            {
                population.CrossoverPopulation(crossoverChance);
                population.Mutation(mutationChance);
                population.TournamentSelection(10000);
            }
        }

    }
}