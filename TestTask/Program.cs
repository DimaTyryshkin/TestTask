using System;
using System.Linq;
using System.Collections.Generic;

namespace TestTask
{
    // *** Работает только с символами кириллицы и латиницы ***

    public static class Program
    {
        private static HashSet<char> _vowels;
        private static string[] _languagesVowels =
        {
            "ауоиэыяюеё", // кириллица
            "aeiouy" // латиница
        };

        /// <summary>
        /// Программа принимает на входе 2 пути до файлов.
        /// Анализирует в первом файле кол-во вхождений каждой буквы (регистрозависимо). Например А, б, Б, Г и т.д.
        /// Анализирует во втором файле кол-во вхождений парных букв (не регистрозависимо). Например АА, Оо, еЕ, тт и т.д.
        /// По окончанию работы - выводит данную статистику на экран.
        /// </summary>
        /// <param name="args">Первый параметр - путь до первого файла.
        /// Второй параметр - путь до второго файла.</param>
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Нужно указать два файла");
                return;
            }
            
            FillVowels();

            using (IReadOnlyStream inputStream1 = GetInputStream(args[0]))
            {
                List<LetterStats> singleLetterStats = FillSingleLetterStats(inputStream1);
                RemoveCharStatsByType(singleLetterStats, CharType.Vowel);
                PrintStatistic(singleLetterStats);
            }

            using (IReadOnlyStream inputStream2 = GetInputStream(args[1]))
            {
                List<LetterStats> doubleLetterStats = FillDoubleLetterStats(inputStream2);
                RemoveCharStatsByType(doubleLetterStats, CharType.Consonants);
                PrintStatistic(doubleLetterStats);
            }

            Console.ReadKey(true);
        }

        public static void FillVowels()
        {
            _vowels = new HashSet<char>();
            foreach (string language in _languagesVowels)
            {
                foreach (char c in language)
                {
                    _vowels.Add(char.ToUpperInvariant(c));
                    _vowels.Add(char.ToLowerInvariant(c));
                }
            }
        }

        /// <summary>
        /// Ф-ция возвращает экземпляр потока с уже загруженным файлом для последующего посимвольного чтения.
        /// </summary>
        /// <param name="fileFullPath">Полный путь до файла для чтения</param>
        /// <returns>Поток для последующего чтения.</returns>
        private static ReadOnlyStream GetInputStream(string fileFullPath)
        {
            return new ReadOnlyStream(fileFullPath);
        }

        /// <summary>
        /// Ф-ция считывающая из входящего потока все буквы, и возвращающая коллекцию статистик вхождения каждой буквы.
        /// Статистика РЕГИСТРОЗАВИСИМАЯ!
        /// </summary>
        /// <param name="stream">Стрим для считывания символов для последующего анализа</param>
        /// <returns>Коллекция статистик по каждой букве, что была прочитана из стрима.</returns>
        public static List<LetterStats> FillSingleLetterStats(IReadOnlyStream stream)
        {
            return FillLetterStats(stream, latterRepeatCount: 1, caseSensitivity: true);
        }

        /// <summary>
        /// Ф-ция считывающая из входящего потока все буквы, и возвращающая коллекцию статистик вхождения парных букв.
        /// В статистику должны попадать только пары из одинаковых букв, например АА, СС, УУ, ЕЕ и т.д.
        /// Статистика - НЕ регистрозависимая!
        /// </summary>
        /// <param name="stream">Стрим для считывания символов для последующего анализа</param>
        /// <returns>Коллекция статистик по каждой букве, что была прочитана из стрима.</returns>
        public static List<LetterStats> FillDoubleLetterStats(IReadOnlyStream stream)
        {
            return FillLetterStats(stream, latterRepeatCount: 2, caseSensitivity: false);
        }

        /// <summary>
        /// Ф-ция перебирает все найденные буквы/парные буквы, содержащие в себе только гласные или согласные буквы.
        /// (Тип букв для перебора определяется параметром charType)
        /// Все найденные буквы/пары соответствующие параметру поиска - удаляются из переданной коллекции статистик.
        /// </summary>
        /// <param name="letters">Коллекция со статистиками вхождения букв/пар</param>
        /// <param name="charType">Тип букв для анализа</param>
        public static void RemoveCharStatsByType(IList<LetterStats> letters, CharType charType)
        {
            if (charType != CharType.Vowel && charType != CharType.Consonants)
                throw new NotSupportedException($"value {charType.ToString()} not supported");

            for (int i = letters.Count - 1; i >= 0; i--)
            {
                char c = letters[i].Letter[0];
                bool isVowels = _vowels.Contains(c);

                if (isVowels)
                {
                    if (charType == CharType.Vowel)
                        letters.RemoveAt(i);
                }
                else
                {
                    if (charType == CharType.Consonants)
                        letters.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Ф-ция выводит на экран полученную статистику в формате "{Буква} : {Кол-во}"
        /// Каждая буква - с новой строки.
        /// Выводить на экран необходимо предварительно отсортировав набор по алфавиту.
        /// В конце отдельная строчка с ИТОГО, содержащая в себе общее кол-во найденных букв/пар
        /// </summary>
        /// <param name="letters">Коллекция со статистикой</param>
        public static void PrintStatistic(IEnumerable<LetterStats> letters)
        {
            int n = 0;
            foreach (LetterStats latter in letters.OrderBy(stats => stats.Letter, new UpperFirstStringComparer()))
            {
                n++;
                Console.WriteLine($"{latter.Letter} : {latter.Count}");
            }

            Console.WriteLine($"ИТОГО : {n}");
        }

        /// <summary>
        /// Метод увеличивает счётчик вхождений по переданной структуре.
        /// </summary>
        /// <param name="letterStats"></param>
        private static void IncStatistic(LetterStats letterStats)
        {
            letterStats.Count++;
        }

        /// <summary>
        /// Ф-ция считывающая из входящего потока все буквы, и возвращающая коллекцию статистик вхождения одинарных/парных/тройных/и.т.д. букв.
        /// </summary>ы
        /// <param name="stream">Стрим для считывания символов для последующего анализа</param>
        /// <param name="latterRepeatCount">Количество повторений буквы, чтобы попасть в статистику</param>
        /// <param name="caseSensitivity">Определяет регистрозависимость статистики. Значение true, чтобы статистика была регистрозависимая</param>
        /// <returns>Коллекция статистик по каждой букве, что была прочитана из стрима.</returns>
        private static List<LetterStats> FillLetterStats(IReadOnlyStream stream, int latterRepeatCount, bool caseSensitivity)
        {
            if (stream.IsEof)
                return new List<LetterStats>();

            var charToStats = new Dictionary<string, LetterStats>(64);

            char lastLatter = (char)0;
            int counter = 0;

            while (!stream.IsEof)
            {
                char c = stream.ReadNextChar();
                if (char.IsLetter(c)) // Считаем только буквы
                {
                    if (!caseSensitivity)
                        c = char.ToUpperInvariant(c);

                    if (counter == 0)
                    {
                        counter++;
                    }
                    else
                    {
                        if (c == lastLatter)
                            counter++;
                        else
                            counter = 1;
                    }

                    if (counter == latterRepeatCount)
                    {
                        AddStats(c);
                        counter = 0;
                    }

                    lastLatter = c;
                }
                else
                {
                    counter = 0;
                }
            }

            void AddStats(char c)
            {
                string key = new string(c, latterRepeatCount);
                if (charToStats.TryGetValue(key, out LetterStats letterStats))
                {
                    IncStatistic(letterStats);
                }
                else
                {
                    charToStats.Add(key, new LetterStats(key, 1));
                }
            }

            return charToStats
                .Select(x => x.Value)
                .ToList();
        }
    }
}