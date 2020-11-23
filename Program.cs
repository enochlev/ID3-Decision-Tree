using System;
using System.Collections.Generic;
using System.Linq;



namespace WindowsFormsApp1
{



    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]

        static void Main(string[] args)
        {
            
            if (args.Length == 0)
            {
                
                Console.Out.WriteLine("<input Training File> <outputTraining File / Optional, by default replaces .in with .out>");
                return;
            }
            string outputfile;
            if (args.Length == 1)
            {
                outputfile = args[0].Replace(".in", ".out");
            }
            else
                outputfile = args[1];

            DataClass DataStorage = new DataClass();

            DataStorage.openFile(args[0]);
            DataStorage.Build_Tree();

            DataStorage.Print_File(outputfile);

        }
    }




    public class DataClass
    {

        private const int DIMENSIONS = 30;//change if you are doing a large dataset with more than 20 attribute nominals

        //some general values
        private int training_Count; // number of training data
        private int numOfAttributes;
        private int numOfAnswers;

        //data storage
        string[] AnswerSet;//list of answers
        string[][] AttributeSet;//list of attributes and info on it
        LinkedList<string[]> TrainSet;

        public DataClass()
        {
            TrainSet = new LinkedList<string[]>();
        }



      


        private class DecisionTree
        {
            public string answer = null;
            public string PreviosePathDecision;
            public string AtributeName;
            public int numofSubattributes;
            public LinkedList<DecisionTree> Paths;
            public LinkedList<string[]> TrainSet;
            public double continuasSplitValue;

        }
        private DecisionTree DecisionTreeRoot;
        private int duplicates = 0;


        private class EntropyClass
        {
            public EntropyClass(string value, int count)
            {
                this.value = value;
                this.count = count;
            }
            public string value;
            public int count;
        }


        public void openFile(string location)//, LinkedList<AttributeSet> dataArray, AnsSet AnsSet, LinkedList<TrainSet> TrainArray)
        {
            int size, num;
            string[] fileText;

            try
            {
                fileText = System.IO.File.ReadAllLines(location);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to Open File");
                return;
            }
            int numOfLines = fileText.Length;//get the number of lines in the file

            //LINE # 1
            try
            {
                numOfAttributes = Int32.Parse(fileText[0]);//first line give me the number of attribtutes
            }
            catch(Exception e)
            {
                Console.WriteLine("Invalid Format");
                return;
            }
            //allocated a 2darray
            AttributeSet = new string[numOfAttributes][];
            for (int p = 0; p < numOfAttributes; ++p)
                AttributeSet[p] = new string[DIMENSIONS];



            //ATTRIBUTE LINES
            int i = 1;
            bool last;
            while (numOfAttributes >= i)
            {
                last = false;
                int count = 0;//keep track of the first one so that we can name the column in the table
                int StrLen = 1;
                string substring, atribName = "";
                                               
                while (last == false)
                {
                    StrLen = fileText[i].Length;
                    try
                    {
                        count++;
                        substring = fileText[i].Substring(0, fileText[i].IndexOf(' '));
                        fileText[i] = fileText[i].Substring(fileText[i].IndexOf(' '), StrLen - fileText[i].IndexOf(' '));
                        fileText[i] = fileText[i].Trim();
                    }
                    catch (System.ArgumentOutOfRangeException)//last word on the line
                    {

                        substring = fileText[i];
                        last = true;
                    }
                   
                    //name of attribute is the first array value
                    AttributeSet[i - 1][count - 1] = substring;
                    
                }

                ++i;
            }

            int countA = 0;
            //ANSWER LINES
            AnswerSet = new string[DIMENSIONS];
            last = false;
            AnswerSet[0] = "Ans";
            while (last == false)
            {


                string substring;
                int StrLen = fileText[i].Length;
                try
                {
                    countA++;
                    substring = fileText[i].Substring(0, fileText[i].IndexOf(' '));
                    fileText[i] = fileText[i].Substring(fileText[i].IndexOf(' '), StrLen - fileText[i].IndexOf(' '));
                    fileText[i] = fileText[i].Trim();
                }
                catch (System.ArgumentOutOfRangeException)//last word on the line
                {
                    substring = fileText[i];
                    last = true;
                }
                StrLen = fileText[i].Length;

                if (substring != "Ans")
                {
                    AnswerSet[countA - 1] = substring;
                }
            }
            numOfAnswers = countA - 1;
            i++;

            //DECISION LINES
            training_Count = 0;
            last = false;
            while (fileText.Length > i)
            {
                last = false;


                string[] TrainString = new string[DIMENSIONS];
                int StrLen = 1;
                string substring;
                int count = 0;

                while (last == false)
                {
                    StrLen = fileText[i].Length;
                    try
                    {
                        count++;
                        substring = fileText[i].Substring(0, fileText[i].IndexOf(' '));
                        fileText[i] = fileText[i].Substring(fileText[i].IndexOf(' '), StrLen - fileText[i].IndexOf(' '));
                        fileText[i] = fileText[i].Trim();
                        TrainString[count] = substring;
                    }
                    catch (System.ArgumentOutOfRangeException)//last word on the line
                    {
                        
                        substring = fileText[i];
                        last = true;
                        TrainString[0] = substring;//asnwer will be the first in the index
                    }
                }
                TrainSet.AddLast(TrainString);
                count = 0;
                ++i;
                training_Count++;
            }
        }

        public void Print_File(string filetoprint)
        {
            System.IO.File.WriteAllText(@filetoprint, dupsFunc() + Print_Tree(null));
        }


        public void Build_Tree()
        {
            DecisionTree DTnode = new DecisionTree();
            DecisionTreeRoot = DTnode;
            DecisionTreeRoot.TrainSet = TrainSet;


            DTnode.numofSubattributes = numOfAttributes;
            string bestRoute = GainCalculation(DTnode);
            DTnode.AtributeName = bestRoute;
            DTnode.Paths = new LinkedList<DecisionTree>();
            //create a decision path node for each attributes
            for (int k = 0; findAttributeSetSize(findAtrib(bestRoute)) > k; k++)
            {
                DecisionTree newDTnode = new DecisionTree();
                
                DTnode.Paths.AddLast(newDTnode);

                if (findAtrib(bestRoute)[1] == "continuous")
                {
                    newDTnode.TrainSet = shrinkDataset(TrainSet, k, findAtribIndex(bestRoute) + 1, DTnode.continuasSplitValue);//add shrink data set for contiuas value

                    string sign;
                    if (k == 0)
                        sign = "< ";
                    else
                        sign = ">= ";

                    newDTnode.PreviosePathDecision = sign + DTnode.continuasSplitValue.ToString();
                }

                else
                {
                    newDTnode.TrainSet = shrinkDataset(TrainSet, findAtrib(bestRoute)[k + 1], findAtribIndex(bestRoute) + 1);//add shrink data set for contiuas value
                    newDTnode.PreviosePathDecision = findAtrib(bestRoute)[k + 1];
                }


                //to help the bad function that i made 7 lines down
                string lessorGreater = "less";
                if (k == 1)
                    lessorGreater = "greater";

                if (findAtrib(bestRoute)[1] != "continuous" && (Entrapy(DTnode, findAtribIndex(bestRoute), AttributeSet[findAtribIndex(bestRoute)][k + 1]) == 0.0))
                    newDTnode.answer = newDTnode.TrainSet.First.Value[0];


                else if (findAtrib(bestRoute)[1] == "continuous" && (continuasEntrapy(DTnode, findAtribIndex(bestRoute), DTnode.continuasSplitValue, lessorGreater) == 0.0))
                    newDTnode.answer = newDTnode.TrainSet.First.Value[0];


                else
                {

                    //LinkedList<DataClass> newTrainSet;// = new LinkedList<DataClass>();
                    newDTnode.numofSubattributes = numOfAttributes - 1;
                    finishBuild(newDTnode);//will finish building one node

                }


            }


        }

        private void finishBuild(DecisionTree DTnode)
        {
            string bestRoute = GainCalculation(DTnode);


            int size = findAttributeSetSize(findAtrib(bestRoute));
            DTnode.Paths = new LinkedList<DecisionTree>();

            for (int k = 0; k < size; k++)
            {
                


                // make a new path for the decision tree
                DecisionTree newDTnode = new DecisionTree();
                newDTnode.numofSubattributes = DTnode.numofSubattributes - 1;
                DTnode.AtributeName = bestRoute;
                DTnode.Paths.AddLast(newDTnode);




                if (findAtrib(bestRoute)[1] == "continuous")
                {
                    newDTnode.TrainSet = shrinkDataset(DTnode.TrainSet, k, findAtribIndex(bestRoute) + 1, DTnode.continuasSplitValue);//add shrink data set for contiuas value

                    string sign;
                    if (k == 0)
                        sign = "< ";
                    else
                        sign = ">= ";

                    newDTnode.PreviosePathDecision = sign + DTnode.continuasSplitValue.ToString();
                }

                else
                {
                    newDTnode.TrainSet = shrinkDataset(DTnode.TrainSet, findAtrib(bestRoute)[k + 1], findAtribIndex(bestRoute) + 1);//add shrink data set for contiuas value
                    newDTnode.PreviosePathDecision = findAtrib(bestRoute)[k + 1];
                }


                //to help the bad function that i made 7 lines down
                string lessorGreater = "less";
                if (k == 1)
                    lessorGreater = "greater";

                if (findAtrib(bestRoute)[1] != "continuous" && (Entrapy(DTnode, findAtribIndex(bestRoute), AttributeSet[findAtribIndex(bestRoute)][k + 1]) == 0.0 || newDTnode.numofSubattributes == 0))
                    newDTnode.answer = mostProbible(newDTnode.TrainSet);

                else if (findAtrib(bestRoute)[1] == "continuous" && (continuasEntrapy(DTnode, findAtribIndex(bestRoute), DTnode.continuasSplitValue, lessorGreater) == 0.0 || newDTnode.numofSubattributes == 0))
                    newDTnode.answer = mostProbible(newDTnode.TrainSet);

                


                //DecisionTree.PreviosePathDecision = findAtrib(BestRoute)[i + 1];
                //DecisionTree.TrainSet = shrinkDataset(newDTnode.TrainSet, AttributeSet[findAtribIndex(BestRoute)][i + 1], findAtribIndex(BestRoute) + 1);

                else
                {
                    finishBuild(newDTnode);
                }
                if (newDTnode.answer == "N/A")
                    DTnode.Paths.Remove(newDTnode);

            }
            

                
                //the problem here is i dont want to compare with attribtutes that are already down the decision tree....
        }

        private string mostProbible(LinkedList<string[]> trainSet)
        {
            int[] possibles = new int[numOfAnswers];

            LinkedListNode<string[]> newNode = trainSet.First;
            while (newNode != null)
            {
                possibles[answerindex(newNode.Value[0]) - 1]++;
                newNode = newNode.Next;
            }

            int mostLikelyIndex = 0;
            int mostLiklycount = possibles[0];

            for (int i = 0; numOfAnswers > i; i++)
            {
                if (possibles[i] > mostLiklycount)
                {
                    mostLikelyIndex = i;
                    mostLiklycount = possibles[i];
                }
            }

            if (mostLiklycount == 0)
                return "N/A";

            return AnswerSet[mostLikelyIndex+ 1];
        
        }


        //return attribute index provided string value
        private int findAtribIndex(string bestRoute)
        {
            for (int i = 0; i < numOfAttributes; i++)
            {
                if (AttributeSet[i][0] == bestRoute)
                    return i;
            }

            throw new Exception("String Missmatched... They should have not");
            
        }


        //shrinks data set so that i will only use the left over training data and no resuse attributes
        private LinkedList<string[]> shrinkDataset(LinkedList<string[]> trainSet
            , string v//should change every itteration*/,
            , int k//should stay the same*/)
            )
        {

            LinkedList<string[]> newlist = new LinkedList<string[]>();
            LinkedListNode<string[]> index = trainSet.First;

            while (index != null)
            {
                if (index.Value[k] == v)

                {
                    string[] newstring = new string[DIMENSIONS];
                    index.Value.CopyTo(newstring, 0);
                    newstring[k] = null;
                    newlist.AddLast(newstring);
                }

                index = index.Next;
            }
            return newlist;

        }

        //same as above but for conitinuas
        private LinkedList<string[]> shrinkDataset(LinkedList<string[]> trainSet
            , int v//0 means less then 1 means greater then
            , int k//should stay the same*/)
            , double j
            )
        {

            LinkedList<string[]> newlist = new LinkedList<string[]>();
            LinkedListNode<string[]> index = trainSet.First;

            while (index != null)
            {
                if (0 == v)
                {
                    if (double.Parse(index.Value[k]) < j)
                    {
                        string[] newstring = new string[DIMENSIONS];
                        index.Value.CopyTo(newstring,0);
                        newstring[k] = null;
                        newlist.AddLast(newstring);
                                            
                    }
                }
                else if (1 == v)
                {
                    if (double.Parse(index.Value[k]) >= j)
                    {
                        string[] newstring = new string[DIMENSIONS];
                        index.Value.CopyTo(newstring, 0);
                        newstring[k] = null;
                        newlist.AddLast(newstring);
                    }
                }

                index = index.Next;
            }
            return newlist;
        }


        //return the string value of the best attribute
        private string GainCalculation(DecisionTree dTnode)
        {
            int DuplicatesFound = 0;
            LinkedListNode<string[]> firstNode = dTnode.TrainSet.First;
            double LargestGainValue = -0.1;
            String LargestGainString = "";
            double largestContinuasValueMain = 0.0;


            for (int i = 0; i < numOfAttributes; i++)
            {

                //this will skip the attributes that were already chosen in the list
                if (firstNode.Value[i + 1] != null)
                {

                    double childnodeAVG = 0.0;
                    double calculatedInfoGain = 0.0;
                    double largestContiniasValues = 0.0;


                    if (AttributeSet[i][1] == "continuous")
                    {
                        LinkedList<double> possiblesList = new LinkedList<double>();
                        LinkedListNode<string[]> node = dTnode.TrainSet.First;
                        while (node != null)
                        {
                            if (!possiblesList.Contains(double.Parse(node.Value[i + 1])))
                            {
                                possiblesList.AddLast(double.Parse(node.Value[i + 1]));
                            }
                            node = node.Next;
                        }

                        double largestContinuasGain = -0.1;
                        LinkedListNode<double> intNode = possiblesList.First;
                        for (int j = 0; j < possiblesList.Count(); j++)
                        {
                            double ParentEntrpoy = continuasEntrapy(dTnode, i, intNode.Value, "all");
                            double LessEntropy = continuasEntrapy(dTnode, i, intNode.Value, "less");
                            double GreaterEntrpoy = continuasEntrapy(dTnode, i, intNode.Value, "greater");

                            childnodeAVG = GreaterEntrpoy + LessEntropy;

                            double TestcalculatedInfoGain = (ParentEntrpoy / dTnode.TrainSet.Count()) - (childnodeAVG / dTnode.TrainSet.Count());

                            if (largestContinuasGain < TestcalculatedInfoGain)
                            {

                                largestContinuasGain = TestcalculatedInfoGain;
                                largestContiniasValues = intNode.Value;
                            }

                            intNode = intNode.Next;
                        }

                        calculatedInfoGain = largestContinuasGain;

                    }

                    else
                    {
                        double ParentI = Entrapy(dTnode, i);

                        for (int j = 0; j < findAttributeSetSize(AttributeSet[i]); j++)
                        {
                            childnodeAVG = childnodeAVG + Entrapy(dTnode, i, AttributeSet[i][j + 1]);
                        }
                        calculatedInfoGain = (ParentI / dTnode.TrainSet.Count()) - (childnodeAVG / dTnode.TrainSet.Count());
                    }


                    if (calculatedInfoGain >= LargestGainValue)
                    {
                        if (calculatedInfoGain == LargestGainValue)
                            DuplicatesFound++;
                        else
                            DuplicatesFound = 0;
                            largestContinuasValueMain = largestContiniasValues;//it will help me keep track of the best continuas split values.... if there is multiple continuas attributes
                        LargestGainValue = calculatedInfoGain;
                        LargestGainString = AttributeSet[i][0];
                    }

                }
               
            }
            if (findAtrib(LargestGainString)[1] == "continuous")
                dTnode.continuasSplitValue = largestContinuasValueMain;
            if (DuplicatesFound > 0)
                duplicates++;

            return LargestGainString;
        }

        //Entrapy for continuas values
        private double continuasEntrapy(DecisionTree dTnode, int i, double value, string v)
        {

            int denominator = dTnode.TrainSet.Count;

            double returnedValue;
            int[] entropyNumerator = new int[DIMENSIONS];

            LinkedListNode<string[]> TrainNode = dTnode.TrainSet.First;
            

            denominator = 0;
            while (TrainNode != null)
            {
                if (v == "all")
                {
                    ++denominator;
                    entropyNumerator[answerindex(TrainNode.Value[0]) - 1]++;

                }
                else if (v == "less")
                {
                    if (double.Parse(TrainNode.Value[i + 1]) < value)
                    {
                        ++denominator;
                        entropyNumerator[answerindex(TrainNode.Value[0]) - 1]++;

                    }
                }
                else if (v == "greater")
                {
                    if (double.Parse(TrainNode.Value[i + 1]) >= value)
                    {
                        ++denominator;
                        entropyNumerator[answerindex(TrainNode.Value[0]) - 1]++;

                    }
                }
                TrainNode = TrainNode.Next;

            }
            if (denominator == 0)
                return 0;

            double sum = 0.0, testsum = 0.0;
            for (int j = 0; j < numOfAnswers; j++)
            {

                testsum = (((entropyNumerator[j] * 1.0) / denominator) * Math.Log(((entropyNumerator[j] * 1.0) / denominator), 2));

                if (double.IsNaN(testsum))
                {
                    testsum = 0.0;
                }
                sum = sum + testsum;
            }
            returnedValue = 0.0 - sum * denominator;//to make it easier for me make an average   




            return returnedValue;

        }

       //Entrapy for nonconinuas values
        private double Entrapy(DecisionTree DTnode, int i, string dataset = "all")
        {
            LinkedListNode<string[]> TrainindNode = DTnode.TrainSet.First;
            int sizeOfAnswerList = numOfAnswers;
            int denominator = 0;
            double[] entropyNumerator = new double[sizeOfAnswerList];

            while (TrainindNode != null)
            {
                if (dataset != "all" && TrainindNode.Value[i + 1] != dataset)
                {
                }
                else
                {
                    int answerIndex = answerindex(TrainindNode.Value[0]);
                    entropyNumerator[answerIndex - 1]++;
                    denominator++;
                }
                TrainindNode = TrainindNode.Next;
            }

            double sum = 0.0, testsum = 0.0;
            for(int j = 0; j < sizeOfAnswerList; j++)
            {
                testsum= ((entropyNumerator[j] / denominator) * Math.Log((entropyNumerator[j] / denominator), 2));

                if (double.IsNaN(testsum))
                {
                    testsum = 0.0;
                }
                sum = sum + testsum;
            }

            

                sum = sum * denominator;

                return 0.0 - sum;//negative value
            
        }


        //returns the index of an answer povided the string of an anwer
        //starts at 1
        private int answerindex(string v)
        {

            for (int i = 1; i <= AnswerSet.Length; i++)
            {
                if (AnswerSet[i] == v)
                    return i;
            }
            return -1;
            //should never be the case
        }

        //returns the attribute string array given the attribute name
        private string[] findAtrib(string v)
        {
            for (int i = 0; i < AttributeSet.Length; i++)
            {
                if (AttributeSet[i][0] == v)
                    return AttributeSet[i];
            }
            return null;
            //should never be the case
        }

        //used to find out how big the attribute size is
        private int findAttributeSetSize(string[] v)
        {
            if (v[1] == "continuous")
                return 2;

            int counter = 1;
            string test = "";
            while (test != null)
            {
                test = v[counter++];

            }
            return counter - 2;
        }

        private string Print_Tree(DecisionTree TreeNode, int tabs = 0)
        {
            
            //base case
            if (TreeNode == null)
                TreeNode = DecisionTreeRoot;

            //final case

            if (TreeNode.answer != null)
                return tabsFunc(tabs) + " " + TreeNode.answer;

            //normalcase
            string outPut = "";

            if (findAtrib(TreeNode.AtributeName)[1] == "continuous")
            {
                outPut = outPut + "\n" + tabsFunc(tabs) + " " + AttributeSet[findAtribIndex(TreeNode.AtributeName)][0] + ">=" + TreeNode.continuasSplitValue.ToString() + ":";
                outPut = outPut + "\n" + Print_Tree(TreeNode.Paths.Last.Value, tabs + 1);
                outPut = outPut + "\n"+  tabsFunc(tabs) + " " + AttributeSet[findAtribIndex(TreeNode.AtributeName)][0] + "<" + TreeNode.continuasSplitValue.ToString() + ":";
                outPut = outPut + "\n" + Print_Tree(TreeNode.Paths.First.Value, tabs + 1);
            }


            else
            {
                LinkedListNode<DecisionTree> IndexNode = TreeNode.Paths.First;
                
                while (IndexNode != null)
                {
                    outPut = outPut + "\n" + tabsFunc(tabs) + " " +TreeNode.AtributeName + "=" + IndexNode.Value.PreviosePathDecision + ":";
                    outPut = outPut + "\n" + Print_Tree(IndexNode.Value, tabs + 1);

                    
                    IndexNode = IndexNode.Next;
                }

            }
            return outPut.Remove(0, 1);
            //return outPut;


        }

        //makes indents for the print
        private string tabsFunc(int tabs )
        {
            string tabsSt= "";
            int i = 0;
            while (i < tabs)
            {
                tabsSt = tabsSt + "\t";
                i++;
            }
            return tabsSt;
        }

        private string dupsFunc()
        {
            string tabsSt = "";
            int i = 0;
            while (i < duplicates)
            {
                tabsSt = tabsSt + "** duplicate information gain **\n";
                i++;
            }
            return tabsSt;
        }
    }

   

}