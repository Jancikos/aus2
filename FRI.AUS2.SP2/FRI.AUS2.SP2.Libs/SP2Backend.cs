using FRI.AUS2.Libs.Structures.Files;
using FRI.AUS2.SP2.Libs.Models;

namespace FRI.AUS2.SP2.Libs
{
    public class SP2Backend
    {
        private HeapFile<Customer> _allData;

        private ExtendableHashFile<CustomerAddressById> _dataById;
        private ExtendableHashFile<CustomerAddressByEcv> _dataByEcv;

        


    }
}
