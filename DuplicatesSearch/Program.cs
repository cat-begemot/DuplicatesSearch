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
			InitProgram(args); // Initializes workspace for program - check all pathes and demands via arguments
			if (stopProgram) return;


			Console.ReadLine();
		}

		/// <summary>
		/// Initializes workspace for program - check all pathes and demands via arguments
		/// </summary>
		/// <param name="args"></param>
		private static void InitProgram(string[] args)
		{
			if (args.Length == 0) // args arrange is empty
			{
				Console.WriteLine("Show help information");
				stopProgram = true;
			}
			else // Arguments were passed
			{
				List<String> initInfo = new List<String>();
				Console.WriteLine("1. Initialization:");
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
									if (initFileLines.Length < 2)
									{
										initInfo.Add("ERR: Initializtion file must has 2 line with pathes");
										stopProgram = true;
									}
									else
									{
										StringBuilder tempPath = new StringBuilder(256);
										tempPath.Append(initFileLines[0]).Replace('/', '\\');
										originFullPath = Path.Combine(Path.GetDirectoryName(initFileFullPath), tempPath.ToString());
										tempPath.Clear().Append(initFileLines[1]).Replace('/', '\\');
										checkingFullPath = Path.Combine(Path.GetDirectoryName(initFileFullPath), tempPath.ToString());

										// Checking whether both pathes are existed
										if (!Directory.Exists(originFullPath))
										{
											initInfo.Add($"ERR: [Origin] directory doesn't exist -> {originFullPath}");
											stopProgram = true;
										}
										else
											initInfo.Add($"INF: [Origin] -> {originFullPath}");
										if (!Directory.Exists(checkingFullPath))
										{
											initInfo.Add($"ERR: [Checking] directory doesn't exist -> {checkingFullPath}");
											stopProgram = true;
										}
										else
											initInfo.Add($"INF: [Checking] -> {checkingFullPath}");
									}
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

				// Processing initInfo variable (checking whether initialization has gone successfully
				StringBuilder resultInitInfo = new StringBuilder(512);
				if (stopProgram) // Stop program if there are errors
				{
					foreach (var str in initInfo)
						if (str.StartsWith("ERR:"))
							resultInitInfo.AppendLine(str);
					resultInitInfo.AppendLine().AppendLine("MSG: The program cannot be run due to lack of complete information from the user");
				}
				else
				{
					foreach (var str in initInfo)
						if (!str.StartsWith("ERR:"))
							resultInitInfo.AppendLine(str);
					resultInitInfo.AppendLine().AppendLine("MSG: The program was initialized succesfully");
				}

				// Display initializator result
				Console.WriteLine(resultInitInfo.ToString());
			}
		}
	}
}