using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace DuplicatesSearch
{
	internal class Program
	{
		private static String originFullPath;
		private static String checkingFullPath;
		private static Boolean saveReport = false;
		private static Boolean moveDuplicates = false;
		private static Boolean stopProgram = false; // The program cannot be run due to lack of complete information from the user

		internal static void Main(string[] args)
		{
			// Initializes workspace for program - check all pathes and demands via arguments
			Int32 result=InitProgram(args);
			
			if(result==0 || stopProgram)
			{
				return; 
			}
			else if(result==1)
			{
				Console.WriteLine($"2. Which files are duplicated inside [Origin]:");
				SearchInsideDir();
			}
			else if(result==2)
			{
				Console.WriteLine("2. Which files in [Checking] are in [Origin]:");
				SearchBetweenDirs();
			}

			Console.ReadLine();
		}

		/// <summary>
		/// Search files into Checking directory which are into Original directory
		/// </summary>
		private static void SearchBetweenDirs()
		{
			StringBuilder duplicatesFiles = new StringBuilder(1024);
			Int32 countPairs = 0;

			String[] checkingFiles = Directory.GetFiles(checkingFullPath, "*.*", SearchOption.AllDirectories);
			String[] originFiles = Directory.GetFiles(originFullPath, "*.*", SearchOption.AllDirectories);

			DirectoryInfo tempDir=null;
			String newPath, newPathParent;


			if (checkingFiles.Length == 0)
				duplicatesFiles.AppendLine("[Checking] directory is empty");
			if (originFiles.Length == 0)
				duplicatesFiles.AppendLine("[Origin] directory is empty.");

			if(checkingFiles.Length!=0 && originFiles.Length != 0)
			{
				for(Int32 ind1=0; ind1<checkingFiles.Length; ind1++)
				{
					for(Int32 ind2=0; ind2<originFiles.Length; ind2++)
					{
						if (String.Equals(Path.GetFileName(checkingFiles[ind1]), Path.GetFileName(originFiles[ind2])))
						{
							countPairs++;
							duplicatesFiles.AppendLine($"-> Checking: {checkingFiles[ind1].Substring(checkingFullPath.Length + 1)}");
							duplicatesFiles.AppendLine($"     Origin: {originFiles[ind2].Substring(originFullPath.Length+1)}");

							/*
							duplicatesFiles.AppendLine($"-> {checkingFiles[ind1]}");
							duplicatesFiles.AppendLine($"   {originFiles[ind2]}");
							*/

							if (moveDuplicates)
							{
								// Create temporary folder for diplicated files
								if (tempDir==null)
									tempDir = Directory.CreateDirectory(Path.Combine(checkingFullPath,
										new StringBuilder("_" + DateTime.Now.ToString().Replace(":", ".").Replace(" ", "_")).ToString()));

								// Save path structure to duplicated file
								newPath = Path.Combine(tempDir.FullName, checkingFiles[ind1].Substring(checkingFullPath.Length + 1));

								// Check and create appropriate path if it doesn't exist
								newPathParent = Path.GetDirectoryName(newPath);
								if (!Directory.Exists(newPathParent))
									Directory.CreateDirectory(newPathParent);

								Directory.Move(checkingFiles[ind1], newPath);
							}

							break;
						}
					}
				}
			}

			if (countPairs == 0)
				Console.WriteLine("INF: Duplicates weren't found.");
			else
				Console.WriteLine($"INF: Total pairs: {countPairs}");

			Console.WriteLine(duplicatesFiles.ToString());
		}

		/// <summary>
		/// Helper: Search duplicated files inside directory
		/// </summary>
		private static void SearchInsideDir()
		{
			StringBuilder duplicatesFiles = new StringBuilder(1024);
			String[] files = Directory.GetFiles(originFullPath, "*.*", SearchOption.AllDirectories);
			Int32 countPairs = 0;
			
			if(files.Length>=2)
			{
				for (Int32 ind1 = 0; ind1 < files.Length - 1; ind1++)
				{
					for (Int32 ind2 = ind1+1; ind2 < files.Length; ind2++)
					{
						if (String.Equals(Path.GetFileName(files[ind1]), Path.GetFileName(files[ind2])))
						{
							countPairs++;
							duplicatesFiles.AppendLine($"-> {files[ind1].Substring(originFullPath.Length+1)}");
							duplicatesFiles.AppendLine($"   {files[ind2].Substring(originFullPath.Length + 1)}");
							break;
						}
					}
				}
			}

			if (countPairs == 0)
				duplicatesFiles.AppendLine("INF: Duplicates weren't found.");
			else
				duplicatesFiles.AppendLine($"INF: Total pairs: {countPairs}");

			Console.WriteLine(duplicatesFiles);
		}

		/// <summary>
		/// Initializes workspace for program - check all pathes and demands via arguments
		/// </summary>
		/// <param name="args"></param>
		private static Int32 InitProgram(string[] args)
		{
			Int32 foldersNumber = 0; // Define how much folders are in init file

			if ( args.Length!=0) // User inputed arguments
			{
				List<String> initInfo = new List<String>();
				initInfo.Add("1. Initialization:");
				for (Int32 ind = 0; ind < args.Length; ind++)
				{
					switch (args[ind])
					{
						case "-i":
							if (ind + 1 < args.Length) // Check whether argument was specifed in complete volume
							{
								// Checking wheter specifed initialization file is existed
								StringBuilder partPath = new StringBuilder(args[ind + 1]);
								partPath.Replace('/', '\\');
								String initFileFullPath = Path.Combine(Environment.CurrentDirectory, partPath.ToString());
								if (File.Exists(initFileFullPath))
								{
									// Read initialization file and fill appropriate variables
									String[] initFileLines = File.ReadAllLines(initFileFullPath);
									if (initFileLines.Length == 0)
									{
										foldersNumber = 0;
										initInfo.Add("ERR: Initializtion file must has 2 line with pathes");
										stopProgram = true;
									}
									else
									{
										foldersNumber = 1;

										StringBuilder tempPath = new StringBuilder(256);

										tempPath.Append(initFileLines[0]).Replace('/', '\\');
										originFullPath = Path.Combine(Path.GetDirectoryName(initFileFullPath), tempPath.ToString());										

										// Checking whether both pathes are existed
										if (!Directory.Exists(originFullPath))
										{
											initInfo.Add($"ERR: [Origin] directory doesn't exist -> {originFullPath}");
											stopProgram = true;
										}
										else
											initInfo.Add($"INF: [Origin] -> {originFullPath}");
										
										if(initFileLines.Length>1 && initFileLines[1].Length!=0)
										{
											foldersNumber = 2; // Take first two lines. Other doesn't matter

											tempPath.Clear().Append(initFileLines[1]).Replace('/', '\\');
											checkingFullPath = Path.Combine(Path.GetDirectoryName(initFileFullPath), tempPath.ToString());
											if (!Directory.Exists(checkingFullPath))
											{
												initInfo.Add($"ERR: [Checking] directory doesn't exist -> {checkingFullPath}");
												stopProgram = true;
											}
											else
												initInfo.Add($"INF: [Checking] -> {checkingFullPath}");
										}
									}
									ind++;
								}
								else
								{
									initInfo.Add($"ERR: Specified initialization file doesn't exist -> {initFileFullPath}");
									stopProgram = true;
								}
							}
							else
							{
								initInfo.Add("ERR: The path to initialize file wasn't specifed after -i argument");
								stopProgram = true;
							}
							break;
						case "-r":
							saveReport = true;
							initInfo.Add("INF: Report will be saved in [Checking]\\Duplicates directory");
							break;
						case "-m":
							moveDuplicates = true;
							initInfo.Add("INF: Duplicated files will be moved to [Checking]\\Duplicates directory");
							break;
					}
				}
				/*
				// Check if neccessary variables were assigned
				if(originFullPath==null || checkingFullPath==null)
					stopProgram=true;
					*/
							// Processing initInfo variable (checking whether initialization has gone successfully
							StringBuilder resultInitInfo = new StringBuilder(512);
				if (stopProgram) // Stop program if there are errors
				{
					foreach (var str in initInfo)
						if (str.StartsWith("ERR:"))
							resultInitInfo.AppendLine(str);
					resultInitInfo.AppendLine("INF: The program cannot be run due to lack of complete information from the user");
				}
				else
				{
					foreach (var str in initInfo)
						if (!str.StartsWith("ERR:"))
							resultInitInfo.AppendLine(str);
					resultInitInfo.AppendLine("INF: The program was initialized succesfully");
				}

				// Display initializator result
				Console.WriteLine(resultInitInfo.ToString());
			}
			else // User didn't input any argument, so show help information
			{
				Console.WriteLine("Show help information");
				stopProgram = true;
			}

			return foldersNumber;
		}
	}
}