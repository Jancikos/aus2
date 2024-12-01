using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FRI.AUS2.Libs.Structures.Files;
using FRI.AUS2.SP2.Libs.Models;

namespace FRI.AUS2.SP2.Libs.Utils
{
    public class CustomersGenerator
    {
        private static int _maxId = 1000;
        private static int _ecvSuffixCount = 2;
        private Random _random;

        private ExtendableHashFile<CustomerAddressById>? _customersIds;
        private ExtendableHashFile<CustomerAddressByEcv>? _customersECVs;

        private readonly string[] _firstnames = { "Jozef", "Jan", "Peter", "Marek", "Martin", "Michal", "Tomas", "Lukas", "Miroslav", "Ivan" };
        private readonly string[] _lastnames = { "Novak", "Horvath", "Kovac", "Varga", "Toth", "Nagy", "Balaz", "Molnar", "Szabo", "Kovacs" };
        private readonly string[] _itemDescriptions = { "Olej", "Filtre", "Brzdy", "Vyfuk", "Pneumatiky", "Baterie", "Interier", "Elektronika" };
        private readonly string[] _ecvPrefix = { "BB", "KA", "BA", "KE", "NR", "PO", "PP", "TT", "MT", "ZV", "SL", "LV" };

        public CustomersGenerator(ExtendableHashFile<CustomerAddressById>? customersIds = null, ExtendableHashFile<CustomerAddressByEcv>? customersECVs = null) : this(DateTime.Now.Millisecond, customersIds, customersECVs) { }
        public CustomersGenerator(int seed, ExtendableHashFile<CustomerAddressById>? customersIds = null, ExtendableHashFile<CustomerAddressByEcv>? customersECVs = null) : this(new Random(seed), customersIds, customersECVs) { }
        public CustomersGenerator(Random random, ExtendableHashFile<CustomerAddressById>? customersIds = null, ExtendableHashFile<CustomerAddressByEcv>? customersECVs = null)
        {
            _random = random;
            _customersIds = customersIds;
            _customersECVs = customersECVs;
        }

        public void SetSeed(int seed)
        {
            _random = new Random(seed);
        }

        public IEnumerable<Customer> GenerateCustomers(int count)
        {
            for (int i = 0; i < count; i++)
            {
                yield return GenerateCustomer();
            }
        }

        public Customer GenerateCustomer() => GenerateCustomer(GenerateId(), GenerateECV());
        public Customer GenerateCustomer(int id, string ecv)
        {
            return new Customer
            {
                Id = id,
                Firstname = GenerateFirstname(),
                Lastname = GenerateLastname(),
                ECV = ecv,
                Visits = GenerateServiceVisits().ToList()
            };
        }

        public int GenerateId()
        {
            int failedAttempts = 0;

            while (true)
            {
                if (failedAttempts > 5)
                {
                    _maxId *= 10;
                    failedAttempts = 0;
                }
                var id = _random.Next(_maxId);

                // check if id is unique
                if (_customersIds is not null)
                {
                    try
                    {
                        _customersIds.Find(new CustomerAddressById { Id = id });
                    
                        // id was found, so it is not unique
                        failedAttempts++;
                        continue;
                    }
                    catch (KeyNotFoundException)
                    {
                        // id was not found, so it is unique
                    }
                }

                return id;
            }
        }

        /// <summary>
        /// ECV format: <prefix><3 numbers><X letters>
        /// </summary>
        /// <returns></returns>
        public string GenerateECV()
        {
            int failedAttempts = 0;

            while (true)
            {
                if (failedAttempts > 5)
                {
                    _ecvSuffixCount++;
                    failedAttempts = 0;
                }

                var ecv = $"{_ecvPrefix[_random.Next(_ecvPrefix.Length)]}{_random.Next(1000):D3}";
                for (int i = 0; i < _ecvSuffixCount; i++)
                {
                    ecv += (char)('A' + _random.Next(26));
                }

                // check if ecv is unique
                if (_customersECVs is not null)
                {
                    try
                    {
                        _customersECVs.Find(new CustomerAddressByEcv { ECV = ecv });

                        // ecv was found, so it is not unique
                        failedAttempts++;
                        continue; 
                    }
                    catch (KeyNotFoundException)
                    {
                        // ecv was not found, so it is unique
                    }
                }

                return ecv;
            }

        }

        public ServiceVisit[] GenerateServiceVisits()
        {
            var items = new ServiceVisit[_random.Next(Customer.VisitsMaxCount)];

            for (int i = 0; i < items.Length; i++)
            {
                items[i] = new ServiceVisit
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(_random.Next(-1000, 1000))),
                    Price = _random.Next(100000) / (double)100,
                    Descriptions = GenerateDescriptions().ToArray()
                };
            }

            return items;
        }

        public string[] GenerateDescriptions()
        {
            var descriptions = new string[_random.Next(ServiceVisit.DescriptionsMaxCount)];

            for (int i = 0; i < descriptions.Length; i++)
            {
                descriptions[i] = _itemDescriptions[_random.Next(_itemDescriptions.Length)];
            }

            return descriptions;
        }

        public string GenerateFirstname() => _firstnames[_random.Next(_firstnames.Length)];
        public string GenerateLastname() => _lastnames[_random.Next(_lastnames.Length)];
    }
}
