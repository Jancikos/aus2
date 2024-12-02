using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FRI.AUS2.Libs.Helpers;
using FRI.AUS2.Libs.Structures.Files;
using FRI.AUS2.StructureTester.HeapFileTester.Models;

namespace FRI.AUS2.StructureTester.HeapFileTester.Utils
{
    public class HeapDataGenerator
    {
        private Random _random;

        private static int _idCounter = 0;
        public static void SetIdCounter(int id)
        {
            _idCounter = id;
        }

        private readonly string[] _firstnames = { "Jozef", "Ján", "Peter", "Marek", "Martin", "Michal", "Tomáš", "Lukáš", "Miroslav", "Ivan" };
        private readonly string[] _lastnames = { "Novák", "Horváth", "Kováč", "Varga", "Tóth", "Nagy", "Baláž", "Molnár", "Szabó", "Kovács" };
        private readonly string[] _itemDescriptions = { "Olej", "Filtre", "Brzdy", "Výfuk", "Pneumatiky", "Baterie", "Interiér", "Elektronika" };
        private readonly string[] _ecvPrefix = { "BA", "KE", "NR", "PO", "PP", "TT", "MT", "ZV", "SL", "LV" };

        public HeapDataGenerator() : this(DateTime.Now.Millisecond) {}
        public HeapDataGenerator(int seed) : this(new Random(seed)) {}
        public HeapDataGenerator(Random random)
        {
            _random = random;
        }

        public IEnumerable<HeapData> GenerateItems(int count)
        {
            var newIds = Enumerable
                .Range(_idCounter, count)
                .OrderBy(_ => _random.Next());
            _idCounter += count;

            foreach (var id in newIds)
            {
                yield return GenerateItem(id);
            }
        }



        private static int _maxId = 10000;
        private static HashSet<int> _usedIds = new HashSet<int>();
        public int GenerateId() 
        {
            var failedAttempts = 0;
            while (true)
            {
                if (failedAttempts > 5)
                {
                    _maxId *= 10;
                    failedAttempts = 0;
                    continue;
                }

                var id = _random.Next(_maxId);
                if (!_usedIds.Contains(id))
                {
                    _usedIds.Add(id);
                    return id;
                }
                failedAttempts++;
            }
        }

        public HeapData GenerateItem() => GenerateItem(GenerateId());

        public HeapData GenerateItem(int id)
        {
            return new HeapData
            {
                Id = id,
                Firstname = _firstnames[_random.Next(_firstnames.Length)],
                Lastname = _lastnames[_random.Next(_lastnames.Length)],
                ECV = GenerateECV(),
                Items = GenerateNestedItems()
            };
        }
        
        public List<NesteHeapDataItem> GenerateNestedItems()
        {
            var items = new List<NesteHeapDataItem>();
            Random random = new Random();

            var itemsCount = random.Next(HeapData.ItemsMaxCount);
            for (int i = 0; i < itemsCount; i++)
            {
                items.Add(new NesteHeapDataItem
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(random.Next(-1000, 1000))),
                    Price = random.Next(100000) / (double)100,
                    Description = _itemDescriptions[random.Next(_itemDescriptions.Length)]
                });
            }

            return items;
        }

        public string GenerateECV()
        {
            // ECV format: <prefix><3 numbers><2 letters>
            return $"{_ecvPrefix[_random.Next(_ecvPrefix.Length)]}{_random.Next(1000):D3}{(char)('A' + _random.Next(26))}{(char)('A' + _random.Next(26))}";
        }
    }
}
