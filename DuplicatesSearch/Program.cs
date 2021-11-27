using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DuplicatesSearch
{
    internal class Program
	{
		private static String originFullPath, originName;
		private static String checkingFullPath, checkingName;
		private static Boolean saveReport = false;
		private static Boolean moveDuplicates = false;
		private static Boolean stopProgram = false; // The program cannot be run due to lack of complete information from the user

		internal static void Main(string[] args)
		{
			// Initializes workspace for program - check all pathes and demands via arguments
			Int32 result = InitProgram(args);

			if (result == 0 || stopProgram)
			{
				return;
			}
			else if (result == 1)-
			{
				WriteColor($"TSK: Which files are duplicated inside [{originName}]:\n", ConsoleColor.DarkGreen);
				SearchInsideDir();
			}
			else if (result == 2)
			{
				WriteColor($"TSK: Which files into [{checkingName}] are into [{originName}]:\n", ConsoleColor.DarkGreen);
				SearchBetweenDirs();
			}
		}

		/// <summary>
		/// Search files into Checking directory which are into Original directory
		/// </summary>
		private static void SearchBetweenDirs()
		{
			Int32 countPairs = 0;

			String[] checkingFiles = Directory.GetFiles(checkingFullPath, "*.*", SearchOption.AllDirectories);
			String[] originFiles = Directory.GetFiles(originFullPath, "*.*", SearchOption.AllDirectories);

			DirectoryInfo tempDir = null;
			String newPath, newPathParent;

			if (checkingFiles.Length == 0)
			{
				WriteColor("INF: ", ConsoleColor.Cyan); Console.WriteLine($"[{checkingName}] directory is empty");
			}
			if (originFiles.Length == 0)
			{
				WriteColor("INF: ", ConsoleColor.Cyan); Console.WriteLine($"[{originName}] directory is empty.");
			}

			if (checkingFiles.Length != 0 && originFiles.Length != 0)
			{
				for (Int32 ind1 = 0; ind1 < checkingFiles.Length; ind1++)
				{
					for (Int32 ind2 = 0; ind2 < originFiles.Length; ind2++)
					{
						if (String.Equals(Path.GetFileName(checkingFiles[ind1]), Path.GetFileName(originFiles[ind2])))
						{
							countPairs++;
							WriteColor("WRN: ", ConsoleColor.Yellow); Console.WriteLine($"{checkingFiles[ind1].Substring(checkingFullPath.Length + 1)}");
							Console.WriteLine($"     {originFiles[ind2].Substring(originFullPath.Length + 1)}");

							if (moveDuplicates)
							{
								// Create temporary folder for diplicated files
								if (tempDir == null)
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
			{
				WriteColor("INF: ", ConsoleColor.Cyan); Console.WriteLine("Duplicates weren't found.");
			}
			else
			{
				WriteColor("INF: ", ConsoleColor.Cyan); Console.WriteLine($"Total pairs: {countPairs}");
			}
		}

		/// <summary>
		/// Helper: Search duplicated files inside directory
		/// </summary>
		private static void SearchInsideDir()
		{
			String[] files = Directory.GetFiles(originFullPath, "*.*", SearchOption.AllDirectories);
			Int32 countPairs = 0;

			if (files.Length >= 2)
			{
				for (Int32 ind1 = 0; ind1 < files.Length - 1; ind1++)
				{
					for (Int32 ind2 = ind1 + 1; ind2 < files.Length; ind2++)
					{
						if (String.Equals(Path.GetFileName(files[ind1]), Path.GetFileName(files[ind2])))
						{
							countPairs++;
							WriteColor("WRN: ", ConsoleColor.Yellow); Console.WriteLine($"{ files[ind1].Substring(originFullPath.Length + 1)}");
							Console.WriteLine($"     {files[ind2].Substring(originFullPath.Length + 1)}");
							break;
						}
					}
				}
			}

			if (countPairs == 0)
				Console.WriteLine("INF: Duplicates weren't found.");
			else
				WriteColor("INF: ", ConsoleColor.Cyan);  Console.WriteLine($"Total pairs: {countPairs}");
		}

		/// <summary>
		/// Initializes workspace for program - check all pathes and demands via arguments
		/// </summary>
		/// <param name="args"></param>
		private static Int32 InitProgram(string[] args)
		{
			Int32 foldersNumber = 0; // Define how much folders are in init file

			if (args.Length != 0) // User inputed arguments
			{
				List<String> initERR = new List<String>();
				List<String> initINF = new List<String>();
				WriteColor("TSK: Program variables initialization:\n", ConsoleColor.DarkGreen);
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
										initERR.Add("Initializtion file must has 1 line with path at least");
										stopProgram = true;
									}
									else
									{
										foldersNumber = 1;

										StringBuilder tempPath = new StringBuilder(256);
										DirectoryInfo dirInfo;

										tempPath.Append(initFileLines[0]).Replace('/', '\\');
										originFullPath = Path.Combine(Path.GetDirectoryName(initFileFullPath), tempPath.ToString());
										dirInfo = new DirectoryInfo(originFullPath);
										originName = dirInfo.Name;

										// Checking whether both pathes are existed
										if (!Directory.Exists(originFullPath))
										{
											initERR.Add($"[{originName}] directory doesn't exist -> {originFullPath}");
											stopProgram = true;
										}
										else
											initINF.Add($"Dir #1: {originFullPath}");

										if (initFileLines.Length > 1 && initFileLines[1].Length != 0)
										{
											foldersNumber = 2; // Take first two lines. Other doesn't matter

											tempPath.Clear().Append(initFileLines[1]).Replace('/', '\\');
											checkingFullPath = Path.Combine(Path.GetDirectoryName(initFileFullPath), tempPath.ToString());
											dirInfo = new DirectoryInfo(checkingFullPath);
											checkingName = dirInfo.Name;
											if (!Directory.Exists(checkingFullPath))
											{
												initERR.Add($"[{checkingName}] directory doesn't exist -> {checkingFullPath}");
												stopProgram = true;
											}
											else
												initINF.Add($"Dir #2: {checkingFullPath}");
										}
									}
									ind++;
								}
								else
								{
									initERR.Add($"Specified initialization file doesn't exist -> {initFileFullPath}");
									stopProgram = true;
								}
							}
							else
							{
								initERR.Add("The path to initialize file wasn't specifed after -i argument");
								stopProgram = true;
							}
							break;
						case "-r":
							saveReport = true;
							initINF.Add($"Report will be saved in [{checkingName}] directory");
							break;
						case "-m":
							moveDuplicates = true;
							initINF.Add($"Duplicated files will be moved to [{checkingName}\\_date_time] directory");
							break;
					}
				}

				// Processing initInfo variable (checking whether initialization has gone successfully
				StringBuilder resultInitInfo = new StringBuilder(512);
				if (stopProgram) // Stop program if there are errors
				{
					initERR.Add("The program cannot be run due to lack of complete information from the user");
					foreach(var str in initERR)
					{
						WriteColor("ERR: ", ConsoleColor.Red); Console.WriteLine(str);
					}
				}
				else
				{
					foreach (var str in initINF)
					{
						WriteColor("INF: ", ConsoleColor.Cyan); Console.WriteLine(str);
					}
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

		/// <summary>
		/// Color print to console
		/// </summary>
		/// <param name="color"></param>
		/// <param name="text"></param>
		private static void WriteColor(String text, ConsoleColor color)
		{
			var tempColor = Console.ForegroundColor;
			Console.ForegroundColor = color;
			Console.Write(text);
			Console.ForegroundColor = tempColor;
		}
	}
}