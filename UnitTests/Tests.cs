using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using TestTask;

namespace UnitTests
{
	[TestFixture]
	public class ReadOnlyStreamTests
	{
		string fileEmpty = "FilesForTesting/empty.txt";
		string fileGenerated = "FilesForTesting/generated.txt";
		string fileSingleLetterStats = "FilesForTesting/singleLetterStats.txt";
		string fileSingleLetterStats2 = "FilesForTesting/singleLetterStats2.txt";
		string fileDoubleLetterStats = "FilesForTesting/doubleLetterStats.txt";
		string fileDoubleLetterStats2 = "FilesForTesting/doubleLetterStats2.txt";
		
		List<LetterStats> singleStatsExpected = new List<LetterStats>()
		{
			new LetterStats("k", 4),

			new LetterStats("A", 1),
			new LetterStats("a", 1),

			new LetterStats("B", 2),
			new LetterStats("b", 3),

			new LetterStats("C", 2),
			new LetterStats("d", 3),
			new LetterStats("e", 1),
			new LetterStats("f", 1),
			new LetterStats("g", 3),
			new LetterStats("h", 1),
			new LetterStats("i", 3),
			new LetterStats("j", 1),
			new LetterStats("m", 2),
			new LetterStats("n", 2),
		};
		

		[Test]
		public void ReadOnlyStream()
		{
			var stream = new ReadOnlyStream(fileEmpty);
			Assert.True(stream.IsEof);

			Assert.Catch<EndOfStreamException>(() => stream.ReadNextChar());

			Assert.Catch<Exception>(() =>
			{
				ReadOnlyStream stream3 = new ReadOnlyStream("noFilePath");
			});

			File.WriteAllText(fileGenerated, "text");
			using (var stream4 = new ReadOnlyStream(fileGenerated))
			{
				Assert.IsFalse(stream4.IsEof);
				Assert.Catch<IOException>(() => File.WriteAllText(fileGenerated, "text"));
			}

			Assert.DoesNotThrow(() => File.WriteAllText(fileGenerated, "text")); 
		}

		[Test]
		public void SingleLetterStats()
		{
			using (var stream1 = new ReadOnlyStream(fileSingleLetterStats))
			{
				List<LetterStats> actualStats = Program.FillSingleLetterStats(stream1);
				AssertEqualStats(singleStatsExpected, actualStats);
			}
		}

		[Test]
		public void RemoveCharStatsByType()
		{
			using (var stream1 = new ReadOnlyStream(fileSingleLetterStats))
			{
				List<LetterStats> actualStats = Program.FillSingleLetterStats(stream1);

				// Тестируем удаление галсные\согласных
				List<LetterStats> consonantsStatExpected = RemoveLetters(singleStatsExpected, "Aaei");
				List<LetterStats> vowelStatExpected = RemoveLetters(singleStatsExpected, "kBbCdfghjmn");
				List<LetterStats> vowelActualStats = actualStats.ToList();
				List<LetterStats> consonantsActualStats = actualStats.ToList();

				Program.FillVowels();
				Program.RemoveCharStatsByType(vowelActualStats, CharType.Consonants);
				Program.RemoveCharStatsByType(consonantsActualStats, CharType.Vowel);

				AssertEqualStats(vowelStatExpected, vowelActualStats);
				AssertEqualStats(consonantsStatExpected, consonantsActualStats);
			}
		}

		[Test]
		public void SingleLetterStats2()
		{
			List<LetterStats> expected = new List<LetterStats>()
			{ 
				new LetterStats("a", 2), 
				new LetterStats("n", 1),
			};

			using (var stream1 = new ReadOnlyStream(fileSingleLetterStats2))
			{
				List<LetterStats> actual = Program.FillSingleLetterStats(stream1);
				Program.PrintStatistic(actual);
				AssertEqualStats(expected, actual);
			}
		}
 
		[Test]
		public void SingleLetterStatsEmptyFile()
		{
			List<LetterStats> expected = new List<LetterStats>();

			using (var stream = new ReadOnlyStream(fileEmpty))
			{
				List<LetterStats> actual = Program.FillSingleLetterStats(stream);
				Program.PrintStatistic(actual);
				AssertEqualStats(expected, actual);
			}
		}
		
		[Test]
		public void DoubleLetterStats()
		{
			List<LetterStats> expected = new List<LetterStats>()
			{  
				new LetterStats("CC", 4),
				new LetterStats("DD", 1),
				new LetterStats("EE", 3),
				new LetterStats("FF", 2),
				new LetterStats("GG", 2),
				new LetterStats("II", 1),
				new LetterStats("JJ", 1),
			};

			using (var stream = new ReadOnlyStream(fileDoubleLetterStats))
			{
				List<LetterStats> actual = Program.FillDoubleLetterStats(stream);
				Program.PrintStatistic(actual);
				AssertEqualStats(expected, actual);
			}
		}
		
		[Test]
		public void DoubleLetterStats2()
		{
			List<LetterStats> expected = new List<LetterStats>()
			{  
				new LetterStats("AA", 1) 
			};

			using (var stream = new ReadOnlyStream(fileDoubleLetterStats2))
			{
				List<LetterStats> actual = Program.FillDoubleLetterStats(stream);
				Program.PrintStatistic(actual);
				AssertEqualStats(expected, actual);
			}
		}

		/// <summary>
		/// Проверяет, что статистики одинаковые
		/// </summary>
		void AssertEqualStats(IList<LetterStats> expected, IList<LetterStats> actual)
		{
			Assert.AreEqual(expected.Count, actual.Count);
			for (int i = 0; i < expected.Count; i++)
			{
				Assert.AreEqual(expected[i].Letter, actual[i].Letter);
				Assert.AreEqual(expected[i].Count, actual[i].Count, $"letter '{expected[i].Letter}'");
			}
		}

		/// <summary>
		/// Удаляет из статистики вхождения букв
		/// </summary>
		/// <param name="lettersToRemove">Буквы дял удаления из статистики</param>
		/// <returns>Новая статистика, без удаленых букв</returns>
		List<LetterStats> RemoveLetters(List<LetterStats> letterStats, string lettersToRemove)
		{
			return letterStats
				.Where(letterStat => !lettersToRemove.Any(toRemove => letterStat.Letter.StartsWith(toRemove.ToString())))
				.ToList();
		}
	}
}