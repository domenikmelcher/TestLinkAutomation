﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using TestLinkAutomation.SVN.LogAnalyzer.CommandLine;
using Plossum.CommandLine;
using System.Diagnostics;
using TestLinkAutomation.Common;
using TestLinkAutomation.Common.CommandLine;
using TestLinkAutomation.SVN.LogAnalyzer.ModulLogic;

namespace TestLinkAutomation.SVN.LogAnalyzer
{
    public class Program
    {
        private static SVNLogAnalyzerModulLogic ModulLogic = new SVNLogAnalyzerModulLogic();

        public static StreamWriter myWriter;

        public static string RegexFunctionPattern = @"(\n(\s*|([\+\-]\s*))?\w+(\s*[*])?\s+\w+)\(([^\)]+)\)";
        public static string RemoveSoughPattern = @"((else)|(if)|(return)|(\{)|(\}))";
        public static string DetermineSVNChangedPattern = @"(\n[\+\-])";

        //WORKING PATTERN V1!!
        //public static string RegexFunctionPattern = @"((\s*|([\+\-]\s*))?\w+(\s*[*])?\s+\w+)[(](((\n*\s*)?|([\+\-]\s*))?\w+(\s*[*])?\s+\w+[,]*)+(\n*\s*[)])?";
        //WORKING PATTERN V2!!
        //public static string RegexFunctionPattern = @"((\s*|([\+\-]\s*))?\w+(\s*[*])*\s+\w+)[(](((\n*\s*)?|(\n*[\+\-]\s*))?(\w+\s*)?\w+((\s*[*]*\s+)|(\s+[*]*))\w+[,]*)+(((\n*\s*)?|(\n*[\+\-]\s*))?[)])?";
        //WORKING PATTERN V3!!
        //public static string RegexFunctionPattern = @"((\s*|([\+\-]\s*))?\w+(\s*[*])?\s+\w+)[(](((\n*\s*)?|(\n*[\+\-]\s*))?(\w+[*]*\s*)?([:]*)?\w+((\s*[*]*\s+)|(\s+[*]*)|([\[][\]]\s+)|(\&?\s+))\w+[,]?(((\s*([\/]?[*]*[<]\s*))([\[]\w+[\]])?(\s*\w+[']?\w+)*([.]\s*[*][\/])?)?))+(((\n*\s*)?|(\n*[\+\-]\s*))?[)])?";


        //ToDo add std:: support
        //Todo add [] and ** support for function return type

        public static string RegexVariable = @"^((\s*|([\+\-]\s*))?\w+(\s*[*])?\s+\w+)?$";

        static int Main()
        {
            ModulLogic.InitializeModul();
            ModulLogic.ExecuteModulLogic();
            ModulLogic.Save();            

            return Environment.ExitCode = 0;
        }

        

        private static void ProcessFile(string fileContent)
        {
            //string[] changedFunctions = null;

            //if (Regex.IsMatch(fileContent, RegexFunctionPattern))
            //    changedFunctions = Regex.Split(fileContent, @"\n" + RegexFunctionPattern);

            string fileHeader = fileContent.Substring(0, fileContent.IndexOf('\n')).Replace(": ",string.Empty).Replace("\r",string.Empty);
            
            //ileContent = Regex.Replace(fileContent, RemoveSoughPattern, string.Empty);
            string[] lines = fileContent.Split('\n');
            
            lines = lines.Where(x => !Regex.IsMatch(x, RemoveSoughPattern)).ToArray();
            //MatchCollection variablesDeclarationMatches = Regex.Matches(fileContent, RegexVariable, RegexOptions.Compiled);

            //List<string> functionnamesString = new List<string>();

            //foreach (string line in lines)
            //{
            //    functionnamesString.AddRange(Regex.Matches(line, RegexFunctionPattern)
            //        .Cast<Match>()
            //        .Select(x => x.Value).ToList());
            //}
            
            //foreach (Match match in variablesDeclarationMatches)
            //{
            //    if (match != null && match.Success && !string.IsNullOrEmpty(match.Value))
            //        Console.WriteLine("{0}", match.Value);
            //}

            //MatchCollection myCol = Regex.Matches(fileContent, RegexFunctionPattern);



            string[] allFunctions = Regex.Matches(fileContent, RegexFunctionPattern)
                    .Cast<Match>()
                    .Select(x => x.Value).ToArray();

            allFunctions = allFunctions.Where(x => !Regex.IsMatch(x, RemoveSoughPattern) && !string.IsNullOrWhiteSpace(x)).ToArray();
           

            List<string> changedFunctions = new List<string>();
            string separatedFileContent = fileContent;

            for (int i = 0; i < allFunctions.Count(); i++)
            {
                int indexFunctionStart = fileContent.IndexOf(allFunctions[i]);

                if (indexFunctionStart != -1)
                {
                    int indexFunctionEnd = fileContent.Length;

                    if (i < allFunctions.Count() - 2)
                        indexFunctionEnd = fileContent.IndexOf(allFunctions[i + 1], indexFunctionStart);

                    if (indexFunctionEnd != -1)
                    {
                        string functionBody = fileContent.Substring(indexFunctionStart, indexFunctionEnd - indexFunctionStart).Replace(allFunctions[i], string.Empty);
                        if (!string.IsNullOrEmpty(functionBody))
                        {
                            MatchCollection col = Regex.Matches(functionBody, DetermineSVNChangedPattern);
                            if (col.Count > 0 && FuncitonStillPresentInCurrentRevision(allFunctions[i]))
                                changedFunctions.Add(allFunctions[i]);
                        }
                    }
                }
            }


            //functionnamesString = functionnamesString.Select(x => x.Replace("\n", string.Empty)).ToArray();
            //functionnamesString = functionnamesString.Select(x => x.Replace(" ", string.Empty)).ToArray();
            //functionnamesString = functionnamesString.Select(x => x.Replace("\r", string.Empty)).ToArray();

            //Match[] functionNamesMatches = new Match[myCol.Count];
            //myCol.CopyTo(functionNamesMatches,0);

            
            //string[] functionnamesString = functionNamesMatches.Select(x => x.Value).ToArray();



            Console.WriteLine("Changed functions:");

            if (changedFunctions.Any())
            {
                myWriter.WriteLine("\nFileName: " + fileHeader + "\n");
                foreach (string functionname in changedFunctions)
                    myWriter.WriteLine(functionname);
            }
           

            //lines = lines.Where(x => Regex.IsMatch(x, RegexFunctionPattern, RegexOptions.IgnoreCase)).ToArray();

            //foreach (string line in lines)
            //{
            //    Match myMatch = null;

            //    if (Regex.IsMatch(line, RegexVariable, RegexOptions.ExplicitCapture))
            //    {
            //        myMatch = Regex.Match(line, RegexVariable);

            //        string match = myMatch.Value;

            //        if (myMatch.Groups != null)
            //        {
            //            foreach (Group g in myMatch.Groups)
            //            {
            //                string val = g.Value;
            //            }
            //        }
            //    }
            //}

            //lines = lines.Where(x => !x.Contains("{") && !x.Contains("}") && !x.Equals(" ") && !x.Contains("//") && !x.Contains("/*") && !x.Contains("*/")).ToArray();


            //foreach (string line in lines)
            //{
            //    MatchCollection myMatch = Regex.Matches(line, RegexFunctionPattern, RegexOptions.IgnoreCase);
            //    try
            //    {
            //        if (myMatch.Count > 0)
            //        {
            //            Console.WriteLine("Changed functions:");

            //            foreach (Match match in myMatch)
            //            {
            //                if (match != null && match.Success)
            //                    Console.WriteLine("{0}", match.Value);
            //            }
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        Console.WriteLine(ex.Message);
            //    }
            //}

        }

        private static bool FuncitonStillPresentInCurrentRevision(string function)
        {
            return !function.Replace("\n", string.Empty).Replace(" ", string.Empty)[0].Equals('-');
        }

        private static void PerformSVNDiff()
        {            
            

            //using (var client = new SvnClient())
            //{
            //    client.Authentication.Clear(); // Clear a previous authentication
            //    client.Authentication.DefaultCredentials = new System.Net.NetworkCredential(op.svn_user, op.svn_password,"wbi.nxp.com");

            //    client.Authentication.SslServerTrustHandlers += delegate(object sender, SvnSslServerTrustEventArgs e)
            //    {
            //        e.AcceptedFailures = e.Failures;
            //        e.Save = true; // Save acceptance to authentication store
            //    };

               

            //    Uri serverUri = new Uri(op.svn_repository);
            //    SvnDiffArgs args = new SvnDiffArgs();

            //    //args.DiffArguments = "-r";

            //    MemoryStream diffStream = new MemoryStream();
            //    string hochkomma = "\"How to add doublequotes\"";
            //    //string arg = "-x \"-U80\" ";
            //    args.DiffArguments.Add("-U 30");
            //    //args.Depth = SvnDepth.Unknown;

            //    client.Diff(new SvnUriTarget(serverUri, op.svn_revision), new SvnUriTarget(serverUri, SvnRevision.Head), args, diffStream);

            //    diffStream.Position = 0;
            //    StreamReader strReader = new StreamReader(diffStream);
            //    string str = strReader.ReadToEnd();
            //    StreamWriter wr = new StreamWriter("diffoutput.txt");
            //    wr.Write(str);
            //    wr.Flush();
            //    wr.Close();
            //    strReader.Close();

           //}
        }
    }
}
