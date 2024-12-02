using System.Diagnostics;
using FRI.AUS2.Libs.Structures.Files;
using FRI.AUS2.SP2.Libs.Models;
using FRI.AUS2.SP2.Libs.Utils;

namespace FRI.AUS2.SP2.Libs
{
    public class SP2Backend :  IDisposable
    {
        public HeapFile<Customer> _allData;

        public ExtendableHashFile<CustomerAddressById> _dataById;
        public ExtendableHashFile<CustomerAddressByEcv> _dataByEcv;


        private CustomersGenerator _generator;
        public CustomersGenerator Generator => _generator;

        public SP2Backend(int blockSize, Uri dataFolder)
        {
            // _allData = new HeapFile<Customer>(2405, new(dataFolder.LocalPath + "allData.bin")); // BlockFactor = 2
            _allData = new HeapFile<Customer>(blockSize, new(dataFolder.LocalPath + "allData.bin")); 

            // _dataById = new ExtendableHashFile<CustomerAddressById>(30, new(dataFolder.LocalPath), "ehfById"); // BlockFactor = 2
            _dataById = new ExtendableHashFile<CustomerAddressById>(blockSize / 100, new(dataFolder.LocalPath), "ehfById"); 

            // _dataByEcv = new ExtendableHashFile<CustomerAddressByEcv>(45, new(dataFolder.LocalPath), "ehfByEcv"); // BlockFactor = 2
            _dataByEcv = new ExtendableHashFile<CustomerAddressByEcv>(blockSize / 100, new(dataFolder.LocalPath), "ehfByEcv");

            _generator = new CustomersGenerator(_dataById, _dataByEcv);
        }

        public void GenerateCustomers(int count, int? seed)
        {
            if (seed is not null)
            {
                _generator.SetSeed(seed.Value);
            }

            foreach (var customer in _generator.GenerateCustomers(count))
            {
                AddCustomer(customer);
            }
        }

        public void AddCustomer(Customer customer)
        {
            if (customer.Id is null || customer.ECV is null)
            {
                throw new ArgumentException("Id and ECV must be set");
            }

            Debug.WriteLine($"Adding customer: {customer}");

            var blockAddress = _allData.Insert(customer);

            _dataById.Insert(new CustomerAddressById
            {
                Id = customer.Id.Value,
                Addreess = blockAddress
            });

            _dataByEcv.Insert(new CustomerAddressByEcv
            {
                ECV = customer.ECV,
                Addreess = blockAddress
            });
        }

        public Customer GetCustomerById(int id)
        {
            var customerAddressById = _dataById.Find(new CustomerAddressById
            {
                Id = id
            });

            return _allData.Find(customerAddressById.Addreess, new Customer()
            {
                Id = id
            }) ?? throw new KeyNotFoundException();
        }

        public Customer GetCustomerByEcv(string ecv)
        {
            var customerAddressByEcv = _dataByEcv.Find(new CustomerAddressByEcv
            {
                ECV = ecv
            });

            return _allData.Find(customerAddressByEcv.Addreess, new Customer()
            {
                ECV = ecv
            }) ?? throw new KeyNotFoundException();
        }

        public void UpdateCustomer(Customer customer)
        {
            if (customer.Id is null || customer.ECV is null)
            {
                throw new ArgumentException("Id and ECV must be set");
            }

            var customerAddressById = _dataById.Find(new CustomerAddressById
            {
                Id = customer.Id.Value
            });

            _allData.Update(customerAddressById.Addreess, customer, customer);
        }

        public void Clear()
        {
            _allData.Clear();
            _dataById.Clear();
            _dataByEcv.Clear();
        }

        public void Dispose()
        {
            _allData.Dispose();
            _dataById.Dispose();
            _dataByEcv.Dispose();
        }
    }
}
