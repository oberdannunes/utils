using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Xml;

namespace ItauOFXCleaner {
	class Program {
		static void Main(string[] args) {
			var  skippedTransactionTypes = new string[] { 
				"SALDO INICIAL", 
				"RES APLIC AUT MAIS", 
				"SALDO APLIC AUT MAIS", 
				"APL APLIC AUT MAIS",
				"SALDO FINAL"
			};

			if (args.Length != 1
				|| !File.Exists(args[0])
				|| Path.GetExtension(args[0]) != ".ofx") {
				Console.WriteLine("Informe o nome do arquivo .ofx");
				return;
			}

			string inputFile = args.FirstOrDefault();
			string outputFile = Path.Combine(Path.GetDirectoryName(inputFile), Path.GetFileNameWithoutExtension(inputFile) + "_out" + Path.GetExtension(inputFile));

			Console.WriteLine($"Processando {outputFile}");

			var linesBuffer = new List<string>();

			using (StreamReader reader = new StreamReader(inputFile)) {
				using (StreamWriter writer = new StreamWriter(outputFile)) {
					string line = null;
					bool insideTransaction = false;
					bool skipTransaction = false;

					while ((line = reader.ReadLine()) != null) {
						if (line == "<STMTTRN>") {
							insideTransaction = true;
							skipTransaction = false;
						} else if (line == "</STMTTRN>") {
							insideTransaction = false;
						}

						if (insideTransaction) {

							if (line.StartsWith("<MEMO>")) {
								string memoType = line.Substring(6);
								if (skippedTransactionTypes.Contains(memoType)) {
									skipTransaction = true;
								}
							}

							linesBuffer.Add(line);
						} else {
							if (linesBuffer.Count > 0) {
								if (!skipTransaction) {
									for (int i = 0, l = linesBuffer.Count; i < l; i++) {
										writer.Write(linesBuffer[i]);
										writer.Write('\n');
									}
									writer.Write(line);
									writer.Write('\n');
								}

								linesBuffer.Clear();
							} else {
								writer.Write(line);
								writer.Write('\n');
							}
						}
					}
				}
			}


		}
	}
}
