using FRI.AUS2.Libs.Helpers;
using FRI.AUS2.StructureTester.Libs.Utils.OperationsGenerator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FRI.AUS2.StructureTester.Libs.Controls
{
    /// <summary>
    /// Interaction logic for OperationsGeneratorForm.xaml
    /// </summary>
    public partial class OperationsGeneratorForm : UserControl
    {
        public string? _logFilePath {get; set;} = null;

        public int Count 
        {
            get => int.Parse(_txtbx_Count.Value);
            set => _txtbx_Count.Value = value.ToString();
        }

        #region OperationsRatioUI
        Dictionary<OperationType, int> OperationsRatios
        {
            get
            {
                return new Dictionary<OperationType, int>()
                {
                    { OperationType.Insert, int.Parse(_txtbx_operationsAdd.Value) },
                    { OperationType.InsertDuplicate, int.Parse(_txtbx_operationsAddDuplicate.Value) },
                    { OperationType.Find, int.Parse(_txtbx_operationsFind.Value) },
                    { OperationType.FindSpecific, int.Parse(_txtbx_operationsFindSpecific.Value) },
                    { OperationType.Delete, int.Parse(_txtbx_operationsDelete.Value) },
                    { OperationType.DeleteSpecific, int.Parse(_txtbx_operationsDeleteSpecific.Value) }
                };
            }
        }

        public int OpereationRatioInsert 
        {
            get => int.Parse(_txtbx_operationsAdd.Value);
            set => _txtbx_operationsAdd.Value = value.ToString();
        }

        public int OpereationRatioInsertDuplicate
        {
            get => int.Parse(_txtbx_operationsAddDuplicate.Value);
            set => _txtbx_operationsAddDuplicate.Value = value.ToString();
        }

        public int OpereationRatioFind
        {
            get => int.Parse(_txtbx_operationsFind.Value);
            set => _txtbx_operationsFind.Value = value.ToString();
        }

        public int OpereationRatioFindSpecific
        {
            get => int.Parse(_txtbx_operationsFindSpecific.Value);
            set => _txtbx_operationsFindSpecific.Value = value.ToString();
        }

        public int OpereationRatioDelete
        {
            get => int.Parse(_txtbx_operationsDelete.Value);
            set => _txtbx_operationsDelete.Value = value.ToString();
        }

        public int OpereationRatioDeleteSpecific
        {
            get => int.Parse(_txtbx_operationsDeleteSpecific.Value);
            set => _txtbx_operationsDeleteSpecific.Value = value.ToString();
        }
        #endregion

        public event EventHandler? RunTest;

        public OperationsGeneratorForm()
        {
            InitializeComponent();
        }

        public void InitializeGenerator<T>(OperationsGenerator<T> operationsGenerator) where T : class
        {
            // init
            operationsGenerator.Seed = int.Parse(_txtbx_Seed.Value);
            operationsGenerator.Count = int.Parse(_txtbx_Count.Value);

            // init ratio of operations
            foreach (var operation in OperationsRatios)
            {
                operation.Value.Repeat(() => operationsGenerator.AddOperation(operation.Key));
            }

            // init log settings
            operationsGenerator.LogsVerbosity = int.Parse(_txtbx_logVerbosity.Value);
            operationsGenerator.LogsStatsFrequency = int.Parse(_txtbx_logStatsFreq.Value);
        }

        public void InitializeForm<T>(OperationsGenerator<T> operationsGenerator) where T : class
        {
            _logFilePath = operationsGenerator.LogsFileUri.AbsolutePath;
        }
        

        private void _btn_testerLog_Click(object sender, RoutedEventArgs e)
        {
            if (_logFilePath is null)
            {
                MessageBox.Show("Log file was not initialized yet.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // copy the path to clipboard
            Clipboard.SetText(_logFilePath);

            MessageBox.Show($"Log file path was copied to clipboard: {_logFilePath}", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void _btn_testerRunTest_Click(object sender, RoutedEventArgs e)
        {
            RunTest?.Invoke(this, EventArgs.Empty);
        }
    }
}
