// Copyright (c) 2015 CNUnit Project
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ***********************************************************************

using System.Reflection;
using System.Collections.Generic;

using NUnit.Runner.Services;
using NUnit.Runner.View;
using NUnit.Runner.ViewModel;
using Xamarin.Forms;
using System.ComponentModel;
using System.Linq;

namespace NUnit.Runner
{
    /// <summary>
    /// The NUnit Xamarin test runner
    /// </summary>
	public partial class App : Application
    {
        private readonly SummaryViewModel _model;

        /// <summary>
        /// Constructs a new app adding the current assembly to be tested
        /// <param name="options">An optional dictionary of options for loading the assembly.</param>
        /// </summary>
		public App(Dictionary<string, object> options = null)
        {
            InitializeComponent();

            if (Device.RuntimePlatform == Device.UWP)
            {
                Resources["defaultBackground"] = Resources["windowsBackground"];
            }

            _model = new SummaryViewModel();
            MainPage = new NavigationPage(new SummaryView(_model));

            UpdateTestAssemblies(options);
            _model.PropertyChanged += OnPropertyChanged;
        }

        /// <summary>
        /// Adds an assembly to be tested.
        /// </summary>
        /// <param name="testAssembly">The test assembly.</param>
        /// <param name="options">An optional dictionary of options for loading the assembly.</param>
        public void AddTestAssembly(Assembly testAssembly, Dictionary<string, object> options = null)
        {
            _model.AddTest(testAssembly, options);
        }

        protected void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Options))
                UpdateTestAssemblies(Options.ToDictionary());
        }

        /// <summary>
        /// User options for the test suite.
        /// </summary>
        public TestOptions Options
        {
            get => _model.Options;
            set => _model.Options = value;
        }

        /// <summary>
        /// Start running the tests with the given filter
        /// </summary>
        public bool RunTests(Dictionary<string,string> parameters)
        {
            if (_model == null)
                return false;

            if (!_model.RunTestsCommand.CanExecute(this))
                return false;

            _model.Options = new TestOptions
            {
                TcpWriterParameters = _model.Options.TcpWriterParameters,

                //convert custom paramters to native options on key match
                AutoRun = parameters.ContainsKey(nameof(TestOptions.AutoRun)) ? bool.Parse(parameters[nameof(TestOptions.AutoRun)]) : _model.Options.AutoRun,
                CreateXmlResultFile = parameters.ContainsKey(nameof(TestOptions.CreateXmlResultFile)) ? bool.Parse(parameters[nameof(TestOptions.CreateXmlResultFile)]) : _model.Options.CreateXmlResultFile,
                ResultFilePath = parameters.ContainsKey(nameof(TestOptions.ResultFilePath)) ? parameters[nameof(TestOptions.ResultFilePath)] : _model.Options.ResultFilePath,
                TerminateAfterExecution = parameters.ContainsKey(nameof(TestOptions.TerminateAfterExecution)) ? bool.Parse(parameters[nameof(TestOptions.TerminateAfterExecution)]) : _model.Options.TerminateAfterExecution,

                PartitionIndex = parameters.ContainsKey(nameof(TestOptions.PartitionIndex)) ? int.Parse(parameters[nameof(TestOptions.PartitionIndex)]) : _model.Options.PartitionIndex,
                TotalPartitionCount = parameters.ContainsKey(nameof(TestOptions.TotalPartitionCount)) ? int.Parse(parameters[nameof(TestOptions.TotalPartitionCount)]) : _model.Options.TotalPartitionCount,
                PartitionRandomizerSeed = parameters.ContainsKey(nameof(TestOptions.PartitionRandomizerSeed)) ? int.Parse(parameters[nameof(TestOptions.PartitionRandomizerSeed)]) : _model.Options.PartitionRandomizerSeed,

                CustomParameters = parameters,
            };

            UpdateTestAssemblies(_model.Options.ToDictionary());

            _model.RunTestsCommand.Execute(this);

            return true;
        }

        private void UpdateTestAssemblies(Dictionary<string, object> options)
        {
            _model.ClearTests();

            _model.AddTest(Assembly.GetEntryAssembly(), options);
        }
    }
}
