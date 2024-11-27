using FRI.AUS2.Libs.Structures.Files;
using FRI.AUS2.SP2.Libs.Models;

namespace FRI.AUS2.SP2.Libs
{
    public class SP2Backend :  IDisposable
    {
        public HeapFile<Customer> _allData;

        public ExtendableHashFile<CustomerAddressById> _dataById;
        public ExtendableHashFile<CustomerAddressByEcv> _dataByEcv;

        public SP2Backend(int blockSize, Uri dataFolder)
        {
            _allData = new HeapFile<Customer>(blockSize, new(dataFolder.LocalPath + "allData.bin"));

            _dataById = new ExtendableHashFile<CustomerAddressById>(blockSize, new(dataFolder.LocalPath), "ehfById");
            _dataByEcv = new ExtendableHashFile<CustomerAddressByEcv>(blockSize, new(dataFolder.LocalPath), "ehfByEcv");
        }

        public void AddCustomer(Customer customer)
        {
            if (customer.Id is null || customer.ECV is null)
            {
                throw new ArgumentException("Id and ECV must be set");
            }

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

        public void Dispose()
        {
            _allData.Dispose();
            _dataById.Dispose();
            _dataByEcv.Dispose();
        }
    }
}
