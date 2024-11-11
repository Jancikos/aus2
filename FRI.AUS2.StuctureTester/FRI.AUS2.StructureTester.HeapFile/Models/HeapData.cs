using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRI.AUS2.StructureTester.HeapFileTester.Models
{
    class HeapData
    {
        private string _firstname = "";
        public string Firstname
        {
            get => _firstname;
            set
            {
                if (value.Length > 15)
                {
                    throw new ArgumentException("Firstname is max 15 characters long");
                }
                _firstname = value;
            }
        }

        private string _lastname = "";
        public string Lastname
        {
            get => _lastname;
            set
            {
                if (value.Length > 20)
                {
                    throw new ArgumentException("Lastname is max 20 characters long");
                }
                _lastname = value;
            }
        }

        public int Id;

    }
}
